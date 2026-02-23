Studio URL: http://localhost:54323 (This is where you see your data).
DB URL: postgresql://postgres:postgres@127.0.0.1:54322/postgres (This is what you put in your .net connection string).

### Apply EF Core migrations (Identity + Competitions)

From the repo root, with Supabase (or PostgreSQL) running and connection string in appsettings:

```bash
# Identity (identity.Users, etc.)
dotnet ef database update \
  --project src/Modules/Identity/OspreyPulseAPI.Modules.Identity.Infrastructure/OspreyPulseAPI.Modules.Identity.Infrastructure.csproj \
  --startup-project src/Host/OspreyPulseAPI.Api/OspreyPulseAPI.Api.csproj \
  --context IdentityDbContext

# Competitions (competitions schema)
dotnet ef database update \
  --project src/Modules/Competitions/OspreyPulseAPI.Modules.Competitions.Infrastructure/OspreyPulseAPI.Modules.Competitions.Infrastructure.csproj \
  --startup-project src/Host/OspreyPulseAPI.Api/OspreyPulseAPI.Api.csproj \
  --context CompetitionsDbContext
```

supabase start
[+] Pulling 145/146
 ✔ postgres-meta 
 ✔ imgproxy                                                              53.5s
 ✔ vector                                                               52.9s
 ✔ studio                                                                99.8s
 ✔ logflare                                                             84.2s
 ✔ mailpit                                                               21.3s
 ✔ postgrest                                                              56.7s
 ✔ postgres                                                             159.7s
 ✔ gotrue                                                                20.5s
 ✔ storage-api                                                          84.2s
 ✔ edge-runtime                                                          77.1s
 ✔ realtime                                                               67.7s
 ✔ kong                                                                 27.7s

Starting database...
Initialising schema...
Started supabase local development setup.

╭──────────────────────────────────────╮
│ 🔧 Development Tools                 │
├─────────┬────────────────────────────┤
│ Studio  │ http://127.0.0.1:54323     │
│ Mailpit │ http://127.0.0.1:54324     │
│ MCP     │ http://127.0.0.1:54321/mcp │
╰─────────┴────────────────────────────╯

╭──────────────────────────────────────────────────────╮
│ 🌐 APIs                                              │
├────────────────┬─────────────────────────────────────┤
│ Project URL    │ http://127.0.0.1:54321              │
│ REST           │ http://127.0.0.1:54321/rest/v1      │
│ GraphQL        │ http://127.0.0.1:54321/graphql/v1   │
│ Edge Functions │ http://127.0.0.1:54321/functions/v1 │
╰────────────────┴─────────────────────────────────────╯

╭───────────────────────────────────────────────────────────────╮
│ ⛁ Database                                                    │
├─────┬─────────────────────────────────────────────────────────┤
│ URL │ postgresql://postgres:postgres@127.0.0.1:54322/postgres │
╰─────┴─────────────────────────────────────────────────────────╯
