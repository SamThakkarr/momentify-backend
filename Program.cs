using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

var builder = WebApplication.CreateBuilder(args);

RegisterValidators(builder.Services);
ConfigureSwagger(builder.Services);
ConfigureAuthentication(builder.Services, builder.Configuration);
ConfigureApplicationServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigurePipeline(app);

app.Run();

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

static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
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
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };
        });

    services.AddAuthorization();
    services.AddControllers();
}

static void ConfigureApplicationServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ITemplateService, TemplateService>();
    services.AddScoped<ISiteService, SiteService>();
    services.AddScoped<IPhotoService, PhotoService>();
    services.AddScoped<IPhotoStorage, LocalPhotoStorage>();
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

static void ConfigurePipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.MapControllers();
}
