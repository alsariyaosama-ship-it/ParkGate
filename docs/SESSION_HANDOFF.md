# Session Handoff

> **Read at the start of every session. Update at the end of every session** (AGENTS.md §1).
> Keep "Snapshot" and "Next session" accurate — they are overwritten each time. The log keeps the last 10 sessions only.

---

## Snapshot

| Field | Value |
|---|---|
| Last updated | 2026-11-04 |
| Last session | S21 — Phase F5: Gift voucher report queries, service, QR adapter, and integration tests [T-G6] |
| Current phase | Phase F5 — Gift Vouchers (QR) and Free-Companion Limit |
| Working branch | `phase/f5-gift-vouchers` |
| Build | Clean (0 errors; 16 pre-existing NuGet vulnerability warnings for `System.Security.Cryptography.Pkcs` 9.0.11) |
| .NET Unit tests | 544 passed (202 domain + 169 application + 110 architecture + 63 infrastructure) |
| .NET Integration tests | 34 passed on LocalDB |
| Total tests | 578 passed |
| Latest migration | `20261102125339_AddGiftVouchersAndCompanionLimit` (not applied to production yet) |

## State of the work

**Done (Phase F5)**
- T-G0 documentation + ADR-020 (`docs/F5_GIFT_VOUCHERS_PLAN.md` is the authoritative task-by-task plan).
- T-G1 Domain: `GiftVoucher`, `GiftVoucherStatus`, `GiftDenialReason`, `IGiftEligibilityPolicy` +
  `RollingIntervalGiftEligibilityPolicy`, `QrPurpose`, `Organization` gift/companion policy,
  `AgeCategory.IsCompanionLimitExempt`.
- T-G2 Application plumbing: `IssueGift`, `RedeemGift`, `OverrideGiftInterval`, `VoidGift` permissions,
  `Errors.Gift.*`, `CheckIn.ExceedsFreeCompanionLimit`, QR signing context per purpose (entry cards unchanged).
- T-G3 Free-companion limit (BR-25/26) end to end in `CheckInService`.
- T-G4 `GiftVoucherService` (eligibility, issue, override issue, redeem, void) + validators + 33 tests.
- T-G5 Persistence: `GiftVoucherConfiguration`, `GiftVoucherRepository`, `UnitOfWork.GiftVouchers`,
  4 new `Organizations` columns + `AgeCategories.IsCompanionLimitExempt`, migration
  `AddGiftVouchersAndCompanionLimit` (also flags the existing «رضيع» row), 16 integration tests
  including two 20-run parallel issue/redeem tests.
- T-G6 Infrastructure & Application reports: `GiftReportDtos`, `IGiftReportQueries`, `GiftReportQueries`,
  `IGiftReportService`, `GiftReportService`, `QrImageRendererAdapter`, DI registrations, 3 unit tests,
  1 multi-scenario integration test on LocalDB.

**In progress**
- Nothing.

**Not started (Phase F5)**
- T-G7 WinForms: organization gift policy + age-category exempt flag screens.
- T-G8 WinForms: desk companion limit + gift panel + gift QR dialog.
- T-G9 WinForms: gift redemption screen (rides window).
- T-G10 WinForms: gift receipts report + Excel export.
- T-G11 Hardening, docs, handoff.

## Next session — start here

1. `dotnet build ParkGate.sln -c Debug` and
   `dotnet test ParkGate.sln --filter "Category!=Integration"` — expect 544 green.
2. Implement **T-G7** (`docs/F5_GIFT_VOUCHERS_PLAN.md` §T-G7), commit, then continue with
   T-G8 → T-G10 (UI) and finish with T-G11.

## Blockers / waiting on the user

- None. The user approved the migration touching the released tables `Organizations` and `AgeCategories`.
- The migration is **not applied to the production database** yet. Run
  `scripts/build-migration-bundle.ps1` and apply `efbundle.exe` when the user asks.

## Known issues / tech debt

- `web/` ingestion (`SyncIngestionService.php`) maps fields explicitly, so the new
  `Organization`/`AgeCategory` outbox fields are safely ignored. `GiftVoucher` is intentionally **not**
  synced (ADR-020).
- `QrTokenVerifier` trusts signing keys loaded at app start (plan risk R1): every workstation must be
  started once before the rides window opens, otherwise `UnknownKey` until restart.
- Left over from before F5 (plan §10): `IssueQrDialog` uses the Infrastructure `QrImageRenderer` directly
  and `CardScanService` shows a misleading "forged" message when a *gift* QR is scanned at the entry desk.
- The UI design review findings reported in the previous session (Beneficiaries QR button clipping,
  `VoidReasonDialog` warning text truncated, companion-total overlap, inconsistent AutoScale baselines)
  were reported fixed by the user; confirm visually on the next UI task.

## Environment notes

- Dev DB: LocalDB `(localdb)\MSSQLLocalDB`, database `ParkGate_Dev` (default in `appsettings.json`).
- Connection string securely overrideable via `%ProgramData%\ParkGate\connection.dat` (DPAPI LocalMachine scope).
- Integration tests create and drop `ParkGate_Test_<guid>` databases on LocalDB.
- EF commands use `src/ParkGate.Infrastructure` as both `--project` and `--startup-project`.

