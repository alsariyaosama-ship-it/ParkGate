# Session Handoff

> **Read at the start of every session. Update at the end of every session** (AGENTS.md §1).
> Keep "Snapshot" and "Next session" accurate — they are overwritten each time. The log keeps the last 10 sessions only.

---

## Snapshot

| Field | Value |
|---|---|
| Last updated | 2026-10-05 |
| Last session | S03 — Phase 2 Persistence |
| Current phase | Phase 2 — Persistence (completed, pending merge) |
| Working branch | `phase/02-persistence` |
| Build | Clean (0 warnings, 0 errors, Release & Debug) |
| Unit tests | 195 passed (151 domain + 10 architecture + 1 app + 33 infra) |
| Integration tests | 14 passed on LocalDB (concurrency 20 runs, round-trip, indexes, void) |
| CI | Ready for Phase 2 PR |
| Last commit | `build(infra): add script to generate efbundle.exe migration bundle [T2.7]` |

## State of the work

**Done**
- Merged Phase 1 (Domain) PR #2 into `main`.
- T2.1 `ParkGateDbContext` + 9 entity configurations matching DATABASE.md (lengths, indexes, filtered indexes, check constraints, rowversion).
- T2.2 Audit-columns `SaveChangesInterceptor` with `DateTimeUtcConverter`.
- T2.3 Migration `InitialCreate` with seed (DATABASE.md §5 & §6, no users) applied on LocalDB.
- T2.4 `UnitOfWorkFactory`, `UnitOfWork` (transaction, `GetDbUtcNowAsync`), repositories incl. `GetForUpdateAsync` with `UPDLOCK, ROWLOCK`.
- T2.5 `Pbkdf2PasswordHasher`, `AppSettingsStore`, `AuditWriter`, DPAPI `ConnectionSettingsStore`.
- T2.6 Integration tests (LocalDB): mapping round-trip, unique indexes, **parallel check-in → exactly one granted (20 consecutive runs)**, void recalculation.
- T2.7 `scripts/build-migration-bundle.ps1` producing `efbundle.exe` for deployment.

**In progress**
- Phase 2 PR to `main`.

**Not started**
- Phase 3: Application (T3.1 .. T3.8).

## Next session — start here

1. User reviews and merges Phase 2 PR (`phase/02-persistence`) into `main`.
2. Checkout `main` and pull (`git checkout main; git pull --ff-only`).
3. Create branch `phase/03-application` from `main`.
4. Start T3.1: `Result`, `Result<T>`, `Error`, error code catalog.
5. Continue with T3.2 `ICurrentUser`, `ICurrentWorkstation`, `Permission` enum and role matrix.

## Blockers / waiting on the user

- User review and merge of Phase 2 Pull Request into `main`.

## Known issues / tech debt

- None.

## Environment notes

- Dev DB: LocalDB `(localdb)\MSSQLLocalDB`, database `ParkGate_Dev` (default in `appsettings.json`).
- Integration tests create and drop `ParkGate_Test_<guid>` databases on LocalDB.

---

## Session log (newest first, max 10)

### S03 — 2026-10-05 — Phase 2 Persistence
- Tasks: T2.1, T2.2, T2.3, T2.4, T2.5, T2.6, T2.7 completed (all Phase 2 tasks).
- Commits: see branch `phase/02-persistence`.
- Decisions: Global DateTime UTC convention configured in DbContext; parallel check-in concurrency verified with UPDLOCK/ROWLOCK across 20 consecutive runs; efbundle.exe script verified.
- Test count: 209 total passed (195 unit/architecture + 14 LocalDB integration tests).

### S02 — 2026-10-04 — Phase 1 Domain
- Tasks: T1.1, T1.2, T1.3, T1.4, T1.5 completed (all Phase 1 tasks).
- Commits: see branch `phase/01-domain`, merged into `main` via PR #2.
- Decisions: Value objects implemented with 100% branch coverage; eligibility policy verified against boundary conditions and check orders; Domain project maintained with zero package/project references.
- Test count: 163 total passed (151 Domain + 10 Architecture + 1 Application + 1 Infrastructure).

### S01 — 2026-10-04 — Phase 0 Foundation
- Tasks: T0.1, T0.2, T0.3, T0.4, T0.5, T0.6 completed (all Phase 0 tasks).
- Commits: see branch `phase/00-foundation`, merged into `main` via PR #1.
- Decisions: Architecture tests implemented via reflection covering assembly references and type constraints; `Strings.Designer.cs` set to `#nullable enable` for zero-warning build.
- GitHub Actions CI verified green on Windows runner.

### S00 — 2026-10-04 — Planning
- Created SPEC, DATABASE, ROADMAP, DECISIONS, AGENTS, CLAUDE, README, CI workflow, PR template, `.gitignore`, `.gitattributes`, `.editorconfig`, `Directory.Build.props`.
- Decisions: ADR-001 … ADR-016 — **approved by the user**.
- User answers: Q2 holder counted separately · Q3 rolling window in hours · Q8 no financial claims, per-organization statistics important · Q11 gate deferred.
- No code yet.
