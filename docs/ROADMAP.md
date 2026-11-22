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

## Phase F8 — Entry days and card-number pattern alerts · branch `phase/f8-entry-days` (approved 2026-11-18; start only on Osama's go-ahead)
Design: SPEC BR-27..BR-29 and §9 F8, DATABASE §3.1, §3.13, §3.14, ADR-024.
Owner, 2026-11-20: the entry days come first; days change rarely and by hand, permanently, with the change in the audit trail (no dated schedule). Card-number patterns (BR-29) follow later.
- [x] T8.1a Domain: allowed weekdays on `Organization` (`WeekDays`), `DayNotAllowed` in the entry and gift policies (table-driven tests)
- [x] T8.2a Infrastructure: migration for `AllowedEntryDays` and `GiftAnyDay`, integration tests
- [x] T8.3a Application: entry and gift use cases and previews apply the days; overrides cover them; the organization policy saves them, audited with the days before and after
- [x] T8.4a WinForms: organization dialog (weekdays, gifts on any day); desk banner and gift panel name the allowed days; reports and audit screen show the new reason and days
- [x] T8.1b Domain: card-number shape and pattern matcher (table-driven tests)
- [x] T8.2b Infrastructure: pattern and alert tables, settings seed, repositories, integration tests
- [x] T8.3b Application: pattern check on register and card change writes alerts; Admin use cases to approve or reject patterns and review alerts
- [x] T8.4b WinForms: amber pattern note; Admin pattern and alert screens
- [x] T8.7 `RtlTabControl` (R5.13) failed with "Error creating window handle" when a window with tabs was created on a 125 % screen (`SettingsForm` too); found by the T8.4b screen check
- [ ] T8.5 Sync: add `AllowedEntryDays` and `GiftAnyDay` to the cloud schema (Q14)
- [x] T8.6 Docs and `TEST_SCRIPT.md` steps for BR-27..BR-29 (TC-36, TC-37, TC-38)

**Done when:** an entry and a gift on a non-allowed day are denied and can be overridden; an unusual number raises an alert without blocking; a new series stops alerting only after an Admin approves it.

---

## Phase F9 — visitor city, optional phone (Osama, 2026-11-22) · branch `phase/f9-visitor-city`
Design: SPEC BR-15 (phone optional) and BR-32 (city), DATABASE §3.2 and §3.18.
- [x] T9.1 Domain + Application: optional phone and `CityId` on `Beneficiary`, `City` entity, `ICityRepository`, `CityService`, validators, `Errors.City.NotFound`, city name in the holder DTOs (unit tests)
- [x] T9.2 Infrastructure + migration `AddCitiesAndOptionalPhone`: `Cities` table seeded with 57 rows, `Phone` nullable, `CityId` FK and index (integration tests)
- [x] T9.3 Desk and management UI: optional phone, city drop-down (fixed order) in the registration panel and the edit dialog, city in the holder panel and the holders list, `Strings.resx`, tests
- [ ] T9.4 Sync: send the city to the cloud (paused with the web work); then remove `CityId` from the excluded set in `OutboxEntityExtractor`

**Done when:** a holder can be registered and edited without a phone, and with a city chosen from the fixed list, which shows in the holder panel and the holders list.

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
- [x] R3.2 Replace trusted `SupervisorUserId` with short-lived single-use approval tokens
- [x] R3.3 Remove the silent `?? 1` / `?? 0` user and workstation id fallbacks
- [x] R3.4 Serialize audit JSON with `System.Text.Json`, not string interpolation
- [x] R3.5 Write audit rows inside the same unit of work, before commit
- [x] R3.6 Use the cached `ISettingsService` instead of four queries inside the locked transaction
- [x] R3.7 Card expiry from the DB clock, not the client clock (BR-05)
- [x] R3.8 Card lookup across organizations with a flag, instead of "not registered"
- [x] R3.9 Return `NextEligibleAtUtc` and remaining minutes instead of integer-divided hours
- [x] R3.10 `EntryCardIssuer` computes the card expiry from the client clock (`TimeProvider.GetLocalNow`) — use `ParkClock` (found in R3.7)

