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
| الزائر (الجمع: الزوار) | `Beneficiary` | Registered card holder. UI never says «الزبون» or «المستفيد» (R7.1) |
| رقم البطاقة | `CardNumber` | Card identifier, unique per organization |
| المرافقون | `Companions` | People entering with the holder |
| الفئة العمرية | `AgeCategory` | بالغ / طفل / رضيع (configurable list) |
| محاولة دخول | `EntryAttempt` | One check-in attempt: granted, denied or overridden |
| فترة الانتظار | `Cooldown` | Minimum time between two granted entries. Not «فترة السماح» / «فترة التهدئة» |
| تجاوز | `Override` | Supervisor grants entry despite a denial |
| إلغاء عملية | `Void` | Supervisor cancels a mistaken entry record |
| موظف الشباك | `Operator` role | Desk employee |
| مشرف | `Supervisor` role | Shift supervisor |
| مدير النظام | `Admin` role | System administrator |
| قسيمة الهدية | `GiftVoucher` | The printed gift QR slip; its number is «رقم القسيمة». Not «كوبون» / «وصل» |
| الشباك (الجمع: الشبابيك) | `Workstation` | A desk PC (later: a gate device). Not «المحطة» |

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
| A5 | Supervisor override (from A3, or from A4 for a suspended holder or an expired card, or on a day that is not an entry day of the organization, BR-27) | Supervisor presses **F9 «تجاوز»**, authenticates if the logged-in user is not a supervisor (username + password; the approval is a single-use token, ADR-023), enters a mandatory reason. Entry recorded as `Overridden`. Not available for Blocked holders or inactive organizations (BR-11). |
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
| BR-06 | **Check order** (first failure is reported): organization inactive → holder Blocked → not an entry day of the organization (BR-27) → holder Suspended → card expired (`CardExpiryDate < today local`) → cooldown. Every reason that cannot be overridden comes before the ones that can, because an override skips only the first reason. |
| BR-07 | **Companions:** a count per active age category, each ≥ 0. Group size (holder + companions) must be ≤ setting `Entry.MaxGroupSize` (default 10). |
| BR-08 | The **holder counts as one person** and is not included in companion counts. `TotalPersons = 1 + Σ companions`. (Confirmed by the client — Q2.) |
| BR-09 | Each entry stores a snapshot of companion counts. Later edits to the holder never change history. |
| BR-10 | Every denied attempt is logged. **Dedupe:** if the same holder was denied at the same workstation within `Entry.DenialDedupMinutes` (default 5), do not insert a new row. |
| BR-11 | **Override:** Supervisor/Admin only, reason mandatory (≥ 5 characters), recorded as `Overridden`, counts as a granted entry for BR-03. Allowed for: cooldown, expired card, suspended holder, not an entry day (BR-27). Not allowed for: Blocked holder, inactive organization. |
| BR-12 | **Concurrency:** optimistic (ADR-026). Every grant updates the holder row, whose `RowVersion` makes a concurrent grant fail at save; the use case retries from the start (at most 3 times) and the retry is judged by the normal rules. Two desks can never both grant the same window. See DATABASE.md §4. |
| BR-13 | **Void:** entry records are immutable. A Supervisor can void a granted entry with a reason. Voiding recalculates `LastGrantedEntryUtc` from the latest remaining non-voided granted entry. Denied attempts cannot be voided. |
| BR-14 | Holders, organizations, users and age categories are **never hard-deleted**; they are deactivated/suspended. |
| BR-15 | **Phone (required):** Libyan mobile normalized to `09XXXXXXXX`. Accept `+2189…`, `002189…`, `2189…` and Arabic-Indic digits as input. |
| BR-16 | **National ID (optional):** digits only after normalization, exactly 12 digits when present. Unique when present (filtered unique index). |
| BR-17 | **Audit:** create/update of holders, status changes, overrides, voids, settings changes, user management and logins are written to the audit log with user, workstation and DB time. |
| BR-18 | **Account lockout:** after `Security.MaxFailedLogins` (default 5) failures, the account is locked for `Security.LockoutMinutes` (default 15). |
| BR-19 | **Organization gift policy (F5):** `GiftEnabled`, `GiftValue` (> 0 when enabled), `GiftIntervalDays` (1–365). Managed by Admin (`ManageOrganizations`). |
| BR-20 | **Gift check order** (first failure is reported): organization inactive → gifts not enabled → holder Blocked → holder Suspended → no granted entry today (BR-30) → not a gift day (BR-28) → interval not elapsed. Eligible when `nowUtc >= lastGiftIssuedUtc + GiftIntervalDays`, or no earlier voucher. The interval starts when a voucher is issued; voided vouchers do not count. DB clock (BR-05). |
| BR-21 | **Gift override:** only "interval not elapsed" and "not a gift day" can be overridden, by Supervisor/Admin, reason ≥ 5 characters, recorded on the voucher (`IsOverride`, `OverrideByUserId`, `OverrideReason`). |
| BR-22 | **Atomic issue:** issuing updates the holder's `LastGiftIssuedUtc`, so two desks issuing at once conflict on the holder's `RowVersion` and the loser retries (ADR-026). |
| BR-23 | **Voucher QR:** the signed 203-digit token of ADR-028 with its own signing context, so an entry-card QR is never accepted as a gift and the reverse. Printed on 80 mm paper with the voucher number and no value. |
| BR-24 | **Redemption is single-use:** `Issued → Redeemed` in one save, guarded by the voucher's `RowVersion`; a second scan gets "already redeemed". Redeemer, desk and DB time are stored. A voucher expires at the end of its business date. A Supervisor can void an issued voucher with a reason (`VoidGift`); redeemed vouchers never change. |
| BR-25 | **Free-companion limit:** when `Organization.MaxFreeCompanions` is set, the companions in categories not flagged `IsCompanionLimitExempt` must not exceed it; otherwise check-in and override are refused (`CheckIn.ExceedsFreeCompanionLimit`). The holder is not counted. BR-07 still applies to the whole group. |
| BR-26 | **Infant exemption:** an age category flagged `IsCompanionLimitExempt` never counts toward BR-25. The seeded «رضيع» is flagged; Admin can change the flag. |
| BR-27 | **Entry days (F8, ADR-024):** each organization has allowed weekdays (`AllowedEntryDays`, default all seven). The weekday is that of the park-local business date from the database clock (BR-05). On another day the entry is denied with `DayNotAllowed`, and the banner names the allowed days. Checked right after "holder blocked" in the BR-06 order (not right after "organization inactive"): an override skips only the first denial reason, so every reason that cannot be overridden must come first. A Supervisor can override it (BR-11). The days are changed by hand in the organization dialog (Admin, `ManageOrganizations`); the change applies from the next lookup and is written to the audit trail with the days before and after (owner, 2026-11-20). There is no dated or temporary schedule. |
| BR-28 | **Gift days (F8):** a gift follows the organization's entry days (BR-27), unless the organization's `GiftAnyDay` option is on. On another day the gift is denied with `DayNotAllowed`, checked after `NoEntryToday` (BR-30); the supervisor gift override can still issue it (Q13). |
| BR-29 | **Card-number pattern alert (F8):** when a holder is registered or their card number changes, the number's shape (length, character class per position, and a prefix shared by most numbers) is compared with the organization's patterns. A mismatch shows a non-blocking amber note to the operator and writes an alert for the Admin. After `CardPattern.MinSample` (default 10) cards, the dominant shape is proposed and used provisionally until the Admin decides. A shape that reaches `CardPattern.NewSeriesThreshold` (default 5) alerts is proposed as a new series. **Only an Admin approves a pattern**; it is never approved automatically. A rejected pattern keeps alerting, and its holders stay on the review list. **Built rules (S41):** the shape is one class per character (`D` digit, `L` letter, `-` dash, `O` other). The first pattern is proposed when the organization has `MinSample` cards and one shape holds more than half of them; its prefix is the first `PrefixLength` characters when `PrefixSharePercent` of those numbers share them. This first (dominant) proposal is used provisionally until the Admin decides; a new-series proposal stops no alert until approved. Approving a pattern closes the open alerts it matches. The check runs in the same save as the registration or card change, and the desk shows «رقم غير معتاد» beside the name of a holder with an open alert. Only Admins see «الإدارة ← أنماط أرقام البطاقات» (`ManageCardPatterns`); every proposal, decision, alert and review is in the audit trail. |
| BR-30 | **Gift only after today's entry (owner, 2026-11-18):** a gift voucher is issued only when the organization has gifts enabled (`GiftEnabled`) **and** the holder has a granted or overridden, non-voided entry on today's park business date (BR-05). Otherwise the gift is denied with `NoEntryToday`, checked right after the holder status in the BR-20 order. It is **not overridable**, not even by the supervisor gift override (BR-21). |
| BR-31 | **Voiding an entry and its gift (owner, 2026-11-19):** a gift rests on the day's entry (BR-30). When the voided entry is the holder's only granted or overridden, non-voided entry of that business date, that day's unredeemed gift vouchers are voided with it, in the same transaction and with the same reason and approver, and the gift interval is recalculated (BR-21). If a gift of that day was **already redeemed**, the entry **cannot be voided** (`CheckIn.GiftAlreadyRedeemed`), with no exception. When another valid entry of that day remains, the gifts are not touched. |

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

