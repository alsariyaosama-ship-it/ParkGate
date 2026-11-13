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
- [x] T2.5 `Pbkdf2PasswordHasher`, `AppSettingsStore`, `AuditWriter`, DPAPI `ConnectionSettingsStore`.
- [x] T2.6 Integration tests (LocalDB): mapping round-trip, unique indexes, **parallel check-in → exactly one granted**, void recalculation.
- [x] T2.7 `scripts/build-migration-bundle.ps1` producing `efbundle.exe` for deployment.

**Done when:** DB created from migrations on LocalDB; integration tests pass locally; concurrency test passes 20 consecutive runs.

## Phase 3 — Application · branch `phase/03-application`
- [x] T3.1 `Result`, `Result<T>`, `Error`, error code catalog.
- [x] T3.2 `ICurrentUser`, `ICurrentWorkstation`, `Permission` enum and role matrix (SPEC §4).
- [x] T3.3 `AuthService`: first-run admin creation, login with lockout (BR-18), change password, must-change flow; workstation auto-registration.
- [x] T3.4 `BeneficiaryService`: lookup by card (multi-org), eligibility preview, register, update, suspend/reactivate/block, search, history.
- [x] T3.5 `CheckInService`: record entry (atomic, BR-07..BR-12), log denial with dedupe (BR-10), override (BR-11), void (BR-13).
- [x] T3.6 `SettingsService` (typed, cached), organizations, age categories, users, workstations services + validators.
- [x] T3.7 Audit calls for every BR-17 action.
- [x] T3.8 Application tests per AGENTS.md §8.

**Done when:** every FR-CHK/FR-BEN/FR-AUTH use case exists and is tested without a UI.

## Phase 4 — UI shell · branch `phase/04-ui-shell`
- [x] T4.1 Startup sequence: DB connectivity → connection dialog (DPAPI) → pending-migrations block → first-run wizard → login → shell.
- [x] T4.2 Main shell: menu by permission, status bar (user, workstation, server time, connection state).
- [x] T4.3 Shared UI kit: `StatusBanner`, `CounterInput` (− value +), busy indicator, `ErrorMessages` mapping, keyboard shortcut handling.
- [x] T4.4 Change-password and re-authentication (supervisor) dialogs.

**Done when:** a user can install on LocalDB, create the admin, log in, and see the shell in RTL at 100 % and 150 % DPI.

## Phase 5 — Check-in desk (core) · branch `phase/05-checkin`
- [x] T5.1 Check-in screen layout per SPEC §5.3.
- [x] T5.2 Flows 5.1 and A1–A7 wired to `CheckInService` / `BeneficiaryService`.
- [x] T5.3 Today grid + today totals by category for this workstation.
- [x] T5.4 Sound on denial, scanner-friendly card box, full keyboard flow.
- [x] T5.5 `docs/TEST_SCRIPT.md`: manual test script (Arabic steps) covering every flow incl. two-desk concurrency.

**Done when:** the user walks through TEST_SCRIPT.md on two PCs (or two app instances) without defects. → **First demo to the client.**

## Phase 6 — Management screens · branch `phase/06-management`
- [x] T6.1 Holders: search, details + history, edit, status changes.
- [x] T6.2 Void entry (from today grid and history).
- [x] T6.3 Organizations, age categories, workstations.
- [x] T6.4 Users (create, role, deactivate, reset password).
- [x] T6.5 Settings screen (cooldown in hours, group size, sound, prefill, park name).

## Phase 7 — Reports · branch `phase/07-reports`
- [x] T7.0 Show the user the list of statistics/report columns before building (Q8 answered: statistics only, no financial claims).
- [x] T7.1 Report queries (`IReportQueries`, projections on `BusinessDate`).
- [x] T7.2 Reports FR-RPT-01..06 with filters.
- [x] T7.3 Excel export (ClosedXML) and print preview.

## Phase 8 — Hardening and deployment · branch `phase/08-deployment`
- [x] T8.1 `docs/DEPLOYMENT.md`: SQL Express install, TCP/IP, firewall, SQL logins (app vs migration), migration bundle, client install, connection setup.
- [x] T8.2 `scripts/backup.ps1` + Task Scheduler instructions; Admin "backup now".
- [x] T8.3 Publish profile (win-x64) and installer script (Inno Setup).
- [x] T8.4 `docs/USER_GUIDE_AR.md` — Arabic user guide for operators and supervisors.
- [x] T8.5 Performance check: lookup < 1 s with 100 000 holders and 1 000 000 attempts (generated data).

