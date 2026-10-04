# ParkGate — Software Requirements Specification

| Item | Value |
|---|---|
| Product | ParkGate — subsidized-entry control for منتزه المرح العائلي |
| Client type | Family fun park (gate desk) |
| Vendor | شركة الركاز المتين لتقنية المعلومات |
| Version | 0.1 (baseline) — 2026-10-04 |
| Status | Approved for Phases 0–5; Future features (§9) designed-for, not built |

> `ParkGate` is a working name. Renaming the solution later is a mechanical task — ask the user before doing it.

---

## 1. Purpose

A Windows desktop application used at the park entrance desk to:

1. Register holders of a membership card issued by **صندوق التضامن الاجتماعي** (and other organizations later).
2. Record each free entry together with the companions entering with the holder, by age category.
3. Enforce a **configurable re-entry window** (default 12 hours): a holder who entered cannot enter again for free until the window has passed. The system refuses and explains why.

## 2. Scope

**MVP (in scope):** user login and roles, holder registration, check-in with companions, cooldown enforcement, denial logging, supervisor override, entry voiding, management screens, reports with Excel export, LAN deployment for several desks against one SQL Server Express instance.

**Out of scope for MVP (designed-for, see §9):** QR credentials, electronic gate/turnstile, cloud read-only dashboard.

**Never in scope unless the user asks:** paid ticket sales for non-members, online payment, a mobile app.

## 3. Glossary

| Arabic (UI term) | Code term | Meaning |
|---|---|---|
| الجهة | `Organization` | Body issuing the membership card (default: صندوق التضامن الاجتماعي) |
| حامل البطاقة / المستفيد | `Beneficiary` | Registered card holder |
| رقم البطاقة | `CardNumber` | Card identifier, unique per organization |
| المرافقون | `Companions` | People entering with the holder |
| الفئة العمرية | `AgeCategory` | بالغ / طفل / رضيع (configurable list) |
| محاولة دخول | `EntryAttempt` | One check-in attempt: granted, denied or overridden |
| فترة الانتظار / مدة السماح بالعودة | `Cooldown` | Minimum time between two granted entries |
| تجاوز | `Override` | Supervisor grants entry despite a denial |
| إلغاء عملية | `Void` | Supervisor cancels a mistaken entry record |
| موظف الشباك | `Operator` role | Desk employee |
| مشرف | `Supervisor` role | Shift supervisor |
| مدير النظام | `Admin` role | System administrator |
| نقطة الدخول / الشباك | `Workstation` | A desk PC (later: a gate device) |

## 4. Actors and permissions

| Permission | Operator | Supervisor | Admin |
|---|:-:|:-:|:-:|
| Lookup card, register new holder, record entry | ✔ | ✔ | ✔ |
| View today's entries at own workstation | ✔ | ✔ | ✔ |
| Edit holder data | — | ✔ | ✔ |
| Suspend / reactivate / block holder | — | ✔ | ✔ |
| Override a denial | — | ✔ | ✔ |
| Void an entry | — | ✔ | ✔ |
| Reports and export | — | ✔ | ✔ |
| Organizations, age categories, settings, users, workstations | — | — | ✔ |
| Backup now | — | — | ✔ |

Permissions are enforced in the **Application layer**. Hiding UI elements is a convenience, not the security boundary.

## 5. Main scenario — check-in at the desk

### 5.1 Basic flow (holder already registered, eligible)

1. Operator's cursor is always in the **card number** box when idle.
2. Operator types the card number or scans it (barcode/QR scanners act as a keyboard and end with Enter).
3. System normalizes the number (BR-01) and looks it up across all active organizations.
4. System shows the holder panel: name, phone, organization, last granted entry (local time), and a **green banner «مسموح بالدخول»**.
5. Companion counters appear (one per active age category). If the setting `Entry.PrefillCompanionsFromLastVisit` is on, they are prefilled from the last granted entry.
6. Operator adjusts counters and presses **F5 / «تسجيل الدخول»**.
7. System records the entry atomically (BR-12), shows a short success confirmation, clears the screen and returns focus to the card box.

### 5.2 Alternative flows