## 9. Additional features (F1, F3, F4, F5, F8 built; F2 future; F6, F7 recommendations only)

The MVP data model and abstractions must make these additive, not rewrites.

### F1 — QR entry card (built)
- An Admin issues an entry card to an active holder from «الزوار» (the same permission as editing a holder, `EditBeneficiary`). The QR is the signed token of ADR-028: a random card id, an expiry, the key id and an ECDSA signature. No personal data.
- Printed on 80 mm cashier paper with the park and organization names (R7.12), without a logo or personal data.
- Scanning the card at the desk (keyboard-wedge scanner) checks signature, expiry, revocation and the holder's status, then runs the normal check-in for that holder. Every scan is logged in `CardScanLog`.
- Reprinting issues a new card id and revokes the old card.

### F2 — Electronic gate / turnstile
- A separate host process (e.g. Windows Service `ParkGate.GateService`) reusing Domain + Application; hardware behind `IGateController` (relay over TCP/serial, vendor SDK).
- **Group problem** (holder + companions through a gate that only sees one QR). Options to decide before F2:
  - **Option A — declared household size:** holder's household (adults/children/infants) registered once; gate allows up to N passages within T seconds after one scan. Fast to build, less accurate.
  - **Option B — dependents with their own QR:** each family member is a dependent linked to the holder with their own QR; each scan = one person; cooldown evaluated per holder household. Most accurate; requires a dependents registry.
  - **Option C — hybrid:** gate validates the holder; an attendant confirms companions on a small screen next to the gate.
  - Recommendation: B if the fund defines family membership, otherwise C.
