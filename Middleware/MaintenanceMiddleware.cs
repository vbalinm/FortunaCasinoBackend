using FortunaCasino.Data;
using Microsoft.EntityFrameworkCore;

namespace FortunaCasino.Middleware
{
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            // Admin endpointokat és auth endpointokat átengedjük
            var path = context.Request.Path.Value ?? "";
            var isAdminRoute = path.StartsWith("/api/admin");
            var isAuthRoute = path.StartsWith("/api/auth");

            if (!isAdminRoute && !isAuthRoute)
            {
                var setting = await db.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == "maintenance_mode");

                if (setting?.Value == "true")
                {
                    context.Response.StatusCode = 503;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"message\":\"Az oldal karbantartás alatt van. Kérjük, próbáld később!\"}");
                    return;
                }
            }
            await _next(context);
        }
    }
}