| ID | Condition | Behavior |
|---|---|---|
| A1 | Card not found | Inline registration panel opens with card number prefilled and organization defaulted to the default organization. Required: full name, phone. Optional: national ID, card expiry date, notes. On save, flow continues at step 5 (a new holder is eligible). |
| A2 | Card found in more than one organization | Operator chooses the organization from a short list, then continue. |
| A3 | Within cooldown | **Red banner** «مرفوض — دخل قبل أقل من {N} ساعة», showing last granted entry time, the time entry becomes allowed, and remaining time (ساعات:دقائق). Warning sound if enabled. Register button disabled. A denied attempt is logged (BR-10). |
| A4 | Holder suspended / blocked, card expired, or organization inactive | Red banner with the specific reason (BR-06 order). Denied attempt logged. |
| A5 | Supervisor override (from A3 or expired card in A4) | Supervisor presses **F9 «تجاوز»**, authenticates if the logged-in user is not a supervisor (username + password prompt), enters a mandatory reason. Entry recorded as `Overridden`. Not available for Blocked holders or inactive organizations (BR-11). |
| A6 | Two desks submit the same holder at the same moment | Exactly one is granted; the other receives the A3 result (BR-12). |
| A7 | Database server unreachable | Clear Arabic message «تعذّر الاتصال بالخادم», retry button; nothing is recorded locally. |
| A8 | Operator recorded the wrong holder | Supervisor voids the entry with a reason (BR-13). |

### 5.3 Check-in screen layout (RTL)

Zones from top to bottom (right side = start in RTL):

1. **Top bar:** park name, logged-in user, workstation name, server time, connection indicator.
2. **Search zone:** card number box (large font), organization filter (default «الكل»), search button.
3. **Status banner:** full width, large text (≥ 20 pt). Neutral / green / red / amber.
4. **Holder panel (start side):** name, phone, organization, national ID (masked except last 4 digits for Operator role), last granted entry, status.
5. **Companions panel (end side):** one counter per age category (− value +), live total «الإجمالي مع حامل البطاقة».
6. **Action bar:** F5 تسجيل الدخول · F9 تجاوز · F2 تسجيل حامل بطاقة جديد · Esc مسح.
7. **Today grid:** last 20 attempts at this workstation (time, name, card, persons, result) + today's totals by age category.

Keyboard-first: the whole flow must be possible without a mouse.

## 6. Business rules

| ID | Rule |
|---|---|
| BR-01 | **Card number normalization:** trim, collapse inner whitespace, convert Arabic-Indic (٠-٩) and Persian (۰-۹) digits to 0-9, uppercase Latin letters. Dashes are kept until Q1 is answered. Card number is unique per organization. |
| BR-02 | **Effective cooldown** = `Organization.CooldownMinutes` if set, otherwise setting `Entry.DefaultCooldownMinutes` (default 720 = 12 h). Stored in minutes, edited in hours in the UI (allowed 1–168 h; minutes allowed for testing via Admin). |
| BR-03 | The window is **rolling** and measured from the holder's **last granted entry** (`Allowed` or `Overridden`, not voided). Denied attempts never reset it. |
| BR-04 | Eligible when `nowUtc >= lastGrantedUtc + cooldown`. Exactly on the boundary = eligible. No previous entry = eligible. |
| BR-05 | **Time source:** the database server clock (`SYSUTCDATETIME()`) is the only clock used for rules and timestamps. Client PC clocks are never trusted. Stored in UTC, displayed in park local time (setting `Park.TimeZoneId`). |
| BR-06 | **Check order** (first failure is reported): organization inactive → holder Blocked → holder Suspended → card expired (`CardExpiryDate < today local`) → cooldown. |
| BR-07 | **Companions:** a count per active age category, each ≥ 0. Group size (holder + companions) must be ≤ setting `Entry.MaxGroupSize` (default 10). |
| BR-08 | The **holder counts as one person** and is not included in companion counts. `TotalPersons = 1 + Σ companions`. (Confirmed by the client — Q2.) |
| BR-09 | Each entry stores a snapshot of companion counts. Later edits to the holder never change history. |
| BR-10 | Every denied attempt is logged. **Dedupe:** if the same holder was denied at the same workstation within `Entry.DenialDedupMinutes` (default 5), do not insert a new row. |
| BR-11 | **Override:** Supervisor/Admin only, reason mandatory (≥ 5 characters), recorded as `Overridden`, counts as a granted entry for BR-03. Allowed for: cooldown, expired card, suspended holder. Not allowed for: Blocked holder, inactive organization. |
| BR-12 | **Concurrency:** eligibility check and entry insert run in one transaction holding an update lock on the holder row (see DATABASE.md §4). Two desks can never both grant the same window. |
| BR-13 | **Void:** entry records are immutable. A Supervisor can void a granted entry with a reason. Voiding recalculates `LastGrantedEntryUtc` from the latest remaining non-voided granted entry. Denied attempts cannot be voided. |
| BR-14 | Holders, organizations, users and age categories are **never hard-deleted**; they are deactivated/suspended. |
| BR-15 | **Phone (required):** Libyan mobile normalized to `09XXXXXXXX`. Accept `+2189…`, `002189…`, `2189…` and Arabic-Indic digits as input. |
| BR-16 | **National ID (optional):** digits only after normalization, exactly 12 digits when present. Unique when present (filtered unique index). |
| BR-17 | **Audit:** create/update of holders, status changes, overrides, voids, settings changes, user management and logins are written to the audit log with user, workstation and DB time. |
| BR-18 | **Account lockout:** after `Security.MaxFailedLogins` (default 5) failures, the account is locked for `Security.LockoutMinutes` (default 15). |

