# Session Handoff

> **Read at the start of every session. Update at the end of every session** (AGENTS.md §1).
> Keep "Snapshot" and "Next session" accurate — they are overwritten each time. The log keeps the last 10 sessions only.

---

## Snapshot

| Field | Value |
|---|---|
| Last updated | 2026-11-20 |
| Last session | S44 — R9.3, R9.5 on `phase/r9-docs`; R9 PR opened |
| Current phase | Phase R9 done (PR waiting for review). Next: production rollout of the desktop app 1.1.0. Web work is paused (Osama, 2026-11-20) |
| Working branch | `phase/r9-docs` (from `main` `441b417`, after PR #25); pushed, PR open. All code is already on `main` |
| Build | `dotnet build ParkGate.sln -c Debug`: 0 errors, 0 warnings (the Pkcs advisories are gone, R8.2) |
| .NET Unit tests | 1066 passed (non-integration): 264 domain + 283 application + 519 architecture |
| .NET Integration tests | Infrastructure project 178 passed on LocalDB (integration and non-integration together) |
| Web tests | `php artisan test`: 101 passed. Needs PHP >= 8.4.1 now (R8.7); the herd-lite `php` 8.4.0 on PATH is too old, PHP 8.4.27 from WinGet works |
| Total tests | 1244 .NET (incl. integration) + 101 web |
| Latest migration | `AddCardPatterns` (S41, F8: `OrganizationCardPatterns`, `CardPatternAlerts`). **Not yet applied** on LocalDB `ParkGate_Dev` or SQLEXPRESS `ParkGate`; Osama runs `dotnet ef database update` as for `AddOrganizationEntryDays` |

## State of the work

**Completed in S33 (on `phase/r4-desk-ui`), one commit per task, each test-first:**

- R4.1 `BusyScope` restores each control's previous state; `UpdateActionButtons()` is the one place that decides
  record/override.
- R4.3 Registration panel has a required organization combo (`_cmbRegOrganization`), preselected: filter, else default.
- R4.4 Card only in another organization: amber «البطاقة مسجلة لدى جهة: X»; a second Enter switches the filter and loads it.
- R4.5 Esc asks before discarding a half-filled registration; with an open dropdown it only closes the list.
- R4.6 New designer-backed `SelectOrganizationDialog`; cancelling picks nobody (neutral banner).
- R4.9 No invented age categories: a load failure blocks recording and the next lookup retries.
- R4.7 `UserMessages.Describe/LogAndDescribe`: no `ex.Message` anywhere in WinForms (architecture test).
- R4.2 Desk is freed right after a granted entry; a scan during the 1.2 s banner is kept; lookups are queued.
  **Deviation (needs Osama):** text typed by hand without Enter during the banner is kept but not looked up.
- R4.8 117 Arabic literals moved to `Strings.resx` (new keys `Ui_<Class>_<NN>`; R7.1 may rename them). Architecture
  test fails on Arabic in non-designer, non-comment WinForms code.
- R4.10 `AppFont.IsCairoInstalled` + `FontInstallWarning`: log always, show admins once per run.
- `docs/TEST_SCRIPT.md`: TC-14 to TC-21 (R4 flows and R3.2 supervisor tokens).

**Completed in S32 (on `phase/r3-application`), one commit per task, each test-first:**

- R3.1 Desk totals from a SQL aggregate (`IEntryAttemptRepository.GetTodayTotalsAsync`); the grid list loads 20 rows.
- R3.3 `Actor.Resolve` returns `General.Unauthorized` / `Auth.WorkstationNotFound` instead of `?? 1` / `?? 0` ids
  (check-in, override, void, today's totals, gift issue/override/redeem). Existing codes reused: no new UI text.
- R3.4 `AuditDetails.ToJson` serializes all 16 audit details (quotes, backslashes, culture-free numbers, readable Arabic).
- R3.5 Check-in and gift audit rows go through `uow.AuditLogs` before `CommitAsync`; both services no longer take
  `IAuditWriter`. Gift issue saves once for the receipt number, then adds the audit row and saves again.
- R3.6 `CheckInService` reads `ISettingsService` before the row lock. **Behaviour change:** `SettingsService` cache now
  expires after one minute, so a change saved on another desk applies there within a minute (before: next query).
- R3.7 / R3.10 New `ParkClock` (DB UTC + `Park.TimeZoneId`). `CardScanService` expiry and `EntryCardIssuer` validity use it.
  R3.10 was found during R3.7 and added to the roadmap.
- R3.8 `LookupByCardNumberAsync(card, organizationId)` returns `CardLookupDto` (`Matches`, `RegisteredElsewhere`).
  The desk passes its selection; showing «مسجلة لدى X» is R4.4, so the desk still offers inline registration.
- R3.9 Application already returned `NextAllowedEntryUtc` and `RemainingCooldownMinutes`; tests pin them. The banner's
  `/ 60` is R5.1's (cooldowns are entered in whole hours, so no wrong value shows today).
- R3.2 `IAuthService.AuthorizeSupervisorAsync` + `SupervisorApprovalStore` (single-use, 2 min, bound to the desk);
  the four supervisor use cases take a token. `SupervisorAuthDialog` lost its approve-as-user-1 fallback. ADR-023 (Proposed).

**Completed in S31 (on `phase/r2-sync`):**

- R2.9 MySQL parity. Ingestion converts every ISO-8601 instant to UTC `Y-m-d H:i:s` (`SyncIngestionService::utc`);
  offsets used to be stored as UTC wall time. `php artisan parkgate:generate-mysql-schema` writes
  `web/database/mysql_schema.sql` from the migrations in pretend mode, with no database server
  (`App\Support\MysqlSchemaGenerator`); `MysqlSchemaTest` fails if the committed file drifts; `scripts/package-cpanel.ps1`
  runs the generator first. New CI job `test-mysql` in `.github/workflows/web.yml` loads the schema into MySQL 8 and runs
  the suite in strict mode (green on GitHub). It found one test-data bug (a 37-character id that SQLite accepted).
- **Seed rows removed from `mysql_schema.sql`.** The old hand-written file inserted three organizations and three age
  categories. Ingestion upserts by `id`, so a seeded row with another id and the same `code` would have broken the unique
  index. The park sends the real rows through the sync (R2.7 re-queues them). Osama: confirm this is acceptable.
- R2 "Done when" end-to-end test: `SyncEndToEndTests` (LocalDB) runs the real outbox, worker and `CloudPushService`
  against an in-process cloud that drops the connection, then answers 503 four times, then recovers. It checks that
  nothing is lost or consumed while failing, that the cloud ends with the local data, and that no request, failed ones
  included, carries phone, national ID, voucher token or key id. Mutation check: re-including `GiftVoucher.Token` made it fail.

**Completed in S30 (on `phase/r2-sync`):**

- R2.7 Gift vouchers sync (ADR-022, Proposed). `GiftVoucher` is in `SyncableTypes`; `Token` and `KeyId` are excluded
  from its payload (the token is the redeemable QR secret). Organization gift policy / `MaxFreeCompanions` and
  `AgeCategory.IsCompanionLimitExempt` were already serialized; the web now stores them. Migration
  `RequeueGiftAndPolicyOutbox` queues one row per existing organization, age category and voucher. Web: `gift_vouchers`
  table (existing migration edited in place), `GiftVoucher` model, ingestion (after `Beneficiary`),
  `GiftSummaryService` (SQL aggregation by issue business date) feeding a `gifts` prop on the dashboard and reports,
  shared `GiftSummary.vue`. `mysql_schema.sql` hand-edited again (R2.9 will generate it).

**Completed in S29 (pushed on `phase/r2-sync`), one commit per task, each test-first:**

- R2.6 Phone and national ID never leave the park (ADR-021). `OutboxEntityExtractor` has an explicit exclusion
  list; `PayloadEncryptionService`, `AesHelper`, `AesKey` and `PARKGATE_AES_KEY` are gone. Migration
  `PurgeSensitiveBeneficiaryOutbox` deletes pending Beneficiary rows and re-queues one clean row per holder.
  Web: columns, model, search, Vue columns and seeder data removed. Ingestion now rejects a payload that is not a
  JSON object (422) instead of skipping it.
- R1.8 Web tests that nothing serves `phone` or `national_id`.
- R2.1 `SyncOutbox.UpdatePayload` removed; payload is sent exactly as stored.
- R2.2 `SyncOutboxes.Sequence` (bigint IDENTITY, migration `AddSyncOutboxSequence`); ordering and dedupe use it.
- R2.3 `CloudPushService` refuses sensitive keys at any depth and non-HTTPS `BaseUrl`
  (`CloudApi:AllowInsecureHttp` for local dev). New `PushFailureKind.Misconfigured` aborts the cycle without
  burning retries.
- R2.4 Web ingestion sorts a batch parent-first (deletes last, child-first).
- R2.5 **Finding:** the agent never sent a cloud heartbeat (only a local setting), so the dashboard could not show
  the park online. `HeartbeatWorker` now counts pending/failed rows, stores them in AppSettings and posts them to
  `/api/sync/heartbeat`. Web stores and shows them. Desk: admins (ManageSettings) get a sync line in the status
  bar (`SyncStatusService`), hidden until the agent has reported.
- R2.8 Dashboard: "today" follows `PARKGATE_TIMEZONE` (default Africa/Tripoli); entries/persons count granted and
  overridden, exclude voided; totals aggregated in SQL; hourly chart in park-local hours.
  Deviation from the plan text: the app timezone stays UTC (changing it would make every `*_utc` column be read as
  Tripoli time); the park zone is a separate config value.

**In progress**
- None.

**Left in Phase R2**
- Nothing. PR #13 merged into `main` (`aefbdcd`) on 2026-11-13 after Osama's go-ahead; build, web and test-mysql were green.

**Not started / Future**
- F2 Electronic gate (deferred by client; SPEC §9 F2).

## Next session — start here

Run sessions on the dev machine (`claude` in `C:\dev\ParkGate`), not from the cloud (see S26 log).

**Priority (Osama, 2026-11-20): the desktop app goes to production and real-world trial. Web work is paused** — do not start T8.5 or other `web/` tasks until he says so.

**S44 (2026-11-20):** R9.3 `AGENTS.md` (five production projects with `ParkGate.SyncAgent`; UI tests live in `ParkGate.ArchitectureTests`; the web test command was left out on Osama's request). R9.5 this handoff. R9 PR opened (`phase/r9-docs` → `main`, docs only).

**Next, in order:**
1. Osama reviews and merges the R9 PR.
2. **Rebuild the installer from `main`** (`scripts\build-installer.ps1`). The S40 build of 1.1.0 lacks T8.7 (tab-control crash at 125 %), F8 card patterns and R6.5; do not ship it.
3. **Production rollout**, following `docs/DEPLOYMENT.md`: server mode on the real server (SQL Server Express, `Setup-ProductionDatabase.ps1` for migrations and `parkgate_app`, §3–§4), then one desk (§5, network must be Private), first-run admin (§6), readiness check (§7). Upgrading an existing 1.0.0 install: §10. Backups: `scripts\backup.ps1` as Administrator (§9).
4. **Before opening to visitors:** set each organization's entry days in «الجهات» (all days allowed by default), enable gifts per organization where needed, and run the manual tests on the real machines: TC-36..TC-39, TC-32 (1366×768), TC-38 (125 %), TC-20 (Cairo missing), the R7.12 slips on the 80 mm printer.
5. Collect trial feedback; each problem becomes a `fix/<name>` branch with a failing test first.

## Known issues (as of 1.1.0)

- The cloud sync agent sends `AllowedEntryDays` and `GiftAnyDay` nowhere yet (T8.5, paused with the web); the local desktop app is not affected.
- No purge command for the approved retention periods (DEPLOYMENT.md §9.3); add it as a task when needed.
- Q-R7 (lowest desk resolution) is open; 1366×768 at 100 % is assumed.
- `main` is not protected; a PR can be merged with red checks (GitHub branch protection, Osama's setting).
- `dotnet ef database update` is refused by the agent's tool classifier; Osama applies migrations on the dev databases himself (`AddCardPatterns` still pending on LocalDB `ParkGate_Dev` and SQLEXPRESS `ParkGate`).
- WinForms tests: never let an awaited continuation touch a control whose handle exists on another thread (hangs the test host); use synchronous fakes or seams, and run `dotnet test` with `timeout`.
- Edit files with the Edit tool or byte-level Python; `sed -i` turns CRLF into LF.

## Blockers / waiting on the user

**Desktop (current priority):**
- Merge the R9 PR; rebuild the installer from `main`; install on the real server and one desk (see Next session).
- Run TC-36..TC-39 on the real machines; approve **ADR-022, ADR-023, ADR-027** (still "Proposed").
- Confirm F8 design choices: card patterns (`Source` column, "more than half", approval closes alerts) and the BR-27 check order.
- Apply migration `AddCardPatterns` on the dev databases.
- Print one entry card and one gift voucher on the 80 mm thermal printer.
- Confirm the R7 wording («فترة الانتظار», «الشباك», «القسيمة»), the one-minute settings cache (R3.6) and the R4.2 deviation.
- Delete the empty test folder `C:\ParkGate_R69_Test`.
- Keep the bundled runtime current: raise `-RuntimeVersion` in both build scripts when a new .NET 10 servicing release ships.

**Web (paused by Osama, 2026-11-20 — keep for later):**
- T8.5 cloud sync of the two organization fields; deploy from the cPanel package (host PHP >= 8.4.1); `set-cloud-key` on the server; regenerate `APP_KEY` and the sync key; `CloudApi:BaseUrl` over `https://`; re-import `web/database/mysql_schema.sql` on an existing cloud database.
- Local PHP: herd-lite 8.4.0 on PATH is too old; PHP 8.4.27 works.
- When web work resumes, add to `AGENTS.md` §1.1 (needs approval): `php artisan test` in `web/` and the npm 10 rule for lock files (S42 log).

---

## Session Log (last 10 sessions)

### S44 — 2026-11-20 — R9.3, R9.5; R9 PR
- Baseline green (1175 non-integration). Osama: web paused, desktop app to production first; the web items of R9.3 were dropped.
- Handoff rewritten around the production rollout; old desktop and web blockers split.

### S43 — 2026-11-20 — R9.1, R9.2, R9.4
- Checking role permissions with `awk` on `RolePermissions.cs` gave a wrong "0"; read the file instead. The first SPEC edit said only Admins issue entry cards and was corrected.
- The old user guide had two sections numbered 8, said backups are compressed (Express cannot, R6.9) and listed shortcuts that no longer matched.

### S42 — 2026-11-20 — CI fix and R6.5
- Root cause of the red web jobs: npm 11 rewrote `package-lock.json` without an optional peer (`react`) that npm 10 needs; regenerate with `npx npm@10 install --package-lock-only` and verify with `npx npm@10 ci` before pushing lock changes.
- PowerShell: `"$name:"` inside a string is a drive-qualified variable; use `${name}:`.
- LocalDB accepts SQL logins (`IsIntegratedSecurityOnly` = 0), so the login script was tested there with `-ConnectionFile` in the scratchpad and `-SkipFirewall`; the test database and login were dropped afterwards.

### S41 — 2026-11-20 — F8 card-number patterns; RtlTabControl crash
- Q-R6 answered: workgroup, at most 4 desks.
- `DatabaseDocumentationTests` skips any table row whose notes contain the word "proposed"; the `SampleCount` note had to be reworded.
- EF filters a unique index on a nullable column (`[Prefix] IS NOT NULL`) by default; `HasFilter(null)` keeps one pattern per shape without a prefix.
- The real-screen probe hung on a modal "Error creating window handle". Unit tests at 96 dpi never reproduce it; the probe's generic path creates windows off-screen at (-20000, -20000), where no DPI scaling happens, so earlier probes missed it. New probe switches: `PROBE_ONSCREEN=1` (create on screen) and a `ThreadException` handler that writes `error.txt` and exits.
- Grid cells with card numbers and shapes use LRE/PDF, or "AA-##" shows as "##-AA" in an RTL cell.

### S40 — 2026-11-20 — Phase R8 (release 1.1.0)
- PR #22 merged. Baseline green (1096 unit).
- R8.2: pinning Pkcs 10.0.12 did not clear the advisory; 10.0.15 did. The WinForms client does not use the package: its Pkcs comes from the Windows Desktop runtime pack, so self-contained builds pin `RuntimeFrameworkVersion` too.
- With the pinned runtime, `dotnet ef migrations script --no-build` failed ("install or update .NET"): the client publish rebuilt Infrastructure for 10.0.15. The script step now builds normally.
- R8.5 found 9 Composer advisories in production packages (R8.7). Symfony 8.1.11 needs PHP 8.4.1; the herd-lite PHP is 8.4.0, so composer and the web tests ran on PHP 8.4.27 with a scratch `php.ini` (`PHPRC`).
- `dotnet ef database update` is still refused by the tool's classifier; not needed this session.

### S39 — 2026-11-20 — R5.15–R5.20; F8 entry days (T8.1a–T8.4a)
- R5.15–R5.20 committed on `phase/r5-desk-ux`; Osama merged PR #21. F8 started from `main` on his request; patterns split off as T8.xb.
- `GiftDenialReason` 6 was already `NoEntryToday` (BR-30), so the gift `DayNotAllowed` is 7; DATABASE.md §2 corrected.
- The seeded SSF organization needed the new values in `HasData`, or the migration got an empty `UpdateData`.
- `dotnet ef database update` was refused by the tool's safety classifier; the migration is only applied on the integration test databases.
- Real-screen probe (`layoutprobe orgdlg`, built with `--artifacts-path art`): the dialog had to grow to 620 px so the seven days fit on one line; the gift policy box was already clipping its last row and got 20 px more.
- R5.20 left seven CS8604 warnings in `AuditLogForm`; fixed.

### S38 — 2026-11-18 — Phase R5 complete (R5.1–R5.14)
- Baseline green (885 unit). PR #20 opened for the two R7 commits missing from `main`. Tests: architecture 340 → 379.
- Layout probe copied to this session's scratchpad; new `PROBE_SCREEN=1` takes a real screen capture (`CopyFromScreen`), because `DrawToBitmap` does not show every text fault. Bounds dumps found R5.11; screen captures found R5.12 and R5.13.
- R5.13 dead ends: `Cairo.ttf` is a variable font, but a static instance (fontTools, loaded only in the probe process) clips the same way; the native `FlatStyle.System` button clips too; pixel tests at the test host's 96 dpi do not reproduce the button fault (the group box one they do).
- R5.10: `MainForm` takes `TimeProvider`; `TestUi` already resolves it.
- `sed -i` turned edited files into LF again; re-checked out after each commit.
- R5.13 fix: the subclasses return an empty `Text` while the base paints, then draw the text themselves; the probe's `PROBE_SIZE` (desk at 1.25× the 100 % size) and `PROBE_TAB` options drove R5.8. Write multi-line Python scripts with the Write tool: Bash heredocs swallow backslashes.

### S37 — 2026-11-18 — Phase R7 (all but R7.12)
- PR #17 was merged; `phase/r7-consistency` from `main`. Baseline green (723 unit). Tests: .NET unit 723 → 865, web 98 → 101, integration 56.
- R7.11 found by Osama (QR save threw `ThreadStateException` under the debugger). R7.12 and R7.13 were added during R7.
- Visual Studio had the app open and locked `bin\`; everything was built with `--artifacts-path artifacts/verify`.
- Layout probe (scratchpad only): renders every form/control off-screen at the machine DPI (120 here), saves PNGs and lists clipped text and overflow. It drove R7.8, R7.2 and R7.13. Arabic text that wraps can break inside a word in WinForms labels: keep such text on one line.
- R7.9: removing parameterless constructors exposed that the desk's `Load` reloads organizations and age categories; tests that seed them now pass fakes returning the same data.
- Bash `sed -i` writes LF; re-checkout such files (`rm f; git checkout -- f`) after committing.

### S36 — 2026-11-18 — Phase R6 complete (except R6.5, R6.16); R6 PR
- Baseline green (701 unit). One commit per task, test-first.
- R6.11: 30 audit calls in 7 services moved into the unit of work; creates that audit a new identity id save twice in one transaction; settings saved in one save (the store did ten saves with the client clock). Card number change audits old and new number.
- R6.3 finding: the audit interceptor was never added to the context options. Registered in `DynamicParkGateDbContextFactory` (one shared instance: EF caches internal services per interceptor instance).
- R6.4: the new drift test found six unnamed CHECK constraints and two F8 columns documented as built; found R6.15 (dead seeded setting key).
- R6.6: no query change needed (~350 ms). R6.10: `gift_vouchers` already existed (R2.7); only indexes changed.
- R6.9 finding: `backup.ps1` used `COMPRESSION`, which Express rejects, so the scheduled backup could never succeed on the target edition.
- Tip: the Write tool strips trailing spaces in text it writes; byte-exact Python edits need care with blank lines that hold spaces.
- After Osama agreed: R6.16 (mask in Application, seed row deleted), retention approved, backup ACL fix. R6.15 (migration `RemoveUnreadSoundSetting`; the new key is not seeded because saved desks already have it), `IAuditWriter` removed, R6.8 (option A). Found R6.16 (mask setting never read).
- Tests: .NET 755 → 773 (integration 54 → 56), web 96 → 98.

### S35 — 2026-11-18 — Phase R6: R6.1, R6.2, R6.12, R6.13, R6.14
- See "Next session" for the summary. Tests 755 green.
