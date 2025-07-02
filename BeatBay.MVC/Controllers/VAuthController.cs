using BeatBay.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BeatBay.MVC.Controllers
{
    public class VAuthController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public VAuthController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7194/api";
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponseDto>(responseContent);

                    // Guardar token en cookie o session
                    HttpContext.Session.SetString("JwtToken", result.Token);
                    HttpContext.Session.SetString("UserId", result.UserId.ToString());
                    HttpContext.Session.SetString("UserName", result.UserName);

                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(errorContent);
                    ModelState.AddModelError("", errorResult.Message ?? "Login failed");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
            }

            return View(model);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CreateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please check your email to confirm your account.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(errorContent);
                    ModelState.AddModelError("", errorResult.Message ?? "Registration failed");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
            }

            return View(model);
        }

        // GET: RegisterArtist
        [HttpGet]
        public IActionResult RegisterArtist()
        {
            return View();
        }

        // POST: RegisterArtist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterArtist(CreateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/auth/register-artist", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Artist registration successful! You can now login.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(errorContent);
                    ModelState.AddModelError("", errorResult.Message ?? "Artist registration failed");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
            }

            return View(model);
        }

        // GET: ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/auth/forgot-password", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Password reset link sent to your email.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(errorContent);
                    ModelState.AddModelError("", errorResult.Message ?? "Failed to send reset link");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
            }

            return View(model);
        }

        // GET: ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid reset link.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordDto
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        // POST: ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            try
            {
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/auth/reset-password", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Password reset successfully! You can now login.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonSerializer.Deserialize<ErrorResponseDto>(errorContent);
                    ModelState.AddModelError("", errorResult.Message ?? "Password reset failed");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
            }

            return View(model);
        }

        // GET: ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid confirmation link.";
                return RedirectToAction("Login");
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/auth/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Email confirmed successfully! You can now login.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Email confirmation failed.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while confirming your email.";
            }

            return RedirectToAction("Login");
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}