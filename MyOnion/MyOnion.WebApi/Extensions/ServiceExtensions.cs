using NSwag;
using NSwag.Generation.Processors.Security;
using MyOnion.WebApi.Filters;
using MyOnion.WebApi.Options;

namespace MyOnion.WebApi.Extensions
{
    public static class ServiceExtensions
    {
        // Extension method to add Swagger documentation to the service collection
        public static void AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddOpenApiDocument(config =>
            {
                config.DocumentName = "v1";
                config.Version = "v1";
                config.Title = "Clean Architecture - MyOnion.WebApi";
                config.Description = "This Api will be responsible for overall data distribution and authorization.";
                config.PostProcess = document =>
                {
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Jane Doe",
                        Email = "jdoe@janedoe.com",
                        Url = "https://janedoe.com/contact",
                    };
                };

                config.AddSecurity("Bearer", new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Name = "Authorization",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
            });
        }

        // Extension method to add and configure controllers with JSON options
        public static void AddControllersExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ExecutionTimingOptions>(configuration.GetSection(ExecutionTimingOptions.SectionName));
            services.AddScoped<ExecutionTimeResultFilter>();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Configure JSON serializer to use camelCase
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                })
                .AddMvcOptions(options =>
                {
                    options.Filters.AddService<ExecutionTimeResultFilter>();
                });
        }

        // Extension method to configure CORS with a policy to allow any origin, header, and method
        public static void AddCorsExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.Trim())
                .ToArray() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                builder =>
                {
                    if (configuredOrigins.Length > 0)
                    {
                        builder.WithOrigins(configuredOrigins)
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                    }
                    else
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    }
                });
            });
        }

        // Extension method to add API versioning and configure the API version explorer
        public static void AddVersionedApiExplorerExtension(this IServiceCollection services)
        {
            var apiVersioningBuilder = services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                new HeaderApiVersionReader("x-api-version"),
                                                new MediaTypeApiVersionReader("x-api-version"));
            });
            apiVersioningBuilder.AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }

        // Extension method to add API versioning for the service
        public static void AddApiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
            });
        }

        // Extension method to set up JWT authentication
        public static void AddJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authority = configuration["Sts:ServerUrl"];
            var audience = configuration["Sts:Audience"];

            if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException("JWT configuration is missing required Sts:ServerUrl or Sts:Audience values.");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.Authority = authority;
                    options.Audience = audience;
                    options.SaveToken = true;

                    var explicitIssuer = configuration["Sts:ValidIssuer"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = string.IsNullOrWhiteSpace(explicitIssuer) ? authority : explicitIssuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });
        }

        // Extension method to add authorization policies based on roles
        public static void AddAuthorizationPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            string admin = configuration["ApiRoles:AdminRole"],
                    manager = configuration["ApiRoles:ManagerRole"], employee = configuration["ApiRoles:EmployeeRole"];

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationConsts.AdminPolicy, policy => policy.RequireRole(admin));
                options.AddPolicy(AuthorizationConsts.ManagerPolicy, policy => policy.RequireRole(manager, admin));
                options.AddPolicy(AuthorizationConsts.EmployeePolicy, policy => policy.RequireRole(employee, manager, admin));
            });
        }
    }
}
