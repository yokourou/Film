using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using app.Models;
using app.Models.app.Models; // Assurez-vous que RecommendationResult est défini dans ce namespace

namespace app.Services
{
    public class RecommendationServicefront
    {
        private readonly HttpClient _httpClient;

        // Injection d'HttpClient (Blazor le fournit automatiquement)
        public RecommendationServicefront(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Appelle l'API back-end pour obtenir les films recommandés pour un utilisateur.
        /// Renvoie une liste d'objets RecommendationResult.
        /// </summary>
        public async Task<List<RecommendationResult>> GetRecommendationsAsync(uint userId, int topN = 5)
        {
            try
            {
                // Appel à l'API pour obtenir des recommandations enrichies
                var response = await _httpClient.GetFromJsonAsync<List<RecommendationResult>>(
                    $"api/recommendations/user/{userId}?topN={topN}"
                );

                return response ?? new List<RecommendationResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recommendations: {ex.Message}");
                // Retourne une liste vide en cas d'erreur pour éviter une exception côté front
                return new List<RecommendationResult>();
            }
        }
    }
}
