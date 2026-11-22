using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using momentify_backend.Commands.CreateOrUpdatePhoto;
using momentify_backend.Commands.CreateSite;
using momentify_backend.Commands.CreateUser;
using momentify_backend.Commands.LoginUser;
using momentify_backend.Commands.PublishSite;
using momentify_backend.Commands.UpdateSite;
using momentify_backend.Data;
using momentify_backend.Platform.Database.Models;
using momentify_backend.Platform.FluentValidators;
using momentify_backend.Platform.Interfaces;
using momentify_backend.Platform.Middleware;
using momentify_backend.Platform.Queries.GetPhoto;
using momentify_backend.Platform.Queries.GetPublicPhoto;
using momentify_backend.Platform.Queries.GetSiteBySlug;
using momentify_backend.Platform.Queries.GetTemplates;
using momentify_backend.Platform.Queries.GetUser;
using momentify_backend.Platform.Queries.GetUserSites;
using momentify_backend.Platform.Readers;
using momentify_backend.Platform.Storage;
using momentify_backend.Platform.Writers;
using momentify_backend.Services;
using momentify_backend.Services.Photo;
using momentify_backend.Services.Site;
using momentify_backend.Services.Template;
using momentify_backend.Services.User;

var aspnetEnvironment =
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (string.Equals(
    aspnetEnvironment,
    "Development",
    StringComparison.OrdinalIgnoreCase))
{
    LoadLocalEnvironmentFile(
        Path.Combine(Directory.GetCurrentDirectory(), ".env.local"));
}

var builder = WebApplication.CreateBuilder(args);

RegisterValidators(builder.Services);
ConfigureSwagger(builder.Services);
ConfigureCors(builder.Services, builder.Configuration);
ConfigureAuthentication(builder.Services, builder.Configuration);
ConfigureApplicationServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigurePipeline(app);

app.Run();

static void LoadLocalEnvironmentFile(string path)
{
    if (!File.Exists(path))
        return;

    foreach (var line in File.ReadLines(path))
    {
        var entry = line.Trim();
        if (entry.Length == 0 || entry.StartsWith('#'))
            continue;

        var separator = entry.IndexOf('=');
        if (separator <= 0)
            continue;

        var key = entry[..separator].Trim();
        var value = entry[(separator + 1)..].Trim();
        if (value.Length >= 2 && value[0] == value[^1] && value[0] is '\'' or '"')
            value = value[1..^1];

        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

static void RegisterValidators(IServiceCollection services)
{
    services.AddFluentValidationAutoValidation();
    services.AddValidatorsFromAssemblyContaining<CreateSiteRequestValidator>();
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token."
        });

        options.AddSecurityRequirement(document =>
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
    });
}

static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
{
    var origins = configuration["Cors:AllowedOrigins"]?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? [];

    services.AddCors(options =>
    {
        options.AddPolicy("MomentifyCors", policy =>
        {
            if (origins.Length > 0)
                policy.WithOrigins(origins);
            else
                policy.AllowAnyOrigin();

            policy.AllowAnyHeader().AllowAnyMethod();
        });
    });
}

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtKey = configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("Set the Jwt:Key configuration value or the Jwt__Key environment variable.");

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    services.AddAuthorization();
    services.AddControllers();
}

