using $safeprojectname$.Middlewares;
using NSwag.AspNetCore;

namespace $safeprojectname$.Extensions
{
    // Static class containing extension methods for IApplicationBuilder
    public static class AppExtensions
    {
        // Extension method to configure and use Swagger for API documentation
        public static void UseSwaggerExtension(this IApplicationBuilder app)
        {
            // Serve the generated OpenAPI document and Swagger UI via NSwag.
            app.UseOpenApi(settings =>
            {
                settings.Path = "/swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUi(settings =>
            {
                settings.Path = "/swagger";
                settings.DocumentPath = "/swagger/{documentName}/swagger.json";
            });
        }

        // Extension method to add custom middleware for error handling
        public static void UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {
            // Uses the ErrorHandlerMiddleware to process exceptions and generate appropriate responses
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }

        public static void UseRequestTimingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestTimingMiddleware>();
        }
    }
}

