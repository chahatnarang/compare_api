using MoviePriceComparison.Helpers;
using MoviePriceComparison.Models;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace MoviePriceComparer.Services
{
    public class MovieService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10); // Set a timeout for HTTP requests

        public MovieService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            _semaphore = new SemaphoreSlim(10); // Limit to 10 concurrent requests
        }

        public async Task<List<MovieDetails>> GetAllMoviesAsync()
        {
            // Fetch movie lists in parallel with fallback mechanism
            var cinemaworldMoviesTask = GetMoviesAsync(Constants.CINEMAWORLD);
            var filmworldMoviesTask = GetMoviesAsync(Constants.FILMWORLD);

            var cinemaworldMovies = await cinemaworldMoviesTask ?? new List<Movie>();
            var filmworldMovies = await filmworldMoviesTask ?? new List<Movie>();

            // Return early if both lists are empty
            if (!cinemaworldMovies.Any() && !filmworldMovies.Any())
            {
                Console.WriteLine("No movies found from either provider.");
                return new List<MovieDetails>();
            }

            // Combine movie lists and fetch details in parallel
            var allMovies = cinemaworldMovies.Concat(filmworldMovies)
                .OrderBy(m => m.Title)
                .ToList();

            // Fetch movie details in parallel with limited concurrency
            var movieDetailsTasks = allMovies.Select(movie => FetchMovieDetailsWithLimit(movie)).ToList();
            var movieDetails = await Task.WhenAll(movieDetailsTasks);

            // Group by Title and select the movie with the lowest price
            var filteredList = movieDetails
                .Where(md => md != null)
                .GroupBy(md => md.Title)
                .Select(group =>
                {
                    var minPrice = group.Min(md => md.Price);
                    return group.First(md => md.Price == minPrice);
                })
                .OrderBy(md => md.Year)
                .ToList();

            return filteredList;
        }

        private async Task<List<Movie>> GetMoviesAsync(string provider)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var cts = new CancellationTokenSource(_timeout); // Set a timeout
                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://webjetapitest.azurewebsites.net/api/{provider}/movies");
                    request.Headers.Add("x-access-token", Constants.API_TOKEN);

                    var response = await _httpClient.SendAsync(request, cts.Token);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var movies = JsonConvert.DeserializeObject<MovieResponse>(content).Movies;
                    return movies;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Console.WriteLine($"Service unavailable for {provider}. Returning empty list.");
                    return new List<Movie>(); // Return an empty list if the service is unavailable
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"Request timed out for {provider}. Returning empty list.");
                    return new List<Movie>(); // Return an empty list if the request times out
                }
            });
        }

        public async Task<MovieDetails?> GetMovieDetailsAsync(string provider, string movieId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var cts = new CancellationTokenSource(_timeout); // Set a timeout
                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://webjetapitest.azurewebsites.net/api/{provider}/movie/{movieId}");
                    request.Headers.Add("x-access-token", Constants.API_TOKEN);

                    var response = await _httpClient.SendAsync(request, cts.Token);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var movieDetails = JsonConvert.DeserializeObject<MovieDetails>(content);

                    // Ensure the Price property is parsed as a double
                    movieDetails.Price = double.Parse(movieDetails.Price.ToString());

                    // Set the provider information
                    movieDetails.Provider = provider;
                    return movieDetails;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Console.WriteLine($"Service unavailable for {provider}. Returning null.");
                    return null; // Return null if the service is unavailable
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Movie not found for {provider} with ID {movieId}. Returning null.");
                    return null; // Return null if the movie is not found
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"Request timed out for {provider}. Returning null.");
                    return null; // Return null if the request times out
                }
            });
        }

        private async Task<MovieDetails?> FetchMovieDetailsWithLimit(Movie movie)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await GetMovieDetailsAsync(GetProvider(movie.ID), movie.ID);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public string GetProvider(string id)
        {
            return id.StartsWith("cw") ? Constants.CINEMAWORLD : Constants.FILMWORLD;
        }
    }
}
