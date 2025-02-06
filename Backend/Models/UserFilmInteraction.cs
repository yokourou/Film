namespace WebApi.Models
{
    public class UserFilmInteraction
    {
        public uint UserId { get; set; } // Assuming UserId is a numeric type
        public string FilmId { get; set; } // Film identifier (can be a string or numeric)
        public float Rating { get; set; } // User's rating for the film
        public string Genre { get; set; } = string.Empty; // Added Genre field
    }

 public class RecommendationPrediction
{
    public float Score { get; set; }
    public string FilmId { get; set; } = string.Empty;
}
public class RecommendationResult
    {
        public string FilmId { get; set; } = string.Empty;
        public string FilmTitle { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public float Score { get; set; }
    }
}
