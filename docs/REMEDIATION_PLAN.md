# ParkGate — Remediation Plan (Phase R)

| Item | Value |
|---|---|
| Created | 2026-11-12 |
| Source | Code review of `main` as of commit `fix(winforms): fix print dialog combase crash…` (2026-11-05) |
| Scope | Cloud web app (`web/`), Sync Agent, WinForms desk UI, Application/Infrastructure, database design, build and deployment |
| Status | Approved to start. Q-R1, Q-R2, Q-R4, Q-R5 answered 2026-11-12; Q-R3, Q-R6, Q-R7 still open |

This plan follows `AGENTS.md`. Where this plan and `AGENTS.md` disagree, stop and ask (AGENTS.md §11).

## 0. How to execute this plan

1. Follow the session protocol in `AGENTS.md` §1 on every session.
2. Work the phases in order (R0 → R9). One task at a time, one commit per task, task ID in the subject, for example `fix(web): disable public registration [R1.1]`.
3. One branch per phase: `phase/r1-web-security`, `phase/r2-sync`, and so on. Open a PR to `main` at phase end. Osama merges.
4. **A bug fix starts with a failing test that reproduces it** (AGENTS.md §8). Each task below names the test to write first.
5. Every finding is tagged:
   - **[Verified]** — confirmed by reading the code. Still write the failing test first.
   - **[Reproduce]** — likely from the code, not proven. Write the reproducing test; if it cannot be reproduced, record that in `SESSION_HANDOFF.md` and skip the fix.
6. Do not change a default, a business rule, or a released table beyond what a task states. Tasks marked **[Blocked: Qn]** wait for the matching answer in §2.
7. R0.3 adds this phase list to `docs/ROADMAP.md`. Tick tasks there as usual.

## 1. Baseline facts (so you do not re-derive them)

- Phases 0–8, F1, F3, F4, F5 are complete. Handoff reports 622 tests green (588 unit/architecture + 34 LocalDB integration).
- Latest migration: `20261102125339_AddGiftVouchersAndCompanionLimit`.
- `dist/installer/ParkGate_Setup_v1.0.0.exe` and `dist/database/ParkGate.Migrations.exe` were built 2026-10-21, **before** F5 and the S24/S25 fixes. Treat them as stale.
- Dev web DB is SQLite; production is MySQL (cPanel) or Docker on a VPS. Both deployment paths exist in the repo.
- Files modified after the last commit (same timestamp, 2026-11-05): `docs/SESSION_HANDOFF.md`, `src/ParkGate.WinForms/Forms/IssueQrDialog.cs`, `Forms/GiftQrDialog.cs`, `Resources/Strings.resx`. Check `git status` in R0.1.

## 2. Decisions

### 2.1 Answered by Osama (2026-11-12) — apply as written

| ID | Decision | Effect on this plan |
|---|---|---|
| Q-R1 | The web app is **not deployed yet**. It will be hosted on **shared cPanel hosting**. | No production secrets to rotate. The cloud schema is unreleased, so the existing Laravel migration may be edited in place. Remove the Docker/VPS path (R8.5). |
| Q-R2 | The cloud must **not store phone numbers or national IDs**. | Neither field leaves the park. Field encryption and the AES key are no longer needed (R2.6). |
| Q-R4 | The UI term for the card holder is **«الزائر»** (plural «الزوار»). | R7.1. |
| Q-R5 | The park does not open past midnight (last entry about 23:00, closes 00:00). | No business-day cutoff. R6.7 is cancelled. |
| Q-R2b | (2026-11-13) `Notes` and `StatusReason` **are synced** to the cloud. | R2.6 excludes only `Phone` and `NationalId`. The web keeps both text fields. |

### 2.2 Still open — ask at the start of R0

Ask with options and a recommendation (AGENTS.md §11). Continue with unblocked tasks meanwhile.

| ID | Question | Recommended default | Blocks |
|---|---|---|---|
| Q-R3 | Who creates web dashboard users? | Admin-only creation via an artisan command; no self-registration | R1.1 (the removal of `/register` is not blocked; only how users are created) |
| Q-R4b | The other mixed terms: «فترة الانتظار» vs «فترة التهدئة», «الشباك» vs «المحطة», «قسيمة» vs «كوبون» vs «وصل». | «فترة الانتظار», «الشباك», «قسيمة» | R7.1 (these three only) |
| Q-R6 | How many desk PCs connect to the SQL Server? Workgroup or domain? | SQL authentication with a least-privilege login | R6.5 |
| Q-R7 | Lowest screen resolution used at the desks? | Design for 1366×768 at 100 % | R5.8 |