**Done when:** installed on the client LAN → tag `v1.0.0`.

---

## Phase F1 — QR Credentials
- [x] TF1.1 Integrate EntryCard source files into Domain, Application, and Infrastructure.
- [x] TF1.2 Add CNG keystore and QRCoder for token signing and generation.
- [x] TF1.3 Add database migration script `001_EntryCards.sql`.
- [x] TF1.4 Ensure 100% tests pass and dependency rules are respected.

**Done when:** QR token generation, signing, and verification are functionally complete and passing all test cases.

---

## Phase F3 — Cloud Sync Agent · branch `phase/f3-sync-agent`
- [x] TF3.1 Architecture & ADR: register ADR-018, update package props, update solution project count rule in architecture tests.
- [x] TF3.2 Domain & Persistence: define `SyncOutbox` entity, EF Core configuration, migration `AddSyncOutboxTable`, `ISyncOutboxRepository`.
- [x] TF3.3 Transactional Outbox Capture: capture `SyncableTypes` mutations inside DbContext SaveChanges transaction.
- [x] TF3.4 Sync Engine: implement `OutboxDeduplicator` (Last-Write-Wins), `PayloadEncryptionService` (AES), `CloudPushService`.
- [x] TF3.5 Standalone Worker Service: create `ParkGate.SyncAgent` project, `OutboxProcessorWorker`, `HeartbeatWorker`, DPAPI connection provider, Serilog logging.
- [x] TF3.6 Deployment & Hardening: add Windows Service install/uninstall scripts, E2E tests, zero-warning verification.

**Done when:** Standalone SyncAgent runs, processes outbox batches, pushes to cloud endpoint, prunes successfully synced rows, and all unit/architecture tests pass with zero warnings.

---