### R4 — Desk UI defects · branch `phase/r4-desk-ui`
- [x] R4.1 `BusyScope` must restore each control's previous `Enabled` state
- [x] R4.2 Do not lose a scan during the success delay; queue lookups instead of dropping them
- [x] R4.3 Organization combo in the inline registration panel (required)
- [x] R4.4 Amber banner when the card is registered under another organization
- [x] R4.5 Confirm before Esc discards a half-filled registration form
- [x] R4.6 Designer-backed `SelectOrganizationDialog` with Accept/Cancel buttons
- [x] R4.7 No raw exception text in front of the operator (11 places)
- [x] R4.8 Move the 94 hardcoded Arabic literals to `Strings.resx`; add an architecture test
- [x] R4.9 Remove the fallback age categories with invented ids
- [x] R4.10 Warn once when the Cairo font family is not installed system-wide
- [x] R4.11 Fix gift redemption screen scaling (`AutoScaleDimensions` 96x96) and add a designer scaling guard test
- [x] R4.12 Lay out the gift redemption scan row with a `TableLayoutPanel`
- [x] R4.13 Stop clipping on the check-in desk (gift panel, registration buttons) at 120 % DPI

### R5 — Desk UX improvements · branch `phase/r5-desk-ux`
- [x] R5.1 Show the remaining time on denial (SPEC A3)
- [x] R5.2 Banner instead of a message box after registration
- [x] R5.3 Larger identity text (name 14 pt bold, card number 16 pt)
- [x] R5.4 Keyboard access to the companion counters
- [x] R5.5 Mark companion counts prefilled from the last visit
- [x] R5.6 Visible «إلغاء العملية…» button above the today grid
- [x] R5.7 Distinct success and denial sounds, with separate settings
- [x] R5.8 Layout at the minimum resolution (`MinimumSize`, 5 grid rows) — blocked on Q-R7
- [x] R5.9 Fix amber banner contrast (below 4.5:1)
- [x] R5.10 Verify, then implement idle auto-lock (FR-AUTH-06)
- [x] R5.11 Search row in a `TableLayoutPanel` (card box and its label overlap at 125 %)
- [x] R5.12 Group box captions lose their last letters in RTL («بيانات الزا»), on every screen
- [x] R5.13 Buttons, check boxes and tabs also lose the last letters of Arabic text («عرض التقر», «بهذه الجه»)
- [x] R5.14 Offer the gift right after a granted entry, without a second scan
- [x] R5.15 Register a new visitor in a pop-up (the compact desk hid the registration fields)
- [x] R5.16 Voiding an entry voids its unredeemed gift; a redeemed gift blocks the void (BR-31)
- [x] R5.17 Screen to void a gift voucher (gift receipts window)
- [x] R5.18 Full audit details for every entry and gift operation (who, approver, visitor, day, data)
- [x] R5.19 Gift receipts window: grid headers cut at 125 % (fixed 44 px header height in `Theme.ApplyGridStyle`) and date boxes show MM/dd instead of dd/MM/yyyy
- [x] R5.20 Audit trail screen (Reports > سجل التتبع), supervisors and admins

### R6 — Database design · branch `phase/r6-database`
- [x] R6.1 Entry-card tables as EF entities and migration (EF only, ADR-025); remove the initializer, procedures, view and SQL script
- [x] R6.2 Rename to conventions (`Utc` suffix, `datetime2(3)`), `WorkstationId` FK, `IX_CardScanLog_CardId`, audit columns
- [x] R6.3 Audit columns from the database clock, not the client clock (BR-05)
- [x] R6.4 Bring `DATABASE.md` up to the model; test the documented table list against it
- [x] R6.5 Least-privilege `parkgate_app` login; restrict the firewall rules (Q-R6: workgroup, 4 desks; ADR-027)
- [x] R6.6 Re-measure the `LIKE '%term%'` name search at 100 000 holders
- [ ] R6.7 ~~Business-day cutoff~~ — cancelled (Q-R5): the park closes at midnight
- [x] R6.8 Database size warning on the Admin backup screen; propose a retention policy (proposal in DEPLOYMENT.md §9.3, awaiting approval; no deletes)
- [x] R6.9 ACL-restricted backup folder; document an encrypted off-site copy
- [x] R6.10 Cloud schema: `gift_vouchers`, composite index, drop low-cardinality indexes
- [x] R6.11 Write management audit rows in the same unit of work; record old and new card number
- [x] R6.12 Read the database clock through a mapped `SYSUTCDATETIME` function instead of SQL text
- [x] R6.13 Optimistic concurrency on `Beneficiaries.RowVersion` replaces `UPDLOCK` (ADR-026); add `LastGiftIssuedUtc`
- [x] R6.14 Architecture test: no raw SQL in `src/` except the backup command
- [x] R6.15 Remove the seeded `Entry.PlaySoundOnDenial` row that nothing reads (migration `RemoveUnreadSoundSetting`); the app uses `Entry.DenialSoundEnabled`
- [x] R6.16 Mask national IDs for operators in Application (`ViewFullNationalId` for supervisors and admins); remove the UI copies of the rule and the unused `Ui.MaskNationalIdForOperators` seed row (Osama: hide)

