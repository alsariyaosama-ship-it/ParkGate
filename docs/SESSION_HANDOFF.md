# Session Handoff

> **Read at the start of every session. Update at the end of every session** (AGENTS.md §1).
> Keep "Snapshot" and "Next session" accurate — they are overwritten each time. The log keeps the last 10 sessions only.

---

## Snapshot

| Field | Value |
|---|---|
| Last updated | 2026-11-18 |
| Last session | S38 — Phase R5: everything done except R5.8, R5.9, R5.13 (waiting on Osama); PR #20 open (R7.12/R7.14 to `main`) |
| Current phase | Phase R — Remediation. R0–R4, R6, R7 merged (PR #11–#15, #17, #18). PR #19 went into the R7 branch; PR #20 brings R7.12 and R7.14 to `main`. R5 on `phase/r5-desk-ux` |
| Working branch | `phase/r5-desk-ux` (from `phase/r7-consistency` `62a1e1c`, so it already holds R7.12 and R7.14) |
| Build | `dotnet build ParkGate.sln -c Debug`: 0 errors, 16 warnings — all NU1902/NU1903 for `System.Security.Cryptography.Pkcs` 9.0.11 (R8.2) |
| .NET Unit tests | 943 passed (non-integration): 210 domain + 240 application + 95 infrastructure + 398 architecture |
| .NET Integration tests | 57 passed on LocalDB |
| Web tests | `php artisan test` (PHP 8.4): 101 passed on SQLite (MySQL job runs in CI) |
| Total tests | 1101 (1000 .NET + 101 web); integration and web not re-run in S38 (no Infrastructure or web change) |
| Latest migration | `RemoveUnusedMaskSetting` (S36), after `RemoveUnreadSoundSetting` and `MapDatabaseFilesView` (snapshot only). Both applied on LocalDB `ParkGate_Dev` and SQLEXPRESS `ParkGate` |

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

Run Phase R sessions on the dev machine (`claude` in `C:\dev\ParkGate`), not from the cloud (see S26 log).

**S38 (2026-11-18, `phase/r5-desk-ux`): R5 in progress.** Found at start: PR #18 was merged into `main` before R7.12 and R7.14 reached the R7 branch, and PR #19 was merged into the R7 branch, so `main` lacks them. Opened **PR #20** (`phase/r7-consistency` → `main`). R5 was branched from the R7 branch.
Done, one commit each, test-first: R5.1 denial banner «يُسمح بالدخول الساعة … — باقي س:دد» (`CooldownText`); R5.2 no message box after registration; R5.3 name 14 pt, card box 16 pt (`Theme.CardNumber`, the one size added to the R7.2 scale); R5.11 search row in a `TableLayoutPanel` (the card box ran under its label at 125 %); R5.12 `RtlGroupBox` (every RTL group box caption lost its last letters); R5.6 «إلغاء العملية… (F8)» above today's grid; R5.10 idle auto-lock (`Security.AutoLockMinutes` was saved but never read; `IdleMonitor`); R5.14 gift offered right after a granted entry for the same visitor (F6, no second scan); R5.5 «(من آخر زيارة)» and tinted counters; R5.4 Alt+1…9, +/−, Enter in a counter (deviation: focus stays in the card box after a lookup, for the scanner); R5.7 two generated tones (`DeskSounds`), new setting `Entry.SuccessSoundEnabled` (not seeded, as R6.15). R5.8 measured only (see below).
**Next:** Osama merges PR #20, answers the questions below, and runs TC-23 to TC-31. Then R5.8, R5.9 and R5.13 as he decides, then the R5 PR. R6.5 waits on Q-R6.

**S37 (2026-11-18, `phase/r7-consistency`): R7 done; PR #18 opened.** One commit per task, test-first:
R7.6 QR through `IQrImageRenderer`; R7.11 `Main` synchronous (it was `async`, so the UI ran on an MTA thread and file/print dialogs threw); R7.7 every message box through `Notify`; R7.8 all designers on `Dpi`; R7.9 one constructor with required services per form/control, no `?? new X(null…)` fallbacks, `TestUi.Create` fills fakes (NSubstitute now in the architecture tests), new ports `ICardScanService` / `IEntryCardIssuer`; R7.10 `InlineRegistrationPanel` and `TodayEntriesPanel` split from the desk; R7.1 glossary («الزائر», «فترة الانتظار», «الشباك», «قسيمة»), `GlossaryTests`; R7.2 type scale 9/10/12/14/20; R7.3 palette in `Theme`, `PaletteTests`; R7.4 Accept/Cancel/tab order/Theme buttons in every dialog; R7.5 web: park-local times (the dashboard and reports printed UTC), glossary, empty chart state; R7.13 clipped text at 125 %.
R7.12 (after Osama's wording, 2026-11-18): entry card and gift voucher print on 80 mm cashier paper through `ReceiptSlips` / `ReceiptPrinter` (company, park, QR, visitor's organization, slip text; no logo, no personal data on the entry card, voucher number but no value on the gift slip, 20 mm cutter feed).
R7.14 (Osama, 2026-11-18): a gift needs gifts enabled for the organization **and** a granted entry today (BR-30, not overridable). PR #19 from `fix/gift-requires-entry`, based on the R7 branch.
**Next:** Osama prints both slips on the thermal printer, reviews and merges PR #18 and PR #19, then **R5** (desk UX; first item: offer the gift right after a granted entry without a second scan). R6.5 waits on Q-R6.

**S36 (2026-11-18, `phase/r6-database`): R6.3, R6.4, R6.6, R6.8, R6.9, R6.10, R6.11, R6.15 done; R6 PR opened.** Management audit rows are saved in the same unit of work (`UnitOfWorkAuditExtensions.AddAuditAsync`) and `IAuditWriter` is gone (AGENTS.md updated with Osama's OK); `AuditSaveChangesInterceptor` was never registered, it now runs on every context and takes the DB clock; `DatabaseDocumentationTests` keeps `DATABASE.md` in line with the EF model; name search ~350 ms at 100 000 holders (kept); cloud `entry_attempts` composite index; `backup.ps1` restricts its folder and no longer asks Express for `COMPRESSION`; the backup message shows the database size against 10 GB and warns at 70 % (`DatabaseFileRow` → `sys.database_files`, ADR-025 amendment); dead seed row `Entry.PlaySoundOnDenial` deleted.
Later in S36: R6.16 (operators get national IDs masked by Application; new permission `ViewFullNationalId` for supervisors and admins), retention periods approved by Osama (DEPLOYMENT.md §9.3; no purge feature built), backup ACL step fixed after Osama's elevated run failed on an old log file.
**Next:** Osama re-runs `scripts\backup.ps1` as Administrator, then reviews and merges PR #17. Then **R7.6–R7.9** (agreed order below). R6.5 waits on Q-R6. A purge command for the approved retention periods is not built; add it as a task when Osama wants it.

**S35 (2026-11-18): R6.1, R6.2, R6.12, R6.13, R6.14.** Entry-card tables are EF entities (migration `AdoptEntryCards`); the DB clock is a mapped `SYSUTCDATETIME`; optimistic concurrency with `ConcurrencyRetry` replaced `UPDLOCK` (migration `AddBeneficiaryLastGiftIssuedUtc`); `EfOnlyDataAccessTests` guards ADR-025. Both dev databases were cleaned of the old entry-card objects with Osama's approval (option A) and migrated.

1. TC-20 (Cairo missing) is still untried, and TC-21's F7 gift override should be re-run on the fixed gift screen. New TC-22 (database size after backup).
2. Agreed session order (review 2026-11-18): **R6** (done) (database: entry-card tables into migrations, DB-clock audit columns, R6.11 audit in the unit of work) -> **R7.6-R7.9** (UI-layer conformance: no Infrastructure in WinForms, Notify only, Dpi everywhere, no null-service fallbacks) -> **R5** (desk UX, with R7.10 split of `CheckInDeskControl` first) -> rest of **R7** -> **R8** (release) -> **F8** (only on Osama's go-ahead) -> **R9** (docs last). Reason: F8 adds a migration and organization fields, so the schema conventions (R6) and the UI layer (R7) should be settled first.
   Gift decisions confirmed 2026-11-18: the interval starts when the voucher is issued, and the gift voucher stays a separate QR slip (current F5 behaviour, no change).
3. WinForms tests: never let an awaited continuation touch a control whose handle exists on another thread — it
   hangs the test host. Use synchronous fakes or a seam (`SuccessDelay`, `ChooseOrganizationOverride`), and run
   `dotnet test` with `timeout`.
4. Edit files with byte-level Python (or the Edit tool); text mode turns CRLF into LF.

Carried over from S25 (do it when a printer is available; it does not block Phase R):
- Verify QR printing on the user machine with a connected printer or "Microsoft Print to PDF", and confirm
  with Osama that customer and gift QR printing works without closing the app.

## Blockers / waiting on the user

- **Merge PR #20** (R7.12 and R7.14 to `main`; already reviewed in PR #19).
- **R5.13 decision:** buttons, check boxes and tabs cut the last Arabic letters at 125 % («عرض التقر»). `AutoEllipsis` and compatible rendering did not fix it reliably. Proposal: `RtlButton` / `RtlCheckBox` that draw their own text, as `RtlGroupBox` does (touches all 93 buttons and check boxes). Approve or choose another way.
- **R5.9 decision:** white on the warning amber `#B26A00` is 4.2:1 (below 4.5:1). The plan darkens it to `#9A5B00` (5.4:1), but AGENTS.md §5.10 fixes `#B26A00`. Approve the change to AGENTS.md, or keep `#B26A00` and accept 4.2:1.
- **R5.6 shortcut:** void is on `F8` (it was free). Confirm, or name another key.
- **R5.8 decision:** today's grid shows 1.4 rows on 1366×768 at 100 % and on 1920×1080 at 125 %, 3 rows on 1920×1080 at 100 % (target 5). Options in REMEDIATION_PLAN R5.8: (A, recommended) compact desk layout, (B) rebalance only, (C) keep. Also answer Q-R7 (lowest desk resolution).
- **R5.4 deviation:** focus stays in the card box after a lookup (the plan said: move to the first counter), so a scanned card never lands in a counter. Confirm.
- **R5.7:** the success sound is on by default. Confirm, or it can default to off.

- **R7 wording to confirm (Q-R4b defaults applied):** «فترة السماح» and «فترة التهدئة» became «فترة الانتظار»; «المحطة» / «محطة العمل» became «الشباك» (menu «الشبابيك»); «الوصل» / «الكوبون» / «الإيصال» became «القسيمة». Two long sentences were shortened (void warning, select-organization header).
- **R7.12:** print one entry card and one gift voucher on the real 80 mm thermal printer (margins, cutter, QR scans at the desk).
- **Visible R7 changes to try:** management windows at 125 % (Dpi scaling, no clipping), the gift receipts window has a close button, Esc closes users/workstations lists, web times are park-local.

- **R6.9 re-run:** `scripts\backup.ps1` as Administrator after commit `4a53206` (ownership + child ACL reset).
- Delete the empty test folder `C:\ParkGate_R69_Test` left by the agent's non-elevated test run (the tool blocks deletes at `C:\`).
- **Behaviour change (R6.3):** `CreatedByUserId`, `UpdatedAtUtc` and `UpdatedByUserId` are now filled on holders, organizations and users; they were always NULL before.
- ~~Apply the three migrations on SQLEXPRESS~~ **Done in S31** (backup first: `C:\ProgramData\ParkGateackups\ParkGate_before_R2.bak`). The 7 pending outbox rows got sequences 1–11 with the re-queued ones and contain no phone, national ID, token or key id. Install the new SyncAgent build before it runs again (still the old one until R8.4).
- **Redeploy the web** after R2: the existing Laravel migration was edited in place (no `phone`/`national_id`, heartbeat counts, gift columns and `gift_vouchers`). On an already-created cloud database, re-import `web/database/mysql_schema.sql` (it is generated now and drops and recreates every table, so it also wipes `users`: create the user again with `parkgate:create-user`), then let the agent re-queue.
- Remove `AesKey` from the agent's `appsettings` and `PARKGATE_AES_KEY` from the host `.env` (ignored now). Set `CloudApi:BaseUrl` to an `https://` address.
- Q-R2b answered 2026-11-13: `Notes` and `StatusReason` **are synced**. R2.6 excludes only `Phone`
  and `NationalId`. R2.6 is unblocked.
- Q-R3 answered 2026-11-13: users are created with `parkgate:create-user` from the cPanel Terminal.
- Osama's local `web/.env` still holds the old default sync key and the `APP_KEY` that was published in
  `.env.production.example` (R1.3). Harmless for dev; regenerate both (`key:generate`,
  `parkgate:generate-key`) and set the agent's `CloudApi:ApiKey` to match.
- **cPanel PHP version.** `web/composer.lock` resolves Symfony 8 / Laravel 13.36, which need PHP ≥ 8.4,
  while `composer.json` still says `^8.3`. CI uses PHP 8.4. Confirm the host offers PHP 8.4 before R8.5,
  and raise `composer.json` to `^8.4` (or re-lock for 8.3) to match.
- Still open (the plan's defaults apply if unanswered): Q-R4b (glossary, R7.1), Q-R6 (desk PCs and
  workgroup/domain, R6.5), Q-R7 (lowest desk resolution, R5.8).
- **Approve ADR-022** (gift token excluded from the cloud) **and ADR-023** (single-use supervisor approval tokens).
- **Confirm the one-minute settings cache** (R3.6): a setting saved on one desk reaches the others within a minute.
- **Confirm the R4.2 deviation:** a card number typed by hand during the success banner is not looked up until Enter.
- **Run the manual test script** (TC-01 to TC-21); R4 is not done until it passes.
- Decisions already made by Osama are in the plan's §2.1 — do not re-ask them.

---

## Session Log (last 10 sessions)

### S38 — 2026-11-18 — Phase R5: R5.1–R5.3, R5.5, R5.6, R5.10–R5.12, R5.14
- Baseline green (885 unit). PR #20 opened for the two R7 commits missing from `main`. Tests: architecture 340 → 379.
- Layout probe copied to this session's scratchpad; new `PROBE_SCREEN=1` takes a real screen capture (`CopyFromScreen`), because `DrawToBitmap` does not show every text fault. Bounds dumps found R5.11; screen captures found R5.12 and R5.13.
- R5.13 dead ends: `Cairo.ttf` is a variable font, but a static instance (fontTools, loaded only in the probe process) clips the same way; the native `FlatStyle.System` button clips too; pixel tests at the test host's 96 dpi do not reproduce the button fault (the group box one they do).
- R5.10: `MainForm` takes `TimeProvider`; `TestUi` already resolves it.
- `sed -i` turned edited files into LF again; re-checked out after each commit.

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

### S34 — 2026-11-15 — PR #15, manual tests, gift screen fix

- Merged `main` (PR #14) into `phase/r4-desk-ui`, opened PR #15, ticked R0, R1 and R2.7 in the remediation plan, ignored VS-generated `.resx` files.
- Osama ran TC-14–TC-21; the F7 step of TC-21 could not run because the gift screen rendered broken.
- R4.11: `GiftRedeemControl` had `AutoScaleDimensions` 7x17 with `AutoScaleMode.Dpi` (sizes multiplied by about 7). Fixed to 96x96; new `DesignerScalingTests` guards every designer.
- R4.12: the scan row overlapped on wide windows; now a `TableLayoutPanel`. Both verified by rendering the control headless (before: one huge button; after: correct rows).
- R4.13: Osama reported the gift panel and the registration buttons clipped on the desk. Cause: `GiftOfferPanel` in Font scaling mode inside a Dpi parent, and too little height for the registration group. Fixed and verified by rendering at 1920x890 and 1366x640. Gifts are disabled for every organization in the dev databases (`GiftEnabled = 0`), which is why the offer panel never showed; enable it per organization in Management > Organizations.
- Tests: 691 unit/architecture, 42 integration, all green. Tip: if the app is running it locks `bin\`; build with `--artifacts-path artifacts/verify` (git-ignored).

### S33 — 2026-11-14 — Phase R4 code complete
- PR #14 was still open, so `phase/r4-desk-ui` was branched from `phase/r3-application` (handoff option).
- Order: R4.1, R4.3, R4.4, R4.5, R4.6, R4.9, R4.7, R4.2, R4.8, R4.10, then TEST_SCRIPT TC-14..TC-21.
- Two test hangs: `Task.Yield`/`Task.Delay` continuations touched controls owned by the test thread. Fixed with seams.
- A heredoc with Arabic and quotes broke Bash parsing once; files with Arabic are written with the Write tool now.
- Line endings: 18 working files with `eol=crlf` were LF or mixed (the S27 item included); re-checked out, no commit
  needed because the blobs were already LF. Helper scripts `addstr.py` and `extract_arabic.py` live in the scratchpad only.
- Test counts: architecture 155 → 183 (unit 662 → 690). No Application or Infrastructure change.
### S32 — 2026-11-14 — Phase R3 complete
- Baseline green. Did R3.1, R3.3, R3.4, R3.5, R3.6, R3.7, R3.9, R3.10, R3.8, R3.2 in that order, one commit each.
- R3.10 (EntryCardIssuer client clock) was found in R3.7 and added to the roadmap before doing it.
- R3.9 needed no production change; the commit is test-only and says so.
- The approval token is spent only after request validation, so a too-short reason does not cost the supervisor a re-login.
- A Python edit wrote three test files with LF; fixed by deleting and re-checking them out after the commit.
- Test counts: .NET unit 620 → 662, integration 40 → 42, web 96 (unchanged).
### S31 — 2026-11-13 — Phase R2: R2.9 and the end-to-end test
- Baseline was green (620 unit, 87 infra, 154 architecture; web 89). Did R2.9 test-first in two commits, then the end-to-end test.
- `phpunit.xml` does not force the SQLite values, so the MySQL job only sets `DB_*` env vars; no change to the default run.
- The generator cannot open a connection: it uses an anonymous `MySqlConnection` subclass that pins `isMaria()` to false and
  `getServerVersion()` to 8.0.36, because the grammar asks the server for both when it builds `uuid` and `date` columns.
- The generated file uses lower-case SQL from Laravel's grammar and now includes `jobs`, `job_batches`, `failed_jobs`
  (they were missing from the hand-written file).
- The plan asked for a PHP and .NET end-to-end run; the test is .NET-only (the web ingestion side has its own tests, and
  the whole web suite runs on MySQL). A single run across both stacks would need a running web server in CI.
- Test counts: .NET integration 39 → 40, web 89 → 96. Pushed; GitHub `web` (both jobs) green.
- Osama said the app is not in production yet. Merged PR #13 (merge commit), took an uncompressed backup (Express has no
  `COMPRESSION`), applied the three migrations with `dotnet ef database update --project src/ParkGate.Infrastructure
  --connection "Server=.\SQLEXPRESS;..."` and checked `__EFMigrationsHistory` and the outbox.
### S30 — 2026-11-13 — Phase R2: R2.7
- Did R2.7 test-first: extractor + exclusion list, data migration, web ingestion/model/summary/Vue, ADR-022.
- Osama approved `laravel/boost` (dev only). Installed v2.10 with `boost:install --guidelines` only; no MCP server and no
  skills (its docs search calls an external service). It rewrote `web/AGENTS.md` and `web/CLAUDE.md`. Composer is not on the
  Bash PATH here; run it from PowerShell. Root `AGENTS.md` §2 still lists the approved packages: add Boost there when you approve.
- `dotnet ef` needs `--project src/ParkGate.Infrastructure` only (the design-time factory lives there); the WinForms
  project is not a valid startup project.
- Ran Pint over all web PHP in a separate `style` commit (R9.6, 14 files, tests still 89 green). New web PHP must pass
  `vendor/bin/pint --dirty --format agent`.
- Test counts: .NET unit 617 → 620, integration 38 → 39, web 79 → 89.
### S29 — 2026-11-13 — Phase R2: R2.1–R2.6, R2.8, R1.8
- Created `phase/r2-sync` from `phase/r1-web-security` (PR #12 still open) and pushed it.
- Did R2.6 → R1.8 → R2.1 → R2.2 → R2.3 → R2.4 → R2.5 → R2.8, one commit each, test-first. Left R2.7, R2.9.
- Found that nothing sent a cloud heartbeat; added it inside R2.5 (see State of the work).
- Web `date` casts changed to `date:Y-m-d`: SQLite stored `2026-11-13 00:00:00`, MySQL `2026-11-13`, which hid a
  date-comparison bug from the tests (relevant to R2.9).
- Test counts: .NET unit 588 → 617, integration 34 → 38, web 58 → 79.
