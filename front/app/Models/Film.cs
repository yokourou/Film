namespace app.Models
{
    public class Film
    {
       public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Ensures non-null
        public string Poster { get; set; } = string.Empty; // Ensures non-null
        public string Imdb { get; set; } = string.Empty; // Ensures non-null
        public string Year { get; set; } = string.Empty; // Ensures non-null
        public string Genre { get; set; } = string.Empty; // Added Genre field
        public string Plot { get; set; }
        public string ImdbRating { get; set; } 

        public Film() { }

          public Film(int id, string name, string poster, string imdb, string year, string genre, string plot, string imdbrating)
        {
            Id = id;
            Name = name;
            Poster = poster;
            Imdb = imdb;
            Year = year;
            Genre = genre;
            Plot=plot ;
            ImdbRating=imdbrating ;
        }
    }

    public class FilmInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Ensures non-null
        public string Poster { get; set; } = string.Empty; // Ensures non-null
        public string Imdb { get; set; } = string.Empty; // Ensures non-null
        public string Year { get; set; } = string.Empty; // Ensures non-null
        public string Genre { get; set; } = string.Empty; // Added Genre field
    }
}
