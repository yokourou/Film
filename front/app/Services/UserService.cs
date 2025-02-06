using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using app.Models;  // ou le namespace o  se trouvent tes classes User, UserInfo, UserCreation
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using app.Models; 


namespace app.Services
{
    public class UserServiceFront
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly ProtectedLocalStorage _localStorage;

        public UserServiceFront(HttpClient httpClient,NavigationManager navigationManager,ProtectedLocalStorage localStorage )
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorage=localStorage;

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

        public async Task<UserWithToken?> LoginAsync(UserInfo userInfo)
        {
            try
            {
                // Envoi des infos de login via un POST
                var response = await _httpClient.PostAsJsonAsync("api/user/login", userInfo);

                if (response.IsSuccessStatusCode)
                {
                    // R cup re l'utilisateur retourn  par l'API
                    Console.WriteLine("Yes");
                    var userWithToken = await response.Content.ReadFromJsonAsync<UserWithToken>();
                    return userWithToken;
                }
                else
                {
                    Console.WriteLine($"Erreur HTTP ({response.StatusCode}) lors de la connexion.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de la connexion : {ex.Message}");
                return null;
            }
        }

        public async Task<User?> CreateUserAsync(UserCreation userCreation)
        {
            try
            {
                // Envoi de userCreation au endpoint POST /api/users/register
                var response = await _httpClient.PostAsJsonAsync("api/user/register", userCreation);

                // On v rifie si la r ponse HTTP est OK (2xx)
                if (response.IsSuccessStatusCode)
                {
                    // On r cup re l'utilisateur cr   via le contenu JSON de la r ponse
                    var createdUser = await response.Content.ReadFromJsonAsync<User>();
                    return createdUser;
                }
                else
                {
                    Console.WriteLine($"Erreur HTTP ({response.StatusCode}) lors de la cr ation de l'utilisateur.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de la cr ation de l'utilisateur : {ex.Message}");
                return null;
            }
        }


        public async Task<List<User>> GetUsersTotalAsync()
        {
            await SetAuthorizedHeaderAsync();
            var response = await _httpClient.GetAsync("api/user/getUsers");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur API : {response.StatusCode} - {response.ReasonPhrase}");
                return new List<User>();
            }

            var users = await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();

            foreach (var user in users)
            {
                Console.WriteLine($"ID : {user.Id}, Name : {user.Pseudo}");
                
            }
            return users;
        }








        public void NavigateTo(string url, bool forceReload = false)
        {
            _navigationManager.NavigateTo(url, forceLoad: forceReload);
        }

        // Méthode pour rediriger vers Home et forcer un rechargement
        public void NavigateToHomeAndReload()
        {
            _navigationManager.NavigateTo("/", forceLoad: true); // "/" pointe vers Home
        }




        

    }
}