## Phase F4 — Cloud Web App & Dashboard (Laravel + Inertia) · branch `phase/f4-cloud-dashboard`
- [x] TF4.1 Scaffold Laravel application with Breeze (Inertia.js + Vue 3) in `web/` directory.
- [x] TF4.2 Design and migrate database schema matching sync entity model (Organizations, AgeCategories, Beneficiaries, EntryAttempts, Companions, SyncBatches).
- [x] TF4.3 Implement Sync API endpoint (`POST /api/sync/push`) with API Key authentication, AES-256 decryption, and atomic batch ingestion.
- [x] TF4.4 Implement Heartbeat API endpoint (`POST /api/sync/heartbeat`) and park status indicator.
- [x] TF4.5 Implement Web Dashboard (Arabic RTL, Live Stats, Today's Admissions, Organizations Summary, Charts).
- [x] TF4.6 Implement Cloud Reports and Beneficiary Search views with Inertia.

**Done when:** Laravel web application ingests sync payloads pushed by `ParkGate.SyncAgent`, stores records, and presents live dashboard and reports in Arabic RTL.

---

## Phase F5 — Gift Vouchers (QR) and Free-Companion Limit · branch `phase/f5-gift-vouchers`
- [x] T-G0 Preconditions, branch, documentation
- [x] T-G1 Domain model and policy
- [x] T-G2 Application plumbing: permissions, errors, QR purpose
- [x] T-G3 Free-companion limit (BR-25/26) end to end in Application
- [x] T-G4 Gift voucher use cases (Application)
- [x] T-G5 Infrastructure persistence + migration
- [x] T-G6 Infrastructure: report queries, QR image adapter, DI
- [x] T-G7 WinForms: organization policy and age-category flag screens
- [x] T-G8 WinForms: desk integration (companion limit + gift panel + QR dialog)
- [x] T-G9 WinForms: Gift redemption screen (rides window)
- [x] T-G10 WinForms: gift receipts report + Excel export
- [x] T-G11 Hardening, docs, handoff

**Done when:** full gift voucher flow (issue → redeem → report) works end-to-end; companion limit enforced; all tests green.

---

## Phase R — Remediation · see `docs/REMEDIATION_PLAN.md`

Source: full code review 2026-11-12. Work R0 → R9 in order, one phase per branch and PR.
Task detail, findings and the test-first requirement for each task live in `docs/REMEDIATION_PLAN.md` — this list is only the tracker.

### R0 — Baseline and safety net · branch `phase/r0-baseline`
- [x] R0.1 `git status` / `git log origin/main..main`; report uncommitted or unpushed work and stop for a decision
- [x] R0.2 Record the baseline (`dotnet build`, unit tests, LocalDB integration tests, `php artisan test`) in `SESSION_HANDOFF.md`
- [x] R0.3 Add Phase R to this roadmap; ask the open questions in the plan's §2.2
- [x] R0.4 CI job for `web/` (`composer install`, `php artisan test`)

### R1 — Web security (critical) · branch `phase/r1-web-security`
- [x] R1.1 Remove public self-registration; add admin-only user creation command
- [x] R1.2 Remove the hardcoded default sync API key; add a key-generation command
- [x] R1.3 Remove the committed `APP_KEY` from `.env.production.example`
- [x] R1.4 Guard the seeder's default admin to local/testing environments
- [x] R1.5 Remove `web/fix-permissions.php` and its references
- [x] R1.6 Sync API: no internals in responses, no personal data in logs, throttle, payload validation
- [x] R1.7 Login throttle, secure session cookie, `APP_DEBUG=false`, force HTTPS in production
- [x] R1.8 Test that no web response, view or model exposes `phone` or `national_id` — **deferred: do it right after R2.6** (the web still stores and shows both fields until R2.6 removes them)

### R2 — Sync correctness · branch `phase/r2-sync`
- [x] R2.6 Phone and national ID never leave the park (do this task first in R2)
- [x] R2.1 Fix double encryption after a failed push; make the outbox payload immutable
- [x] R2.2 Order and deduplicate the outbox by a DB `Sequence`, not client clocks
- [x] R2.3 Last-line guard in `CloudPushService` against sensitive fields and non-HTTPS base URLs
- [x] R2.4 Sort cloud ingestion records by dependency (parent before child)
- [x] R2.5 Report poison outbox rows in the heartbeat, dashboard and desk status bar
- [x] R2.7 Sync gift vouchers and the new organization / age-category fields
- [x] R2.8 Fix dashboard counts (timezone, Overridden, voided, aggregate in SQL)
- [x] R2.9 MySQL parity: CI run against MySQL 8; generate `mysql_schema.sql` from the migrations
- [x] R2.10 End-to-end sync test: failing-then-recovering cloud, cloud data equals local data minus excluded fields (R2 "Done when")

### R3 — Desktop Application and Infrastructure · branch `phase/r3-application`
- [x] R3.1 Today's totals must not be capped at 50 attempts — add an aggregate query
- [ ] R3.2 Replace trusted `SupervisorUserId` with short-lived single-use approval tokens
- [x] R3.3 Remove the silent `?? 1` / `?? 0` user and workstation id fallbacks
- [x] R3.4 Serialize audit JSON with `System.Text.Json`, not string interpolation
- [ ] R3.5 Write audit rows inside the same unit of work, before commit
- [ ] R3.6 Use the cached `ISettingsService` instead of four queries inside the locked transaction
- [ ] R3.7 Card expiry from the DB clock, not the client clock (BR-05)
- [ ] R3.8 Card lookup across organizations with a flag, instead of "not registered"
- [ ] R3.9 Return `NextEligibleAtUtc` and remaining minutes instead of integer-divided hours

### R4 — Desk UI defects · branch `phase/r4-desk-ui`
- [ ] R4.1 `BusyScope` must restore each control's previous `Enabled` state
- [ ] R4.2 Do not lose a scan during the success delay; queue lookups instead of dropping them
- [ ] R4.3 Organization combo in the inline registration panel (required)
- [ ] R4.4 Amber banner when the card is registered under another organization
- [ ] R4.5 Confirm before Esc discards a half-filled registration form
- [ ] R4.6 Designer-backed `SelectOrganizationDialog` with Accept/Cancel buttons
- [ ] R4.7 No raw exception text in front of the operator (11 places)
- [ ] R4.8 Move the 94 hardcoded Arabic literals to `Strings.resx`; add an architecture test
- [ ] R4.9 Remove the fallback age categories with invented ids
- [ ] R4.10 Warn once when the Cairo font family is not installed system-wide

### R5 — Desk UX improvements · branch `phase/r5-desk-ux`
- [ ] R5.1 Show the remaining time on denial (SPEC A3)
- [ ] R5.2 Banner instead of a message box after registration
- [ ] R5.3 Larger identity text (name 14 pt bold, card number 16 pt)
- [ ] R5.4 Keyboard access to the companion counters
- [ ] R5.5 Mark companion counts prefilled from the last visit
- [ ] R5.6 Visible «إلغاء العملية…» button above the today grid
- [ ] R5.7 Distinct success and denial sounds, with separate settings
- [ ] R5.8 Layout at the minimum resolution (`MinimumSize`, 5 grid rows) — blocked on Q-R7
- [ ] R5.9 Fix amber banner contrast (below 4.5:1)
- [ ] R5.10 Verify, then implement idle auto-lock (FR-AUTH-06)

### R6 — Database design · branch `phase/r6-database`
- [ ] R6.1 Bring the entry-card tables under EF migrations; remove the runtime initializer
- [ ] R6.2 Fix convention drift in those tables (`WorkstationId` FK, `IX_CardScanLog_CardId`)
- [ ] R6.3 Audit columns from the database clock, not the client clock (BR-05)
- [ ] R6.4 Bring `DATABASE.md` up to the model; test the documented table list against it
- [ ] R6.5 Least-privilege `parkgate_app` login; restrict the firewall rules — blocked on Q-R6
- [ ] R6.6 Re-measure the `LIKE '%term%'` name search at 100 000 holders
- [ ] R6.7 ~~Business-day cutoff~~ — cancelled (Q-R5): the park closes at midnight
- [ ] R6.8 Database size warning on the Admin backup screen; propose a retention policy
- [ ] R6.9 ACL-restricted backup folder; document an encrypted off-site copy
- [ ] R6.10 Cloud schema: `gift_vouchers`, composite index, drop low-cardinality indexes

### R7 — Wording and visual consistency · branch `phase/r7-consistency`
- [ ] R7.1 One glossary: the card holder is «الزائر» / «الزوار» (Q-R4); remaining terms on Q-R4b
- [ ] R7.2 One type scale mapped to `Theme` (Caption 9, Body 10, Section 12, Title 14, Banner 20)
- [ ] R7.3 One neutral palette; test against `Color.FromArgb` literals outside `Theme.cs`
- [ ] R7.4 Dialog conventions: `AcceptButton`, `CancelButton`, tab order, `Theme` button styles
- [ ] R7.5 Web UI: same glossary, park-local times, empty states, no phone or national ID

### R8 — Build, packaging and deployment · branch `phase/r8-release`
- [ ] R8.1 Fix the doubled backslash in the published `appsettings.json` server path
- [ ] R8.2 Re-enable the NuGet vulnerability audit; pin `System.Security.Cryptography.Pkcs`
- [ ] R8.3 Store the Sync Agent cloud API key DPAPI-protected, with a CLI switch to set it
- [ ] R8.4 Rebuild the release as `v1.1.0`; delete the stale `artifacts/efbundle.exe`
- [ ] R8.5 cPanel as the only web deployment path; write `docs/DEPLOYMENT_WEB_CPANEL.md` (Q-R1)
- [ ] R8.6 "Upgrade from 1.0.0" section in `docs/DEPLOYMENT.md`

### R9 — Documentation sync · branch `phase/r9-docs`
- [ ] R9.1 `SPEC.md`: F1 as built, F3/F4/F5 rules, the A5/BR-11 mismatch, close answered questions
- [ ] R9.2 `DECISIONS.md`: supersede ADR-013; new ADRs for R2.1, R2.2, R3.2, R6.1, R6.5
- [ ] R9.3 `AGENTS.md` (needs Osama's approval): project count, web test command, UI test location
- [ ] R9.4 `USER_GUIDE_AR.md` and `TEST_SCRIPT.md`: new shortcuts, registration, void button, glossary
- [ ] R9.5 `SESSION_HANDOFF.md`: final state, known issues, next steps
- [x] R9.6 Format all web PHP with Pint (done early in S30, in its own `style` commit; run `vendor/bin/pint --dirty --format agent` after each PHP change)

**Done when:** R0–R9 are ticked, every phase PR is merged, and the acceptance line at the end of each phase in `docs/REMEDIATION_PLAN.md` holds.

---

## Future (do not start without explicit approval)
- **F2 Electronic gate** — deferred by the client; decide Q11 when scheduled; `IGateController`; `ParkGate.GateService` host. (SPEC §9 F2)