---

## Phase R0 — Baseline and safety net · branch `phase/r0-baseline`

- [ ] **R0.1** `git status`, `git log origin/main..main`. Report uncommitted or unpushed work to Osama and stop until he says commit or discard. Do not discard anything yourself.
- [ ] **R0.2** Record the baseline: `dotnet build`, unit tests, integration tests on LocalDB, `php artisan test` in `web/`. Write the numbers in `SESSION_HANDOFF.md`.
- [ ] **R0.3** Add Phase R (R0–R9 task list) to `docs/ROADMAP.md`. Ask the §2 questions.
- [ ] **R0.4** Add a CI job for `web/` (`composer install`, `php artisan test`) to `.github/workflows/build.yml`, or a new `web.yml`. Today only the .NET solution is built in CI.

**Done when:** baseline is green and recorded, roadmap updated, questions sent.

---

## Phase R1 — Web security (critical) · branch `phase/r1-web-security`

- [ ] **R1.1 [Verified] Public self-registration exposes personal data.**
  `web/routes/auth.php` serves `GET/POST /register`. `App\Models\User` does not implement `MustVerifyEmail`, so the `verified` middleware passes for any new account. Any visitor can register and open `/beneficiaries`.
  Fix: remove the register routes, `RegisteredUserController`, and the Register page and links now. Add `php artisan parkgate:create-user` (name, email, prompts for password) unless Osama answers Q-R3 differently. On cPanel without SSH the command must also be runnable from the cPanel Terminal or a one-time signed setup route that disables itself once a user exists — ask Osama which he has. Keep password reset only if mail is configured; otherwise remove those routes too.
  Test first: feature test — `GET /register` and `POST /register` return 404; a guest gets redirected from `/beneficiaries`.
- [ ] **R1.2 [Verified] Hardcoded default sync API key.**
  `web/config/parkgate.php` falls back to `'parkgate-cloud-secret-key-2026'`. The same value is in `web/.env.production.example`.
  Fix: default to `null` (the middleware already rejects an empty expected key). Replace the example value with a placeholder. Add `php artisan parkgate:generate-key` that prints a random API key. Nothing is deployed yet, so there is no key to rotate.
  Test first: with no `PARKGATE_API_KEY` set, `POST /api/sync/push` returns 401 for any header value, including the old default.
- [ ] **R1.3 [Verified] Committed `APP_KEY` in `.env.production.example`.** Replace with an empty value and a comment to run `php artisan key:generate`.
- [ ] **R1.4 [Verified] Default admin in the seeder.** `DatabaseSeeder` creates `admin@parkgate.ly` with password `password`. Guard all seeding with `app()->environment('local', 'testing')`. Check `web/database/mysql_schema.sql` for an equivalent `INSERT` into `users` and remove it.
  Test first: seeding in `production` environment creates no user.
- [ ] **R1.5 [Verified] Remove `web/fix-permissions.php`** (tracked, contains a fixed key in the source). Remove references in `scripts/package-cpanel.ps1` and `web/README.md`.
- [ ] **R1.6 [Verified] Sync API leaks internals and logs personal data.**
  `SyncApiController` returns `$e->getMessage()` to the caller. `SyncIngestionService` logs the whole item (`['item' => $item]`) on failure.
  Fix: return a generic message plus a short error id; log entity type, entity id and the error id only. Add `throttle` middleware to `/api/sync/*`. Validate the payload shape with a FormRequest (max records per batch, required keys).
  Test first: a malformed batch returns 422 without exception text; the log line contains no payload content.
- [ ] **R1.7 Web authorization and hardening.** Add a `throttle` to `POST /login` if Breeze's default was removed, set `SESSION_SECURE_COOKIE=true` and `APP_DEBUG=false` in the production example, and force HTTPS in production (`URL::forceScheme('https')`).
- [x] **R1.8 Phone and national ID on the web.** Removed entirely by R2.6 (Q-R2). After R2.6, add a test that no web response, view or model exposes `phone` or `national_id`.

**Done when:** all R1 tests pass, and a fresh production-mode install has no way to obtain an account or a valid API key from the repository contents.

---

