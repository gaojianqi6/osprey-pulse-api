using OspreyPulseAPI.Modules.Identity.Application.Abstractions;

namespace OspreyPulseAPI.Modules.Identity.Infrastructure.Supabase;

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly global::Supabase.Client _client;
    private bool _initialized;

    public SupabaseAuthService(global::Supabase.Client client) => _client = client;

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized) return;
        await _client.InitializeAsync();
        _initialized = true;
    }

    public async Task<Guid> SignUpAsync(string email, string password, IReadOnlyDictionary<string, string>? userMetadata, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var options = new global::Supabase.Gotrue.SignUpOptions
        {
            Data = userMetadata != null
                ? userMetadata.ToDictionary(k => k.Key, v => (object)v.Value)
                : null
        };

        var authResponse = await _client.Auth.SignUp(email, password, options);
        if (authResponse?.User == null)
            throw new InvalidOperationException("Auth registration failed.");

        return Guid.Parse(authResponse.User.Id);
    }

    public async Task<string> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var session = await _client.Auth.SignIn(email, password);
        if (session?.AccessToken == null)
            throw new InvalidOperationException("Invalid credentials.");

        return session.AccessToken;
    }
}
