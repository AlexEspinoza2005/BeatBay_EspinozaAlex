using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using BeatBay.DTOs;

namespace BeatBayMVC.Controllers
{
    public class ArtistStatisticsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ArtistStatisticsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7037/api";
        }

        private void AddAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private async Task<bool> IsUserLoggedInAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/auth/profile");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<string>> GetUserRolesAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return new List<string>();

            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/auth/user-roles");

                if (!response.IsSuccessStatusCode)
                    return new List<string>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private async Task<bool> UserHasRoleAsync(string role)
        {
            var userRoles = await GetUserRolesAsync();
            return userRoles.Contains(role);
        }

        // GET: ArtistStatistics
        public async Task<IActionResult> Index()
        {
            if (!await IsUserLoggedInAsync())
            {
                TempData["Error"] = "Debes iniciar sesión para ver estadísticas.";
                return RedirectToAction("Login", "Auth");
            }

            if (!await UserHasRoleAsync("Artist") && !await UserHasRoleAsync("Admin"))
            {
                TempData["Error"] = "Debes ser artista para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                AddAuthHeader();

                // Obtener resumen
                var summaryResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/artiststatistics/summary");
                var summaryJson = await summaryResponse.Content.ReadAsStringAsync();
                var summary = JsonConvert.DeserializeObject<dynamic>(summaryJson);

                // Obtener estadísticas de canciones
                var songsResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/artiststatistics/my-songs-stats");
                var songsJson = await songsResponse.Content.ReadAsStringAsync();
                var songs = JsonConvert.DeserializeObject<List<SongStatsDto>>(songsJson) ?? new List<SongStatsDto>();

                // Obtener top canciones
                var topSongsResponse = await _httpClient.GetAsync($"{_apiBaseUrl}/artiststatistics/top-songs?limit=5");
                var topSongsJson = await topSongsResponse.Content.ReadAsStringAsync();
                var topSongs = JsonConvert.DeserializeObject<TopSongsDto>(topSongsJson) ?? new TopSongsDto();

                ViewBag.Summary = summary;
                ViewBag.TopSongs = topSongs.Songs;

                return View(songs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar estadísticas: {ex.Message}";
                return View(new List<SongStatsDto>());
            }
        }

        // GET: ArtistStatistics/SongDetails/5
        public async Task<IActionResult> SongDetails(int id)
        {
            if (!await IsUserLoggedInAsync())
            {
                TempData["Error"] = "Debes iniciar sesión para ver estadísticas.";
                return RedirectToAction("Login", "Auth");
            }

            if (!await UserHasRoleAsync("Artist") && !await UserHasRoleAsync("Admin"))
            {
                TempData["Error"] = "Debes ser artista para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/artiststatistics/song-details/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudieron obtener los detalles de la canción.";
                    return RedirectToAction(nameof(Index));
                }

                var json = await response.Content.ReadAsStringAsync();
                var playbackStats = JsonConvert.DeserializeObject<List<PlaybackStatisticDto>>(json) ?? new List<PlaybackStatisticDto>();

                ViewBag.SongId = id;
                ViewBag.SongTitle = playbackStats.FirstOrDefault()?.SongTitle ?? "Canción desconocida";

                return View(playbackStats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar detalles: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ArtistStatistics/TopSongs
        public async Task<IActionResult> TopSongs()
        {
            if (!await IsUserLoggedInAsync())
            {
                TempData["Error"] = "Debes iniciar sesión para ver estadísticas.";
                return RedirectToAction("Login", "Auth");
            }

            if (!await UserHasRoleAsync("Artist") && !await UserHasRoleAsync("Admin"))
            {
                TempData["Error"] = "Debes ser artista para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/artiststatistics/top-songs?limit=20");

                if (!response.IsSuccessStatusCode)
                    return View(new List<SongStatsDto>());

                var json = await response.Content.ReadAsStringAsync();
                var topSongs = JsonConvert.DeserializeObject<TopSongsDto>(json) ?? new TopSongsDto();

                return View(topSongs.Songs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar top canciones: {ex.Message}";
                return View(new List<SongStatsDto>());
            }
        }
    }
}