- Gate failures must fail **closed** (stay locked) and alert the desk.

### F3 — Cloud sync agent (built)
- One-way push from the local database (the source of truth) to the cloud; the cloud never writes back.
- Changes are captured in `SyncOutboxes` in the same save as the business change. Rows are immutable and ordered by a database sequence (ADR-029). The Sync Agent (a Windows service, ADR-018) pushes batches over HTTPS with an API key stored with DPAPI (R8.3), retries on failure and sends a heartbeat.
- **Phone and national ID never leave the park** (ADR-021); notes and status reason do (Q-R2b). Gift vouchers sync without their redeemable token (ADR-022).

### F4 — Cloud web dashboard (built)
- Laravel + Inertia (Vue 3) in `web/`, Arabic RTL, on cPanel hosting (DEPLOYMENT_WEB_CPANEL.md). Users are created with `php artisan parkgate:create-user`; there is no self-registration.
- Today's figures, organizations summary, hourly chart, reports and a holder search, all in park-local time (R7.5). A park status indicator from the heartbeat.

### F5 — Gift vouchers and free-companion limit (built)
- Rules BR-19 to BR-26, BR-30 and BR-31. The desk offers the gift right after a granted entry (R5.14); the gift redemption screen scans the voucher; «التقارير ← تقرير قسائم الهدايا» lists vouchers and lets a Supervisor void one (R5.17).

