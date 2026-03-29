using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using EcommerceAI.Models;
using EcommerceAI.API.Middleware;
using EcommerceAI.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        b => b.MigrationsAssembly("EcommerceAI.API")));

// ─── DI: Services & Repositories ─────────────────────────
builder.Services.AddApplicationServices();

// ─── FluentValidation ────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMemoryCache();

// ─── JWT Authentication ──────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForDevelopmentOnly12345!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EcommerceAI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EcommerceAI",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// ─── Controllers & Swagger ──────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcommerceAI API",
        Version = "v1",
        Description = "AI-Powered E-Commerce Backend with Ollama Integration"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── CORS ────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─── HttpClient for Chatbot ─────────────────────────────
builder.Services.AddHttpClient<EcommerceAI.Services.Interfaces.IChatbotService,
    EcommerceAI.Services.Implementations.ChatbotService>();

builder.Services.AddHttpClient<EcommerceAI.Services.Interfaces.IRecommendationService,
    EcommerceAI.Services.Implementations.RecommendationService>();

builder.Services.AddHttpClient<EcommerceAI.Services.Interfaces.IUserActivityService,
    EcommerceAI.Services.Implementations.UserActivityService>();

builder.Services.AddHttpClient<EcommerceAI.Services.Interfaces.ISearchService,
    EcommerceAI.Services.Implementations.SearchService>();

var app = builder.Build();

// ─── Middleware Pipeline ────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceAI API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ─── Auto-migrate & AI Config Logic ────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment())
    {
        db.Database.EnsureCreated();
    }

    // AI Configuration Logging
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var aiProvider = config["AI:Provider"] ?? "Ollama";
    Console.WriteLine("\n" + new string('=', 40));
    Console.WriteLine($"🚀 AI Provider: {aiProvider}");
    
    if (aiProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"📦 Ollama Model: {config["AI:OllamaModel"] ?? "llama3.2:1b"}");
        Console.WriteLine($"🔗 Ollama URL: {config["AI:OllamaBaseUrl"] ?? "http://localhost:11434"}");
    }
    else if (aiProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
    {
        var hasKey = !string.IsNullOrEmpty(config["AI:GeminiApiKey"]);
        Console.WriteLine($"🔑 Gemini API Key: {(hasKey ? "Configured ✔" : "MISSING ❌")}");
    }
    Console.WriteLine(new string('=', 40) + "\n");
}

app.Run();
