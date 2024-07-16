using NLog;
using NLog.Web;
using SwiftAPI.Data;
using SwiftAPI.Helpers;
using SwiftAPI.Interfaces;
using SwiftAPI.Repositories;
using SwiftAPI.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure NLog for logging
LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace); // Set the minimum level for built-in logging
builder.Host.UseNLog();

builder.Services.AddControllers();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SwiftAPI", Version = "v1" });
});

// Read connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("The connection string 'DefaultConnection' is not configured.");
}
builder.Services.AddSingleton(new DatabaseConnectionFactory(connectionString));

// Register helpers
builder.Services.AddScoped<ISwiftFileParser, SwiftFileParser>();
builder.Services.AddScoped<ISwiftMessageParser, SwiftMessageParser>();
builder.Services.AddScoped<ISwiftMessageValidator, SwiftMessageValidator>();

// Register repositories
builder.Services.AddScoped<ISwiftMessageRepository, SwiftMessageRepository>();

// Register services
builder.Services.AddScoped<ISwiftMessageService, SwiftMessageService>();

var app = builder.Build();

// Initialize the database
var initializer = new DatabaseInitializer(app.Services.GetRequiredService<DatabaseConnectionFactory>());
await initializer.InitializeAsync();

app.UseExceptionHandler("/error");
app.UseHsts();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SwiftAPI v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseRouting();

app.MapControllers();

app.Run();