---

## Session log (newest first, max 10)

### S21 — 2026-11-04 — Phase F5: Gift voucher report queries, service, QR adapter, and tests
- Tasks: T-G6.
- Commits: `feat(gift): gift report queries, service and qr image adapter [T-G6]`.
- Summary: Implemented `GiftReportDtos` with `GiftReceiptFilter`, `IGiftReportQueries` and `GiftReportQueries` (with multi-scenario status, date, organization and workstation filters), `IGiftReportService` and `GiftReportService`, `QrImageRendererAdapter : IQrImageRenderer`, registered transient/singleton services in DI, and added 3 unit tests plus 1 comprehensive integration test on LocalDB.
- Test count: 578 total (544 unit/architecture + 34 LocalDB integration), zero errors.

### S20 — 2026-11-02 — Phase F5: Application use cases, persistence and migration
- Tasks: T-G4, T-G5 (T-G0..T-G3 were committed in the previous session; ROADMAP now reflects T-G0..T-G5 done).
- Commits: `d65b427 feat(gift): gift voucher use cases in application layer [T-G4]`,
  `40dbc2f feat(gift): persist gift vouchers and companion limits [T-G5]`.
- Decisions: Migration `AddGiftVouchersAndCompanionLimit` approved by Osama (new columns on the released
  `Organizations`/`AgeCategories` tables). The migration carries a name-based
  `UPDATE dbo.AgeCategories ... WHERE NameAr = N'رضيع'` so existing databases get BR-26 without re-seeding.
  `GiftVoucher.MaxVoidReasonLength = 250` added to the domain to match `EntryAttempt`.
- Test count: 574 total (541 unit/architecture + 33 LocalDB integration), zero errors.

### S19 — 2026-10-21 — Production Installer, SQLEXPRESS Setup, Rebranding & App Icon
- Tasks: Configured production SQLEXPRESS connection, fixed missing migrations, introduced Inno Setup Server/Client selection, rebranded to "بوابة منتزه المرح العائلي", integrated official app icon (`ICON\icon.ico`), and generated `ParkGate_Setup_v1.0.0.exe`.
- Commits: Pending final session commit.
- Decisions: Integrated wizard role selection in Inno Setup to run migrations and local SQL server configuration only when "Server" mode is chosen. Embedded custom application icon in WinForms assembly and base form (`RtlFormBase`).
- Test count: 426 total passed unit & architecture tests (zero warnings).

### S18 — 2026-10-14 — Cloud Web App & Dashboard (Phase F4)
- Tasks: TF4.1..TF4.6 completed on `phase/f4-cloud-dashboard`. Merged Phase F3 into `main`. Registered ADR-019.
- Commits: `Merge branch 'phase/f3-sync-agent' into main`, `feat(web): implement Laravel Inertia Vue 3 web dashboard and sync API [TF4.1..TF4.6]`.
- Decisions: Adopted Laravel 11 with Breeze (Inertia.js + Vue 3) inside `web/`. Built sync API endpoint (`/api/sync/push`) with API key validation, AES-256 decryption, and atomic transaction ingestion. Built Heartbeat status monitor, Arabic RTL Dashboard, Beneficiaries search, and Reports.
- Test count: 475 total passed (443 .NET tests + 32 Laravel tests, zero warnings/errors).

### S17 — 2026-10-13 — Standalone Cloud Sync Agent (Phase F3)
- Tasks: TF3.1, TF3.2, TF3.3, TF3.4, TF3.5, TF3.6 completed on `phase/f3-sync-agent`.
- Commits: ADR-018, outbox persistence, transactional capture, sync engine (deduplicator, AES encryption, cloud push), `ParkGate.SyncAgent` Worker Service, Windows Service scripts and integration tests.
- Decisions: Adopted standalone Worker Service (`ParkGate.SyncAgent`) matching `HRSys.SyncAgent` architecture. Integrated transactional outbox capture inside DbContext, AES-256 field encryption for beneficiary data, Last-Write-Wins batch pruning, and DPAPI-secured connection resolution.
- Test count: 443 total passed (376 unit/architecture + 67 LocalDB infrastructure/integration tests, zero warnings).

### S16 — 2026-10-10 — System-wide Audit & Hardening
- Tasks: Complete audit across architecture, code logic, performance, and UX.
- Commits: `fix(core): resolve DI registration, integrate QR desk scanner, add print/save, and eliminate N+1 queries`.
- Decisions:
  1. Registered `GateStation` in WinForms DI composition root to fix `CardScanService` resolution.
  2. Integrated `CardScanService` into `CheckInDeskControl` to allow desk barcode scanners to scan printed QR cards directly.
  3. Added Print and Save PNG capabilities to `IssueQrDialog`.
  4. Added `GetByIdsAsync` to `IBeneficiaryRepository` to eliminate N+1 database queries in today's entries widget.
- Test count: 404 total passed tests (zero warnings).

