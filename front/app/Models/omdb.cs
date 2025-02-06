using Newtonsoft.Json;
namespace app.Models
{
	public class OmdbFilm
	{
		public string Title { get; set; }
		public string Year { get; set; }
		public string ImdbId { get; set; }
		public string Poster { get; set; }
		public string Plot { get; set; }
        public string ImdbRating { get; set; } 
		[JsonProperty("genre")] // Assurez-vous que cela correspond à la clé JSON
		public string Genre { get; set; }
	}
	public class OmdbFilmRating
	{
		public string Source { get; set; }
		public string Value { get; set; }
	}
	public class OmdbFilmDetail
	{
		public string Title { get; set; }
		public int Id {  get; set; }
		public string Year { get; set; }
		public string Rated { get; set; }
		public string Released { get; set; }
		public string Runtime { get; set; }
		public string Genre { get; set; }
		public string Director { get; set; }
		public string Writer { get; set; }
		public string Actors { get; set; }
		public string Plot { get; set; }
		public string Language { get; set; }
		public string Country { get; set; }
		public string Awards { get; set; }
		public string Poster { get; set; }
		public List<OmdbFilmRating> Ratings { get; set; }
		public string Metascore { get; set; }
		public string ImdbRating { get; set; }
		public string ImdbVotes { get; set; }
		public string ImdbId { get; set; }
		public string Type { get; set; }
		public string DVD { get; set; }
		public string BoxOffice { get; set; }
		public string Production { get; set; }
		public string Website { get; set; }
		public string Response { get; set; }
	}
	public class OmdbSearchResponse
	{
		public List<OmdbFilmDetail>  Search { get; set; }
	}
}
