namespace app.Models
{

    public class Favourite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FilmId { get; set; }
        public int? Rating{ get;set;}
        public Favourite() { }
        public Favourite(int Id, int UserId, int FilmId,int rating)
        {
            Id = Id;
            UserId = UserId;
            FilmId = FilmId;
            Rating=rating ;
        }
    }
    public class FavouriteId
    {
        public int filmId {get;set;}
        public int rating {get;set;}
    }

}