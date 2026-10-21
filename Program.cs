using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Commands.CreateSite;
using momentify_backend.Commands.CreateUser;
using momentify_backend.Commands.PublishSite;
using momentify_backend.Data;
using momentify_backend.Interfaces;
using momentify_backend.Models;
using momentify_backend.Queries.GetSiteBySlug;
using momentify_backend.Queries.GetTemplates;
using momentify_backend.Queries.GetUser;
using momentify_backend.Queries.GetUserSites;
using momentify_backend.Readers;
using momentify_backend.Services;
using momentify_backend.Writers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<ITemplateFactory, TemplateFactory>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<ISiteReader, SiteReader>();
builder.Services.AddScoped<IUserReader, UserReader>();
builder.Services.AddScoped<ITemplateReader, TemplateReader>();

builder.Services.AddScoped<ISiteWriter, SiteWriter>();
builder.Services.AddScoped<IUserWriter, UserWriter>();
builder.Services.AddScoped<ITemplateWriter, TemplateWriter>();

builder.Services.AddScoped<
    IGetUserSitesQueryHandler,
    GetUserSitesQueryHandler>();

builder.Services.AddScoped<
    IGetSiteBySlugQueryHandler,
    GetSiteBySlugQueryHandler>();

builder.Services.AddScoped<
    IGetUserQueryHandler,
    GetUserQueryHandler>();

builder.Services.AddScoped<
    IGetTemplatesQueryHandler,
    GetTemplatesQueryHandler>();

builder.Services.AddScoped<
    ICreateSiteCommandHandler,
    CreateSiteCommandHandler>();

builder.Services.AddScoped<
    ICreateUserCommandHandler,
    CreateUserCommandHandler>();

builder.Services.AddScoped<
    IPublishSiteCommandHandler,
    PublishSiteCommandHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map API controllers
app.MapControllers();

app.Run();