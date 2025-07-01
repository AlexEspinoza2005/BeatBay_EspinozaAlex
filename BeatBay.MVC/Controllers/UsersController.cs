using BeatBay.API.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BeatBay.MVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:7037/api";

        public UsersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Vista para el formulario de registro (GET)
        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new CreateUserDto
            {
                UserName = "",
                Email = "",
                Name = "",
                Bio = "",
                PlanId = 1 // Plan por defecto (Free)
            };
            return View(model);
        }

        // Acción para registrar un nuevo usuario (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Register(CreateUserDto createUserDto)
        {
            if (createUserDto == null)
            {
                ModelState.AddModelError("", "Datos de registro inválidos.");
                return View(new CreateUserDto());
            }

            if (!ModelState.IsValid)
            {
                return View(createUserDto);
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient("BeatBayAPI");

                var json = JsonConvert.SerializeObject(createUserDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/Users/register", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario registrado exitosamente. Por favor, inicie sesión.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error al registrar usuario: {errorContent}");
                    return View(createUserDto);
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Error de conexión con la API: {ex.Message}");
                return View(createUserDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return View(createUserDto);
            }
        }

        // Vista para el formulario de login (GET)
        [AllowAnonymous]
        public ActionResult Login()
        {
            var model = new LoginDto
            {
                UserName = "",
                Password = ""
            };
            return View(model);
        }

        // Acción para iniciar sesión (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                ModelState.AddModelError("", "Datos de login inválidos.");
                return View(new LoginDto());
            }

            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient("BeatBayAPI");

                var json = JsonConvert.SerializeObject(loginDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/Users/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginDto.UserName ?? ""),
                        new Claim("UserName", loginDto.UserName ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync("Cookies", claimsPrincipal);

                    TempData["WelcomeMessage"] = "¡Bienvenido! Has iniciado sesión correctamente.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Credenciales incorrectas: {errorContent}");
                    return View(loginDto);
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Error de conexión con la API: {ex.Message}");
                return View(loginDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return View(loginDto);
            }
        }
    }
}
