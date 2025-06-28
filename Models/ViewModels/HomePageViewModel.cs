namespace Film_website.Models
{
    public class HomePageViewModel
    {
        public List<Movie> NewMovies { get; set; } = new List<Movie>();
        public List<Movie> HighScoreMovies { get; set; } = new List<Movie>();
        public Movie? FeaturedMovie { get; set; }
        public int TotalMoviesCount { get; set; }

        // Additional properties for future features
        public List<Movie> TrendingMovies { get; set; } = new List<Movie>();
        public List<string> PopularGenres { get; set; } = new List<string>();
    }
}