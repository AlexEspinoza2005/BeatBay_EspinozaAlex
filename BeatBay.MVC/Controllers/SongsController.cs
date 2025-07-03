using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeatBay.DTOs;
using Newtonsoft.Json;
using System.Text;

namespace BeatBayMVC.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public SongsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration.GetSection("ApiSettings:BaseUrl").Value ?? "https://localhost:7037/api";
        }

        // GET: Songs
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var songs = JsonConvert.DeserializeObject<List<SongDto>>(json);
                    return View(songs);
                }

                ViewBag.Error = "Error al cargar las canciones";
                return View(new List<SongDto>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View(new List<SongDto>());
            }
        }

        // GET: Songs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var song = JsonConvert.DeserializeObject<SongDto>(json);
                    return View(song);
                }

                return NotFound();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // GET: Songs/Create
        [Authorize(Roles = "Artist,Admin")]
        public IActionResult Create()
        {
            return View();
        }
        // POST: Songs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist,Admin")]
        public async Task<IActionResult> Create(CreateSongDto dto, IFormFile audioFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var token = HttpContext.Session.GetString("AuthToken");
                    if (!string.IsNullOrEmpty(token))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }

                    using var content = new MultipartFormDataContent();

                    // Agregar datos del DTO
                    content.Add(new StringContent(dto.Title), "Title");
                    content.Add(new StringContent(dto.Duration.ToString()), "Duration");
                    content.Add(new StringContent(dto.Genre ?? ""), "Genre");

                    // Agregar archivo de audio (requerido)
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        var fileContent = new StreamContent(audioFile.OpenReadStream());
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(audioFile.ContentType);
                        content.Add(fileContent, "audioFile", audioFile.FileName);
                    }
                    else
                    {
                        ViewBag.Error = "Debe seleccionar un archivo de audio";
                        return View(dto);
                    }

                    var response = await _httpClient.PostAsync($"{_apiBaseUrl}/songs", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Canción creada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ViewBag.Error = $"Error al crear la canción: {errorContent}";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = $"Error: {ex.Message}";
                }
            }

            return View(dto);
        }

        // GET: Songs/Edit/5
        [Authorize(Roles = "Artist,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var song = JsonConvert.DeserializeObject<SongDto>(json);

                    var updateDto = new UpdateSongDto
                    {
                        Title = song.Title,
                        Duration = song.Duration,
                        Genre = song.Genre,
                        StreamingUrl = song.StreamingUrl,
                        IsActive = song.IsActive
                    };

                    ViewBag.SongId = id;
                    return View(updateDto);
                }

                return NotFound();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // POST: Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist,Admin")]
        public async Task<IActionResult> Edit(int id, UpdateSongDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var token = HttpContext.Session.GetString("AuthToken");
                    if (!string.IsNullOrEmpty(token))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }

                    var json = JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync($"{_apiBaseUrl}/songs/{id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "Canción actualizada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ViewBag.Error = $"Error al actualizar la canción: {errorContent}";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = $"Error: {ex.Message}";
                }
            }

            ViewBag.SongId = id;
            return View(dto);
        }

        // GET: Songs/Delete/5
        [Authorize(Roles = "Artist,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var song = JsonConvert.DeserializeObject<SongDto>(json);
                    return View(song);
                }

                return NotFound();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // POST: Songs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/songs/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Canción eliminada exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al eliminar la canción";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Songs/Play/5
        [HttpPost]
        public async Task<IActionResult> Play(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/songs/{id}/play", null);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, data = JsonConvert.DeserializeObject(json) });
                }

                return Json(new { success = false, message = "Error al iniciar reproducción" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}