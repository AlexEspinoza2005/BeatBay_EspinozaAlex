using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1) Servicios
builder.Services.AddControllersWithViews();

// 2) HttpClient nombrado para interactuar con la API
builder.Services.AddHttpClient("BeatBay.API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);  // Usa la URL de la API definida en appsettings.json
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// 3) Autenticación con Cookie (almacena el JWT en la cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/VAuth/Login";  // Redirige al login en el controlador VAuthController
        options.LogoutPath = "/VAuth/Logout"; // Redirige al logout en el controlador VAuthController
        options.Cookie.Name = "auth_cookie"; // Nombre de la cookie de autenticación
        options.Cookie.HttpOnly = true;  // Solo accesible por el servidor, no en JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // Asegura la cookie en producción
        options.ExpireTimeSpan = TimeSpan.FromHours(2);  // Duración de la sesión (2 horas)
    });

// 4) Configuración de CORS (permitir acceso desde tu frontend MVC)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb",
        policy => policy
            .WithOrigins("https://localhost:7194")  // Cambia esto por tu URL de frontend en producción
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// 5) Middleware de excepciones y HSTS
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Mostrar excepciones completas en desarrollo
}
else
{
    app.UseExceptionHandler("/Home/Error");  // Redirige a la acción Error en producción
    app.UseHsts();  // HSTS en producción para mayor seguridad
}

// 6) Resto del pipeline
app.UseHttpsRedirection();  // Redirige todo a HTTPS
app.UseStaticFiles();  // Permite servir archivos estáticos

app.UseRouting();  // Habilita el enrutamiento

// 7) Middleware de autenticación y autorización
app.UseAuthentication();  // Habilita la autenticación
app.UseAuthorization();   // Habilita la autorización

// 8) Configuración de CORS
app.UseCors("AllowWeb");  // Habilita CORS para el frontend

// 9) Mapear rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"  // Ruta predeterminada para los controladores MVC
);

app.Run();