## 7. Functional requirements

Priority: **M** = MVP, **L** = later in MVP roadmap (Phase 6–8).

### 7.1 Startup and authentication (FR-AUTH)
- FR-AUTH-01 (M) Startup checks DB connectivity; on failure shows a connection settings dialog (server, instance, database, SQL login) and stores it protected (DPAPI, machine scope).
- FR-AUTH-02 (M) If the schema has pending migrations, the app refuses to continue with a clear Arabic message (migrations are applied by deployment, ADR-011).
- FR-AUTH-03 (M) If no users exist, a first-run wizard creates the first Admin. No default password ships in the repo.
- FR-AUTH-04 (M) Login with username + password; lockout per BR-18; `MustChangePassword` forces a change after first login or admin reset.
- FR-AUTH-05 (M) Workstation is identified by machine name and auto-registered on first run; Admin can rename it.
- FR-AUTH-06 (L) Auto-lock after idle time (setting).

### 7.2 Check-in (FR-CHK)
- FR-CHK-01 (M) Lookup by card number with normalization and multi-organization disambiguation.
- FR-CHK-02 (M) Eligibility preview on lookup with reason and next allowed time.
- FR-CHK-03 (M) Register entry with companions (atomic).
- FR-CHK-04 (M) Inline registration of a new holder.
- FR-CHK-05 (M) Denied attempt logging with dedupe.
- FR-CHK-06 (M) Supervisor override with re-authentication when needed.
- FR-CHK-07 (M) Today grid and today totals at this workstation.
- FR-CHK-08 (M) Sound on denial (setting).
- FR-CHK-09 (L) Void entry from the today grid or holder history (Supervisor).

### 7.3 Holders (FR-BEN)
- FR-BEN-01 (M) Search by card number, name (contains), phone, national ID.
- FR-BEN-02 (M) View holder details and full entry history.
- FR-BEN-03 (M) Edit holder (Supervisor), including card number change (audited).
- FR-BEN-04 (M) Suspend / reactivate / block with reason.

### 7.4 Administration (FR-ADM)
- FR-ADM-01 (M) Organizations: add/edit/deactivate, optional per-organization cooldown.
- FR-ADM-02 (M) Age categories: name, description (e.g. «من 3 إلى 12 سنة»), sort order, active.
- FR-ADM-03 (M) Settings screen for all keys in DATABASE.md §5.
- FR-ADM-04 (M) Users: create, edit role, deactivate, reset password.
- FR-ADM-05 (L) Backup now (to a folder on the SQL Server machine) + documented scheduled backup.

### 7.5 Reports (FR-RPT) — Phase 7
- FR-RPT-01 Daily summary: granted entries, persons by age category, denials, overrides.
- FR-RPT-02 Date range summary per day.
- FR-RPT-03 **Per organization statistics** (high priority for the client; no financial claims are made): entries and persons by category in a date range, with daily/monthly breakdown.
- FR-RPT-04 Per operator and per workstation.
- FR-RPT-05 Denials list with reasons; overrides list with reasons and approver.
- FR-RPT-06 Hourly distribution (peak hours).
- FR-RPT-07 Export to Excel (.xlsx) and print preview.

## 8. Non-functional requirements