## Phase R2 — Sync correctness · branch `phase/r2-sync`

- [x] **R2.1 [Verified] Double encryption after a failed push.**
  `OutboxProcessorWorker.ProcessBatchAsync` calls `record.UpdatePayload(encryptedPayload)` on tracked entities (`SyncOutboxRepository.GetPendingBatchAsync` does not use `AsNoTracking`). On `PayloadRejected` or `ServerError` the method ends with `SaveChangesAsync`, which persists the encrypted payload. The next cycle encrypts the fields again.
  Fix: R2.6 removes field encryption altogether (Q-R2). Do R2.6 first, then make the outbox payload immutable: remove `SyncOutbox.UpdatePayload` and send `PayloadJson` as stored.
  Test first (`OutboxProcessorWorkerTests`): push fails with 500 once, then succeeds; the stored `PayloadJson` is byte-identical before and after the failure and equals what the second request sent.
- [x] **R2.2 [Verified] Outbox ordering uses client clocks.** `ParkGateDbContext.SaveChangesWithOutboxAsync` stamps `CreatedAtUtc = DateTime.UtcNow`, and `OutboxDeduplicator` picks the last write by that value. Two desks with skewed clocks can make an older write win.
  Fix: add `Sequence bigint IDENTITY` to `SyncOutboxes` (new migration), order and deduplicate by `Sequence`. Keep `CreatedAtUtc` with a `SYSUTCDATETIME()` column default.
  Test first: two outbox rows for the same entity where the later-inserted row has an earlier `CreatedAtUtc`; the later-inserted one must win.
- [x] **R2.3 Guard against sensitive fields leaving the park.** After R2.6, add a last-line check in `CloudPushService`: if any outgoing payload contains a `Phone` or `NationalId` key, do not push and log an error. Also refuse a `CloudApi:BaseUrl` that is not `https://` outside Development.
  Test first: a hand-built outbox row with `Phone` in its payload → no HTTP call is made.
- [x] **R2.4 [Reproduce] Parent/child ordering on the cloud.** `SyncIngestionService` runs the whole batch in one transaction with foreign keys. If an `EntryAttempt` arrives before its `Beneficiary` (different batches, or after dedupe), the batch fails with a foreign-key error. Sort records inside `ingestBatch` by dependency: Organization, AgeCategory, Beneficiary, EntryAttempt, EntryAttemptCompanion.
  Test first: a batch with the child listed before the parent ingests successfully.
- [x] **R2.5 [Verified] Poison rows are silent.** Rows that reach `MaxRetries` stay in `SyncOutboxes` forever and nothing reports them. Send `pendingCount` and `failedCount` in the heartbeat; show them on the dashboard's park status card; add a "sync status" line on the desk status bar for Admins (count of failed rows).
- [x] **R2.6 [Decided: Q-R2] Phone and national ID never leave the park. Do this task first in R2.**
  Desktop: exclude `Phone` and `NationalId` from the `Beneficiary` outbox payload in `OutboxEntityExtractor` (an explicit per-entity exclusion list, tested). Remove `PayloadEncryptionService`, `IPayloadEncryptionService`, `AesHelper`, `CloudApiOptions.AesKey` and their DI registrations and tests. Add a new EF migration that deletes pending `SyncOutboxes` rows of type `Beneficiary` and re-queues one clean row per holder, so no old payload with these fields is ever pushed.
  Web (unreleased, edit the existing migration): drop `phone` and `national_id` from `beneficiaries`, the model, `SyncIngestionService` (remove `decrypt` and `parkgate.aes_key`), `BeneficiaryController` search and output, the Vue pages, factories and seeders, and `PARKGATE_AES_KEY` from the env examples.
  `Notes` and `StatusReason` keep syncing (Q-R2b, decided 2026-11-13 against the recommendation to exclude them).
  Docs: SPEC §9 F3 and Q12, ADR-018 and ADR-019 (superseded parts), DEPLOYMENT.md.
  Test first: the serialized `Beneficiary` payload has no `Phone`/`NationalId` keys; ingestion of a payload without them succeeds; the `beneficiaries` table has no such columns.
