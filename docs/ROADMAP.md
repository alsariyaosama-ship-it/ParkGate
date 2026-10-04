# ParkGate — Roadmap

Rules: work phases in order, one task at a time, tick `[x]` when the task meets the Definition of Done (AGENTS.md §10). Each phase lives on its own branch and ends with a PR to `main`. Add discovered work as new unchecked tasks — never do it silently.

Legend: `[ ]` todo · `[x]` done · `[~]` in progress · `[!]` blocked (reason in SESSION_HANDOFF.md)

---

## Phase 0 — Foundation · branch `phase/00-foundation`
- [x] T0.1 Create `ParkGate.sln` (`dotnet new sln -n ParkGate --format sln`) and the 4 src + 4 test projects from AGENTS.md §3 with correct target frameworks.
- [x] T0.2 Project references per the dependency rule. Add `Directory.Packages.props` (Central Package Management, latest stable versions of approved packages). Confirm `Directory.Build.props` applies.
- [x] T0.3 `ParkGate.ArchitectureTests`: reflection tests enforcing AGENTS.md §3 (referenced assemblies per layer).
- [x] T0.4 WinForms composition root: Generic Host, `appsettings.json` (LocalDB dev default), Serilog file sink to `%ProgramData%\ParkGate\logs`, global exception handlers, `IFormFactory`, `PerMonitorV2` DPI.
- [x] T0.5 `RtlFormBase`, `Notify` helper, `Strings.resx` (ar), empty `MainForm` titled from resources. App starts and shows an RTL window.
- [x] T0.6 CI green on GitHub Actions (`.github/workflows/build.yml`).

**Done when:** solution builds with zero warnings, architecture tests pass, CI green, app opens an Arabic RTL window.

## Phase 1 — Domain · branch `phase/01-domain`
- [x] T1.1 Enums from DATABASE.md §2 with explicit numeric values.
- [x] T1.2 Value objects `CardNumber`, `PhoneNumber`, `NationalId` with normalization (BR-01, BR-15, BR-16) incl. Arabic-Indic/Persian digits.
- [x] T1.3 Entities `Organization`, `Beneficiary`, `AgeCategory`, `EntryAttempt` (+ companions), `User`, `Workstation`, `AppSetting`, `AuditLog` with invariants and behavior methods.
- [x] T1.4 `IEntryEligibilityPolicy`, `EligibilityContext`, `EligibilityDecision`, `RollingWindowEligibilityPolicy` (BR-02..BR-06) + override eligibility rules (BR-11).
- [x] T1.5 Domain tests per AGENTS.md §8 (table-driven boundaries).

**Done when:** all Domain BRs covered by passing tests; Domain has zero references.

## Phase 2 — Persistence · branch `phase/02-persistence`
- [x] T2.1 `ParkGateDbContext` + one configuration per entity matching DATABASE.md (lengths, indexes, filtered indexes, check constraints, rowversion).
- [x] T2.2 Audit-columns `SaveChangesInterceptor`.
- [x] T2.3 Migration `InitialCreate` with seed (DATABASE.md §6, no users).
- [x] T2.4 `UnitOfWorkFactory`, `UnitOfWork` (transaction, `GetDbUtcNowAsync`), repositories incl. `GetForUpdateAsync` with `UPDLOCK, ROWLOCK`.
- [ ] T2.5 `Pbkdf2PasswordHasher`, `AppSettingsStore`, `AuditWriter`, DPAPI `ConnectionSettingsStore`.
- [ ] T2.6 Integration tests (LocalDB): mapping round-trip, unique indexes, **parallel check-in → exactly one granted**, void recalculation.
- [ ] T2.7 `scripts/build-migration-bundle.ps1` producing `efbundle.exe` for deployment.

**Done when:** DB created from migrations on LocalDB; integration tests pass locally; concurrency test passes 20 consecutive runs.

