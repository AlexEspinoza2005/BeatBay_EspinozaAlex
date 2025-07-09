using BeatBay.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BeatBay.MVC.Controllers
{
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

        // Mejorado: Validación de token más robusta
        private void AddAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Mejorado: Verificar si el usuario está autenticado
        private bool IsAuthenticated()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            return !string.IsNullOrEmpty(token);
        }

        // Método para verificar si el usuario está logueado antes de acciones que requieren auth
        private IActionResult CheckAuthenticationAndRedirect()
        {
            if (!IsAuthenticated())
            {
                TempData["Error"] = "Debes iniciar sesión para acceder a esta función.";
                return RedirectToAction("Login", "Account");
            }
            return null;
        }

        // Método helper para extraer mensajes de error del JSON
        private async Task<string> ExtractErrorMessage(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                var errorObj = JsonConvert.DeserializeObject<dynamic>(content);

                // Intentar obtener el mensaje de diferentes estructuras posibles
                if (errorObj?.message != null)
                    return errorObj.message;

                if (errorObj?.Message != null)
                    return errorObj.Message;

                return content;
            }
            catch
            {
                return "Error procesando la solicitud.";
            }
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

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Limpiar sesión y redirigir
                    HttpContext.Session.Remove("JwtToken");
                    return default(T);
                }

                _logger.LogError("Error en API: {StatusCode} - {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando al endpoint de API: {Endpoint}", endpoint);
                return default(T);
            }
        }

        private async Task<ApiResponse<T>> ApiPostAsync<T>(string endpoint, T data)
        {
            try
            {
                AddAuthHeader();
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/{endpoint}", content);

                string errorMessage = null;
                if (!response.IsSuccessStatusCode)
                {
                    errorMessage = await ExtractErrorMessage(response);
                }

                return new ApiResponse<T>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    Content = response.IsSuccessStatusCode ? data : default(T),
                    ErrorMessage = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en POST al endpoint de API: {Endpoint}", endpoint);
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<ApiResponse<T>> ApiPutAsync<T>(string endpoint, T data)
        {
            try
            {
                AddAuthHeader();
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{endpoint}", content);

                string errorMessage = null;
                if (!response.IsSuccessStatusCode)
                {
                    errorMessage = await ExtractErrorMessage(response);
                }

                return new ApiResponse<T>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    Content = response.IsSuccessStatusCode ? data : default(T),
                    ErrorMessage = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en PUT al endpoint de API: {Endpoint}", endpoint);
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<ApiResponse<object>> ApiDeleteAsync(string endpoint)
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{endpoint}");

                string errorMessage = null;
                string successMessage = null;

                if (response.IsSuccessStatusCode)
                {
                    // Para respuestas exitosas, intentar extraer el mensaje de éxito
                    try
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var responseObj = JsonConvert.DeserializeObject<dynamic>(content);

                        if (responseObj?.message != null)
                            successMessage = responseObj.message;
                    }
                    catch
                    {
                        // Si no se puede parsear, no es crítico para una respuesta exitosa
                        successMessage = "Operación completada exitosamente";
                    }
                }
                else
                {
                    // Solo extraer mensaje de error para respuestas no exitosas
                    errorMessage = await ExtractErrorMessage(response);
                }

                return new ApiResponse<object>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    ErrorMessage = errorMessage,
                    SuccessMessage = successMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DELETE al endpoint de API: {Endpoint}", endpoint);
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message
                };
            }
        }

        // GET: Playlists - Públicas, sin autorización
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

        // GET: Playlists/MyPlaylists - Requiere autenticación
        public async Task<IActionResult> MyPlaylists()
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            try
            {
                var playlists = await ApiGetAsync<List<PlaylistDto>>("playlists/my-playlists");

                if (playlists == null)
                {
                    TempData["Error"] = "Error cargando playlists. Por favor inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Account");
                }

                return View(playlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando mis playlists");
                ViewBag.Error = "Error cargando sus playlists. Por favor intente nuevamente.";
                return View(new List<PlaylistDto>());
            }
        }

        // GET: Playlists/Details/5 - Público, pero con funcionalidad adicional si está autenticado
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var playlist = await ApiGetAsync<PlaylistDto>($"playlists/{id}");
                if (playlist == null)
                    return NotFound();

                // Cargar canciones disponibles solo si el usuario está autenticado
                if (IsAuthenticated())
                {
                    var allSongs = await ApiGetAsync<List<SongDto>>("songs?isActive=true") ?? new List<SongDto>();
                    var playlistSongIds = playlist.Songs.Select(s => s.Id).ToHashSet();
                    ViewBag.AvailableSongs = allSongs.Where(s => !playlistSongIds.Contains(s.Id)).ToList();
                }

                return View(playlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando detalles de playlist");
                ViewBag.Error = "Error cargando detalles de la playlist.";
                return View();
            }
        }

        // GET: Playlists/Create - Requiere autenticación
        public async Task<IActionResult> Create()
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            return View();
        }

        // POST: Playlists/Create - Requiere autenticación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlaylistDto dto)
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var response = await ApiPostAsync("playlists", dto);

                if (response.IsSuccess)
                {
                    TempData["Success"] = "Playlist creada exitosamente!";
                    return RedirectToAction(nameof(MyPlaylists));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] = "Tu sesión ha expirado. Por favor inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.Error = response.ErrorMessage ?? "Error creando playlist.";
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando playlist");
                ViewBag.Error = "Error creando playlist.";
                return View(dto);
            }
        }

        // GET: Playlists/Edit/5 - Requiere autenticación y propiedad
        public async Task<IActionResult> Edit(int id)
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            try
            {
                var playlist = await ApiGetAsync<PlaylistDto>($"playlists/{id}");
                if (playlist == null)
                {
                    TempData["Error"] = "Playlist no encontrada o no tienes permisos para editarla.";
                    return RedirectToAction(nameof(MyPlaylists));
                }

                var updateDto = new CreatePlaylistDto { Name = playlist.Name };
                ViewBag.PlaylistId = id;
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando playlist para edición");
                TempData["Error"] = "Error cargando playlist para edición.";
                return RedirectToAction(nameof(MyPlaylists));
            }
        }

        // POST: Playlists/Edit/5 - Requiere autenticación y propiedad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreatePlaylistDto dto)
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            if (!ModelState.IsValid)
            {
                ViewBag.PlaylistId = id;
                return View(dto);
            }

            try
            {
                var response = await ApiPutAsync($"playlists/{id}", dto);

                if (response.IsSuccess)
                {
                    TempData["Success"] = "Playlist actualizada exitosamente!";
                    return RedirectToAction(nameof(MyPlaylists));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] = "Tu sesión ha expirado. Por favor inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["Error"] = response.ErrorMessage ?? "No tienes permisos para editar esta playlist.";
                    return RedirectToAction(nameof(MyPlaylists));
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "La playlist no fue encontrada.";
                    return RedirectToAction(nameof(MyPlaylists));
                }

                ViewBag.Error = response.ErrorMessage ?? "Error actualizando playlist.";
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

        // GET: Playlists/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            try
            {
                var playlist = await ApiGetAsync<PlaylistDto>($"playlists/{id}");
                if (playlist == null)
                {
                    TempData["Error"] = "Playlist no encontrada.";
                    return RedirectToAction(nameof(MyPlaylists));
                }

                return View(playlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando playlist para eliminación");
                TempData["Error"] = "Error cargando playlist.";
                return RedirectToAction(nameof(MyPlaylists));
            }
        }

        // POST: Playlists/DeleteConfirmed/5 - Requiere autenticación y propiedad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            try
            {
                var response = await ApiDeleteAsync($"playlists/{id}");

                if (response.IsSuccess)
                {
                    TempData["Success"] = response.SuccessMessage ?? "Playlist eliminada exitosamente!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] = "Tu sesión ha expirado. Por favor inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Account");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["Error"] = response.ErrorMessage ?? "No tienes permisos para eliminar esta playlist.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "La playlist no fue encontrada.";
                }
                else
                {
                    TempData["Error"] = response.ErrorMessage ?? "Error eliminando playlist.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando playlist");
                TempData["Error"] = "Error eliminando playlist.";
            }

            return RedirectToAction(nameof(MyPlaylists));
        }

        // POST: Playlists/AddSong/5 - Requiere autenticación y propiedad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int id, AddSongToPlaylistDto dto)
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            try
            {
                var response = await ApiPostAsync($"playlists/{id}/songs", dto);

                if (response.IsSuccess)
                {
                    TempData["Success"] = "Canción agregada a la playlist exitosamente!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] = "Tu sesión ha expirado. Por favor inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Account");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["Error"] = "No tienes permisos para modificar esta playlist.";
                }
                else
                {
                    TempData["Error"] = response.ErrorMessage ?? "Error agregando canción a la playlist.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando canción a playlist");
                TempData["Error"] = "Error agregando canción a la playlist.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Playlists/RemoveSong/5 - Requiere autenticación y propiedad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int id, int songId)
        {
            var authCheck = CheckAuthenticationAndRedirect();
            if (authCheck != null) return authCheck;

            try
            {
                var response = await ApiDeleteAsync($"playlists/{id}/songs/{songId}");

                if (response.IsSuccess)
                {
                    TempData["Success"] = response.SuccessMessage ?? "Canción removida de la playlist exitosamente!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["Error"] = "Tu sesión ha expirado. Por favor inicia sesión nuevamente.";
                    return RedirectToAction("Login", "Account");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["Error"] = "No tienes permisos para modificar esta playlist.";
                }
                else
                {
                    TempData["Error"] = response.ErrorMessage ?? "Error removiendo canción de la playlist.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removiendo canción de playlist");
                TempData["Error"] = "Error removiendo canción de la playlist.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // Clase helper para manejar respuestas de API
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public T? Content { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}