- [ ] **R2.7 [Verified] Gift vouchers are not synced.** `OutboxEntityExtractor.SyncableTypes` has no `GiftVoucher`; the cloud has no table for it. Add the entity to the outbox, a `gift_vouchers` table and model on the web, ingestion, and a gifts summary on the dashboard and reports (issued, redeemed, voided, value by organization). Also sync the new organization fields (`MaxFreeCompanions`, gift policy) and `AgeCategory.IsCompanionLimitExempt`.
- [x] **R2.8 [Verified] Dashboard numbers are wrong.** `DashboardController`: (a) "today" uses `Carbon::today()` in the app timezone while `business_date` is park-local — set `APP_TIMEZONE=Africa/Tripoli` and compute today from it; (b) persons and entries count `result = 1` only — include Overridden (`3`) and exclude `is_voided`; (c) it loads all of today's attempts into memory — aggregate in SQL.
  Test first: one Allowed, one Overridden, one voided Allowed → entries 2, persons from the two non-voided rows.
- [x] **R2.9 [Reproduce] MySQL parity.** Tests run on SQLite, production is MySQL. Add a CI job (or documented local run) that runs the web test suite against MySQL 8. Check that ISO-8601 timestamps with 7 fractional digits and `Z` ingest correctly into `timestamp` columns under strict mode; normalize in `SyncIngestionService` with `Carbon::parse(...)->utc()` if not. Shared cPanel hosting may have no SSH to run `artisan migrate`: keep `web/database/mysql_schema.sql` but generate it from the migrations in `package-cpanel.ps1` (never hand-edit it), and add a test that the generated file matches the migrations.

**Done when:** an end-to-end test pushes holders, entries and gift vouchers through a failing-then-recovering cloud endpoint, the cloud data equals the local data minus the excluded fields, and no phone or national ID exists anywhere in the cloud database.

---

## Phase R3 — Desktop Application and Infrastructure fixes · branch `phase/r3-application`

- [x] **R3.1 [Verified] Today's totals are capped at 50 attempts.** `CheckInService.GetTodayDeskEntriesAsync` loads `GetTodayByWorkstationIdAsync(..., 50, ...)` and computes the totals from that list. Add an aggregate query (`IEntryAttemptRepository.GetTodayTotalsAsync`: granted, overridden, denied, persons, persons per category) and keep the list for the grid only.
  Test first (integration): 60 granted attempts → `TotalGranted == 60`.
- [x] **R3.2 [Verified] Supervisor approval is trusted from the UI.** `OverrideEntryAsync`, `VoidEntryAsync` and the gift override accept a bare `SupervisorUserId`. Replace it with a short-lived approval: `IAuthService.AuthorizeSupervisorAsync(username, password)` returns an opaque single-use token (in-memory, 2-minute expiry, bound to the workstation); the use cases accept the token and resolve the approver from it. Remove the `_authService == null → SupervisorUserId = 1` branch from `SupervisorAuthDialog`; tests must inject a fake service instead.
  Test first: calling `OverrideEntryAsync` as an Operator with a made-up supervisor id is rejected; a token cannot be used twice.
