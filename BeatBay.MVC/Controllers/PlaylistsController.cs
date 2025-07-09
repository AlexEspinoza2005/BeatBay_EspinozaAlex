using BeatBay.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BeatBay.MVC.Controllers
{
    // QUITAR [Authorize] - SIN VALIDACION DE AUTENTICACION
    public class PlaylistsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly ILogger<PlaylistsController> _logger;

        public PlaylistsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PlaylistsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7037/api";
            _logger = logger;
        }

        // SIMPLIFICAR - NO USAR TOKEN
        private void AddAuthHeader()
        {
            // Comentar validacion de token
            // var token = HttpContext.Session.GetString("JwtToken");
            // if (!string.IsNullOrEmpty(token))
            // {
            //     _httpClient.DefaultRequestHeaders.Authorization =
            //         new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // }
        }

        private async Task<T?> ApiGetAsync<T>(string endpoint)
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{endpoint}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }

                // Simplificar manejo de errores
                _logger.LogError("Error en API: {StatusCode}", response.StatusCode);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando al endpoint de API: {Endpoint}", endpoint);
                return default(T);
            }
        }

        private async Task<bool> ApiPostAsync<T>(string endpoint, T data)
        {
            try
            {
                AddAuthHeader();
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en POST al endpoint de API: {Endpoint}", endpoint);
                return false;
            }
        }

        private async Task<bool> ApiPutAsync<T>(string endpoint, T data)
        {
            try
            {
                AddAuthHeader();
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{endpoint}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en PUT al endpoint de API: {Endpoint}", endpoint);
                return false;
            }
        }

        private async Task<bool> ApiDeleteAsync(string endpoint)
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{endpoint}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DELETE al endpoint de API: {Endpoint}", endpoint);
                return false;
            }
        }

        // GET: Playlists - SIN VALIDACION DE LOGIN
        public async Task<IActionResult> Index()
        {
            try
            {
                var playlists = await ApiGetAsync<List<PlaylistDto>>("playlists") ?? new List<PlaylistDto>();
                return View(playlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando playlists");
                ViewBag.Error = "Error cargando playlists. Por favor intente nuevamente.";
                return View(new List<PlaylistDto>());
            }
        }

        // GET: Playlists/MyPlaylists - SIN VALIDACION DE LOGIN
        public async Task<IActionResult> MyPlaylists()
        {
            try
            {
                var playlists = await ApiGetAsync<List<PlaylistDto>>("playlists/my-playlists") ?? new List<PlaylistDto>();
                return View(playlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando mis playlists");
                ViewBag.Error = "Error cargando sus playlists. Por favor intente nuevamente.";
                return View(new List<PlaylistDto>());
            }
        }

        // GET: Playlists/Details/5 - SIN VALIDACION DE LOGIN
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var playlist = await ApiGetAsync<PlaylistDto>($"playlists/{id}");
                if (playlist == null)
                    return NotFound();

                // Cargar canciones disponibles para agregar
                var allSongs = await ApiGetAsync<List<SongDto>>("songs?isActive=true") ?? new List<SongDto>();
                var playlistSongIds = playlist.Songs.Select(s => s.Id).ToHashSet();
                ViewBag.AvailableSongs = allSongs.Where(s => !playlistSongIds.Contains(s.Id)).ToList();

                return View(playlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando detalles de playlist");
                ViewBag.Error = "Error cargando detalles de la playlist.";
                return View();
            }
        }

        // GET: Playlists/Create - SIN VALIDACION DE LOGIN
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Playlists/Create - SIN VALIDACION DE LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlaylistDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var success = await ApiPostAsync("playlists", dto);
                if (success)
                {
                    TempData["Success"] = "Playlist creada exitosamente!";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Error = "Error creando playlist.";
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando playlist");
                ViewBag.Error = "Error creando playlist.";
                return View(dto);
            }
        }

        // GET: Playlists/Edit/5 - SIN VALIDACION DE LOGIN
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var playlist = await ApiGetAsync<PlaylistDto>($"playlists/{id}");
                if (playlist == null)
                    return NotFound();

                var updateDto = new CreatePlaylistDto { Name = playlist.Name };
                ViewBag.PlaylistId = id;
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando playlist para edición");
                ViewBag.Error = "Error cargando playlist para edición.";
                return View();
            }
        }

        // POST: Playlists/Edit/5 - SIN VALIDACION DE LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreatePlaylistDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PlaylistId = id;
                return View(dto);
            }

            try
            {
                var success = await ApiPutAsync($"playlists/{id}", dto);
                if (success)
                {
                    TempData["Success"] = "Playlist actualizada exitosamente!";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Error = "Error actualizando playlist.";
                ViewBag.PlaylistId = id;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando playlist");
                ViewBag.Error = "Error actualizando playlist.";
                ViewBag.PlaylistId = id;
                return View(dto);
            }
        }

        // POST: Playlists/Delete/5 - SIN VALIDACION DE LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await ApiDeleteAsync($"playlists/{id}");
                if (success)
                    TempData["Success"] = "Playlist eliminada exitosamente!";
                else
                    TempData["Error"] = "Error eliminando playlist.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando playlist");
                TempData["Error"] = "Error eliminando playlist.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Playlists/AddSong/5 - SIN VALIDACION DE LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int id, AddSongToPlaylistDto dto)
        {
            try
            {
                var success = await ApiPostAsync($"playlists/{id}/songs", dto);
                if (success)
                    TempData["Success"] = "Canción agregada a la playlist exitosamente!";
                else
                    TempData["Error"] = "Error agregando canción a la playlist.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando canción a playlist");
                TempData["Error"] = "Error agregando canción a la playlist.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Playlists/RemoveSong/5 - SIN VALIDACION DE LOGIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int id, int songId)
        {
            try
            {
                var success = await ApiDeleteAsync($"playlists/{id}/songs/{songId}");
                if (success)
                    TempData["Success"] = "Canción removida de la playlist exitosamente!";
                else
                    TempData["Error"] = "Error removiendo canción de la playlist.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removiendo canción de playlist");
                TempData["Error"] = "Error removiendo canción de la playlist.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}