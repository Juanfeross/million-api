using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file (if exists)
Env.TraversePath().Load();

// Read environment variables
var mongoConn = Environment.GetEnvironmentVariable("MONGO_CONN");
var mongoDb = Environment.GetEnvironmentVariable("MONGO_DB");

// Add to configuration so services can use them later
builder.Configuration["DatabaseSettings:ConnectionString"] = mongoConn;
builder.Configuration["DatabaseSettings:DatabaseName"] = mongoDb;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (temporal)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