- [x] **R3.3 [Verified] Silent fallbacks to user/workstation id 1.** `_currentWorkstation.WorkstationId ?? 1` and `_currentUser.UserId ?? 1` in `CheckInService` (and `?? 0` in `VoidEntryAsync`). Return `Errors.Auth.NotAuthenticated` / a workstation-not-registered error instead. Search the Application layer for the same pattern.
- [x] **R3.4 [Verified] Audit JSON built by string interpolation.** `detailsJson: $"{{\"Reason\":\"{request.Reason}\"…"` breaks on quotes. Serialize with `System.Text.Json` everywhere `IAuditWriter.WriteAsync` is called.
  Test first: a reason containing `"` and `\` produces valid JSON.
- [x] **R3.5 [Verified] Audit rows are written after commit in a separate call.** A crash between the two loses the audit entry for a granted entry, override or void. Write the audit row through the same unit of work before `CommitAsync`.
- [x] **R3.6 [Verified] Check-in reads four settings with four queries inside the locked transaction.** Use the cached `ISettingsService` instead of `GetSettingsAsync` in `CheckInService`. Remove the duplicated default literals.
- [x] **R3.7 [Verified] Card expiry uses the client clock.** `CardScanService` uses `TimeProvider.GetLocalNow()`. Use the database UTC time converted with `Park.TimeZoneId` (BR-05).
- [x] **R3.8 [Verified] Lookup by card ignores the organization filter until after the query**, and a card that exists in another organization is reported as "not registered". Return matches from all organizations plus a flag, so the UI can say "registered under «X»" (used by R4.4).
- [x] **R3.9 [Verified] Cooldown hours are shown with integer division** (`EffectiveCooldownMinutes / 60`). Return `NextEligibleAtUtc` and the remaining minutes in `EligibilityPreviewDto`; the UI formats them (R5.1).

**Done when:** all new tests pass, architecture tests pass, no `?? 1` identity fallbacks remain in Application.

---

## Phase R4 — Desk UI defects · branch `phase/r4-desk-ui`

All in `src/ParkGate.WinForms/Controls/CheckInDeskControl.cs` unless stated. UI tests live in `tests/ParkGate.ArchitectureTests`.

- [x] **R4.1 [Verified] `BusyScope` re-enables buttons unconditionally.** After a successful entry, `ResetDesk()` disables «تسجيل الدخول» and «تجاوز المشرف», then `BusyScope.Dispose` enables them again on an empty desk. Same after a non-overridable denial. Make `BusyScope` remember each control's previous `Enabled` state and restore it, and call `UpdateLiveTotalPersons()` after the scope ends so the desk state decides.
  Test first: after `RecordEntryAsync` succeeds and the desk resets, both buttons are disabled.
- [ ] **R4.2 [Verified] A scan during the 1.2 s success delay is lost.** The card box stays editable while `Task.Delay(AutoResetDelayMs)` runs, then `ResetDesk()` clears it. Reset the holder panel immediately, keep the success banner for the delay without clearing the card box, and if the box received input meanwhile, run the lookup. Also: a lookup requested while `_isSearching` is true must be queued, not dropped.
  Test first: type a card number during the delay → it is still there and looked up after the delay.
- [x] **R4.3 [Verified] New holders can be registered under the wrong organization.** With the filter on «الكل», `SaveInlineRegistrationAsync` uses the first active organization. `ShowInlineRegistration(cardNumber, orgId)` ignores `orgId`. Add an organization combo to the registration panel: preselect the filter value, else the default organization (`IsDefault`); it is required.
  Test first: filter «الكل», two organizations, default is the second → the request carries the default organization's id.
- [x] **R4.4 [Verified] Card found in another organization opens the registration form.** Using R3.8, show an amber banner «البطاقة مسجلة لدى جهة: X» with an action to switch the filter and load that holder. Open registration only when the card exists nowhere.
- [x] **R4.5 [Verified] Esc discards a half-filled registration form.** In `ProcessCmdKey`, when the registration panel is visible and any field has text, ask for confirmation through `Notify.Confirm`. Do not treat Esc as reset while a combo dropdown is open.
- [x] **R4.6 [Verified] Organization disambiguation dialog.** `DisambiguateOrganization` builds a form in code, has no `AcceptButton`/`CancelButton`, and returns `matches[0]` when the user closes it. Move it to a designer-backed `SelectOrganizationDialog` (AGENTS.md §5.4), Enter = select, Esc = cancel, double-click = select; cancelling returns to the card box with a neutral banner.
- [ ] **R4.7 [Verified] Raw exception text reaches the operator** (11 places use `ex.Message`, for example the QR scan error and inline registration). Log the exception, show a `Strings.resx` message. For connection failures show the existing «تعذّر الاتصال…» banner with a retry hint.
- [ ] **R4.8 [Verified] Hardcoded Arabic strings** (94 literals outside designer files, against AGENTS.md §5.7). Move them to `Strings.resx`. Add an architecture test that fails on Arabic characters in non-designer `.cs` files under `ParkGate.WinForms`.
- [ ] **R4.9 [Verified] Fallback age categories with fake ids.** `CreateDefaultAgeCategories` invents ids 1–3 when loading fails, and its text («12 سنة فما فوق») differs from the seed («13 سنة فأكثر»). On load failure, disable recording and show the connection banner instead.
- [ ] **R4.10 [Reproduce] Fonts set as `new Font("Cairo", …)` in designer files** resolve only if Cairo is installed system-wide. The installer installs it; an xcopy deployment does not. At startup, if `AppFont` loaded Cairo from the embedded resource but the family is not installed, log a warning and show it once to Admins. Do not rewrite the designer files.

**Done when:** every flow in `docs/TEST_SCRIPT.md` passes, plus new script steps for R4.2–R4.6.

---

## Phase R5 — Desk UX improvements · branch `phase/r5-desk-ux`

- [ ] **R5.1 Remaining time on denial.** Banner details: «يُسمح بالدخول الساعة {time} — باقي {h}:{mm}» (SPEC A3 requires the remaining time). Uses R3.9.
- [ ] **R5.2 Replace the "registered successfully" message box** with a banner message; continue straight to the holder panel.
- [ ] **R5.3 Larger identity text.** Holder name 14 pt bold, card number box 16 pt. Use `Theme` fonts.
- [ ] **R5.4 Keyboard access to companion counters.** `Alt+1…9` focuses counter n; `+` / `−` change the focused counter; after a successful lookup of an eligible holder, focus moves to the first counter and Enter records the entry when the card box is not focused. Show the shortcut hints next to the counters.
- [ ] **R5.5 Mark prefilled companions.** When counts come from the last visit, show «من آخر زيارة» beside the total and tint the counters until the operator changes one or confirms.
- [ ] **R5.6 Visible void action.** Add a «إلغاء العملية…» button above the today grid (enabled for a selected granted row), with a shortcut. Keep the context menu.
- [ ] **R5.7 Distinct sounds.** Short WAV resources for success and denial instead of `SystemSounds`; separate settings for each. Add the setting keys to DATABASE.md §5 and a migration seed.
- [ ] **R5.8 [Blocked: Q-R7] Layout at the minimum resolution.** Set `MinimumSize` on `MainForm`. At 1366×768 the today grid must show at least 5 rows: make the gift panel collapse to one line when no gift applies, and reduce the fixed rows. Verify at 100 % and 150 % scaling.
- [ ] **R5.9 Banner contrast.** Detail text on the amber banner (`#F0F0F0` on `#B26A00`) is below 4.5:1. Use white and darken amber to `#9A5B00`. Keep the SPEC colors for green and red.
- [ ] **R5.10 Idle auto-lock (FR-AUTH-06)** if not implemented: verify, then implement with a setting.