## Phase 3 — Application · branch `phase/03-application`
- [ ] T3.1 `Result`, `Result<T>`, `Error`, error code catalog.
- [ ] T3.2 `ICurrentUser`, `ICurrentWorkstation`, `Permission` enum and role matrix (SPEC §4).
- [ ] T3.3 `AuthService`: first-run admin creation, login with lockout (BR-18), change password, must-change flow; workstation auto-registration.
- [ ] T3.4 `BeneficiaryService`: lookup by card (multi-org), eligibility preview, register, update, suspend/reactivate/block, search, history.
- [ ] T3.5 `CheckInService`: record entry (atomic, BR-07..BR-12), log denial with dedupe (BR-10), override (BR-11), void (BR-13).
- [ ] T3.6 `SettingsService` (typed, cached), organizations, age categories, users, workstations services + validators.
- [ ] T3.7 Audit calls for every BR-17 action.
- [ ] T3.8 Application tests per AGENTS.md §8.

**Done when:** every FR-CHK/FR-BEN/FR-AUTH use case exists and is tested without a UI.

## Phase 4 — UI shell · branch `phase/04-ui-shell`
- [ ] T4.1 Startup sequence: DB connectivity → connection dialog (DPAPI) → pending-migrations block → first-run wizard → login → shell.
- [ ] T4.2 Main shell: menu by permission, status bar (user, workstation, server time, connection state).
- [ ] T4.3 Shared UI kit: `StatusBanner`, `CounterInput` (− value +), busy indicator, `ErrorMessages` mapping, keyboard shortcut handling.
- [ ] T4.4 Change-password and re-authentication (supervisor) dialogs.

**Done when:** a user can install on LocalDB, create the admin, log in, and see the shell in RTL at 100 % and 150 % DPI.

## Phase 5 — Check-in desk (core) · branch `phase/05-checkin`
- [ ] T5.1 Check-in screen layout per SPEC §5.3.
- [ ] T5.2 Flows 5.1 and A1–A7 wired to `CheckInService` / `BeneficiaryService`.
- [ ] T5.3 Today grid + today totals by category for this workstation.
- [ ] T5.4 Sound on denial, scanner-friendly card box, full keyboard flow.
- [ ] T5.5 `docs/TEST_SCRIPT.md`: manual test script (Arabic steps) covering every flow incl. two-desk concurrency.

**Done when:** the user walks through TEST_SCRIPT.md on two PCs (or two app instances) without defects. → **First demo to the client.**

## Phase 6 — Management screens · branch `phase/06-management`
- [ ] T6.1 Holders: search, details + history, edit, status changes.
- [ ] T6.2 Void entry (from today grid and history).
- [ ] T6.3 Organizations, age categories, workstations.
- [ ] T6.4 Users (create, role, deactivate, reset password).
- [ ] T6.5 Settings screen (cooldown in hours, group size, sound, prefill, park name).

## Phase 7 — Reports · branch `phase/07-reports`
- [ ] T7.0 Show the user the list of statistics/report columns before building (Q8 answered: statistics only, no financial claims).
- [ ] T7.1 Report queries (`IReportQueries`, projections on `BusinessDate`).
- [ ] T7.2 Reports FR-RPT-01..06 with filters.
- [ ] T7.3 Excel export (ClosedXML) and print preview.

## Phase 8 — Hardening and deployment · branch `phase/08-deployment`
- [ ] T8.1 `docs/DEPLOYMENT.md`: SQL Express install, TCP/IP, firewall, SQL logins (app vs migration), migration bundle, client install, connection setup.
- [ ] T8.2 `scripts/backup.ps1` + Task Scheduler instructions; Admin "backup now".
- [ ] T8.3 Publish profile (win-x64) and installer script (Inno Setup).
- [ ] T8.4 `docs/USER_GUIDE_AR.md` — Arabic user guide for operators and supervisors.
- [ ] T8.5 Performance check: lookup < 1 s with 100 000 holders and 1 000 000 attempts (generated data).

**Done when:** installed on the client LAN → tag `v1.0.0`.

---

## Future (do not start without explicit approval)
- **F1 QR credentials** — `AccessCredentials` table, token issue/revoke, QR card printing, `QrDesk` channel. (SPEC §9 F1)
- **F2 Electronic gate** — deferred by the client; decide Q11 when scheduled; `IGateController`; `ParkGate.GateService` host. (SPEC §9 F2)
- **F3 Cloud read-only** — decide Q12 first; one-way sync worker. (SPEC §9 F3)
