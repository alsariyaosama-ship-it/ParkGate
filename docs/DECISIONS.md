# ParkGate — Architecture Decision Records

Append-only. To change a decision, add a new ADR that supersedes the old one and mark the old one `Superseded by ADR-xxx`. Format: Status · Context · Decision · Consequences.

---

### ADR-001 — .NET 10 LTS, C#, Windows Forms
- **Status:** Accepted (2026-10-04)
- **Context:** Client requested WinForms + C#. .NET 8 support ends Nov 2026; .NET 10 is the current LTS.
- **Decision:** Target `net10.0` for Domain/Application/Infrastructure and `net10.0-windows` for the UI.
- **Consequences:** Core layers stay Windows-independent and reusable by a future gate service or cloud worker.

### ADR-002 — Clean Architecture in 4 production projects
- **Status:** Accepted
- **Context:** Client asked for clean architecture, max 4 layers, DI and SOLID.
- **Decision:** Domain, Application, Infrastructure, WinForms. Dependency rule in AGENTS.md §3, enforced by `ParkGate.ArchitectureTests`.
- **Consequences:** Slightly more ceremony than a 2-tier app; rules testable without UI or DB.

### ADR-003 — EF Core with `IDbContextFactory`, unit of work per use case
- **Status:** Accepted
- **Context:** WinForms has no request scope; long-lived `DbContext` instances cause stale data, growing change trackers and threading bugs.
- **Decision:** `AddDbContextFactory`; Application uses `IUnitOfWorkFactory.Create()` per use case; disposed at the end.
- **Consequences:** No context shared between forms; each operation sees fresh data.

### ADR-004 — Database clock is the only time source; store UTC
- **Status:** Accepted
- **Context:** Several desk PCs may have wrong clocks; cooldown must be consistent.
- **Decision:** Rules and timestamps use `SYSUTCDATETIME()`. Store UTC, display in `Park.TimeZoneId`. Persist `BusinessDate` for reports.
- **Consequences:** One extra lightweight query per check-in; correct results regardless of client clocks; cloud-ready timestamps.

### ADR-005 — Cooldown concurrency via row lock + denormalized `LastGrantedEntryUtc`
- **Status:** Accepted
- **Context:** Two desks could check in the same holder at the same moment.
- **Decision:** Transaction with `UPDLOCK, ROWLOCK` on the holder row, evaluate policy in Domain, insert attempt, update `LastGrantedEntryUtc`, commit (DATABASE.md §4).
- **Consequences:** Business rule stays in C# and unit-testable; correctness guaranteed by the DB lock; an integration test guards it.

### ADR-006 — Eligibility as a strategy (`IEntryEligibilityPolicy`)
- **Status:** Accepted
- **Context:** Client wants the window configurable; other organizations or "once per day" rules may come.
- **Decision:** Strategy interface; MVP ships `RollingWindowEligibilityPolicy`. Duration configurable globally and per organization.
- **Consequences:** New policy = new class + registration, no edits to existing logic.

### ADR-007 — Log denied attempts with dedupe
- **Status:** Accepted
- **Context:** Denials are operationally important (abuse, disputes) but repeated lookups would flood the table.
- **Decision:** Log every denial; skip if same holder + workstation was denied within `Entry.DenialDedupMinutes`.
- **Consequences:** Reliable denial reports without noise.

### ADR-008 — Sequential GUID keys for holders and attempts
- **Status:** Accepted
- **Context:** Future cloud sync (F3) and possibly multiple parks could merge data from several databases.
- **Decision:** `uniqueidentifier` keys with EF sequential generation for `Beneficiaries` and `EntryAttempts`; `int` identity for lookup/admin tables; `bigint` for `AuditLogs`.
- **Consequences:** Merge-safe IDs with low index fragmentation; slightly larger keys.

