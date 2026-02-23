namespace OspreyPulseAPI.Modules.Identity.Application.Abstractions;

/// <summary>
/// Supabase Auth operations. Implemented by Infrastructure; used by Application handlers.
/// Keeps Application independent of the Supabase SDK.
/// </summary>
public interface ISupabaseAuthService
{
    /// <summary>
    /// Sign up a user in Supabase Auth. Returns the new user's Id (Guid).
    /// </summary>
    Task<Guid> SignUpAsync(string email, string password, IReadOnlyDictionary<string, string>? userMetadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sign in with email and password. Returns the access token.
    /// </summary>
    Task<string> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
}
