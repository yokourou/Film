using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using app.Models;
using System;
using Microsoft.AspNetCore.Components;



namespace app.Services
{
    public class FavouriteService
    {
        private readonly HttpClient _httpClient;
        private readonly ProtectedLocalStorage _localStorage;
       private readonly NavigationManager _navigationManager;
        public FavouriteService(HttpClient httpClient, ProtectedLocalStorage localStorage,NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _navigationManager = navigationManager;
           
        }

        /// <summary>
        /// Sets the authorization header using the stored JWT token.
        /// </summary>
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


        /// <summary>
        /// Fetches the user's list of favorite films from the server.
        /// </summary>
        public async Task<List<Favourite>> GetFavouritesAsync()
        {
            await SetAuthorizedHeaderAsync();
            var response = await _httpClient.GetAsync("api/favourite");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur API : {response.StatusCode} - {response.ReasonPhrase}");
                return new List<Favourite>();
            }

            var favourites = await response.Content.ReadFromJsonAsync<List<Favourite>>() ?? new List<Favourite>();

            foreach (var favourite in favourites)
            {
                Console.WriteLine($"ID : {favourite.Id}, Name : {favourite.UserId}");
                
            }

            return favourites;
        }

        /// <summary>
        /// Adds a film to the user's favorites with an optional rating.
        /// </summary>
        public async Task<bool> AddFavouriteAsync(int filmId, int rating)
        {
            try
            {
                await SetAuthorizedHeaderAsync();

                var favouriteData = new 
                {
                    FilmId = filmId,
                    Rating = rating
                };

                var response = await _httpClient.PostAsJsonAsync($"api/Favourite/add", favouriteData);
                Console.WriteLine(favouriteData.Rating);

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
        public async Task<bool> SupprimerFavouriteAsync(int filmId)
        {
            try
            {
                await SetAuthorizedHeaderAsync();

                // Utilisation d'un objet HttpRequestMessage pour envoyer des données avec DELETE
                var favouriteData = new
                {
                    filmId = filmId,
                };

                var request = new HttpRequestMessage(HttpMethod.Delete, "api/Favourite/delete")
                {
                    Content = JsonContent.Create(favouriteData)
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("favoris supprimé avec succès.");
                    
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erreur lors de la suppression du favori hahaha : {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de la suppression du favori: {ex.Message}");
                return false;
            }
        }
        public void NavigateToFavoriteAndReload()
        {
            //_navigationManager.NavigateTo("/", forceLoad: true); // "/" pointe vers Home
            _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad:true);

        }
        public void NavigateTo(string url)
        {
            _navigationManager.NavigateTo(url, forceLoad: true);
        }

    }
}