| Area | Requirement |
|---|---|
| Platform | Windows 10/11 x64. .NET 10 (LTS) desktop. |
| Topology | One SQL Server Express instance on a server PC; 1–10 desk PCs on the same LAN. |
| Performance | Lookup + eligibility < 1 s on LAN; full check-in < 10 s of operator time. |
| Language | Arabic UI, full RTL (forms, grids, dialogs, message boxes). Western digits 0-9 in display. Dates `dd/MM/yyyy`, times `hh:mm` with ص/م. |
| Input | Keyboard-first; keyboard-wedge barcode/QR scanners supported in the card box. |
| Security | PBKDF2 password hashing; role checks in Application; least-privilege SQL login; connection secret protected with DPAPI; national ID masked for Operators. |
| Reliability | Every write is transactional. No offline mode in MVP: if the server is down, the desk cannot record entries (decision, see Q6). |
| Observability | Serilog rolling files in `%ProgramData%\ParkGate\logs`, retained 30 days. Global exception handlers. |
| Data | SQL Server Express limits (10 GB per database) are far above expected volume. Daily backups required (Phase 8). |
| Maintainability | Clean Architecture (4 projects), SOLID, DI, automated tests on rules and use cases, CI on GitHub. |

## 9. Future requirements (design for now, build only when approved)

The MVP data model and abstractions must make these additive, not rewrites.

### F1 — QR credential
- Issue a QR to a holder; QR content is an **opaque random token** (≥ 128 bits, base64url), never personal data.
- Only the **SHA-256 hash** of the token is stored. Reprinting = issuing a new token and revoking the old one.
- Scanning the QR at the desk (keyboard-wedge scanner) resolves to the holder and runs the same check-in flow with `Channel = QrDesk`.
- Print a card-size QR with the holder's name and organization logo.

### F2 — Electronic gate / turnstile
- A separate host process (e.g. Windows Service `ParkGate.GateService`) reusing Domain + Application; hardware behind `IGateController` (relay over TCP/serial, vendor SDK).
- **Group problem** (holder + companions through a gate that only sees one QR). Options to decide before F2:
  - **Option A — declared household size:** holder's household (adults/children/infants) registered once; gate allows up to N passages within T seconds after one scan. Fast to build, less accurate.
  - **Option B — dependents with their own QR:** each family member is a dependent linked to the holder with their own QR; each scan = one person; cooldown evaluated per holder household. Most accurate; requires a dependents registry.
  - **Option C — hybrid:** gate validates the holder; an attendant confirms companions on a small screen next to the gate.
  - Recommendation: B if the fund defines family membership, otherwise C.
- Gate failures must fail **closed** (stay locked) and alert the desk.

### F3 — Cloud read-only dashboard (hybrid)
- One-way push from the local DB (source of truth) to a cloud store; the cloud never writes back.
- Push only what dashboards need; personal data minimized or excluded (Q12).
- Change tracking via `rowversion` / `CreatedAtUtc` watermark; a background sync worker with retry; works when internet is intermittent.

## 10. Data retention and privacy (MVP)
- Entry history is kept indefinitely until the client defines a policy.
- Personal data is visible only to authenticated users; exports are limited to Supervisor/Admin.

## 11. Open questions

Defaults below are applied until the user answers. An agent must not change a default silently.

| ID | Question | Default applied | Blocks |
|---|---|---|---|
| Q1 | Card number format/length? Does the card have a barcode? Are dashes significant? | Free text ≤ 50 chars, BR-01 normalization, dashes kept | — |
| Q2 | Is the holder counted inside the adult count or separately? | **Answered 2026-10-04:** separately (BR-08) | — |
| Q3 | Rolling 12 h window, or "once per calendar day / per opening day"? | **Answered 2026-10-04:** rolling window in hours (BR-02/BR-03), chosen for finer control | — |
| Q4 | Maximum group size? Limits per category? | Max 10 total, no per-category limit | — |
| Q5 | Which holder fields are required by the client (national ID, card expiry)? | Name + phone required; others optional | — |
| Q6 | How many desks? Is an offline fallback needed if the server PC fails? | Up to 10 desks, no offline mode | — |
| Q7 | Do other organizations get different rules (cooldown, group size)? | Per-organization cooldown only | — |
| Q8 | Does the park invoice the fund for free entries? What report format does the fund require? | **Answered 2026-10-04:** no financial claims; per-organization statistics are important (FR-RPT-03) | — |
| Q9 | Must card validity be checked against the fund's own database? | No integration | — |
| Q10 | Operating hours — any rule tied to closing time? | None | — |
| Q11 | Gate group handling: Option A, B or C (§9 F2)? | **Deferred by the client** until F2 is scheduled | F2 |
| Q12 | Which data may go to the cloud? | Aggregates only | F3 |