static void ConfigureApplicationServices(IServiceCollection services, IConfiguration configuration)
{
    var databaseConnectionString = GetDatabaseConnectionString(configuration);
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(databaseConnectionString));

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ITemplateService, TemplateService>();
    services.AddScoped<ISiteService, SiteService>();
    services.AddScoped<IPhotoService, PhotoService>();
    services.AddScoped<IPhotoStorage>(provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var bucketName = Environment.GetEnvironmentVariable("GCS_BUCKET")
            ?? Environment.GetEnvironmentVariable("PhotoStorage__Bucket")
            ?? configuration["PhotoStorage:Bucket"];

        return string.IsNullOrWhiteSpace(bucketName)
            ? new LocalPhotoStorage(provider.GetRequiredService<IWebHostEnvironment>())
            : new GcsPhotoStorage(configuration);
    });
    services.AddScoped<ITemplateFactory, TemplateFactory>();
    services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

    services.AddScoped<ISiteReader, SiteReader>();
    services.AddScoped<ISiteDataReader, SiteDataReader>();
    services.AddScoped<IUserReader, UserReader>();
    services.AddScoped<ITemplateReader, TemplateReader>();
    services.AddScoped<IPhotoReader, PhotoReader>();

    services.AddScoped<ISiteWriter, SiteWriter>();
    services.AddScoped<IUserWriter, UserWriter>();
    services.AddScoped<ITemplateWriter, TemplateWriter>();
    services.AddScoped<IPhotoWriter, PhotoWriter>();

    services.AddScoped<ITemplateDefinition, OurStoryTemplate>();
    services.AddScoped<ITemplateDefinition, MemoriesTemplate>();
    services.AddScoped<ITemplateDefinition, TheBigAskTemplate>();

    services.AddScoped<IGetUserSitesQueryHandler, GetUserSitesQueryHandler>();
    services.AddScoped<IGetSiteBySlugQueryHandler, GetSiteBySlugQueryHandler>();
    services.AddScoped<IGetUserQueryHandler, GetUserQueryHandler>();
    services.AddScoped<IGetTemplatesQueryHandler, GetTemplatesQueryHandler>();
    services.AddScoped<IGetPhotoQueryHandler, GetPhotoQueryHandler>();
    services.AddScoped<IGetPublicPhotoQueryHandler, GetPublicPhotoQueryHandler>();

    services.AddScoped<ICreateSiteCommandHandler, CreateSiteCommandHandler>();
    services.AddScoped<ICreateUserCommandHandler, CreateUserCommandHandler>();
    services.AddScoped<ILoginUserCommandHandler, LoginUserCommandHandler>();
    services.AddScoped<IUpdateSiteCommandHandler, UpdateSiteCommandHandler>();
    services.AddScoped<IPublishSiteCommandHandler, PublishSiteCommandHandler>();
    services.AddScoped<ICreateOrUpdatePhotoCommandHandler, CreateOrUpdatePhotoCommandHandler>();
}

static string GetDatabaseConnectionString(IConfiguration configuration)
{
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? configuration["DATABASE_URL"]
        ?? configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException(
            "Set ConnectionStrings:DefaultConnection or the DATABASE_URL environment variable.");

    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var databaseUri) ||
        (databaseUri.Scheme != "postgres" && databaseUri.Scheme != "postgresql"))
    {
        return connectionString;
    }

    var credentials = databaseUri.UserInfo.Split(':', 2);
    if (credentials.Length != 2)
        throw new InvalidOperationException("DATABASE_URL must include a database username and password.");

    var options = ParseConnectionOptions(databaseUri.Query);
    var npgsqlConnectionString = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.IsDefaultPort || databaseUri.Port < 0 ? 5432 : databaseUri.Port,
        Database = Uri.UnescapeDataString(databaseUri.AbsolutePath.Trim('/')),
        Username = Uri.UnescapeDataString(credentials[0]),
        Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = ParseSslMode(options.GetValueOrDefault("sslmode")),
        ChannelBinding = ParseChannelBinding(options.GetValueOrDefault("channel_binding"))
    };

    return npgsqlConnectionString.ConnectionString;
}

static Dictionary<string, string> ParseConnectionOptions(string query)
{
    return query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(option => option.Split('=', 2))
        .Where(option => option.Length == 2)
        .ToDictionary(
            option => Uri.UnescapeDataString(option[0]),
            option => Uri.UnescapeDataString(option[1]),
            StringComparer.OrdinalIgnoreCase);
}

static SslMode ParseSslMode(string? value) => value?.ToLowerInvariant() switch
{
    "disable" => SslMode.Disable,
    "prefer" => SslMode.Prefer,
    "verify-ca" => SslMode.VerifyCA,
    "verify-full" => SslMode.VerifyFull,
    _ => SslMode.Require
};

static ChannelBinding ParseChannelBinding(string? value) => value?.ToLowerInvariant() switch
{
    "disable" => ChannelBinding.Disable,
    "require" => ChannelBinding.Require,
    _ => ChannelBinding.Prefer
};

static void ConfigurePipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("MomentifyCors");
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
        .AllowAnonymous();
    app.MapControllers();
}