### S15 — 2026-10-08 — Fix QR Issue Deadlock and Loop
- Tasks: Diagnosed and resolved UI freeze / infinite loop when clicking "إصدار بطاقة QR".
- Commits: `fix(ui): resolve deadlock and GDI stream invalidation when issuing QR cards`.
- Decisions:
  1. Offloaded DI factory async registration onto ThreadPool using `Task.Run` and added `.ConfigureAwait(false)` in `EntryCardRepositories` and `CngSigningKeyStore` to prevent blocking the WinForms UI `SynchronizationContext`.
  2. Fixed premature disposal of `CngKey` in `CngSigningKeyStore.OpenOrCreate`.
  3. Fixed GDI+ paint exception by cloning the PNG stream into an independent `Bitmap` instead of binding directly to a disposed `MemoryStream`.
  4. Added `EntryCardSchemaInitializer` and hooked it into `DatabaseStartupService` and `StartupCoordinator` to automatically create entry cards DB tables and stored procedures if missing.
  5. Added `CngSigningKeyStoreTests` unit test.
- Test count: 404 total passed tests (zero warnings).

### S13 — 2026-10-08 — QR Credentials (F1)
- Tasks: TF1.1, TF1.2, TF1.3, TF1.4 completed on `main`.
- Commits: `feat(entrycards): integrate QR entry cards functionality [F1]`
- Decisions: Integrated EntryCards into Domain, Application, and Infrastructure using Clean Architecture. Re-used QRCoder for QR rendering and CngSigningKeyStore for Windows CNG ECDSA P-256 keys. Added database schema `001_EntryCards.sql`. Also fixed UI issues: added SelectAll behavior on text enter and clearing UI data upon customer number change.
- Test count: 403 total passed tests.

### S11 — 2026-10-07 — UI Polish & Terminology
- Tasks: Enlarge touch counter controls, system-wide terminology replacement ("حامل" -> "زبون"), and LoginForm redesign.
- Commits: `feat(ui): enlarge touch counter controls, rename holder to customer, and redesign login form`.
- Decisions: Sized `CounterInput` buttons to 50x48px (18pt Bold) and textbox to 70x44px (15pt Bold) for touch screen ergonomics; standardized all user-facing terminology to "زبون جديد" and "الزبائن" across `Strings.resx`, controls, forms, reports, and Excel exports; redesigned `LoginForm` with brand emerald header, workstation chip, elevated card layout, spacious 12pt Cairo inputs with correct `RightToLeft = Yes`, and touch-friendly 48px Login button.
- Test count: 382 non-integration unit/architecture tests passed (100% green, 0 warnings).

### S10 — 2026-10-07 — Phase 8 Hardening & Deployment (T8.1..T8.5)
- Tasks: T8.1, T8.2, T8.3, T8.4, T8.5 completed on `phase/08-deployment` (Phase 8 complete).
- Commits: `docs(deployment): add comprehensive production deployment guide [T8.1]`, `feat(deployment): implement backup script and in-app admin backup [T8.2]`, `build(deployment): add win-x64 publish profile and Inno Setup installer script [T8.3]`, `docs: add comprehensive Arabic user guide [T8.4]`, `test(performance): verify lookup latency under large dataset [T8.5]`.
- Decisions: Complete hardening and deployment package implemented; `DEPLOYMENT.md` covers SQL Express 2022 setup, TCP/IP, firewall, least-privilege logins, and DPAPI setup; automated backup script `scripts/backup.ps1` with 14-day pruning and in-app Admin backup via `IDatabaseBackupService`; Inno Setup script `scripts/installer.iss` and `win-x64.pubxml`; comprehensive Arabic user guide `USER_GUIDE_AR.md`; high-volume benchmark test generated 100,000 holders and 1,000,000 attempts, verifying card lookup and eligibility evaluation executes in < 1.0 s; all 398 tests pass (100% green, 0 warnings).
- Test count: 398 total passed (343 unit/architecture + 55 LocalDB infrastructure tests).

### S09 — 2026-10-07 — Phase 7 Reports & Statistics (T7.0..T7.3) and Phase 6 Management
- Tasks: T7.0, T7.1, T7.2, T7.3 completed on `phase/07-reports` (Phase 7 complete); T6.1..T6.5 completed on `phase/06-management`.
- Commits: `feat(reports): implement report queries and application service [T7.1]`, `feat(reports): implement reports UI with filters, ClosedXML export, and print preview [T7.2, T7.3]`, `feat(management): implement beneficiary management and void entry [T6.1] [T6.2]`, `feat(management): implement organizations, age categories, workstations, users and settings screens [T6.3] [T6.4] [T6.5]`.
- Decisions: All 6 reports implemented per approved specifications; `IReportQueries` projects on `BusinessDate` with `AsNoTracking()`; `ClosedXmlExcelExportService` generates RTL `.xlsx` sheets; management suite built with modal dialogs, full Arabic validation, self-modification guards in `UsersForm`; `SettingsForm` edits cooldown in hours while storing minutes.
- Test count: 397 total passed (343 unit/architecture + 54 LocalDB infrastructure tests).
