using BeatBay.APIConsumer; // Esto es para usar la clase Crud<T>
using BeatBay.Data; // Asegúrate de que esté el DbContext de tu API
using BeatBay.Model;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatBay.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configurar los endpoints de la API para la clase Crud
            ConfigureApiEndpoints();

            // Configurar servicios MVC
            builder.Services.AddControllersWithViews();

            // Agregar HttpClient para hacer solicitudes a la API
            builder.Services.AddHttpClient();

            // Configurar HttpClient con la URL base de la API
            builder.Services.AddHttpClient("BeatBayAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7037/"); // Asegúrate de que la API esté corriendo en este puerto
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Configuración de la autenticación con cookies
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Users/Login";  // Ruta de login si no está autenticado
                    options.LogoutPath = "/Users/Logout"; // Ruta de logout
                    options.AccessDeniedPath = "/Home/AccessDenied"; // Ruta cuando se niega el acceso
                    options.ExpireTimeSpan = TimeSpan.FromHours(24); // Tiempo de expiración de la cookie
                    options.SlidingExpiration = true; // Renovación automática de la cookie
                });

            // Configuración de autorización
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy; // Requiere autenticación por defecto
            });

            // Agregar el contexto de base de datos (si tienes acceso a DB en MVC, pero normalmente deberías usar solo la API)
            builder.Services.AddDbContext<BeatBayDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("BeatBayDBContext"))); // Usa PostgreSQL si es el caso

            var app = builder.Build();

            // Configuración de la pipeline del middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Middleware de autenticación y autorización
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Para servir archivos estáticos (CSS, JS, imágenes)
            app.UseRouting();
            app.UseAuthentication();  // Necesario para manejar la autenticación
            app.UseAuthorization();   // Necesario para manejar la autorización

            // Configurar las rutas del controlador
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        // Configura los endpoints de la API para la clase Crud<T>
        private static void ConfigureApiEndpoints()
        {
            Crud<User>.EndPoint = "https://localhost:7037/api/Users";
            Crud<Plan>.EndPoint = "https://localhost:7037/api/Plans";
            Crud<Song>.EndPoint = "https://localhost:7037/api/Songs";
            Crud<Playlist>.EndPoint = "https://localhost:7037/api/Playlists";
            Crud<AdminActionLog>.EndPoint = "https://localhost:7037/api/Admin/action-logs";
            Crud<Payment>.EndPoint = "https://localhost:7037/api/Payments";
            Crud<PlaybackStatistic>.EndPoint = "https://localhost:7037/api/Statistics/playback";
        }
    }
}
