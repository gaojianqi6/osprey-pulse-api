using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Infrastructure.Supabase;

namespace OspreyPulseAPI.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSupabaseAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Supabase");
        var url = section["Url"];
        var serviceRoleKey = section["ServiceRoleKey"];

        if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(serviceRoleKey))
        {
            services.AddScoped<global::Supabase.Client>(_ =>
                new global::Supabase.Client(
                    url,
                    serviceRoleKey,
                    new global::Supabase.SupabaseOptions
                    {
                        AutoConnectRealtime = false,
                        AutoRefreshToken = true
                    }));

            services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();
        }

        return services;
    }
}