### R7 — Wording and visual consistency · branch `phase/r7-consistency`
- [x] R7.1 One glossary: the card holder is «الزائر» / «الزوار» (Q-R4); remaining terms on Q-R4b
- [x] R7.2 One type scale mapped to `Theme` (Caption 9, Body 10, Section 12, Title 14, Banner 20)
- [x] R7.3 One neutral palette; test against `Color.FromArgb` literals outside `Theme.cs`
- [x] R7.4 Dialog conventions: `AcceptButton`, `CancelButton`, tab order, `Theme` button styles
- [x] R7.5 Web UI: same glossary, park-local times, empty states, no phone or national ID
- [x] R7.6 Remove the Infrastructure reference from `IssueQrDialog`; architecture test
- [x] R7.7 All message boxes through `Notify`; architecture test
- [x] R7.8 All designers on `AutoScaleMode.Dpi`; extend `DesignerScalingTests`
- [x] R7.9 Resolve forms and controls through `IFormFactory`; required services, no null fallbacks
- [x] R7.10 Split `CheckInDeskControl` into smaller user controls
- [x] R7.11 UI thread is STA: synchronous `Main`, async startup inside the message loop (file/print dialogs threw `ThreadStateException`)
- [x] R7.12 Entry-card QR print layout needs design work (Osama, 2026-11-18: save and print work, the printed style does not)
- [x] R7.14 Gift only after a granted entry today (BR-30, owner 2026-11-18); not overridable
- [x] R7.13 Text still clipped at 125 % (found in R7.8, older than it): `ResetUserPasswordDialog` prompt and button, `UsersForm` add button, `SelectOrganizationDialog` and `VoidReasonDialog` long texts, `GiftReceiptsForm` export/print buttons outside the filter panel, `BeneficiariesControl` details rows 8-9 and void button, `ConnectionSettingsForm` labels, `EditAgeCategoryDialog` description label, page headers in `LoginForm`/`OrganizationsForm`/`AgeCategoriesForm`

### R8 — Build, packaging and deployment · branch `phase/r8-release`
- [x] R8.1 Fix the doubled backslash in the published `appsettings.json` server path
- [x] R8.2 Re-enable the NuGet vulnerability audit; pin `System.Security.Cryptography.Pkcs`
- [x] R8.3 Store the Sync Agent cloud API key DPAPI-protected, with a CLI switch to set it
- [x] R8.4 Rebuild the release as `v1.1.0`; delete the stale `artifacts/efbundle.exe`. Also: installer client self-contained (was framework-dependent with no runtime check), fresh migration bundle every build, Sync Agent in both build scripts and the installer (server mode), bundled runtime pinned to 10.0.15 (`-RuntimeVersion`)
- [x] R8.5 cPanel as the only web deployment path; write `docs/DEPLOYMENT_WEB_CPANEL.md` (Q-R1)
- [x] R8.6 "Upgrade from 1.0.0" section in `docs/DEPLOYMENT.md`
- [x] R8.7 Web: update `laravel/framework` and `symfony/*` past their security advisories (found in R8.5: 9 advisories in production packages); PHP >= 8.4.1

### R9 — Documentation sync · branch `phase/r9-docs`
- [x] R9.1 `SPEC.md`: F1 as built, F3/F4/F5 rules, the A5/BR-11 mismatch, close answered questions
- [x] R9.2 `DECISIONS.md`: ADR-013 superseded by ADR-028; ADR-029 (R2.1, R2.2), ADR-030 (R6.1); R3.2 and R6.5 are ADR-023 and ADR-027
- [x] R9.3 `AGENTS.md` (approved 2026-11-20): five production projects with `ParkGate.SyncAgent`, UI tests documented in ArchitectureTests; web items left out on Osama's request (desktop first)
- [x] R9.4 `USER_GUIDE_AR.md` rewritten for 1.1.0 (roles, shortcuts, registration pop-up, F8 void and gifts, entry days, card patterns, audit trail); `TEST_SCRIPT.md` glossary fix
- [x] R9.5 `SESSION_HANDOFF.md`: final state, known issues, next steps (desktop rollout first)
- [x] R9.6 Format all web PHP with Pint (done early in S30, in its own `style` commit; run `vendor/bin/pint --dirty --format agent` after each PHP change)

**Done when:** R0–R9 are ticked, every phase PR is merged, and the acceptance line at the end of each phase in `docs/REMEDIATION_PLAN.md` holds.

---

## Future (do not start without explicit approval)
- **F2 Electronic gate** — deferred by the client; decide Q11 when scheduled; `IGateController`; `ParkGate.GateService` host. (SPEC §9 F2)

