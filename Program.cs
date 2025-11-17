using DotNetEnv;
using Infrastructure.Data.Configurations;
using Infrastructure.Data.MongoDB.Context;
using Core.Domain.Interfaces;
using Infrastructure.Data.MongoDB.Repositories;
using Core.Application.Interfaces;
using Core.Application.Services;
using Core.Application.Mappings;
using Core.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using API.Middleware;
using API.Filters;

var builder = WebApplication.CreateBuilder(args);

// Load .env file (if exists)
Env.TraversePath().Load();

// Read environment variables
var mongoConn = Environment.GetEnvironmentVariable("MONGO_CONN");
var mongoDb = Environment.GetEnvironmentVariable("MONGO_DB");

// Configure MongoDB settings
var databaseSettings = new DatabaseSettings
{
    ConnectionString = mongoConn ?? throw new InvalidOperationException("MONGO_CONN environment variable is not set"),
    DatabaseName = mongoDb ?? throw new InvalidOperationException("MONGO_DB environment variable is not set")
};

builder.Services.Configure<DatabaseSettings>(options =>
{
    options.ConnectionString = databaseSettings.ConnectionString;
    options.DatabaseName = databaseSettings.DatabaseName;
});

// Add MongoDB Context
builder.Services.AddSingleton<MongoDbContext>();

// Add Repositories
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
builder.Services.AddScoped<Core.Domain.Interfaces.IPropertyImageRepository, PropertyImageRepository>();

// Add Application Services
builder.Services.AddScoped<IPropertyService, PropertyService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(PropertyProfile));

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<PropertyFilterDtoValidator>();

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MillionBack API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Add middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();

// CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
