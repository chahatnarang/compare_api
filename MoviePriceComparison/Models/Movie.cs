using Newtonsoft.Json;

namespace MoviePriceComparison.Models
{
    public class Movie
    {

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Year")]
        public string Year { get; set; }

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Poster")]
        public string Poster { get; set; }
    }

    public class MovieResponse
    {

        [JsonProperty("Movies")]
        public List<Movie> Movies { get; set; }
    }

    public class MovieDetails
    {

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Year")]
        public string Year { get; set; }

        [JsonProperty("Rated")]
        public string Rated { get; set; }

        [JsonProperty("Released")]
        public string Released { get; set; }

        [JsonProperty("Runtime")]
        public string Runtime { get; set; }

        [JsonProperty("Genre")]
        public string Genre { get; set; }

        [JsonProperty("Director")]
        public string Director { get; set; }

        [JsonProperty("Writer")]
        public string Writer { get; set; }

        [JsonProperty("Actors")]
        public string Actors { get; set; }

        [JsonProperty("Plot")]
        public string Plot { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }

        [JsonProperty("Awards")]
        public string Awards { get; set; }

        [JsonProperty("Poster")]
        public string Poster { get; set; }

        [JsonProperty("Metascore")]
        public string Metascore { get; set; }

        [JsonProperty("Rating")]
        public string Rating { get; set; }

        [JsonProperty("Votes")]
        public string Votes { get; set; }

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Price")]
        public double Price { get; set; }

        [JsonProperty("Provider")]
        public string Provider { get; set; }
    }
}