### F6 — Phone verification (recommendation to park management, not built)
- Highest assurance option. After each free entry or gift, the holder receives an SMS or WhatsApp notification; sensitive actions (gift issue, lost-card replacement, new registration) require a one-time code sent to the holder's phone.
- Stops an operator from spending the quota of an absent holder without the holder knowing. A code can still be forwarded, and the phone number must come from the organization or be verified once at registration, or the operator could enter their own.
- Needs an SMS gateway or `WhatsApp Business API` (cost per message), internet at the desk with a fallback that never blocks entry, and a change to ADR-021 because the phone number would leave the park. Requires an ADR and Osama's approval before any work.

### F7 — Organization card-number list (recommendation to park management, not built)
- Each organization provides the card numbers of its employees, as an exact list (preferred) or a range, without names; full employee data only if the organization agrees.
- The desk can register a holder only with a number on the list, so an operator cannot invent holders. A number registered by someone else is reported to a supervisor when its real owner arrives.
- A range is much weaker than a list: any number inside it passes.
- To decide when approved: what happens for an organization without a list (recommended: supervisor approval), and when a number disappears from a new list (recommended: suspend the holder with the reason shown). Import from Excel with ClosedXML.

### F8 — Entry days and card-number pattern alerts (approved 2026-11-18; built 2026-11-20)
- Mitigates the forged-card risk accepted in ADR-024. Rules: BR-27, BR-28, BR-29. Data: DATABASE.md §3.1, §3.13, §3.14.
- Admin screens: allowed weekdays and "gifts on any day" in the organization dialog; a pattern list (approve or reject proposed patterns); an alert list (mark reviewed, or open the holder to suspend or block).
- The desk shows the amber pattern note under the holder data and never blocks the registration because of it.

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
| Q6 | How many desks? Is an offline fallback needed if the server PC fails? | **Answered 2026-11-20 (Q-R6):** at most 4 desks in a workgroup; no offline mode. One SQL login for the desks (ADR-027) | — |
| Q7 | Do other organizations get different rules (cooldown, group size)? | Per-organization cooldown only | — |
| Q8 | Does the park invoice the fund for free entries? What report format does the fund require? | **Answered 2026-10-04:** no financial claims; per-organization statistics are important (FR-RPT-03) | — |
| Q9 | Must card validity be checked against the fund's own database? | **Answered 2026-11-16:** no integration now; the physical card is the reference (ADR-024). The organization's number list is recommended (F7) | — |
| Q10 | Operating hours — any rule tied to closing time? | **Answered (Q-R5):** the park closes at midnight (last entry about 23:00), so the calendar date is the business date | — |
| Q11 | Gate group handling: Option A, B or C (§9 F2)? | **Deferred by the client** until F2 is scheduled | F2 |
| Q12 | Which data may go to the cloud? | Answered (Q-R2/Q-R2b): everything except phone and national ID | F3 |
| Q13 | May the supervisor gift override issue a gift on a day the organization does not allow (BR-28)? | **Answered and built:** yes, like the entry override (BR-21) | — |
| Q14 | Do the new F8 fields, patterns and alerts sync to the cloud? | **Decided:** the two organization fields yes, patterns and alerts stay local. The organization fields are not synced yet (T8.5) | T8.5 |
