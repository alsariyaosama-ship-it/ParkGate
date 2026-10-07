# AGENTS.md — Rules for any AI agent working on ParkGate

You are implementing **ParkGate**, a Windows Forms desktop app that controls free entry of membership-card holders (صندوق التضامن الاجتماعي and other organizations) to a family fun park, with a configurable re-entry window.

This file holds the **stable rules**. It changes rarely and only with the user's approval.

| File | Role | Who edits |
|---|---|---|
| `AGENTS.md` | Rules (this file) | User approves changes |
| `docs/SPEC.md` | What to build: requirements, business rules (BR-xx), open questions (Qx) | User approves changes |
| `docs/DATABASE.md` | Data model and the atomic check-in algorithm | Agent proposes, user approves |
| `docs/ROADMAP.md` | Phases and tasks (Tx.y) with checkboxes | Agent ticks tasks |
| `docs/DECISIONS.md` | Architecture decisions (ADR-xxx) | Agent appends, user approves |
| `docs/SESSION_HANDOFF.md` | Where we are now, what's next | Agent, every session |

---

## 1. Session protocol (mandatory)

### 1.1 Start of session
1. Read, in order: `AGENTS.md` → `docs/SESSION_HANDOFF.md` → `docs/ROADMAP.md` (current phase) → the SPEC/DATABASE/DECISIONS sections the next task touches.
2. Run `git status` and `git pull --ff-only`. If the tree is dirty or the branch differs from the handoff, report it and stop.
3. If the solution exists: `dotnet build ParkGate.sln -c Debug` and `dotnet test ParkGate.sln --filter "Category!=Integration"`. A red baseline is fixed **first**, before new work.
4. State in 3–6 lines: the task IDs you will do this session and anything blocking.

### 1.2 During the session
- Work **one task (Tx.y) at a time**. Finish it (Definition of Done, §10) and commit before starting the next.
- Stay inside the current phase. Do not start a later phase or a Future feature (F1–F3) without the user's explicit go-ahead.
- If you discover new work, add it to `ROADMAP.md` under the right phase as an unchecked task — do not do it silently.

### 1.3 End of session (also when context is running low)
1. Build and unit tests green; if not, commit only finished work and record the breakage in the handoff.
2. Update `docs/SESSION_HANDOFF.md`: Snapshot, Next session, Blockers, and add one log entry (keep the last 10; delete older ones — git keeps history).
3. Tick finished tasks in `docs/ROADMAP.md`. Append any decision to `docs/DECISIONS.md`.
4. Commit (`docs: update session handoff`) and **push**. Never end a session with unpushed commits.

---

## 2. Technology stack (fixed — changes need an ADR and user approval)

| Concern | Choice |
|---|---|
| Language / runtime | C# (latest), **.NET 10 LTS** |
| UI | **Windows Forms** (`net10.0-windows`), Arabic, full RTL |
| Database | **SQL Server Express** 2022+ on the LAN; LocalDB for development and integration tests |
| ORM | **EF Core 10** (SqlServer provider), code-first migrations |
| DI / config / logging host | `Microsoft.Extensions.Hosting` (Generic Host inside WinForms) |
| Logging | Serilog → rolling file in `%ProgramData%\ParkGate\logs` |
| Validation | FluentValidation (Application layer) |
| Tests | xUnit, Shouldly, NSubstitute (or hand-written fakes) |
| Excel export (Phase 7) | ClosedXML |
| QR (F1 only) | QRCoder |

**Approved packages are only the ones above.** Any other package → ask first.
**Forbidden:** MediatR, AutoMapper, FluentAssertions v8+ (licensing — ADR-010), any commercial UI suite, any package that sends telemetry, `System.Data.SqlClient` (use `Microsoft.Data.SqlClient` via EF).

Package versions live only in `Directory.Packages.props` (Central Package Management). Use the latest **stable** versions compatible with .NET 10; never preview.

---

## 3. Solution structure and dependency rules

