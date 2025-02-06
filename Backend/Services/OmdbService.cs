using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebApi.Models;
using Microsoft.Extensions.Logging;

namespace WebApi.Services
{
    public class OmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OmdbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["OmdbApiKey"] ?? throw new ArgumentNullException("OmdbApiKey is not configured.");
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


        // Search for film details by IMDb ID
        public async Task<OmdbFilmDetail> GetFilmDetailByImdbIdAsync(string imdbId)
        {
            var url = $"http://www.omdbapi.com/?i={Uri.EscapeDataString(imdbId)}&apikey={_apiKey}"; // Utilisez votre clé API valide
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    LogError(response);
                    return null;
                }

                var filmDetail = await response.Content.ReadFromJsonAsync<OmdbFilmDetail>();
                return filmDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching film details: {ex.Message}");
                throw;
            }
        }

         // Recherche d'un film par ID IMDB
        public async Task<Film> împortByImdbIdAsync(string imdbId)
		{
			try
			{
				var url = $"http://www.omdbapi.com/?i={imdbId}&apikey={_apiKey}";
				var response = await _httpClient.GetAsync(url);

				// Vrifier si la requte a russi (code HTTP 200-299)
				response.EnsureSuccessStatusCode();

				// Lire le contenu de la rponse en tant que chane (string)
				var responseContent = await response.Content.ReadAsStringAsync();

				// Dsrialiser la rponse JSON en objet
				var filmDetail = JsonConvert.DeserializeObject<OmdbFilmDetail>(responseContent);
				var film = new Film()
				{
					Name = filmDetail.Title,
					Year = filmDetail.Year,
					Id = filmDetail.Id,
					Poster = filmDetail.Poster,
					Imdb = filmDetail.ImdbId,
                    Genre=filmDetail.Genre,
                    Plot=filmDetail.Plot,
                    ImdbRating=filmDetail.ImdbRating 
				};
				return film;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erreur lors de la rcupration du film : {ex.Message}");
				throw new Exception("Erreur");
			}
		}
        // Import film into the local database (with duplicate check)
        public async Task<bool> ImportFilmAsync(Film film)
        {
            if (film == null || string.IsNullOrWhiteSpace(film.Imdb))
                throw new ArgumentException("The film or IMDb ID is invalid.", nameof(film));

            try
            {
                // Check if the film already exists
                var existingFilms = await GetImportedFilmsAsync();
                if (existingFilms.Exists(f => f.Imdb == film.Imdb))
                {
                    Console.WriteLine($"Film with IMDb ID {film.Imdb} already exists.");
                    return false;
                }

                // Import the film
                var response = await _httpClient.PostAsJsonAsync("api/film", film);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Successfully imported film: {film.Name}");
                    return true;
                }

                LogError(response);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing film: {ex.Message}");
                return false;
            }
        }

        // Load popular film suggestions
        public async Task<List<OmdbFilm>> LoadSuggestionsAsync()
        {
            try
            {
                return await SearchByTitleAsync("popular");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading suggestions: {ex.Message}");
                return new List<OmdbFilm>();
            }
        }

        // Helper to retrieve all imported films
        private async Task<List<Film>> GetImportedFilmsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Film>>("api/film") ?? new List<Film>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching imported films: {ex.Message}");
                return new List<Film>();
            }
        }

        // Log HTTP response errors
        private void LogError(HttpResponseMessage response)
        {
            Console.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }

    // Models for OMDb API response
    public class OmdbSearchResponse
    {
        public List<OmdbFilm> Search { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
    }

    // public class OmdbFilm
    // {
    //     public string Title { get; set; }
    //     public string Year { get; set; }
    //     public string ImdbId { get; set; }
    //     public string Poster { get; set; }
    //     public string Genre { get; set; }
    // }

    // public class FilmDetails
    // {
    //     public string Title { get; set; }
    //     public string Year { get; set; }
    //     public string ImdbId { get; set; }
    //     public string Poster { get; set; }
    //     public string Genre { get; set; }
    //     public string Plot { get; set; }
    //     public string Director { get; set; }
    //     public string Actors { get; set; }
    //     public string Runtime { get; set; }
    // }
}
