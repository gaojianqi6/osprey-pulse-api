namespace OspreyPulseAPI.Modules.Identity.Domain;

/// <summary>
/// App user. Maps to: users
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    /// <summary>Economy balance. Default 1000.</summary>
    public long OspreyPoints { get; set; } = 1000;
    public string? AvatarUrl { get; set; }
    public bool IsAdmin { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