```
ParkGate.sln
src/
  ParkGate.Domain/          net10.0          entities, value objects, enums, domain policies. ZERO package/project references.
  ParkGate.Application/     net10.0          use cases, ports (interfaces), DTOs, validators, permissions, Result types. → Domain
  ParkGate.Infrastructure/  net10.0          EF Core DbContext, configurations, migrations, repositories, UoW, password hasher, clock, settings store. → Application, Domain
  ParkGate.WinForms/        net10.0-windows  forms, controls, presenters/view logic, composition root (Program.cs). → Application, Domain; → Infrastructure ONLY for DI registration in the composition root
tests/
  ParkGate.Domain.Tests/
  ParkGate.Application.Tests/
  ParkGate.Infrastructure.Tests/   (Category=Integration, LocalDB)
  ParkGate.ArchitectureTests/      (enforces the rules below by reflection)
docs/   scripts/   .github/
```

**Dependency rule (enforced by ArchitectureTests):**
- Domain references nothing.
- Application references Domain only. No EF Core, no `Microsoft.Data.SqlClient`, no `System.Windows.Forms`, no `System.Drawing`.
- Infrastructure references Application + Domain. No WinForms.
- WinForms never uses `DbContext`, repositories, or EF types directly — only Application services/ports. The single exception is `Program.cs` / `DependencyInjection` registration calling `services.AddInfrastructure(...)`.

Maximum 4 production projects. Do not add a project without an ADR (the future F2 gate host is the planned exception).

---

## 4. Architecture patterns

### 4.1 Domain
- Entities protect their invariants: private setters, factory methods / constructors that validate, behavior methods (`Suspend(reason)`, `RecordGrantedEntry(nowUtc)`), no anemic public setters for rule-bearing fields.
- Value objects: `CardNumber`, `PhoneNumber`, `NationalId` with normalization (BR-01, BR-15, BR-16) and `TryCreate` returning a result.
- Eligibility is a **strategy**: `IEntryEligibilityPolicy.Evaluate(EligibilityContext) → EligibilityDecision`. MVP implementation: `RollingWindowEligibilityPolicy` (BR-03..BR-06). New policies (e.g. calendar-day) are new classes, not `if` branches (Open/Closed).
- Domain never reads the clock. Time is passed in.

### 4.2 Application
- One class per use case or a small cohesive service per module (`CheckInService`, `BeneficiaryService`, `AuthService`, `SettingsService`, `ReportService`). Each public method = one use case.
- Ports defined here, implemented in Infrastructure: `IUnitOfWorkFactory`, `IUnitOfWork`, repositories, `IPasswordHasher`, `IAppSettingsStore`, `IAuditWriter`, `IReportQueries`.
- **Unit of work per use case** (WinForms has no request scope):
  ```csharp
  await using var uow = _uowFactory.Create();      // fresh DbContext from IDbContextFactory
  await uow.BeginTransactionAsync(ct);
  var holder = await uow.Beneficiaries.GetForUpdateAsync(id, ct);   // UPDLOCK
  var nowUtc = await uow.GetDbUtcNowAsync(ct);                       // DB clock (BR-05)
  var decision = _policy.Evaluate(new EligibilityContext(...));
  ...
  await uow.SaveChangesAsync(ct);
  await uow.CommitAsync(ct);
  ```
- **Expected failures return `Result` / `Result<T>`** with a typed `Error` (code + optional data). Exceptions only for unexpected failures (DB down, bugs). Never throw for "card not found" or "cooldown active".
- Authorization: every use case that needs a permission checks `ICurrentUser.Has(Permission.X)` first and returns `Errors.Forbidden` otherwise.
- Validation with FluentValidation validators on input DTOs; validators return error codes, not Arabic text.
- Manual mapping (extension methods or static `ToDto()`), no mapping libraries.
- DTOs are `sealed record`s. Never return entities to the UI.

### 4.3 Infrastructure
- `ParkGateDbContext` registered with `AddDbContextFactory` (ADR-003). **Never** inject a long-lived `DbContext` into forms or singletons.
- One `IEntityTypeConfiguration<T>` per entity, matching `docs/DATABASE.md` exactly (names, lengths, indexes, check constraints).
- Audit columns via a `SaveChangesInterceptor`; audit log rows via `IAuditWriter`.
- Read-heavy screens/reports use `AsNoTracking()` and projections to DTOs.
- Never edit a migration that has been pushed; add a new one. Migration names: `YYYYMMDD_Description` style via `dotnet ef migrations add <Description>`.
- Password hashing: `Rfc2898DeriveBytes.Pbkdf2`, SHA-256, 16-byte salt, 32-byte hash, 600 000 iterations, constant-time comparison, format in DATABASE.md §3.6. Re-hash on login if the iteration count is older.