**Done when:** Osama walks through the desk flow keyboard-only on the smallest desk screen.

---

## Phase R6 — Database design · branch `phase/r6-database`

Every schema change needs a new migration, a `docs/DATABASE.md` update, and Osama's approval because all tables are released (AGENTS.md §7). Never edit an existing migration.

- [ ] **R6.1 [Verified] Entry-card tables live outside EF migrations.** `QrSigningKeys`, `EntryCards`, `CardScanLog`, the view and two procedures are defined twice (`scripts/001_EntryCards.sql` and a string in `EntryCardSchemaInitializer`) and created by the client at startup (`DatabaseStartupService.EnsureEntryCardsSchemaAsync`). This contradicts ADR-011 and forces DDL rights on the app login.
  Fix: add EF entities/configurations (or keep raw SQL but inside a migration using `migrationBuilder.Sql`) in a new migration `AdoptEntryCardsSchema` that is idempotent for databases that already have the objects (`IF OBJECT_ID(...) IS NULL`). Remove the runtime initializer and the duplicate SQL file. Supersede the relevant note in ADR-020/ENTRY_CARDS.md with a new ADR.
  Test first (integration): a fresh database from migrations contains the entry-card objects; a database that already has them migrates without error.
- [ ] **R6.2 [Verified] Convention drift in those tables.** `datetime2(0)` and names without the `Utc` suffix (`IssuedAt`, `RevokedAt`, `ScannedAt`), `CardScanLog.StationName` free text instead of `WorkstationId`, no index on `CardScanLog.CardId`. In the same migration as R6.1: add `WorkstationId int NULL` FK (keep `StationName` for history), add `IX_CardScanLog_CardId`. Renaming columns is optional — propose it to Osama with the cost; default is to document the exception in DATABASE.md.
- [ ] **R6.3 [Verified] Audit columns use the client clock.** `AuditSaveChangesInterceptor` uses `TimeProvider.GetUtcNow()`. BR-05 says the database clock is the only clock. Add `SYSUTCDATETIME()` defaults for `CreatedAtUtc`, and have the interceptor take the time from the unit of work's DB time when one was already fetched in the transaction; otherwise leave the default.
- [ ] **R6.4 [Verified] `DATABASE.md` is behind the model.** Document `SyncOutboxes`, `GiftVouchers`, the new `Organizations` and `AgeCategories` columns, entry-card tables, all check constraints present in the snapshot, and the settings keys added since October. Add an integration test that compares the documented table list with the EF model.
- [ ] **R6.5 [Verified] [Blocked: Q-R6] Least-privilege login is documented but not implemented.** `Setup-ProductionDatabase.ps1` stores a `Trusted_Connection` string for all clients and opens ports 1433/1434 with `profile=any`. Create `parkgate_app` (reader, writer, execute) and a separate migration login in the script, store the app login in `connection.dat`, restrict the firewall rules to `profile=private,domain`. After R6.1 the app needs no DDL rights.
- [ ] **R6.6 [Reproduce] Name search cannot use its index.** `BeneficiaryRepository.SearchAsync` uses `LIKE '%term%'`. Re-run the T8.5 measurement (100 000 holders). If over 1 s, switch to prefix search on normalized name tokens or add full-text; if under, document the result and close.
- [x] ~~**R6.7 Business-day cutoff.**~~ Cancelled (Q-R5): the park closes at midnight, so the calendar date is the business date. Record this in SPEC Q10.
- [ ] **R6.8 Retention.** `AuditLogs`, `CardScanLog` and denied `EntryAttempts` grow without limit; SQL Express caps a database at 10 GB. Add a size check to the Admin backup screen (warn at 70 %) and propose a retention policy to Osama. No deletes without his approval (BR-14 covers business data, not logs).
- [ ] **R6.9 Backups contain personal data in clear.** (More important now that the park database is the only copy of phones and national IDs.) SQL Express has no backup encryption. Have `scripts/backup.ps1` write to a folder with an ACL limited to Administrators and document an encrypted off-site copy in DEPLOYMENT.md.
- [ ] **R6.10 Cloud schema.** The cloud schema is unreleased, so edit the existing Laravel migration instead of stacking new ones. Add the `gift_vouchers` table (R2.7), composite index `entry_attempts(business_date, result, is_voided)`, and drop single-column indexes on low-cardinality columns (`result`, `is_voided`, `status`).

