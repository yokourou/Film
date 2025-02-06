using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using app.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

namespace app.Services
{
    public class OmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ProtectedLocalStorage _localStorage;

        public OmdbService(HttpClient httpClient, IConfiguration configuration, ProtectedLocalStorage localStorage)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _apiKey = configuration["OmdbApiKey"] ?? throw new ArgumentNullException("OmdbApiKey is not configured.");
        }

        // Helper to set the Authorization header
        private async Task SetAuthorizationHeaderAsync()
        {
            var token = (await _localStorage.GetAsync<string>("authToken")).Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Search for films by title
public async Task<List<OmdbFilm>> SearchByTitleAsync(string title)
{
    var apiKey = _apiKey;  // Assurez-vous que votre clé API est stockée de manière sécurisée
    var baseUrl = "http://www.omdbapi.com/";
    var url = $"{baseUrl}?s={Uri.EscapeDataString(title)}&apikey={apiKey}";
    var client = _httpClient;

    try
    {
        var searchResponse = await client.GetAsync(url);
        if (!searchResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"API Search failed with status code: {searchResponse.StatusCode}");
            return new List<OmdbFilm>();  // Retourne une liste vide ou gère selon le besoin
        }

        var searchContent = await searchResponse.Content.ReadAsStringAsync();
        var searchResult = JsonConvert.DeserializeObject<OmdbSearchResponse>(searchContent);
        if (searchResult == null || searchResult.Search == null)
        {
            Console.WriteLine("No search results found.");
            return new List<OmdbFilm>();  // Retourne une liste vide ou gère selon le besoin
        }

        List<OmdbFilm> filmsWithDetails = new List<OmdbFilm>();
        foreach (var filmSummary in searchResult.Search)
        {
            try
            {
                var detailUrl = $"{baseUrl}?i={filmSummary.ImdbId}&apikey={apiKey}";
                var detailResponse = await client.GetAsync(detailUrl);
                if (!detailResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Detail fetch failed for IMDb ID {filmSummary.ImdbId} with status code: {detailResponse.StatusCode}");
                    continue;  // Passe au film suivant
                }

                var detailContent = await detailResponse.Content.ReadAsStringAsync();
                var filmDetail = JsonConvert.DeserializeObject<OmdbFilmDetail>(detailContent);
                if (filmDetail == null)
                {
                    Console.WriteLine($"Failed to deserialize details for IMDb ID {filmSummary.ImdbId}.");
                    continue;  // Passe au film suivant
                }

                filmsWithDetails.Add(new OmdbFilm
                {
                    Title = filmDetail.Title,
                    Year = filmDetail.Year,
                    ImdbId = filmDetail.ImdbId,
                    Poster = filmDetail.Poster,
                    Genre = filmDetail.Genre ?? "Unknown",
                    Plot=filmDetail.Plot  ,
                    ImdbRating=filmDetail.ImdbRating 

                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching or processing details for IMDb ID {filmSummary.ImdbId}: {ex.Message}");
            }
        }

        return filmsWithDetails;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during the search operation: {ex.Message}");
        return new List<OmdbFilm>();  // Retourne une liste vide ou gère selon le besoin
    }
}

        // Import film into the local API
public async Task<string> ImportFilmAsync(string imdbId)
        {
            if (string.IsNullOrWhiteSpace(imdbId))
            {
                Console.WriteLine("❌ L'ID IMDb est invalide.");
                return "L'ID IMDb est invalide.";
            }

            try
            {
                var url = $"api/Omdb/import/{imdbId}";
                Console.WriteLine($"📥 Importation du film par IMDb ID : {url}");

                // 🔹 Cette requête ajoute directement le film en base via le back-end
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Film importé et ajouté à la base.");
                    return "Film importé et ajouté à la bibliothèque !";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    Console.WriteLine("⚠️ Ce film existe déjà.");
                    return "⚠️ Ce film est déjà dans la bibliothèque.";
                }
                else
                {
                    Console.WriteLine($"⚠️ Erreur HTTP : {response.StatusCode}");
                    return "Erreur lors de l'importation du film.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'importation : {ex.Message}");
                return "Erreur lors de l'importation du film.";
            }
        }

        // Get all imported films
        public async Task<List<Film>> GetImportedFilmsAsync()
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                var films = await _httpClient.GetFromJsonAsync<List<Film>>("api/film");
                return films ?? new List<Film>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching imported films: {ex.Message}");
                return new List<Film>();
            }
        }

        // Load popular film suggestions
        public async Task<List<OmdbFilm>> LoadSuggestionsAsync()
        {
            try
            {
                return await SearchByTitleAsync("Love");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading suggestions: {ex.Message}");
                return new List<OmdbFilm>();
            }
        }
    }

    // Models for OMDb API response
    public class OmdbSearchResponse
    {
        public List<OmdbFilm> Search { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
    }
}
