using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using WebApi.Models;
using System.Text;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;

        public RecommendationController(RecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetRecommendations(uint userId, [FromQuery] int topN = 5)
        {
            try
            {
                var recommendations = _recommendationService.RecommendBasedOnSimilarUsers(userId, topN);
                return Ok(recommendations);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Une erreur est survenue.", Details = ex.Message });
            }
        }

        [HttpPost("train")]
        public IActionResult TrainModel()
        {
            try
            {
                var model = _recommendationService.TrainModelFromDatabase();
                return Ok(new { Message = "Modèle entraîné avec succès." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erreur pendant l'entraînement du modèle.", Details = ex.Message });
            }
        }

        [HttpGet("download-sample-csv")]
        public IActionResult DownloadSampleCsv()
        {
            var csvContent = "UserId,FilmId,Rating,Genre\n" +
                             "1,tt0111161,9.3,Drama\n" +
                             "1,tt0068646,8.5,\"Crime,Drama\"\n" +
                             "2,tt0468569,9.0,\"Action,Crime\"\n" +
                             "3,tt0111161,7.8,Drama\n" +
                             "3,tt0086190,8.0,\"Sci-Fi,Adventure\"";

            var fileBytes = Encoding.UTF8.GetBytes(csvContent);
            return File(fileBytes, "text/csv", "sample_user_film_interactions.csv");
        }

        [HttpPost("load-from-csv")]
        public IActionResult LoadDataFromCsv([FromBody] string csvPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvPath))
                    throw new ArgumentException("Le chemin du fichier CSV ne peut pas être vide.");

                var data = _recommendationService.LoadDataFromCsv(csvPath);
                return Ok(new { Message = "Données chargées avec succès.", DataCount = data.Count });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Erreur lors du chargement des données.", Details = ex.Message });
            }
        }
    }
}
