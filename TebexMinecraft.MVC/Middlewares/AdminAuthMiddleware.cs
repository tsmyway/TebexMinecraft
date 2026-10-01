using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace TebexMinecraft.MVC.Middlewares;

public class AdminAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public AdminAuthMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        
        if (path == "/admin-logout")
        {
            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Acceso Restringido Staff\"";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Has cerrado sesión de administrador. Cierra o recarga esta pestaña.");
            return;
        }

        bool isProtectedPath = path.Contains("/create") || 
                               path.Contains("/edit") || 
                               path.Contains("/delete");

        if (isProtectedPath)
        {
            string authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
                    if (authHeaderVal.Parameter != null)
                    {
                        var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderVal.Parameter)).Split(':', 2);
                        string user = credentials[0];
                        string pass = credentials.Length > 1 ? credentials[1] : "";

                        string? expectedUser = _config["AdminSecurity:User"];
                        string? expectedPass = _config["AdminSecurity:SecretKey"];

                        if (user == expectedUser && pass == expectedPass)
                        {
                            await _next(context);
                            return;
                        }
                    }
                }
                catch
                {
                }
            }

            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Acceso Restringido Staff\"";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Acceso no autorizado.");
            return;
        }

        await _next(context);
    }
}