**Done when:** a fresh install and an upgrade from the 2026-11-02 schema both reach the same schema, the app runs under `parkgate_app` without DDL rights, and DATABASE.md matches the model.

---

## Phase R7 — Wording and visual consistency · branch `phase/r7-consistency`

- [ ] **R7.1 [Decided: Q-R4] One glossary.** The card holder is **«الزائر»** (plural «الزوار», for example menu «الزوار», «زائر جديد (F2)», «الإجمالي مع الزائر»). Replace «زبون/الزبون/الزبائن» (26 strings) and «مستفيد» (14) in `Strings.resx`, the web views, `USER_GUIDE_AR.md`, `TEST_SCRIPT.md`, and SPEC §3. Review each sentence for gender and plural agreement; do not blind-replace. Code identifiers stay `Beneficiary`. The other mixed terms («فترة التهدئة»/«فترة الانتظار», «محطة»/«شباك», «قسيمة»/«كوبون»/«وصل», «شباك الدخول»/«بوابة الدخول») wait for Q-R4b; use the recommended defaults if Osama does not answer by the time R7 starts, and list them in the PR description.
- [ ] **R7.2 One type scale.** Designer files use seven sizes between 9 and 12.5 pt. Map them to `Theme`: Caption 9, Body 10, Section 12, Title 14, plus Banner 20. Change values in designer files only; no logic there.
- [ ] **R7.3 One neutral palette.** Two gray families are mixed (`#2D3748/#EDF2F7/#CBD5E0` and `Theme`'s `#1E293B/#E2E8F0`) plus `#455A64`. Replace literals with the `Theme` values. Add a test that fails on `Color.FromArgb` literals outside `Theme.cs`, `StatusBanner.cs` and designer files that match `Theme`.
- [ ] **R7.4 Dialog conventions.** Every dialog sets `AcceptButton` and `CancelButton`, has explicit tab order, and uses `Theme.ApplyPrimaryButtonStyle` / `ApplySecondaryButtonStyle`. Extend `RtlFormTests` to check this for all forms.
- [ ] **R7.5 Web UI.** Same glossary; show times in park-local time with the «ص/م» format; empty states for dashboard cards; no phone or national ID anywhere (R2.6).

**Done when:** a grep for the retired terms returns nothing in UI text and docs.

---

## Phase R8 — Build, packaging and deployment · branch `phase/r8-release`

- [ ] **R8.1 [Verified] Wrong server path in the published `appsettings.json`.** `scripts/Build-Production.ps1` assigns `"Server=.\\SQLEXPRESS;…"` in PowerShell, where `\\` is two characters; the file ends up with a doubled backslash. Use a single backslash. Add a Pester or simple script check that reads the output JSON and asserts the server value.
- [ ] **R8.2 [Verified] Vulnerability audit is disabled** (`--property:NuGetAudit=false` in the production build) while the handoff reports 16 warnings for `System.Security.Cryptography.Pkcs` 9.0.11. Find the package that pulls it in, pin a fixed version in `Directory.Packages.props` (transitive pinning is already enabled), remove the `NuGetAudit=false` flags. Ask before adding any new package (AGENTS.md §2).
- [ ] **R8.3 Sync Agent secret.** `CloudApi:ApiKey` is read from plain JSON in `%ProgramData%\ParkGate\appsettings.local.json`. Store it DPAPI-protected next to `connection.dat`, with a small CLI switch on the agent to set it (`ParkGate.SyncAgent.exe set-cloud-key`).
- [ ] **R8.4 Rebuild the release.** Bump to `v1.1.0`, rebuild the client, migration bundle and installer with `Build-Production.ps1` + `build-installer.ps1`. Delete the stale `artifacts/efbundle.exe`. Do not commit binaries.
- [ ] **R8.5 [Decided: Q-R1] cPanel is the only web deployment path.** Remove `.github/workflows/deploy-web.yml`, `web/docker/`, `web/docker-compose.prod.yml` and their mentions in `web/README.md`. Make `scripts/package-cpanel.ps1` the release path: it runs the web tests, builds assets, generates `mysql_schema.sql` (R2.9), excludes `.env`, tests and dev files, and produces the zip. Write `docs/DEPLOYMENT_WEB_CPANEL.md`: PHP version, document root pointing at `public/` (or the root `.htaccess` rewrite), creating the MySQL database, `.env` values, `APP_KEY`, the API key, creating the first user (R1.1), and the cron entry if the queue or scheduler is used.
- [ ] **R8.6 Upgrade guide.** Add an "Upgrade from 1.0.0" section to `docs/DEPLOYMENT.md`: backup, run the migration bundle with the migration login, install the client, set the new SQL login, set the cloud API key, verify the sync status.

**Done when:** a clean Windows machine upgraded from the 1.0.0 installer passes `docs/TEST_SCRIPT.md`, and Osama tags `v1.1.0`.

---

## Phase R9 — Documentation sync · branch `phase/r9-docs`

- [ ] **R9.1 SPEC.md** — add F1 (as built: signed tokens, not the opaque-hash design), F3, F4, F5 rules (BR-19 onward, including the companion limit and gift rules now only in `F5_GIFT_VOUCHERS_PLAN.md`), fix the A5/BR-11 mismatch (override for suspended holders), and close answered questions.
- [ ] **R9.2 DECISIONS.md** — mark ADR-013 as superseded by the signed-token design; add ADRs for R2.1/R2.2 (outbox immutability and sequence ordering), R3.2 (supervisor approval tokens), R6.1 (entry cards under migrations), R6.5 (SQL login model).
- [ ] **R9.3 AGENTS.md** (needs Osama's approval) — project count is now 5 production projects plus `web/`; add the web test command to the session protocol; rename the misleading note that UI tests live in `ParkGate.ArchitectureTests`, or move them to a new `ParkGate.WinForms.Tests` project (ask first).
- [ ] **R9.4 USER_GUIDE_AR.md and TEST_SCRIPT.md** — new shortcuts, registration with organization choice, void button, glossary.
- [ ] **R9.5 SESSION_HANDOFF.md** — final state, known issues, next steps.

---

## Appendix — Things reviewed and found sound (do not rework)

- Atomic check-in: `UPDLOCK, ROWLOCK` on the holder row inside a read-committed transaction, policy evaluated in Domain, covered by a parallel integration test.
- Password hashing (PBKDF2-SHA256, 600 000 iterations), first-run admin wizard, DPAPI-protected connection settings.
- QR signing: ECDSA P-256, non-exportable CNG key, separate signing context for gift vouchers, public keys only on verifying desks.
- Check constraints on `EntryAttempts`, `GiftVouchers`, `Organizations`; filtered unique indexes; restrict-delete foreign keys.
- API key comparison with `hash_equals`; batch ingestion in a single transaction.
- Secrets (`.env`, `connection.dat`, `appsettings.Local.json`) are git-ignored and not tracked.
