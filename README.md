# ParkGate

نظام تسجيل ومراقبة دخول حاملي بطاقات صندوق التضامن الاجتماعي (وجهات أخرى لاحقاً) إلى منتزه المرح العائلي.

Windows desktop app that registers membership-card holders, records their free entries with companions, and enforces a configurable re-entry window (default 12 hours).

## Stack
C# · .NET 10 LTS · Windows Forms (Arabic, RTL) · SQL Server Express · EF Core · Clean Architecture (4 projects) · DI · xUnit

## Documentation
| File | Content |
|---|---|
| [AGENTS.md](AGENTS.md) | Rules for AI agents and contributors |
| [docs/SPEC.md](docs/SPEC.md) | Requirements, business rules, scenarios, future features, open questions |
| [docs/DATABASE.md](docs/DATABASE.md) | Data model and atomic check-in algorithm |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Phases and tasks |
| [docs/DECISIONS.md](docs/DECISIONS.md) | Architecture decisions |
| [docs/SESSION_HANDOFF.md](docs/SESSION_HANDOFF.md) | Current state and next steps |

## Development setup
1. Visual Studio 2026 (or any version supporting the .NET 10 SDK) with the ".NET desktop development" workload, or the .NET 10 SDK.
2. SQL Server LocalDB (installed with Visual Studio) or SQL Server Express.
3. `dotnet build ParkGate.sln`
4. Apply migrations to the dev database:
   `dotnet ef database update -p src/ParkGate.Infrastructure -s src/ParkGate.WinForms`
5. Run `ParkGate.WinForms`. The first run creates the admin account.

Tests: `dotnet test ParkGate.sln --filter "Category!=Integration"` (unit) · `dotnet test ParkGate.sln` (all, needs LocalDB).

## Working with an AI agent
Point the agent at the repository root. It reads `AGENTS.md` (Claude Code reads `CLAUDE.md`, which imports it), then `docs/SESSION_HANDOFF.md`, and continues from there. Each session ends with the handoff updated and pushed.

---
© شركة الركاز المتين لتقنية المعلومات