### ADR-009 — Companions as rows per age category
- **Status:** Accepted
- **Context:** Categories (بالغ/طفل/رضيع) may change or grow.
- **Decision:** `AgeCategories` lookup + `EntryAttemptCompanions(EntryAttemptId, AgeCategoryId, Count)`; `TotalPersons` stored on the attempt.
- **Consequences:** Adding a category needs no schema change; reports pivot by category.

### ADR-010 — No MediatR, AutoMapper or FluentAssertions v8+
- **Status:** Accepted
- **Context:** These moved to commercial licensing; they add indirection a small desktop app does not need.
- **Decision:** Plain application services, manual mapping, Shouldly for assertions.
- **Consequences:** Less magic, no license exposure for the client.

### ADR-011 — Migrations applied by deployment, not by every client
- **Status:** Accepted
- **Context:** Several desks start the app; silent auto-migration from any client is risky and needs elevated DB rights.
- **Decision:** Ship an EF migration bundle run with a migration login on the server. The app checks pending migrations at startup and refuses to run with a clear Arabic message.
- **Consequences:** App login can be least-privilege; upgrades are explicit.

### ADR-012 — No default admin password; first-run wizard
- **Status:** Accepted
- **Context:** A default password in the repo or installer is a common breach.
- **Decision:** If `Users` is empty, the first-run wizard creates the Admin.
- **Consequences:** Installation step documented in DEPLOYMENT.md.

### ADR-013 — QR token is opaque; store only its hash (F1)
- **Status:** Accepted (applies when F1 is approved)
- **Context:** QR must not expose personal data and must be revocable.
- **Decision:** Random ≥128-bit token, SHA-256 hash stored, reprint = new token.
- **Consequences:** A leaked DB cannot be used to forge QR codes.

### ADR-014 — Arabic UI text in resources; English code
- **Status:** Accepted
- **Decision:** All UI strings in `Strings.resx`; identifiers, comments, commits and docs in English.
- **Consequences:** Consistent wording, easy future localization, clean diffs.

### ADR-015 — `Result` for expected failures
- **Status:** Accepted
- **Decision:** Use cases return `Result`/`Result<T>` with error codes; exceptions only for unexpected failures; UI maps codes to Arabic.
- **Consequences:** Predictable flows, no exception-driven control flow at the desk.

### ADR-016 — Git workflow
- **Status:** Accepted
- **Decision:** Branch per phase, Conventional Commits with task IDs, push every session, PR per phase reviewed by the user, CI on Windows runner (unit + architecture tests).
- **Consequences:** Every session leaves a recoverable, reviewable state on GitHub.

### ADR-017 — Cairo primary Arabic typography
- **Status:** Accepted
- **Context:** The application UI is entirely in Arabic (RTL). Segoe UI was the initial standard font, but Cairo provides superior Arabic legibility, modern calligraphic balance, and aesthetics for kiosks and desktop screens.
- **Decision:** Adopt Google Font **Cairo** as the primary typography. Embed `Cairo.ttf` in `ParkGate.WinForms` resources, manage loading via `AppFont` and `PrivateFontCollection` with fallback to `Segoe UI`, and set globally via `Application.SetDefaultFont` and `RtlFormBase`.
- **Consequences:** Beautiful, uniform Arabic typography across all client PCs without external font dependencies.

### ADR-018 — Standalone Sync Agent Service Project (`ParkGate.SyncAgent`)
- **Status:** Accepted
- **Context:** Cloud dashboard synchronization (Phase F3) requires a robust background worker that pushes outbox events to the cloud API without impacting desk operator responsiveness. The user decided to adopt a standalone service application model matching the proven `HRSys.SyncAgent` pattern.
- **Decision:** Introduce a standalone Worker Service project `src/ParkGate.SyncAgent` targeting `net10.0`. It runs as a Windows Service (via `Microsoft.Extensions.Hosting.WindowsServices`) or interactive console, reads DB connection from `%ProgramData%\ParkGate\connection.dat` (DPAPI), processes the transactional outbox table in batches, deduplicates mutations (Last-Write-Wins), ~~encrypts sensitive beneficiary fields (AES)~~ *(superseded by ADR-021: phone and national ID are not synced at all)*, and pushes batches to the cloud endpoint with immediate pruning of successful records.
- **Consequences:** Increases approved production projects from 4 to 5. Decouples synchronization resilience and execution lifecycle from desk user logins.

