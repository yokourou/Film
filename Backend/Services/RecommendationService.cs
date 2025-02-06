using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using WebApi.Models;
using System.Globalization;
using CsvHelper;

namespace WebApi.Services
{
    public class RecommendationService
    {
        private readonly MLContext _mlContext;
        private readonly BddContext _context;

        public RecommendationService(BddContext context)
        {
            _mlContext = new MLContext();
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool ModelExists(string modelPath) => File.Exists(modelPath);

        public ITransformer LoadModel(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Modèle introuvable.", modelPath);

            return _mlContext.Model.Load(modelPath, out _);
        }

        /// <summary>
        /// Entraîne le modèle collaboratif à partir des données de la base et le sauvegarde.
        /// </summary>
        public ITransformer TrainModelFromDatabase()
        {
            var trainingData = _context.Favourites
                .Include(f => f.Film)
                .Where(f => f.Film != null && f.Rating.HasValue)
                .Select(f => new UserFilmInteraction
                {
                    UserId = (uint)f.UserId,
                    FilmId = f.Film.Imdb ?? string.Empty,
                    Rating = (float)f.Rating.Value,
                    Genre = f.Film.Genre ?? "Unknown"
                })
                .ToList();

            if (!trainingData.Any())
                throw new InvalidOperationException("No training data available.");

            var model = TrainModel(trainingData);

            string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "recommendation-model.zip");
            SaveModel(model, modelPath);

            return model;
        }

        /// <summary>
        /// Entraîne le modèle de factorisation matricielle en utilisant ML.NET.
        /// Le pipeline inclut une transformation MapKeyToValue pour restituer FilmId sous forme de chaîne.
        /// </summary>
        public ITransformer TrainModel(List<UserFilmInteraction> trainingData)
        {
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(UserFilmInteraction.UserId),
                MatrixRowIndexColumnName = nameof(UserFilmInteraction.FilmId),
                LabelColumnName = nameof(UserFilmInteraction.Rating),
                NumberOfIterations = 50,
                ApproximationRank = 100
            };

            var pipeline = _mlContext.Transforms.Conversion
                .MapValueToKey(nameof(UserFilmInteraction.UserId))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(UserFilmInteraction.FilmId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                     outputColumnName: nameof(RecommendationPrediction.FilmId),
                     inputColumnName: nameof(UserFilmInteraction.FilmId)));

