namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Post type: user-created or ingested news (e.g. from ESPN).
/// Stored as string in DB for flexibility.
/// </summary>
public enum PostType
{
    User,
    News
}
