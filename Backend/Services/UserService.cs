using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebApi.Models;

namespace WebApi.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Récupérer un utilisateur par ID
        public async Task<User> GetUserById(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<User>($"http://localhost:5102/api/users/{id}");
            if (user == null)
            {
                throw new KeyNotFoundException("Utilisateur introuvable.");
            }
            return user;
        }

        // Créer un nouvel utilisateur (Register)
        public async Task<User> Register(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5102/api/users/register", user);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erreur lors de l'enregistrement : {response.StatusCode}");
            }
            return await response.Content.ReadFromJsonAsync<User>();
        }

        // Récupérer tous les utilisateurs
        public async Task<List<User>> GetAllUsers()
        {
            var users = await _httpClient.GetFromJsonAsync<List<User>>("http://localhost:5102/api/users");
            return users ?? new List<User>();
        }

        // Mettre à jour un utilisateur
        public async Task UpdateUser(int id, User user)
        {
            var response = await _httpClient.PutAsJsonAsync($"http://localhost:5102/api/users/{id}", user);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erreur lors de la mise à jour : {response.StatusCode}");
            }
        }

        // Supprimer un utilisateur
        public async Task DeleteUser(int id)
        {
            var response = await _httpClient.DeleteAsync($"http://localhost:5102/api/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erreur lors de la suppression : {response.StatusCode}");
            }
        }

        // Authentification utilisateur
        public async Task<User> Login(UserInfo userInfo)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5102/api/users/login", userInfo);
            if (!response.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException("Connexion échouée.");
            }
            return await response.Content.ReadFromJsonAsync<User>();
        }
    }
}