### 4.4 Presentation (WinForms)
- Forms are thin: event handler → call Application service → render result. **No business rules, SQL, or EF in forms.**
- Forms are created through DI with an `IFormFactory` (forms are `Transient`). No `new SomeForm()` for forms that need services.
- Map `Error` codes → Arabic messages in one place (`ErrorMessages` + `Strings.resx`).
- All event handlers that do I/O are `async void` **only** at the event-handler level, wrap in try/catch → logger + friendly message, disable the triggering controls while busy, use `CancellationToken` for long operations. Never `.Result` / `.Wait()`.
- Global handlers: `Application.ThreadException`, `AppDomain.CurrentDomain.UnhandledException`, `TaskScheduler.UnobservedTaskException` → log + Arabic message.

### 4.5 DI lifetimes
| Type | Lifetime |
|---|---|
| `IDbContextFactory`, `IUnitOfWorkFactory`, `IPasswordHasher`, policies, `ICurrentUser`, `ICurrentWorkstation`, `ISettingsService` (cached, invalidated on save) | Singleton |
| Application services, validators | Transient |
| Forms, user controls | Transient (via `IFormFactory`) |

---

## 5. WinForms, Arabic and RTL rules

1. Every form derives from `RtlFormBase` which sets `RightToLeft = Yes`, `RightToLeftLayout = true`, `AutoScaleMode = Dpi`, the app font (Cairo 10 pt; status banner ≥ 20 pt), and `KeyPreview = true`.
2. High DPI: `ApplicationConfiguration.Initialize()` with `PerMonitorV2`. Test at 100 % and 150 % scaling.
3. Layout with `TableLayoutPanel` / `FlowLayoutPanel` and docking; avoid absolute positions so mirroring is correct.
4. **Designer compatibility:** every form/user control must open in the Visual Studio WinForms designer. Layout code lives in `*.Designer.cs` `InitializeComponent()` only — no logic, no lambdas, no DI there.
5. `DataGridView`: `RightToLeft = Yes`, read-only for lists, column headers Arabic, `AutoGenerateColumns = false`.
6. Message boxes only through a `Notify` helper using `MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign`.
7. All user-visible text in `Resources/Strings.resx` (Arabic). No Arabic string literals in `.cs` files except in `Strings.resx` and designer-generated resources.
8. Display: Western digits 0-9, `dd/MM/yyyy`, `hh:mm tt` with ص/م (`ar-LY` culture with Latin digits). Times shown in park local time converted from UTC.
9. Normalize Arabic-Indic digits in **every** numeric input (card, phone, national ID, counters).
10. Colors: granted `#1B873F`, denied `#C62828`, warning `#B26A00`, neutral `#455A64`; never rely on color alone — banner text always states the result.
11. Keyboard: Enter = search/confirm, F5 = record entry, F9 = override, F2 = new holder, Esc = clear, Ctrl+F = holder search. Tab order set explicitly.

---

## 6. C# coding standards

- `Nullable` enabled, `ImplicitUsings` enabled, file-scoped namespaces, `sealed` by default, `record` for DTOs and value-like types.
- Naming: PascalCase types/methods/properties, `_camelCase` private fields, `I` prefix for interfaces, `Async` suffix for async methods, English identifiers.
- Methods short and single-purpose; classes with one reason to change (SRP). Depend on abstractions at layer boundaries (DIP). Prefer composition over inheritance (except `RtlFormBase`).
- Guard clauses over nested `if`. No magic numbers — constants or settings.
- Use `DateTime` **UTC only** in code paths that touch rules or persistence. `DateTime.Now` / `DateTime.UtcNow` are forbidden in Domain and Application — use the DB clock via the unit of work, or `TimeProvider` where DB time is not required (UI clock display).
- Comments explain *why*, not *what*. XML docs on public Application ports and services.
- Logging: structured (`_logger.LogInformation("Entry granted {BeneficiaryId} {Persons}", ...)`). **Never log** passwords, password hashes, full national IDs, or connection strings.
- Zero compiler warnings in Domain, Application, Infrastructure (warnings are errors there via `Directory.Build.props`). Keep WinForms warning-free too, except designer nullable noise handled with `#nullable disable` at the top of `*.Designer.cs` files.

