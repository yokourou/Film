using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using WebApi.Models;

namespace app.Services
{
    public class FavouriteService
    {
        private readonly HttpClient _httpClient;
        private readonly ProtectedLocalStorage _localStorage;

        public FavouriteService(HttpClient httpClient, ProtectedLocalStorage localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task SetAuthorizedHeader()
        {
            var token = (await _localStorage.GetAsync<string>("authToken")).Value;
            if (token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Favourite>> GetFavouritesAsync()
        {
            await SetAuthorizedHeader();
            var response = await _httpClient.GetAsync("api/Favourite");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur API : {response.StatusCode} - {response.ReasonPhrase}");
                return new List<Favourite>();
            }

            return await response.Content.ReadFromJsonAsync<List<Favourite>>() ?? new List<Favourite>();
        }

        public async Task<bool> AddFavouriteAsync(int filmId, float? rating = null)
        {
            Console.WriteLine($"Film Id est : {filmId}, Rating : {rating}");

            try
            {
                await SetAuthorizedHeader();

                var favouriteid = new FavouriteId() { filmId = filmId };

                // Add optional rating to query string
                var query = rating.HasValue ? $"?rating={rating.Value}" : string.Empty;

                var response = await _httpClient.PostAsJsonAsync($"api/Favourite/add{query}", favouriteid);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Film ajouté aux favoris avec succès.");
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erreur lors de l'ajout du film aux favoris : {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de l'ajout du favori : {ex.Message}");
                return false;
            }
        }
    }
}
