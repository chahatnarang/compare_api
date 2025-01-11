using Microsoft.AspNetCore.Mvc;
using MoviePriceComparer.Services;

namespace MoviePriceComparer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieService _movieService;

        public MoviesController(MovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMovies()
        {
            var cheapestMovies = await _movieService.GetAllMoviesAsync();
            return Ok(cheapestMovies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieDetails(string id)
        {
            var provider = _movieService.GetProvider(id);
            var movieDetails = await _movieService.GetMovieDetailsAsync(provider, id);

            if (movieDetails == null)
            {
                return NotFound();
            }

            return Ok(movieDetails);
        }
    }
}