### ADR-019 — Laravel for Cloud Web Application and Dashboard
- **Status:** Accepted
- **Context:** Cloud dashboard (Phase F3 / Cloud Reporting) requires a server-side web application and REST API endpoint to ingest the SyncAgent's push payload (`/api/sync/push`) and display cloud dashboards and management reports.
- **Decision:** Use **Laravel** (PHP) as the framework for the cloud web application, API, and control panel.
- **Consequences:** SyncAgent will post JSON batches to Laravel API endpoints; Laravel will authenticate via API key, ~~receive AES-256 encrypted fields~~ *(superseded by ADR-021)*, process/store synchronization data, and render the web dashboard.

---

### ADR-020 — Gift Vouchers: ECDSA with separate signing context, EF-managed table, rolling interval, companion limit
- **Status:** Accepted (2026-10-29)
- **Context:** Phase F5 adds gift vouchers issued at the gate and redeemed at the rides window. Decisions required on: signing, storage, interval semantics, companion limit.
- **Decision:**
  1. **Signing context:** Gift QR tokens use the same ECDSA P-256 / SHA-256 infrastructure as entry cards (`QrTokenFormat`) but a separate context string `"ParkGate.GiftVoucher/v1"` so a gift QR can never be accepted at the entry desk and vice versa.
  2. **Storage:** `GiftVouchers` is an EF Core-managed table (entity + `IEntityTypeConfiguration`), not a raw SQL table like `EntryCards`. Migration: `AddGiftVouchersAndCompanionLimit`.
  3. **Rolling interval:** gift eligibility uses a rolling window of days (`GiftIntervalDays` per org, default 30) measured from the last non-voided voucher's `IssuedAtUtc`, matching BR-03/BR-04 style.
  4. **Companion limit:** `MaxFreeCompanions` per org (NULL = no limit). Infants (age category `IsCompanionLimitExempt = true`) do not count. Exceeding the limit **rejects** check-in; no supervisor override.
  5. **Cloud sync:** `GiftVoucher` is NOT added to `OutboxEntityExtractor.SyncableTypes` in Phase F5.
  6. **Keyboard shortcuts:** F6 = issue gift, F7 = supervisor override gift (added to AGENTS.md §5.11).
- **Consequences:** Existing entry-card QRs continue to verify unchanged (regression-tested). New `GiftVouchers` table isolated from the sync outbox. Companion limit enforced server-side in `CheckInService`.

---

### ADR-021 — Phone and national ID never leave the park
- **Status:** Accepted (Osama, Q-R2, 2026-11-12; Q-R2b, 2026-11-13)
- **Context:** The cloud only needs aggregates and holder names. Field-level AES encryption (ADR-018/019) added a shared key to distribute, a double-encryption bug after a failed push (R2.1), and still put personal data on a shared host.
- **Decision:** `Phone` and `NationalId` are excluded from the `Beneficiary` outbox payload by an explicit per-entity exclusion list in `OutboxEntityExtractor`. The cloud has no columns for them. `PayloadEncryptionService`, `AesHelper`, `CloudApiOptions.AesKey` and `PARKGATE_AES_KEY` are removed. `Notes` and `StatusReason` keep syncing (Q-R2b). Migration `PurgeSensitiveBeneficiaryOutbox` deletes pending Beneficiary rows and re-queues one clean row per holder. The cloud rejects a payload that is not a JSON object (422) instead of skipping it.
- **Consequences:** No key management between the park and the host. Old cloud rows are not migrated: the web schema is unreleased and is edited in place (R2.6).

---

<!-- Template for new ADRs:
### ADR-0XX — Title
- **Status:** Proposed | Accepted | Superseded by ADR-0YY
- **Context:**
- **Decision:**
- **Consequences:**
-->
