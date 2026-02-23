using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OspreyPulseAPI.Modules.Identity.Application;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Infrastructure.Persistence;
using OspreyPulseAPI.Api.GraphQL;
using OspreyPulseAPI.Modules.Competitions.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContexts (Infrastructure)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<CompetitionsDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Identity abstraction: Application uses IIdentityDbContext, Infrastructure provides IdentityDbContext
builder.Services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());

// 3. JWT & Security (Supabase Auth)
var supabaseSection = builder.Configuration.GetSection("Supabase");
var authority = supabaseSection["Authority"];
var jwtSecret = supabaseSection["JwtSecret"];
var audience = supabaseSection["Audience"] ?? "authenticated";

if (!string.IsNullOrEmpty(authority) && !string.IsNullOrEmpty(jwtSecret))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authority;

            // REQUIRED FOR LOCAL DEV:
            options.RequireHttpsMetadata = false; 
        
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateAudience = true,
                ValidAudience = audience
            };
        });
    builder.Services.AddAuthorization();
}

// 4. MediatR (finds Handlers via AssemblyReference)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
});

// 5. GraphQL (Hot Chocolate)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType(d => d.Name("Mutation"))
    .AddIdentityModule();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Pipeline: Auth before endpoints
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGraphQL();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
