using System.Text;
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
using momentify_backend.Interfaces;
using momentify_backend.Middleware;
using momentify_backend.Models;
using momentify_backend.Queries.GetPhoto;
using momentify_backend.Queries.GetPublicPhoto;
using momentify_backend.Queries.GetSiteBySlug;
using momentify_backend.Queries.GetTemplates;
using momentify_backend.Queries.GetUser;
using momentify_backend.Queries.GetUserSites;
using momentify_backend.Readers;
using momentify_backend.Services;
using momentify_backend.Storage;
using momentify_backend.Writers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSwaggerGen(options =>
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
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IPhotoStorage, LocalPhotoStorage>();
builder.Services.AddScoped<ITemplateFactory, TemplateFactory>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<ISiteReader, SiteReader>();
builder.Services.AddScoped<ISiteDataReader, SiteDataReader>();
builder.Services.AddScoped<IUserReader, UserReader>();
builder.Services.AddScoped<ITemplateReader, TemplateReader>();
builder.Services.AddScoped<IPhotoReader, PhotoReader>();

builder.Services.AddScoped<ISiteWriter, SiteWriter>();
builder.Services.AddScoped<IUserWriter, UserWriter>();
builder.Services.AddScoped<ITemplateWriter, TemplateWriter>();
builder.Services.AddScoped<IPhotoWriter, PhotoWriter>();

builder.Services.AddScoped<ITemplateDefinition, OurStoryTemplate>();
builder.Services.AddScoped<ITemplateDefinition, MemoriesTemplate>();

builder.Services.AddScoped<IGetUserSitesQueryHandler, GetUserSitesQueryHandler>();
builder.Services.AddScoped<IGetSiteBySlugQueryHandler, GetSiteBySlugQueryHandler>();
builder.Services.AddScoped<IGetUserQueryHandler, GetUserQueryHandler>();
builder.Services.AddScoped<IGetTemplatesQueryHandler, GetTemplatesQueryHandler>();
builder.Services.AddScoped<IGetPhotoQueryHandler, GetPhotoQueryHandler>();
builder.Services.AddScoped<IGetPublicPhotoQueryHandler, GetPublicPhotoQueryHandler>();

builder.Services.AddScoped<ICreateSiteCommandHandler, CreateSiteCommandHandler>();
builder.Services.AddScoped<ICreateUserCommandHandler, CreateUserCommandHandler>();
builder.Services.AddScoped<ILoginUserCommandHandler, LoginUserCommandHandler>();
builder.Services.AddScoped<IUpdateSiteCommandHandler, UpdateSiteCommandHandler>();
builder.Services.AddScoped<IPublishSiteCommandHandler, PublishSiteCommandHandler>();
builder.Services.AddScoped<ICreateOrUpdatePhotoCommandHandler, CreateOrUpdatePhotoCommandHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Map API controllers
app.MapControllers();

app.Run();