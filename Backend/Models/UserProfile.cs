namespace WebApi.Models
{
    public class UserProfile
    {
        public uint UserId { get; set; }
        public Dictionary<string, float> GenrePreferences { get; set; } = new();
    }
}
