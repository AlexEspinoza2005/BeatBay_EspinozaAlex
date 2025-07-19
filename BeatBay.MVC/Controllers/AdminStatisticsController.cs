using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BeatBay.MVC.Controllers
{
    public class AdminStatisticsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminStatisticsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> General()
        {
            try
            {
                using var client = CreateHttpClient();
                var response = await client.GetAsync("https://localhost:7037/api/AdminStatistics/general");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // Log para debug
                    System.Console.WriteLine($"API Response: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var stats = JsonSerializer.Deserialize<object>(json, options);
                    return Json(stats);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, $"Error obteniendo estadísticas: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception in General: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Songs()
        {
            try
            {
                using var client = CreateHttpClient();
                var response = await client.GetAsync("https://localhost:7037/api/AdminStatistics/songs");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Songs API Response: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var stats = JsonSerializer.Deserialize<object>(json, options);
                    return Json(stats);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Songs API Error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, $"Error obteniendo estadísticas de canciones: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception in Songs: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            try
            {
                using var client = CreateHttpClient();
                var response = await client.GetAsync("https://localhost:7037/api/AdminStatistics/users");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Users API Response: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var stats = JsonSerializer.Deserialize<object>(json, options);
                    return Json(stats);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Users API Error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, $"Error obteniendo estadísticas de usuarios: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception in Users: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Playback()
        {
            try
            {
                using var client = CreateHttpClient();
                var response = await client.GetAsync("https://localhost:7037/api/AdminStatistics/playback");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Playback API Response: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var stats = JsonSerializer.Deserialize<object>(json, options);
                    return Json(stats);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Playback API Error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, $"Error obteniendo estadísticas de reproducción: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception in Playback: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Payments()
        {
            try
            {
                using var client = CreateHttpClient();
                var response = await client.GetAsync("https://localhost:7037/api/AdminStatistics/payments");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Payments API Response: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var stats = JsonSerializer.Deserialize<object>(json, options);
                    return Json(stats);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Payments API Error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, $"Error obteniendo estadísticas de pagos: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception in Payments: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                using var client = CreateHttpClient();
                var response = await client.GetAsync("https://localhost:7037/api/AdminStatistics/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Dashboard API Response: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var stats = JsonSerializer.Deserialize<object>(json, options);
                    return Json(stats);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"Dashboard API Error: {response.StatusCode} - {errorContent}");
                    return StatusCode((int)response.StatusCode, $"Error obteniendo estadísticas del dashboard: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception in Dashboard: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        // Vistas
        public IActionResult DashboardView()
        {
            return View("Dashboard");
        }

        public IActionResult SongsView()
        {
            return View("Songs");
        }

        public IActionResult UsersView()
        {
            return View("Users");
        }

        public IActionResult PlaybackView()
        {
            return View("Playback");
        }

        public IActionResult PaymentsView()
        {
            return View("Payments");
        }

        [HttpGet]
        public async Task<IActionResult> AllStatistics()
        {
            try
            {
                using var client = CreateHttpClient();

                var generalTask = client.GetAsync("https://localhost:7037/api/AdminStatistics/general");
                var songsTask = client.GetAsync("https://localhost:7037/api/AdminStatistics/songs");
                var usersTask = client.GetAsync("https://localhost:7037/api/AdminStatistics/users");
                var playbackTask = client.GetAsync("https://localhost:7037/api/AdminStatistics/playback");
                var paymentsTask = client.GetAsync("https://localhost:7037/api/AdminStatistics/payments");

                var responses = await Task.WhenAll(generalTask, songsTask, usersTask, playbackTask, paymentsTask);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var allStats = new
                {
                    General = responses[0].IsSuccessStatusCode ?
                        JsonSerializer.Deserialize<object>(await responses[0].Content.ReadAsStringAsync(), options) : null,
                    Songs = responses[1].IsSuccessStatusCode ?
                        JsonSerializer.Deserialize<object>(await responses[1].Content.ReadAsStringAsync(), options) : null,
                    Users = responses[2].IsSuccessStatusCode ?
                        JsonSerializer.Deserialize<object>(await responses[2].Content.ReadAsStringAsync(), options) : null,
                    Playback = responses[3].IsSuccessStatusCode ?
                        JsonSerializer.Deserialize<object>(await responses[3].Content.ReadAsStringAsync(), options) : null,
                    Payments = responses[4].IsSuccessStatusCode ?
                        JsonSerializer.Deserialize<object>(await responses[4].Content.ReadAsStringAsync(), options) : null
                };

                return Json(allStats);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception in AllStatistics: {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}