---

## 7. Data and security rules

- Follow `docs/DATABASE.md` exactly. Schema change = update DATABASE.md + new migration + ask the user if it touches a released table.
- Business time = DB time (BR-05). Store UTC; compute `BusinessDate` with `Park.TimeZoneId`.
- The check-in path must use the locked transaction in DATABASE.md §4. Do not "optimize" it away.
- Parameterized SQL only (`FromSqlInterpolated`, never string concatenation).
- Connection strings: `appsettings.json` contains a LocalDB dev default only. Real connection settings are stored in `%ProgramData%\ParkGate\connection.dat`, protected with DPAPI (`ProtectedData`, `LocalMachine`). Never commit real servers, logins or passwords.
- No default admin password anywhere (ADR-012).

---

## 8. Testing rules

| Layer | What must be tested |
|---|---|
| Domain | Every BR in SPEC §6 that lives in Domain, table-driven (`[Theory]`): cooldown boundary (exact, 1 s before, 1 s after), no previous entry, each status, expiry today/yesterday/tomorrow, inactive org, check order (BR-06), normalization of card/phone/national ID including Arabic-Indic digits. Target 100 % branch coverage of `RollingWindowEligibilityPolicy` and value objects. |
| Application | Each use case: success, each expected error, permission denied, validation failure. Fakes for UoW/repositories, fixed clock. |
| Infrastructure | `[Trait("Category","Integration")]`, LocalDB, fresh database per test class. Mapping round-trip, unique indexes, **parallel check-in concurrency test**, void recalculation. |
| Architecture | Dependency rules in §3. |

- Test naming: `Method_Scenario_ExpectedResult`.
- A bug fix starts with a failing test that reproduces it.
- CI runs everything except `Category=Integration`. Run integration tests locally before finishing any Phase 2+ task that touches Infrastructure.

---

## 9. Git and GitHub rules

- Remote is GitHub. `main` is always buildable.
- **Branch per phase:** `phase/00-foundation`, `phase/01-domain`, … Small fixes: `fix/<short-name>`.
- **Commit after every finished task**, Conventional Commits in English, task ID in the subject:
  `feat(domain): add rolling window eligibility policy [T1.4]`
  Types: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`, `build`, `ci`.
- **Push at least at the end of every session.**
- At phase end: open a Pull Request to `main` using `.github/pull_request_template.md`. The **user** reviews and merges. The agent does not merge its own PR unless the user says so.
- Never: force-push, rewrite pushed history, commit to `main` directly (except the initial docs commit), commit secrets, `bin/`, `obj/`, `.vs/`, `*.user`, `.mdf/.ldf/.bak`, logs, or `appsettings.Local.json`.
- After the user merges a phase PR, tag `v0.<phase>.0` if the user asks.

---

## 10. Definition of Done (every task)

- [ ] Meets the referenced FR/BR in SPEC and the DATABASE model.
- [ ] Builds with zero warnings in Domain/Application/Infrastructure.
- [ ] New/changed logic covered by tests; all unit tests pass; integration tests pass locally if Infrastructure was touched.
- [ ] Architecture tests pass.
- [ ] UI text in `Strings.resx`, RTL verified, keyboard flow works (UI tasks).
- [ ] No rule in this file violated.
- [ ] `ROADMAP.md` task ticked; `DECISIONS.md` updated if a decision was made.
- [ ] Committed with Conventional Commit + task ID.

---

## 11. Stop and ask the user when

- A requirement is ambiguous **and** the answer changes the data model, a business rule, or something the client sees.
- An open question in SPEC §11 marked as blocking the current task is still open.
- You want a package not in §2, a new project, or a change to any ADR.
- A change would alter a released migration or delete/rename data.
- A task would take the work outside the current phase.
- Two documents contradict each other.

Ask concisely: the question, 2–3 options, your recommendation. Continue with non-blocked tasks meanwhile.

---

## 12. Communication with the user

- The user is **Osama** (owner/developer). Reply to him in **Arabic**, short and direct, with clear reasoning.
- Code, identifiers, comments, commit messages, and these docs: **English**. UI text: **Arabic**.
- Keep Arabic and English on separate lines in terminal summaries; do not mix them inside one sentence when avoidable.
- End-of-session summary: what was done (task IDs), what's next, anything needing his decision.
