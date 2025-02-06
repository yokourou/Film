namespace app.Models
{
    public class UserFilmInteraction
    {
        public uint UserId { get; set; }
        public string FilmId { get; set; }
        public float Rating { get; set; }
        public string Genre { get; set; }
    }

    public class UserProfile
    {
        public uint UserId { get; set; }
        public Dictionary<string, float> GenrePreferences { get; set; } = new();
    }

  public class RecommendationPrediction
    {
        public string FilmId { get; set; } = string.Empty; // Ensure FilmId is present
        public float Score { get; set; }
    }
    namespace app.Models
{
    public class RecommendationResult
    {
        public string FilmId { get; set; } = string.Empty;
        public string FilmTitle { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public float Score { get; set; }
         public string Year { get; set; } = string.Empty;
    }
}

}