            return pipeline.Fit(dataView);
        }

        /// <summary>
        /// Calcule le profil de chaque utilisateur en regroupant leurs interactions favorites par genre.
        /// </summary>
        private List<UserProfile> ComputeUserProfiles()
        {
            return _context.Favourites
                .Include(f => f.Film)
                .Where(f => f.Film != null && f.Rating.HasValue)
                .AsEnumerable()
                .GroupBy(f => f.UserId)
                .Select(group => new UserProfile
                {
                    UserId = (uint)group.Key,
                    GenrePreferences = group
                        .GroupBy(f => f.Film.Genre)
                        .ToDictionary(
                            g => g.Key ?? "Unknown",
                            g => (float)g.Average(f => f.Rating.Value)
                        )
                })
                .ToList();
        }

        /// <summary>
        /// Méthode sécurisée pour obtenir les recommandations.
        /// En cas d'erreur, renvoie des suggestions par défaut.
        /// </summary>
        public List<RecommendationResult> SafeGetRecommendations(uint userId, int topN)
        {
            try
            {
                return RecommendBasedOnSimilarUsers(userId, topN);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans RecommendBasedOnSimilarUsers pour l'utilisateur {userId} : {ex.Message}");
                return GetDefaultSuggestions(topN);
            }
        }

        /// <summary>
        /// Retourne des suggestions par défaut basées sur les films les mieux notés.
        /// </summary>
        public List<RecommendationResult> GetDefaultSuggestions(int topN)
        {
            var defaultFilmIds = _context.Films
                // Attention : si ImdbRating est stocké en chaîne, envisagez de le convertir pour un tri numérique.
                .OrderByDescending(f => f.ImdbRating)
                .Select(f => f.Imdb)
                .Distinct()
                .Take(topN * 2)
                .ToList();

            var testData = defaultFilmIds.Select(fid => new UserFilmInteraction
            {
                UserId = 0,
                FilmId = fid,
                Rating = 0,
                Genre = "Unknown"
            }).ToList();

            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "recommendation-model.zip");
            var model = LoadModel(modelPath);
            var predictions = Predict(model, testData)
                                .OrderByDescending(p => p.Score)
                                .Take(topN)
                                .ToList();

            return EnrichPredictions(predictions);
        }

        /// <summary>
        /// Calcule la similarité cosinus entre deux profils utilisateurs.
        /// </summary>
        private double ComputeSimilarity(UserProfile user1, UserProfile user2)
        {
            var commonGenres = user1.GenrePreferences.Keys.Intersect(user2.GenrePreferences.Keys);
            if (!commonGenres.Any())
                return 0;

            double dotProduct = commonGenres.Sum(genre => user1.GenrePreferences[genre] * user2.GenrePreferences[genre]);
            double norm1 = Math.Sqrt(user1.GenrePreferences.Values.Sum(val => val * val));
            double norm2 = Math.Sqrt(user2.GenrePreferences.Values.Sum(val => val * val));
            return (norm1 == 0 || norm2 == 0) ? 0 : dotProduct / (norm1 * norm2);
        }

        /// <summary>
        /// Fournit des recommandations pour un utilisateur en se basant sur la similarité avec d'autres utilisateurs.
        /// Si l'utilisateur n'a aucune interaction, renvoie un fallback par défaut.
        /// </summary>
        public List<RecommendationResult> RecommendBasedOnSimilarUsers(uint userId, int topN)
        {
            var userProfiles = ComputeUserProfiles();
            var targetUser = userProfiles.FirstOrDefault(u => u.UserId == userId);

            if (targetUser == null)
            {
                return GetDefaultSuggestions(topN);
            }

            var similarUsers = userProfiles
                .Where(u => u.UserId != userId)
                .Select(u => new { User = u, Similarity = ComputeSimilarity(targetUser, u) })
                .OrderByDescending(x => x.Similarity)
                .Take(5)
                .ToList();

            var similarUserIds = similarUsers.Select(x => (int)x.User.UserId).ToList();

            var candidateFilmIds = _context.Favourites
                .Include(f => f.Film)
                .Where(f => similarUserIds.Contains(f.UserId) && f.Film != null)
                .Select(f => f.Film.Imdb)
                .Distinct()
                .ToList();

            var targetUserFilmIds = _context.Favourites
                .Include(f => f.Film)
                .Where(f => f.UserId == userId && f.Film != null)
                .Select(f => f.Film.Imdb)
                .Distinct()
                .ToHashSet();

            var candidateFilms = candidateFilmIds.Where(fid => !targetUserFilmIds.Contains(fid)).ToList();

            if (!candidateFilms.Any())
            {
                return GetDefaultSuggestions(topN);
            }

            var testData = candidateFilms.Select(fid => new UserFilmInteraction
            {
                UserId = userId,
                FilmId = fid,
                Rating = 0,
                Genre = "Unknown"
            }).ToList();

            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "recommendation-model.zip");
            var model = LoadModel(modelPath);

            var predictions = Predict(model, testData)
                                .OrderByDescending(p => p.Score)
                                .Take(topN)
                                .ToList();

            return EnrichPredictions(predictions);
        }

        /// <summary>
        /// Utilise le modèle fourni pour prédire les scores pour une liste d'interactions utilisateur-film.
        /// </summary>
        public List<RecommendationPrediction> Predict(ITransformer model, List<UserFilmInteraction> testData)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (testData == null || testData.Count == 0)
                throw new ArgumentException("Les données de test ne peuvent pas être nulles ou vides.");

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<UserFilmInteraction, RecommendationPrediction>(model);

            return testData.Select(td =>
            {
                var prediction = predictionEngine.Predict(td);
                if (float.IsNaN(prediction.Score) || float.IsInfinity(prediction.Score))
                {
                    prediction.Score = 5;
                }
                else
                {
                    prediction.Score = Math.Max(1, Math.Min(5, prediction.Score));
                }
                return prediction;
            }).ToList();
        }

        /// <summary>
        /// Enrichit les prédictions ML en joignant les détails du film depuis la base.
        /// </summary>
        private List<RecommendationResult> EnrichPredictions(List<RecommendationPrediction> predictions)
        {
            var results = new List<RecommendationResult>();

            foreach (var pred in predictions)
            {
                var film = _context.Films.FirstOrDefault(f => f.Imdb == pred.FilmId);
                results.Add(new RecommendationResult
                {
                    FilmId = pred.FilmId,
                    Score = pred.Score,
                    FilmTitle = film?.Name ?? pred.FilmId,
                    Poster = film?.Poster ?? string.Empty,
                    Genre = film?.Genre ?? string.Empty
                });
            }

            return results;
        }

        /// <summary>
        /// Sauvegarde le modèle entraîné dans le chemin spécifié.
        /// </summary>
        public void SaveModel(ITransformer model, string modelPath)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            _mlContext.Model.Save(model, null, modelPath);
        }

        /// <summary>
        /// Charge des données à partir d'un fichier CSV.
        /// </summary>
        public List<UserFilmInteraction> LoadDataFromCsv(string csvPath)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"Fichier CSV introuvable : {csvPath}");

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<UserFilmInteraction>().ToList();

            if (!records.Any())
                throw new InvalidOperationException("Aucune donnée trouvée dans le fichier CSV.");

            return records;
        }
    }
}
