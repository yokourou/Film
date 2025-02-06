using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using app.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace app.Services
{
    public class FilmService
    {
        private readonly HttpClient _httpClient;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly NavigationManager _navigationManager;
        public FilmService(HttpClient httpClient,ProtectedLocalStorage localStorage,NavigationManager navigationManager)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localStorage =localStorage;
            _navigationManager=navigationManager;
        }
        private async Task SetAuthorizedHeaderAsync()
        {
            var token = (await _localStorage.GetAsync<string>("authToken")).Value;
            if (token != null)
            {
                Console.WriteLine($"Using Token: {token}"); // Debugging
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("No token found!"); // Debugging
            }
        }

        // Récupérer tous les films depuis l'API
        public async Task<List<Film>> GetFilmsAsync()
        {
            try
            {
                await SetAuthorizedHeaderAsync();
                return await _httpClient.GetFromJsonAsync<List<Film>>("api/film") ?? new List<Film>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des films : {ex.Message}");
                return new List<Film>();
            }
        }

        // Récupérer les films favoris depuis l'API
        public async Task<List<Film>> GetFavoriteFilmsAsync()
        {
            try
            {
                // Assurez-vous que "api/favorites" correspond au point de terminaison réel de votre API pour les favoris
                return await _httpClient.GetFromJsonAsync<List<Film>>("api/favorites") ?? new List<Film>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des favoris : {ex.Message}");
                return new List<Film>();
            }
        }

        // Récupérer le total des films
        public async Task<int> GetTotalFilmsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<int>("api/film/total");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du total des films : {ex.Message}");
                return 0;
            }
        }

        // Récupérer le nombre de films favoris
        public async Task<int> GetFavoriteFilmsCountAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<int>("api/favorites/count");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des favoris : {ex.Message}");
                return 0;
            }
        }

        // Récupérer les recommandations
        public async Task<List<Film>> GetRecommendationsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Film>>("api/film/recommendations") ?? new List<Film>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des recommandations : {ex.Message}");
                return new List<Film>();
            }
        }
        public async Task<Film> GetFilmByIdAsync(int filmId)
        {
            var response = await _httpClient.GetAsync($"api/Film/{filmId}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur API : {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            return await response.Content.ReadFromJsonAsync<Film>();
        }
     public async Task<List<Film>?> SearchFilmsAsync(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    Console.WriteLine("🔴 Mot-clé vide. Recherche annulée.");
                    return new List<Film>();
                }

                var url = $"api/films/search?keyword={Uri.EscapeDataString(keyword)}";
                Console.WriteLine($"🔵 Envoi de la requête API : {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var films = await response.Content.ReadFromJsonAsync<List<Film>>();
                    Console.WriteLine($"🟢 {films.Count} films reçus depuis l'API");
                    return films;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"🔴 Aucun film trouvé pour '{keyword}'.");
                    return new List<Film>();
                }
                else
                {
                    Console.WriteLine($"❌ Erreur HTTP {response.StatusCode} lors de la recherche de films.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception lors de la recherche de films : {ex.Message}");
                return null;
            }
        }
        
      public async Task<bool> SupprimerFilmAsync(int filmId)
        {
            try
            {
                await SetAuthorizedHeaderAsync();

                // Utilisation d'un objet HttpRequestMessage pour envoyer des données avec DELETE
                var filmData = new
                {
                    filmId = filmId,
                };

                var request = new HttpRequestMessage(HttpMethod.Delete, "api/film/Delete")
                {
                    Content = JsonContent.Create(filmData)
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("film supprimé avec succès.");
                    
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erreur lors de la suppression du film haha : {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de la suppression du film  {ex.Message}");
                return false;
            }
        } 
        public void NavigateTo(string url)
        {
            _navigationManager.NavigateTo(url, forceLoad:true);
        }


    }
}
