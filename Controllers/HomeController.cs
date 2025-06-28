using System.Diagnostics;
using Film_website.Models;
using Film_website.Services;
using Microsoft.AspNetCore.Mvc;

namespace Film_website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MovieService _movieService;

        public HomeController(ILogger<HomeController> logger, MovieService movieService)
        {
            _logger = logger;
            _movieService = movieService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get all movies from database
                var allMovies = (await _movieService.GetAllMoviesAsync()).ToList();

                // Create a view model with categorized movies
                var viewModel = new HomePageViewModel
                {
                    // Get the latest 6 movies for "New this week"
                    NewMovies = allMovies
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(6)
                        .ToList(),

                    // Get 6 random movies for "High Score" (you can modify this logic)
                    HighScoreMovies = allMovies
                        .OrderBy(m => Guid.NewGuid()) // Random order
                        .Take(6)
                        .ToList(),

                    // Get a featured movie for the hero section (latest movie)
                    FeaturedMovie = allMovies
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),

                    // Total count for statistics
                    TotalMoviesCount = allMovies.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading homepage data");

                // Return empty view model in case of error
                var emptyViewModel = new HomePageViewModel
                {
                    NewMovies = new List<Movie>(),
                    HighScoreMovies = new List<Movie>(),
                    FeaturedMovie = null,
                    TotalMoviesCount = 0
                };

                ViewBag.ErrorMessage = "Unable to load movies at this time.";
                return View(emptyViewModel);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}