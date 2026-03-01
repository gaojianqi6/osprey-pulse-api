using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OspreyPulseAPI.Modules.Identity.Application;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Infrastructure;
using OspreyPulseAPI.Modules.Identity.Infrastructure.Persistence;
using OspreyPulseAPI.Api.GraphQL;
using OspreyPulseAPI.Api.Services;
using OspreyPulseAPI.Modules.Competitions.Application;
using OspreyPulseAPI.Modules.Competitions.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContexts (Infrastructure)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<CompetitionsDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    // Allow "database update" when snapshot matches model; suppress pending-model-changes as error
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// 1b. ESPN NBA HTTP client with 1 rps rate limiting
builder.Services.AddSingleton<EspnRateLimitedHandler>();
builder.Services.AddHttpClient<IEspnNbaClient, EspnNbaHttpClient>(client =>
    {
        client.BaseAddress = new Uri("https://site.api.espn.com/apis/site/v2/sports/basketball/nba/");
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddHttpMessageHandler<EspnRateLimitedHandler>();

// 1c. ESPN NBA ingestion
builder.Services.AddScoped<IEspnNbaIngestionService, EspnNbaIngestionService>();

// 2. Identity abstraction: Application uses IIdentityDbContext, Infrastructure provides IdentityDbContext
builder.Services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());

// 2b. Supabase Auth (backend signup/login)
builder.Services.AddSupabaseAuth(builder.Configuration);

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

// 4b. CORS (temporarily allow any origin for testing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 5. GraphQL (Hot Chocolate)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType(d => d.Name("Mutation"))
    .AddIdentityModule()
    .AddCompetitionsModule();

// 6. Static NBA seed (Channel, League, Seasons)
builder.Services.AddHostedService<NbaDataSeeder>();

// 6b. NBA news: load on startup if missing, then daily at midnight NY
builder.Services.AddHostedService<NbaNewsIngestionHostedService>();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Pipeline: CORS and Auth before endpoints
app.UseCors("AllowAll");
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
