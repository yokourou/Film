namespace WebApi.Models
{

    public class Favourite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FilmId { get; set; }
        public int? Rating{ get ; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Favourite() { }
        public Favourite(int id, int userId, int filmId, int rating)
{
    this.Id = id;
    this.UserId = userId;
    this.FilmId = filmId;
    this.Rating = rating;
}

        public User User { get; set; } = null!; // Relation avec User

        public Film Film { get; set; }

    }
    public class FavouriteId
    {
        public int filmId {get;set;}
        public int Rating{get;set;}
    }
    public class FavouriteDTO
    {
        public int FavouriteId { get; set; } // ID du favori
        public int FilmId { get; set; } // ID du film
        public string Name { get; set; } = string.Empty;
        public string Year { get; set; }
        public int? Rating { get; set; } // Note attribuée par l'utilisateur
    }
}