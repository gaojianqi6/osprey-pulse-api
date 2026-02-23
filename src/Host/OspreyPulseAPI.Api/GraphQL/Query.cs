namespace OspreyPulseAPI.Api.GraphQL;

/// <summary>
/// Root Query type. Hot Chocolate requires at least one field; this placeholder keeps the schema valid.
/// </summary>
public class Query
{
    public string Health => "ok";
}
