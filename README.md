# Project: Osprey Pulse

**Tagline:** *The Heartbeat of the Game.*

## 1. Concept & Mission

Osprey Pulse is a premium sports community and data platform. It bridges the gap between cold statistics and fan passion through a **User-Generated Rating System**. Inspired by the "Hupu" model, it allows fans to rate players, coaches, and referees on a 1–10 scale, creating a "Social Box Score" that defines the conversation.

**Osprey Pulse** is a high-performance "Social Sports Data" platform built for the New Zealand and global markets. It transforms the sports viewing experience from passive to active by integrating a **Wisdom of the Crowd Rating System**. After every game, fans "Pulse" the performance of players, coaches, and refs on a 1–10 tactile scale, creating a live, community-driven "Social Box Score."

## 2. Product Roadmap

- **Launch:** **NBA** (Daily engagement engine).
- **Expansion:** **Rugby** (All Blacks/Super Rugby) & **NBL** (Breakers).
- **Diversification:** **League of Legends** (Esports).
- **Global:** **Football** (Premier League/Champions League).

---

## 3. This project: osprey-backend (The Core Engine)

- **Framework:** **.NET 10** (Modular Monolith architecture).
- **Primary API:** **Hot Chocolate GraphQL** (High-performance data fetching).
- **Admin API:** **Minimal APIs / REST** (Management and internal webhooks).
- **Database:** **PostgreSQL** (Relational) + **Redis** (Live session cache).
- **Performance:** **.NET 10 HybridCache**. This new standard automatically manages L1 (In-memory) and L2 (Redis) caching, ensuring score updates reach users in sub-10ms.
- **Architecture:** Modular Monolith with **Interface-based League Providers** (add new sports by just adding a new C# module).

---
Flow: GraphQL → IdentityMutations.RegisterUserAsync → MediatR → RegisterUserHandler → IIdentityDbContext → PostgreSQL.

### Test
1. dotnet test tests/OspreyPulseAPI.Tests.Unit/OspreyPulseAPI.Tests.Unit.csproj

### Prepare Step (Run Migrations)
Now, we run the "Code-First" commands to actually create the tables in your Supabase. Run these in your terminal:

**1. Create Identity Tables:**
```bash
dotnet ef migrations add InitialIdentity --context IdentityDbContext --project src/Modules/Identity/OspreyPulseAPI.Modules.Identity.Infrastructure --startup-project src/Host/OspreyPulseAPI.Api
```

**2. Create Competitions Tables:**
```bash
dotnet ef migrations add InitialCompetitions --context CompetitionsDbContext --project src/Modules/Competitions/OspreyPulseAPI.Modules.Competitions.Infrastructure --startup-project src/Host/OspreyPulseAPI.Api
```

**3. Push to Database:**
```bash
dotnet ef database update --context IdentityDbContext --startup-project src/Host/OspreyPulseAPI.Api
dotnet ef database update --context CompetitionsDbContext --startup-project src/Host/OspreyPulseAPI.Api
```

**Once you do this, refresh your Supabase Studio (localhost:54323). You should see your new schemas and tables!** Let me know if you run into any errors during the `ef migrations` step.