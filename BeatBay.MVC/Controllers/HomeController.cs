using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BeatBay.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Mostrar mensaje de bienvenida si existe
            if (TempData["WelcomeMessage"] != null)
            {
                ViewBag.WelcomeMessage = TempData["WelcomeMessage"];
            }

            // Mostrar mensaje de info si existe (como logout)
            if (TempData["InfoMessage"] != null)
            {
                ViewBag.InfoMessage = TempData["InfoMessage"];
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            ViewBag.Message = "No tienes permisos para acceder a esta página.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(errorViewModel);
        }
    }

    // Modelo para la vista de Error
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}