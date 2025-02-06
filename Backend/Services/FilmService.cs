using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebApi.Models;

namespace  WebApi.Services
{
    public class FilmService
    {
        private readonly HttpClient _httpClient;

        public FilmService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Get the total number of films
        public async Task<int> GetTotalFilmsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<int>("api/film/total");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP error while fetching total films: {httpEx.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while fetching total films: {ex.Message}");
                return 0;
            }
        }

        // Get the count of favorite films
        public async Task<int> GetFavoriteFilmsCountAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<int>("api/film/favorites/count");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP error while fetching favorite films count: {httpEx.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while fetching favorite films count: {ex.Message}");
                return 0;
            }
        }

        // Get recommended films for the user
        public async Task<List<Film>> GetRecommendationsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Film>>("api/film/recommendations");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP error while fetching recommendations: {httpEx.Message}");
                return new List<Film>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while fetching recommendations: {ex.Message}");
                return new List<Film>();
            }
        }
    }
}
