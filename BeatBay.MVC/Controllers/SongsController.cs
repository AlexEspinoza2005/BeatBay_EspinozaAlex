using Microsoft.AspNetCore.Mvc;
using BeatBay.DTOs;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace BeatBayMVC.Controllers
{
    public class SongsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public SongsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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

        // GET: Songs
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs");
                if (!response.IsSuccessStatusCode)
                    return View(new List<SongDto>());

                var json = await response.Content.ReadAsStringAsync();
                var songs = JsonConvert.DeserializeObject<List<SongDto>>(json);
                return View(songs);
            }
            catch
            {
                return View(new List<SongDto>());
            }
        }

        // GET: Songs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs/{id}");
                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var json = await response.Content.ReadAsStringAsync();
                var song = JsonConvert.DeserializeObject<SongDto>(json);
                return View(song);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Songs/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSongDto dto, IFormFile audioFile)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (audioFile == null || audioFile.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un archivo de audio.");
                return View(dto);
            }

            try
            {
                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(dto.Title), nameof(dto.Title));
                content.Add(new StringContent(dto.Duration.ToString()), nameof(dto.Duration));
                content.Add(new StringContent(dto.Genre ?? ""), nameof(dto.Genre));

                var fileContent = new StreamContent(audioFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(audioFile.ContentType);
                content.Add(fileContent, "audioFile", audioFile.FileName);

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/songs", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Canción creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Error al crear la canción: {error}");
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(dto);
            }
        }


        // GET: Songs/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            AddAuthHeader();

            if (!await UserHasRoleAsync("Artist") && !await UserHasRoleAsync("Admin"))
            {
                TempData["Error"] = "Debes ser artista o admin para editar canciones.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs/{id}");
                if (!response.IsSuccessStatusCode)
                    return NotFound();

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
            catch
            {
                return NotFound();
            }
        }

        // POST: Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSongDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SongId = id;
                return View(dto);
            }

            AddAuthHeader();

            if (!await UserHasRoleAsync("Artist") && !await UserHasRoleAsync("Admin"))
            {
                TempData["Error"] = "Debes ser artista o admin para editar canciones.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/songs/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Canción actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Error al actualizar la canción: {error}");
                ViewBag.SongId = id;
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewBag.SongId = id;
                return View(dto);
            }
        }

        // GET: Songs/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            AddAuthHeader();

            if (!await UserHasRoleAsync("Artist") && !await UserHasRoleAsync("Admin"))
            {
                TempData["Error"] = "Debes ser artista o admin para eliminar canciones.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/songs/{id}");
                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var json = await response.Content.ReadAsStringAsync();
                var song = JsonConvert.DeserializeObject<SongDto>(json);
                return View(song);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Songs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AddAuthHeader();

            if (!await UserHasRoleAsync("Artist") && !await UserHasRoleAsync("Admin"))
            {
                TempData["Error"] = "Debes ser artista o admin para eliminar canciones.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/songs/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Canción eliminada correctamente.";
                }
                else
                {
                    TempData["Error"] = "Error al eliminar la canción.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UserHasRoleAsync(string role)
        {
            AddAuthHeader();

            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/auth/profile");
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<UserProfileDto>(json);

            if (userProfile?.Roles == null) return false;

            return userProfile.Roles.Contains(role);
        }
    }

    public class UserProfileDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public List<string>? Roles { get; set; }
    }
}
