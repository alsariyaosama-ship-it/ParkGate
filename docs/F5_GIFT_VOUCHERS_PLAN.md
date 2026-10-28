# Phase F5 — Gift Vouchers (QR) and Free-Companion Limit — Implementation Plan

> **Audience:** a lightweight coding agent (Gemini 3.8 Flash) that executes this plan task by task, then Claude reviews the result.
> **Status:** PROPOSED — needs Osama's approval before T-G0 starts (SPEC/DATABASE/AGENTS changes are listed in §9).
> **Branch:** `phase/f5-gift-vouchers` · **Language rule:** code/docs/commits in English, UI text Arabic in `Strings.resx` only.

---

## 0. How you (the executing agent) must work — read first

These rules exist because earlier plans failed when the agent guessed names. Follow them literally.

1. **Never invent a name.** Before you edit or reference any existing class, method, property, resx key or file, open the file and read it. If a name in this plan does not exist, **stop and report**; do not create a lookalike.
2. **One task (T-Gx) at a time.** Finish it, run its "Verify" commands, commit, then start the next. Do not start a later task early.
3. **Do not touch files outside the list of the current task.** The working tree may contain unrelated edits; never run `git add -A`, `git add .`, `git stash`, `git reset`, `git checkout -- <file>`, or any force operation. Stage with explicit paths only.
4. **Do not reformat.** Keep each file's existing line endings, indentation and using-order. No "cleanup" edits.
5. **Signature changes ripple.** Many tests construct `Organization`, `OrganizationDto`, `AgeCategoryDto`, requests and `FakeUnitOfWork` directly. This plan is designed so existing call sites keep compiling (new members are added, existing constructors are unchanged, new record parameters are trailing with defaults). If the build breaks in a test project, fix the *test* minimally; never weaken an assertion.
6. **Build and test after every task:**
   ```
   dotnet build ParkGate.sln -c Debug
   dotnet test ParkGate.sln --filter "Category!=Integration"
   ```
   Integration tests (LocalDB) are required for T-G5 and T-G11:
   `dotnet test tests/ParkGate.Infrastructure.Tests --filter "Category=Integration"`.
7. **Zero warnings** in Domain, Application, Infrastructure (warnings are errors). Keep `sealed`, file-scoped namespaces, `_camelCase` fields, `Async` suffix.
8. **No new NuGet package, no new project.** Everything below fits in the 4 existing production projects (+ `ParkGate.SyncAgent`, which you must not edit).
9. **No Arabic string literals in `.cs` files.** Use `Strings.resx` + `Strings.Designer.cs` (see §8.3). Existing `IssueQrDialog` violates this — do **not** copy its style.
10. **If anything is ambiguous, stop and ask Osama** (AGENTS §11). List the question and 2–3 options.
11. **Commit format:** `feat(gift): <what> [T-Gx]`, ending with the attribution line required by the session. Push at the end of the session only if Osama asked; otherwise leave local and say so.

---

## 1. The business feature (plain language)

1. A customer (member of an organization) arrives at the park gate. The gate employee types the card number **or scans the customer's entry QR** (already supported by `CardScanService`).
2. Besides the entry result, the desk now also shows a **Gift panel**: "eligible for a gift: yes/no, and why/when."
3. If eligible, the employee presses **Issue gift QR**. The system creates a **gift voucher** (value fixed by the customer's organization) and shows a **signed QR** to print or show to the customer.
4. The customer walks to the **rides ticket window**. The rides employee (also logged in to ParkGate over the LAN) scans the QR on the **Gift redemption screen**. The system confirms it is genuine, unused and not expired, shows the **value**, and records the redemption as a **receipt** (رقم الوصل). The employee then hands over the physical rides coupon of that value. **Physical coupons are not issued by the system**; the system only records receipts, which can be reported.
5. Each organization is configured by the Admin: gifts on/off, gift value, minimum interval between gifts per person (e.g. 30 days). Only a **Supervisor/Admin** can issue a gift before the interval ends (with a reason).
6. **Update (companion limit):** the Admin can also set, per organization, the maximum number of companions that may enter free with the holder (e.g. 3), regardless of adults/children. **Infants are always free and do not count toward the limit.**

---

## 2. Confirmed decisions (answered by Osama on 2026-10-28)

| ID | Decision |
|---|---|
| D1 | The free-companion limit counts **companions only**, not the holder. Limit 3 ⇒ holder + 3 companions (+ any infants). Consistent with BR-08. |
| D2 | When non-exempt companions exceed the organization limit, **check-in is rejected** with a clear error; the employee lowers the count. **No supervisor override** for this rule. (Extra people pay outside the system.) |
| D3 | Gift eligibility is **independent of entry**. No requirement for a granted entry today. |

## 3. Assumed decisions (Claude's recommendation — Osama may veto before T-G0)

| ID | Assumption | Why |
|---|---|---|
| A1 | Gift interval is a **rolling number of days** per organization (default 30), measured from the last **non-voided** voucher's `IssuedAtUtc`. Boundary: exactly at the end = eligible. | Same style as BR-03/BR-04, deterministic, testable. |
| A2 | An issued-but-never-redeemed voucher **still counts** toward the interval (so a lost QR cannot be re-issued without a supervisor override or a void). | Prevents abuse; supervisor void is the escape hatch. |
| A3 | A voucher is valid until the **end of the park business day** it was issued (`ExpiresOn = BusinessDate`). | Gift is meant for the same visit. No new setting. |
| A4 | Gift value is a fixed amount per organization in the park currency, `decimal(10,3)`, shown with the Arabic currency label from `Strings.resx` (default label «د.ل»). | Simple; no per-age pricing requested. |
| A5 | Card expiry of the holder is **not** checked for gifts (not requested). Holder `Suspended`/`Blocked` and inactive organization **do** block gifts. | Minimal rules; revisit later. |
| A6 | Voiding an **unredeemed** voucher is allowed for Supervisor/Admin with a reason (≥ 5 chars); a redeemed voucher can never be voided. A voided voucher no longer counts toward the interval. | Needed for printing mistakes. |
| A7 | Gift data is **not synced to the cloud** in this phase (Q12: aggregates only). `GiftVoucher` is NOT added to `OutboxEntityExtractor.SyncableTypes`. | Keeps SyncAgent/web untouched. |
| A8 | New keyboard shortcuts on the desk: **F6** = issue gift, **F7** = issue gift with supervisor override. (F10 is avoided: Windows uses it for the menu.) | Needs AGENTS §5.11 update (§9). |

---

## 4. New business rules (to be added to SPEC §6 in T-G0)

| ID | Rule |
|---|---|
| BR-19 | **Organization gift policy:** `GiftEnabled`, `GiftValue` (> 0 when enabled), `GiftIntervalDays` (1–365). Managed by Admin (`ManageOrganizations`). |
| BR-20 | **Gift eligibility check order** (first failure reported): organization inactive → gifts not enabled for the organization → holder Blocked → holder Suspended → interval not elapsed. Eligible when `nowUtc >= lastGiftIssuedUtc + GiftIntervalDays` or no previous voucher. Time source is the DB clock (BR-05). |
| BR-21 | **Override:** only the *interval* denial can be overridden, only by Supervisor/Admin, reason mandatory (≥ 5 chars), recorded on the voucher (`IsOverride`, `OverrideByUserId`, `OverrideReason`). All other denials are not overridable. |
| BR-22 | **Atomic issue:** the holder row is locked (`UPDLOCK`) via `GetForUpdateAsync` during eligibility check + insert, so two desks cannot issue two gifts in the same interval. |
| BR-23 | **Voucher QR** is an ECDSA-signed numeric token (same 203-digit format) with a **separate signing context** so an entry-card QR can never be accepted as a gift and vice versa. |
| BR-24 | **Redemption is single-use and atomic:** the voucher row is locked and flipped `Issued → Redeemed` in one transaction. A second scan returns `AlreadyRedeemed`. Redeemer, workstation and DB time are stored. Redeemed vouchers are immutable. |
| BR-25 | **Free-companion limit:** if `Organization.MaxFreeCompanions` is set, the sum of companion counts in categories **not** flagged `IsCompanionLimitExempt` must be ≤ the limit; otherwise check-in/override is rejected (`CheckIn.ExceedsFreeCompanionLimit`). Holder is not counted. BR-07 (`Entry.MaxGroupSize`, total incl. holder and infants) still applies. |
| BR-26 | **Infant exemption:** an age category flagged `IsCompanionLimitExempt` never counts toward BR-25. The seeded «رضيع» category is flagged by the migration; Admin can change the flag on the age-category screen. |

---

## 5. Data model changes (to be added to `docs/DATABASE.md` in T-G0)

### 5.1 Enums (never renumber)
```
GiftVoucherStatus : 1 Issued, 2 Redeemed, 3 Voided
GiftDenialReason  : 1 OrganizationInactive, 2 GiftsNotEnabled, 3 HolderBlocked,
                    4 HolderSuspended, 5 IntervalNotElapsed
```
Stored as `tinyint`.

### 5.2 `Organizations` — new columns (released table → migration + Osama's approval)
| Column | Type | Notes |
|---|---|---|
| MaxFreeCompanions | int NULL | NULL = no org-specific limit. CHECK `NULL OR (0..50)` named `CK_Organizations_MaxFreeCompanions` |
| GiftEnabled | bit NOT NULL | default 0 |
| GiftValue | decimal(10,3) NOT NULL | default 0 |
| GiftIntervalDays | int NOT NULL | default 30. CHECK 1..365 named `CK_Organizations_GiftIntervalDays` |

Extra CHECK `CK_Organizations_GiftValue`: `[GiftEnabled] = 0 OR [GiftValue] > 0`.

### 5.3 `AgeCategories` — new column
| Column | Type | Notes |
|---|---|---|
| IsCompanionLimitExempt | bit NOT NULL | default 0. Migration also runs `UPDATE dbo.AgeCategories SET IsCompanionLimitExempt = 1 WHERE NameAr = N'رضيع'` |

### 5.4 `GiftVouchers` — new table (EF-managed, migration, **not** raw SQL)
| Column | Type | Notes |
|---|---|---|
| Id | int IDENTITY PK | This is the **receipt number** (رقم الوصل) |
| Token | uniqueidentifier NOT NULL | Random 128-bit from `QrTokenSigner.NewCardId()`; the value inside the QR. Unique index `UX_GiftVouchers_Token` |
| KeyId | tinyint NOT NULL | Signing key id used (no FK: `QrSigningKeys` is created by raw SQL, not EF) |
| BeneficiaryId | uniqueidentifier NOT NULL | FK → Beneficiaries, Restrict |
| OrganizationId | int NOT NULL | FK → Organizations, Restrict (snapshot at issue) |
| Value | decimal(10,3) NOT NULL | Snapshot at issue |
| IssuedAtUtc | datetime2(3) NOT NULL | DB time |
| BusinessDate | date NOT NULL | Park-local date of issue |
| ExpiresOn | date NOT NULL | = BusinessDate (A3) |
| Status | tinyint NOT NULL | `GiftVoucherStatus` |
| IssuedByUserId | int NOT NULL | FK → Users, Restrict |
| IssuedWorkstationId | int NOT NULL | FK → Workstations, Restrict |
| IsOverride | bit NOT NULL | |
| OverrideByUserId | int NULL | FK → Users |
| OverrideReason | nvarchar(250) NULL | |
| RedeemedAtUtc | datetime2(3) NULL | |
| RedeemedBusinessDate | date NULL | |
| RedeemedByUserId | int NULL | FK → Users |
| RedeemedWorkstationId | int NULL | FK → Workstations |
| VoidedAtUtc | datetime2(3) NULL | |
| VoidedByUserId | int NULL | FK → Users |
| VoidReason | nvarchar(250) NULL | |
| RowVersion | rowversion | |

CHECK constraints (names + logic):
- `CK_GiftVouchers_Override`: `([IsOverride] = 0 AND [OverrideByUserId] IS NULL AND [OverrideReason] IS NULL) OR ([IsOverride] = 1 AND [OverrideByUserId] IS NOT NULL AND [OverrideReason] IS NOT NULL)`
- `CK_GiftVouchers_Status`: `([Status] = 1 AND [RedeemedAtUtc] IS NULL AND [VoidedAtUtc] IS NULL) OR ([Status] = 2 AND [RedeemedAtUtc] IS NOT NULL AND [RedeemedBusinessDate] IS NOT NULL AND [RedeemedByUserId] IS NOT NULL AND [RedeemedWorkstationId] IS NOT NULL AND [VoidedAtUtc] IS NULL) OR ([Status] = 3 AND [VoidedAtUtc] IS NOT NULL AND [VoidedByUserId] IS NOT NULL AND [VoidReason] IS NOT NULL AND [RedeemedAtUtc] IS NULL)`
- `CK_GiftVouchers_Value`: `[Value] > 0`

Indexes: `UX_GiftVouchers_Token (Token)` unique · `IX_GiftVouchers_Beneficiary_Time (BeneficiaryId, IssuedAtUtc DESC)` · `IX_GiftVouchers_BusinessDate (BusinessDate) INCLUDE (Status, Value, OrganizationId)` · `IX_GiftVouchers_RedeemedBusinessDate (RedeemedBusinessDate) INCLUDE (Value, OrganizationId, RedeemedWorkstationId) WHERE [RedeemedBusinessDate] IS NOT NULL`.

`GiftVoucher` does **not** implement `IAuditableEntity` (it carries explicit who/when columns instead). It does not touch `Beneficiaries` (no new column there); "last gift" is derived by query under the holder lock.

### 5.5 New settings
None.

---

## 6. Architecture overview (what goes where)

```
Domain
  Enums/GiftVoucherStatus.cs, Enums/GiftDenialReason.cs
  Entities/GiftVoucher.cs
  Entities/Organization.cs        (+ props, + 2 methods)      Entities/AgeCategory.cs (+ prop, + 1 method)
  Policies/IGiftEligibilityPolicy.cs, GiftEligibilityContext.cs, GiftEligibilityDecision.cs,
           RollingIntervalGiftEligibilityPolicy.cs
  EntryCards/QrPurpose.cs         QrTokenFormat.cs (context per purpose)

Application
  Security/Permission.cs, RolePermissions.cs  (+ IssueGift, RedeemGift, OverrideGiftInterval, VoidGift)
  Common/Errors.cs                (+ CheckIn.ExceedsFreeCompanionLimit, + Errors.Gift.*)
  EntryCards/QrTokenSigner.cs, QrTokenVerifier.cs  (+ optional QrPurpose param, default EntryCard)
  Ports/Persistence/IGiftVoucherRepository.cs, IUnitOfWork.cs (+ GiftVouchers)
  Ports/Reports/IGiftReportQueries.cs
  Ports/EntryCards/IQrImageRenderer.cs
  GiftVouchers/DTOs, Services (IGiftVoucherService, GiftVoucherService), Validators
  CheckIn/Services/CheckInService.cs            (+ free-companion limit)
  Organizations/…                               (+ SetPolicyAsync)
  AgeCategories/…                               (+ IsCompanionLimitExempt)

Infrastructure
  Data/Configurations/GiftVoucherConfiguration.cs, OrganizationConfiguration.cs, AgeCategoryConfiguration.cs
  Data/ParkGateDbContext.cs (+ DbSet), UnitOfWork.cs (+ GiftVouchers), Repositories/GiftVoucherRepository.cs
  Data/Migrations/<timestamp>_AddGiftVouchersAndCompanionLimit.cs   (generated by dotnet ef)
  Reports/GiftReportQueries.cs, EntryCards/QrImageRendererAdapter.cs
  DependencyInjection.cs (register new services)

WinForms
  Controls/GiftOfferPanel (+Designer)      Forms/GiftQrDialog (+Designer)
  Controls/GiftRedeemControl (+Designer)   Forms/GiftReceiptsForm (+Designer)
  EditOrganizationDialog (+ policy group)  EditAgeCategoryDialog (+ exempt checkbox)
  CheckInDeskControl (companion limit + gift panel hook)   MainForm (menus)
  Resources/Strings.resx + Strings.Designer.cs, Common/ErrorMessages.cs, DependencyInjection.cs
```

Layer rules still hold: Application never references EF/WinForms; WinForms never touches `DbContext` or Infrastructure types (use the new `IQrImageRenderer` port, not `QrImageRenderer` directly).

---

## 7. Tasks

Each task lists: **Files**, **Do**, **Tests**, **Verify**, **Done when**. Test naming: `Method_Scenario_ExpectedResult`.

### T-G0 — Preconditions, branch, documentation (no production code)

**Preconditions (stop if any fails and tell Osama):**
- `git status` must be clean. At plan time the tree has ~30 modified/untracked files from the cloud-sync/QR-provider work. **Osama must commit or otherwise resolve them first.** Do not do it yourself.
- Osama has approved §9.

**Do:**
1. `git switch -c phase/f5-gift-vouchers` from the branch Osama names.
2. Run the baseline build + unit tests; if red, stop and report.
3. Docs only (propose the diff to Osama, commit after approval):
   - `docs/SPEC.md`: add BR-19..BR-26 (§4 of this plan) and FRs (issue gift, redeem gift, gift report, org gift policy, free-companion limit).
   - `docs/DATABASE.md`: add §2 enums, §3.2/§3.3 new columns, new §3.12 `GiftVouchers`, note on §3.11.
   - `docs/DECISIONS.md`: append **ADR-020** — "Gift vouchers use the same ECDSA infrastructure with a separate signing context; EF-managed table (not raw SQL like EntryCards); rolling interval per org; companion limit per org with infant exemption flag".
   - `docs/ROADMAP.md`: add "Phase F5" with T-G1..T-G11 unchecked.
   - `AGENTS.md` §5.11: add F6/F7 — **only if Osama approves** (AGENTS changes need his approval).
4. Commit: `docs: plan gift vouchers and free-companion limit [T-G0]`.

**Verify:** `git status` clean; docs diff reviewed by Osama.

---

### T-G1 — Domain model and policy

**Files (create):**
- `src/ParkGate.Domain/Enums/GiftVoucherStatus.cs`, `GiftDenialReason.cs` (`enum … : byte`, explicit numbers per §5.1).
- `src/ParkGate.Domain/Entities/GiftVoucher.cs`
- `src/ParkGate.Domain/Policies/IGiftEligibilityPolicy.cs`, `GiftEligibilityContext.cs`, `GiftEligibilityDecision.cs`, `RollingIntervalGiftEligibilityPolicy.cs`
- `src/ParkGate.Domain/EntryCards/QrPurpose.cs` — `public enum QrPurpose : byte { EntryCard = 0, GiftVoucher = 1 }`

**Files (modify):** `Organization.cs`, `AgeCategory.cs`, `EntryCards/QrTokenFormat.cs`.

**Do:**
1. **Organization** — add read-only properties (private set): `int? MaxFreeCompanions`, `bool GiftEnabled`, `decimal GiftValue`, `int GiftIntervalDays` (initialize `GiftIntervalDays = DefaultGiftIntervalDays`, constant 30). Add constants `MinGiftIntervalDays = 1`, `MaxGiftIntervalDays = 365`, `MaxFreeCompanionsLimit = 50`, `DefaultGiftIntervalDays = 30`. Add methods:
   - `SetMaxFreeCompanions(int? value)` — validates `null` or `0..50` else `ArgumentOutOfRangeException`.
   - `ConfigureGift(bool enabled, decimal value, int intervalDays)` — validates interval range; if `enabled` then `value > 0`; if not enabled, value must be `>= 0`; else `ArgumentOutOfRangeException`. Round value to 3 decimals.
   **Do not change the constructor or `Update(...)`.**
2. **AgeCategory** — add `bool IsCompanionLimitExempt { get; private set; }` and `SetCompanionLimitExempt(bool value)`. Constructor/`Update` unchanged.
3. **GiftVoucher** (sealed, private parameterless ctor for EF, private setters). Static factory
   `Issue(Guid token, byte keyId, Guid beneficiaryId, int organizationId, decimal value, DateTime issuedAtUtc, DateOnly businessDate, int issuedByUserId, int issuedWorkstationId, int? overrideByUserId = null, string? overrideReason = null)`:
   - guards: token not empty; value > 0; if override user is set then reason required (trim, ≥ 5 chars, ≤ 250) and vice-versa; `ExpiresOn = businessDate`; `Status = Issued`; `IsOverride = overrideByUserId.HasValue`.
   Behavior methods: `Redeem(int userId, int workstationId, DateTime nowUtc, DateOnly businessDate)` (only from `Issued`, else `InvalidOperationException`), `Void(int userId, string reason, DateTime nowUtc)` (only from `Issued`, reason ≥ 5 chars). Add a `bool IsExpired(DateOnly today) => today > ExpiresOn`.
4. **Policy** (pure, no clock, no I/O):
   - `GiftEligibilityContext(Organization Organization, Beneficiary Holder, DateTime? LastGiftIssuedUtc, DateTime NowUtc)`.
   - `GiftEligibilityDecision(bool IsEligible, GiftDenialReason? Reason, DateTime? NextEligibleAtUtc, bool CanBeOverridden)` with static `Eligible(...)` / `Denied(...)` helpers.
   - `RollingIntervalGiftEligibilityPolicy.Evaluate`: order exactly as BR-20. Interval: `next = last + TimeSpan.FromDays(org.GiftIntervalDays)`; `now >= next` ⇒ eligible. `CanBeOverridden = reason == IntervalNotElapsed`. Read `RollingWindowEligibilityPolicy.cs` first and mirror its style.
5. **QrTokenFormat** — keep the existing context bytes `"ParkGate.EntryCard/v1"` **unchanged** (existing printed cards must still verify). Replace the single `Context` field with `private static byte[] ContextFor(QrPurpose purpose)` returning `"ParkGate.EntryCard/v1"` or `"ParkGate.GiftVoucher/v1"`. Change `MessageToSign(ReadOnlySpan<byte> body)` to `MessageToSign(ReadOnlySpan<byte> body, QrPurpose purpose = QrPurpose.EntryCard)`. Nothing else in the format changes.

**Tests (ParkGate.Domain.Tests):**
- `RollingIntervalGiftEligibilityPolicyTests` table-driven: eligible with no previous; exact boundary eligible; 1 second before denied (`IntervalNotElapsed`, `CanBeOverridden = true`, correct `NextEligibleAtUtc`); 1 second after eligible; org inactive; gifts not enabled; blocked; suspended; **check order** (e.g. blocked + interval active ⇒ `HolderBlocked`, `CanBeOverridden = false`); non-interval reasons never overridable. 100% branch coverage of the policy.
- `OrganizationTests`: `ConfigureGift` valid/invalid (interval 0/366, enabled with value 0, negative), `SetMaxFreeCompanions` (null, 0, 50, 51, -1).
- `AgeCategoryTests`: flag default false; setter works.
- `GiftVoucherTests`: issue ok; override requires reason; redeem from Issued ok; redeem twice throws; void from Issued ok; void after redeem throws; redeem after void throws; `IsExpired` boundary (same day not expired, next day expired).
- `QrTokenFormatTests` (add to existing file or new): message for `EntryCard` equals the old bytes (regression: compute expected from the literal context string); message for `GiftVoucher` differs.

**Verify:** build + unit tests green.
**Done when:** all above pass; no existing test changed except additions.

---

### T-G2 — Application plumbing: permissions, errors, QR purpose

**Files (modify):** `Security/Permission.cs`, `Security/RolePermissions.cs`, `Common/Errors.cs`, `EntryCards/QrTokenSigner.cs`, `EntryCards/QrTokenVerifier.cs`; tests `Security/AuthorizationTests.cs` and `Results/ErrorCatalogTests.cs` (read first; extend, don't weaken).

**Do:**
1. **Permissions** — append (do not reorder existing): `IssueGift`, `RedeemGift`, `OverrideGiftInterval`, `VoidGift`.
   - Operator: `IssueGift`, `RedeemGift`.
   - Supervisor: Operator's + `OverrideGiftInterval`, `VoidGift`.
   - Admin: all four.
   Viewing the gift report reuses existing `ViewReports`; org policy editing reuses `ManageOrganizations`.
2. **Errors** — add to `Errors.CheckIn`: `ExceedsFreeCompanionLimit = new("CheckIn.ExceedsFreeCompanionLimit", "…")`. Add `public static class Gift` with (code → English message): `OrganizationInactive`, `NotEnabled`, `HolderBlocked`, `HolderSuspended`, `IntervalNotElapsed`, `OverrideReasonRequired`, `OverrideNotAllowed`, `NotFound`, `AlreadyRedeemed`, `Voided`, `Expired`, `InvalidCode` (unreadable/unknown key), `Forged` (bad signature or token/row mismatch), `CannotVoidRedeemed`, `VoidReasonRequired`, `InvalidPolicy`. Codes use the `Gift.<Name>` form, e.g. `"Gift.AlreadyRedeemed"`.
3. **Signer/Verifier** — add optional trailing parameter `QrPurpose purpose = QrPurpose.EntryCard`: `Encode(Guid cardId, DateOnly expiresOn, QrPurpose purpose = QrPurpose.EntryCard)` and `Verify(string? scannedText, QrPurpose purpose = QrPurpose.EntryCard)`; pass `purpose` to `QrTokenFormat.MessageToSign`. All existing callers keep working unchanged.

**Tests:**
- Signer/Verifier: a token encoded with `GiftVoucher` verifies with `GiftVoucher` and is `InvalidSignature` with `EntryCard`, and vice versa (add to `EntryCardsTests.cs` as new `[Fact]`s; use the same `ECDsa` setup as the existing tests).
- Permission matrix: Operator has IssueGift/RedeemGift but not OverrideGiftInterval/VoidGift; Supervisor has all four; Admin all four. Update any existing test that enumerates *all* permissions per role.
- Error catalog: if `ErrorCatalogTests` enumerates `Errors.*` classes by reflection and checks code uniqueness/format, make sure the new ones pass.

**Verify:** build + unit tests. **Done when** green.

---

### T-G3 — Free-companion limit (BR-25/26) end to end in Application

**Files (modify):** `Organizations/DTOs/OrganizationDtos.cs`, `Organizations/Services/IOrganizationService.cs`, `OrganizationService.cs`, `AgeCategories/DTOs/*.cs`, `AgeCategories/Services/AgeCategoryService.cs` (+ interface if needed), `CheckIn/Services/CheckInService.cs`; create `Organizations/Validators/SetOrganizationPolicyRequestValidator.cs`.

**Do:**
1. **DTOs (additive, trailing defaults — existing constructions keep compiling):**
   - `OrganizationDto(... bool IsActive, int? MaxFreeCompanions = null, bool GiftEnabled = false, decimal GiftValue = 0m, int GiftIntervalDays = 30)`; update `ToDto()` to fill them.
   - `AgeCategoryDto(... bool IsActive, bool IsCompanionLimitExempt = false)`; update `ToDto()`.
   - `CreateAgeCategoryRequest(..., int SortOrder, bool IsCompanionLimitExempt = false)` and `UpdateAgeCategoryRequest(..., int SortOrder, bool IsCompanionLimitExempt = false)`; the service calls `SetCompanionLimitExempt` after create/update (and writes it into the existing audit details if the service already audits changes — read it first).
   - **Do NOT change** `CreateOrganizationRequest` / `UpdateOrganizationRequest` (avoids accidental overwrite of policy by old callers).
2. **Org policy use case:** new record `SetOrganizationPolicyRequest(int OrganizationId, int? MaxFreeCompanions, bool GiftEnabled, decimal GiftValue, int GiftIntervalDays)` in `OrganizationDtos.cs`. Add `Task<Result<OrganizationDto>> SetPolicyAsync(SetOrganizationPolicyRequest request, CancellationToken ct = default)` to `IOrganizationService` and implement in `OrganizationService`, mirroring `UpdateAsync` structure: `ManageOrganizations` check → validator → UoW → load org → `SetMaxFreeCompanions` + `ConfigureGift` → save → audit action `Organization.PolicyChanged` with old/new values (no secrets) → return DTO. Validator error codes: `MaxFreeCompanionsOutOfRange`, `GiftIntervalOutOfRange`, `GiftValueRequired`, `GiftValueNegative`. Add every new code to `Strings.resx` later in T-G7 as `Val_<Code>`.
3. **CheckInService** — in both `RecordEntryAsync` and `OverrideEntryAsync`, **after** the existing `ExceedsMaxGroupSize` check and **before** the eligibility evaluation, call a new private method `ExceedsFreeCompanionLimitAsync(IUnitOfWork uow, Organization org, IReadOnlyList<CompanionRequestDto> companions, CancellationToken ct)`:
   - if `org.MaxFreeCompanions` is null → `false`;
   - load categories via `uow.AgeCategories.GetAllAsync(ct)` (includes inactive); build set of exempt ids;
   - `counted = companions.Where(c => !exempt.Contains(c.AgeCategoryId)).Sum(c => c.Count)` (unknown ids count as non-exempt);
   - return `counted > org.MaxFreeCompanions.Value`.
   If true → `return Errors.CheckIn.ExceedsFreeCompanionLimit;` (no denied attempt row is written, same as the group-size check; the transaction is simply disposed).
4. Do not touch `RollingWindowEligibilityPolicy`.

**Tests (ParkGate.Application.Tests):** extend `CheckInServiceTests` using the existing fixture style (read it first):
- limit null ⇒ any companions allowed (still bound by group size);
- limit 3, three adults ⇒ allowed; four ⇒ rejected with `CheckIn.ExceedsFreeCompanionLimit`;
- limit 3, three adults + five infants (exempt) ⇒ allowed unless total exceeds `MaxGroupSize`;
- limit 0, one child ⇒ rejected; limit 0, only infants ⇒ allowed;
- holder is not counted (limit 3 with exactly 3 companions passes);
- same rule applies in `OverrideEntryAsync` (supervisor cannot bypass);
- rejected request writes **no** `EntryAttempt`.
- `OrganizationService.SetPolicyAsync`: success, forbidden (Operator), validation failure per code, not found, audit written.
- `AgeCategoryService`: create/update persists the flag.

**Verify:** build + unit tests. **Done when** green.

---

### T-G4 — Gift voucher use cases (Application)

**Files (create):**
- `Ports/Persistence/IGiftVoucherRepository.cs`
- `Ports/Reports/IGiftReportQueries.cs`
- `Ports/EntryCards/IQrImageRenderer.cs` — `byte[] RenderPng(string qrText, int pixelsPerModule = 10);`
- `GiftVouchers/DTOs/GiftVoucherDtos.cs`
- `GiftVouchers/Services/IGiftVoucherService.cs`, `GiftVoucherService.cs`
- `GiftVouchers/Validators/IssueGiftOverrideRequestValidator.cs`, `VoidGiftVoucherRequestValidator.cs`

**Files (modify):** `Ports/Persistence/IUnitOfWork.cs` (+ `IGiftVoucherRepository GiftVouchers { get; }`), `DependencyInjection.cs` (register `IGiftEligibilityPolicy` singleton and `IGiftVoucherService` transient — validators are auto-registered by assembly scan; read the file), tests `Common/TestDoubles.cs` (add `FakeGiftVoucherRepository` + wire into `FakeUnitOfWork`; read how the other fakes are built).

**Repository port:**
```csharp
public interface IGiftVoucherRepository
{
    Task<GiftVoucher?> GetByIdAsync(int id, CancellationToken ct = default);
    /// UPDLOCK, ROWLOCK — used by redemption.
    Task<GiftVoucher?> GetByTokenForUpdateAsync(Guid token, CancellationToken ct = default);
    /// Latest IssuedAtUtc among non-voided vouchers of the holder; null if none.
    Task<DateTime?> GetLatestIssuedUtcAsync(Guid beneficiaryId, CancellationToken ct = default);
    Task<IReadOnlyList<GiftVoucher>> GetRecentByBeneficiaryAsync(Guid beneficiaryId, int maxResults = 20, CancellationToken ct = default);
    void Add(GiftVoucher voucher);
}
```

**DTOs (sealed records):**
- `GiftEligibilityDto(bool IsEligible, GiftDenialReason? Reason, string? ReasonCode, DateTime? NextEligibleAtUtc, bool CanBeOverridden, decimal GiftValue, DateTime? LastGiftIssuedUtc)`
- `IssueGiftVoucherRequest(Guid BeneficiaryId)`
- `IssueGiftOverrideRequest(Guid BeneficiaryId, string Reason, int? SupervisorUserId = null)`
- `IssuedGiftVoucherDto(int ReceiptNo, Guid BeneficiaryId, string HolderName, string CardNumber, string OrganizationName, decimal Value, DateTime IssuedAtUtc, DateOnly ExpiresOn, bool IsOverride, string QrText)`
- `GiftRedemptionDto(int ReceiptNo, decimal Value, string HolderName, string CardNumber, string OrganizationName, DateTime IssuedAtUtc, DateTime RedeemedAtUtc)`
- `VoidGiftVoucherRequest(int VoucherId, string Reason)`
- Report DTOs (used by T-G6): `GiftReceiptRowDto`, `GiftReceiptsReportDto` — define in T-G6 to keep this task focused.

**Service `IGiftVoucherService` (each method = one use case; constructor takes `IUnitOfWorkFactory`, `IGiftEligibilityPolicy`, `QrTokenSigner`, `QrTokenVerifier`, `IAuditWriter`, `ICurrentUser`, `ICurrentWorkstation`, validators — model the constructor and style on `CheckInService`):**

1. `GetEligibilityAsync(Guid beneficiaryId, ct)` → `Result<GiftEligibilityDto>`. Needs `IssueGift`. **Read-only, no transaction/lock.** Loads holder, org, `GetLatestIssuedUtcAsync`, DB now; evaluates the policy; maps decision to DTO (`ReasonCode` = the matching `Errors.Gift.*` code, e.g. `Gift.IntervalNotElapsed`). Not found → `Errors.Beneficiary.NotFound`.
2. `IssueAsync(IssueGiftVoucherRequest, ct)` → `Result<IssuedGiftVoucherDto>`. Needs `IssueGift`. Flow (copy the structure of `RecordEntryAsync`): `BeginTransactionAsync` → `GetForUpdateAsync` → org → DB now → settings timezone (reuse the same `Park.TimeZoneId` lookup pattern as `CheckInService.GetSettingsAsync`; **do not** refactor that method — add a small private helper in the gift service) → `GetLatestIssuedUtcAsync` → policy. If not eligible → commit nothing, return the mapped `Errors.Gift.*` error (no row written). Else: `token = QrTokenSigner.NewCardId()`, `expiresOn = businessDate`, `qr = _signer.Encode(token, expiresOn, QrPurpose.GiftVoucher)`, `GiftVoucher.Issue(...)` with `org.GiftValue`, add, save, commit, audit `Gift.Issued` (details: receipt no., value; **never** the QR text), return DTO. The QR text is returned to the caller only.
3. `IssueOverrideAsync(IssueGiftOverrideRequest, ct)`. Approver resolution **exactly like `CheckInService.OverrideEntryAsync`**: if `_currentUser.Has(Permission.OverrideGiftInterval)` use current user; else if `SupervisorUserId` provided, load that user and require active Supervisor/Admin; else `Errors.Auth.SupervisorRequired`. Validate reason (≥ 5 chars → `Gift.OverrideReasonRequired`). Same locked flow; if decision denied and `!CanBeOverridden` → `Errors.Gift.OverrideNotAllowed`; if the decision is actually eligible, still issue normally **but** do not record override (or simply return `Errors.Gift.OverrideNotAllowed`? **Decision: if already eligible, issue as a normal voucher without override flags**). Otherwise issue with override fields. Audit `Gift.IssuedOverride` with reason.
4. `RedeemAsync(string? scannedText, ct)` → `Result<GiftRedemptionDto>`. Needs `RedeemGift`. Steps:
   1. `_verifier.Verify(text, QrPurpose.GiftVoucher)`: `Unreadable` → `Gift.InvalidCode`; `UnknownKey` → `Gift.InvalidCode`; `InvalidSignature` → `Gift.Forged`.
   2. `BeginTransactionAsync` → `GetByTokenForUpdateAsync(token.CardId)`; null → `Gift.NotFound`.
   3. If `voucher.ExpiresOn != token.ExpiresOn` → `Gift.Forged`.
   4. DB now → business date (timezone helper). `Voided` → `Gift.Voided`; `Redeemed` → `Gift.AlreadyRedeemed`; `IsExpired(businessDate)` → `Gift.Expired`.
   5. `voucher.Redeem(userId, workstationId, nowUtc, businessDate)`, save, commit, audit `Gift.Redeemed`. Load holder + org names for the DTO.
   6. Failed scans (not found / forged / expired / already redeemed) are **not** persisted in this phase; they are logged at `Warning` level via `ILogger` **without the QR text** (inject `ILogger<GiftVoucherService>`; optional param pattern as in other services — read one first).
5. `VoidAsync(VoidGiftVoucherRequest, int? supervisorUserId = null, ct)` → `Result`. Same approver pattern as `CheckInService.VoidEntryAsync` but permission `VoidGift`. Reason ≥ 5 chars (`Gift.VoidReasonRequired`). Loads voucher by id under transaction (use `GetByIdAsync`, then it is tracked; for safety lock the holder row via `uow.Beneficiaries.GetForUpdateAsync(voucher.BeneficiaryId)` first). `Redeemed` → `Gift.CannotVoidRedeemed`; already voided → `Gift.Voided`. Audit `Gift.Voided`.

**Tests (ParkGate.Application.Tests/GiftVouchers/GiftVoucherServiceTests.cs)** — use `FakeUnitOfWorkFactory`, real `RollingIntervalGiftEligibilityPolicy`, real `QrTokenSigner`/`QrTokenVerifier` (as `EntryCardsTests` does), fixed DB clock from the fake UoW (read how `CheckInServiceTests` sets time):
- Eligibility: each denial reason, eligible, forbidden for a user without `IssueGift`.
- Issue: success writes voucher with org value + snapshot, QR verifies with `GiftVoucher` purpose and fails with `EntryCard` purpose; second issue immediately → `Gift.IntervalNotElapsed`; after interval → success; voided voucher does not count; disabled org → `Gift.NotEnabled`; blocked/suspended holder; operator forbidden when lacking permission.
- Override: Operator without supervisor id → `Auth.SupervisorRequired`; Operator with valid supervisor id + reason → success with `IsOverride`; reason too short → validation error; override of `HolderBlocked` → `Gift.OverrideNotAllowed`; Supervisor user directly → success.
- Redeem: success; same QR twice → second `Gift.AlreadyRedeemed`; wrong purpose QR (entry-card QR) → `Gift.Forged`; garbage → `Gift.InvalidCode`; unknown token (valid signature, no row) → `Gift.NotFound`; expired (advance DB clock by a day) → `Gift.Expired`; voided → `Gift.Voided`; forbidden without `RedeemGift`.
- Void: success; redeemed → `Gift.CannotVoidRedeemed`; after void, holder can be issued again immediately.
- Validators: reason length boundaries (4 fails, 5 passes).

**Verify:** build + unit tests (incl. architecture tests — Application must still not reference EF/WinForms). **Done when** green.

---

### T-G5 — Infrastructure persistence + migration (released tables → ask Osama before generating)

**Files (create):** `Data/Configurations/GiftVoucherConfiguration.cs`, `Repositories/GiftVoucherRepository.cs`, migration (generated).
**Files (modify):** `Data/ParkGateDbContext.cs` (`public DbSet<GiftVoucher> GiftVouchers => Set<GiftVoucher>();`), `Data/UnitOfWork.cs` (+ property/instance), `Data/Configurations/OrganizationConfiguration.cs`, `AgeCategoryConfiguration.cs`, `DependencyInjection.cs` only if a registration is needed.

**Do:**
1. Read `EntryAttemptConfiguration.cs`, `OrganizationConfiguration.cs`, `AgeCategoryConfiguration.cs`, `ParkGateDbContext.cs` (how configurations are applied — assembly scan or explicit) and `Repositories/BeneficiaryRepository.cs` first.
2. `OrganizationConfiguration`: map the 4 new properties exactly per §5.2 (column types, defaults `HasDefaultValue(false)`, `HasDefaultValue(0m)`, `HasDefaultValue(30)`, CHECK constraints with the names in §5.2). `AgeCategoryConfiguration`: `IsCompanionLimitExempt` required, default false.
3. `GiftVoucherConfiguration`: map **every column, type, length, index, FK (`DeleteBehavior.Restrict`) and CHECK exactly as §5.4** (decimal `HasPrecision(10, 3)`, datetime `HasPrecision(3)`, `Status` stored as `tinyint` via `HasConversion<byte>()`, `RowVersion` `IsRowVersion()`, `Id` `UseIdentityColumn()` — int key identity by convention). `Token` unique index named `UX_GiftVouchers_Token`. Filtered index uses `HasFilter("[RedeemedBusinessDate] IS NOT NULL")`. Descending index: `HasIndex(x => new { x.BeneficiaryId, x.IssuedAtUtc }).IsDescending(false, true)`.
4. `GiftVoucherRepository` (constructor `ParkGateDbContext`): `GetByTokenForUpdateAsync` uses `_context.GiftVouchers.FromSqlInterpolated($"SELECT * FROM dbo.GiftVouchers WITH (UPDLOCK, ROWLOCK) WHERE Token = {token}")` (parameterized, as `BeneficiaryRepository.GetForUpdateAsync` does). `GetLatestIssuedUtcAsync`: `Where(v => v.BeneficiaryId == id && v.Status != GiftVoucherStatus.Voided).MaxAsync(v => (DateTime?)v.IssuedAtUtc)`.
5. **Migration:** run from repo root (check how the previous migration was created: same startup project/context as `20261013152901_AddSyncOutboxTable`; read `scripts/build-migration-bundle.ps1` for the exact `dotnet ef` arguments):
   `dotnet ef migrations add AddGiftVouchersAndCompanionLimit --project src/ParkGate.Infrastructure --startup-project <the one used by the bundle script> --context ParkGateDbContext --output-dir Data/Migrations`
   Then **open the generated `.cs`** and verify: 4 `AddColumn` on Organizations, 1 on AgeCategories, `CreateTable GiftVouchers`, the CHECK constraints, indexes. **Manually append** to `Up(...)`:
   `migrationBuilder.Sql("UPDATE dbo.AgeCategories SET IsCompanionLimitExempt = 1 WHERE NameAr = N'رضيع';");`
   (Arabic literal inside a migration SQL string is acceptable; it is data, not UI text.) `Down` must reverse everything. Never edit this migration after it is pushed.
6. Do **not** add `GiftVoucher` to `OutboxEntityExtractor.SyncableTypes`. But `Organization` and `AgeCategory` *are* syncable and the payload serializes all properties, so the outbox JSON now includes the new fields. Read `web/app/Services/SyncIngestionService.php` and confirm it ignores unknown keys (explicit field mapping). If it does **not**, stop and tell Osama — do not edit `web/` in this phase.

**Tests (ParkGate.Infrastructure.Tests/Integration, `[Trait("Category","Integration")]`, follow `LocalDbTestFixture` + `UniqueIndexesTests` style):**
- `GiftVoucherMappingTests`: round-trip all columns incl. decimal precision and Arabic override reason; `Id` auto-generated and increasing.
- Unique `Token` violation throws; CHECK `CK_GiftVouchers_Status` rejects a Redeemed row without redeemer (raw SQL insert/update attempt) ; CHECK Override.
- `Organization` new columns round-trip + CHECK rejects `GiftIntervalDays = 0` and `GiftEnabled = 1 with GiftValue = 0`.
- Migration data fix: after migration on a fresh DB, the seeded «رضيع» row has `IsCompanionLimitExempt = 1` and others 0.
- **`ParallelGiftRedeemTests`:** 10 parallel `RedeemAsync` calls (real `GiftVoucherService` or repository + raw transaction pattern as `ParallelCheckInConcurrencyTests` does — read it) on the same token ⇒ exactly **one** success, nine `Gift.AlreadyRedeemed`. Repeat 20 times as the check-in test does.
- **`ParallelGiftIssueTests`:** parallel `IssueAsync` for the same holder ⇒ exactly one voucher is created, the rest `Gift.IntervalNotElapsed`.

**Verify:** build; unit tests; integration tests green (`Category=Integration`). Also run the migration bundle script only if Osama asks.
**Done when:** all pass and the migration file was reviewed line by line.

---

### T-G6 — Infrastructure: report queries, QR image adapter, DI

**Files (create):** `Application/GiftVouchers/DTOs/GiftReportDtos.cs`, `Application/Ports/Reports/IGiftReportQueries.cs` (if not already created in T-G4 — create here), `Application/GiftVouchers/Services/IGiftReportService.cs` + `GiftReportService.cs`, `Infrastructure/Reports/GiftReportQueries.cs`, `Infrastructure/EntryCards/QrImageRendererAdapter.cs`.
**Files (modify):** `Infrastructure/DependencyInjection.cs`, `Application/DependencyInjection.cs`.

**Do:**
1. Read `Infrastructure/Reports/ReportQueries.cs` and `Application/Reports/Services/ReportService.cs`; mirror them (DbContext factory, `AsNoTracking`, projection to DTOs, `ViewReports` permission, `Errors.Reports.InvalidDateRange` for bad ranges).
2. Query contract: `GetReceiptsAsync(DateOnly from, DateOnly to, int? organizationId, int? workstationId, GiftReceiptFilter filter, ct)` where `GiftReceiptFilter` = `Redeemed | Issued | Voided | All`. Rows: receipt no., holder name, card number, organization, value, issued at / by, redeemed at / by, redeemed workstation display name, status, override flag + reason. Date filter: for `Redeemed` use `RedeemedBusinessDate`; otherwise `BusinessDate`. Totals: count and sum of `Value` per status and per organization. Order by `Id` descending.
3. `QrImageRendererAdapter : IQrImageRenderer` simply calls the existing static `QrImageRenderer.RenderPng`. Register as singleton. Register `IGiftReportQueries` (transient) in `AddInfrastructure`; register `IGiftReportService` (transient) in `AddApplication`.

**Tests:** Application: `GiftReportServiceTests` (forbidden without `ViewReports`, invalid date range, passes filter through, uses fake `IGiftReportQueries`). Infrastructure integration: seed several vouchers (issued/redeemed/voided, two orgs, two dates) and assert counts, totals and date filtering for each filter; verify a voucher redeemed after midnight local time is reported under the redemption business date.

**Verify:** build + unit + integration. **Done when** green.

---

### T-G7 — WinForms: organization policy and age-category flag screens

**Files (modify):** `Forms/EditOrganizationDialog.cs` + `.Designer.cs`, `Forms/EditAgeCategoryDialog.cs` + `.Designer.cs`, `Forms/OrganizationsForm.cs` + designer (add grid columns), `Forms/AgeCategoriesForm.cs` + designer (add a column), `Resources/Strings.resx`, `Resources/Strings.Designer.cs`, `Common/ErrorMessages.cs`; tests in `ParkGate.ArchitectureTests/ManagementFormsTests.cs` (read first; extend its fakes).

**Do:**
1. Read §8 (UI rules) and the existing dialogs. Designer layout code stays in `*.Designer.cs` only (no lambdas/logic there). Use `TableLayoutPanel`, RTL, tab order set explicitly.
2. **EditOrganizationDialog:** add a group «سياسة الهدايا والمرافقين» containing: `NumericUpDown`/checkbox «حد المرافقين المجانيين» (checkbox "تحديد حد" enables a 0–50 numeric), checkbox «تفعيل الهدايا», numeric «قيمة الهدية» (3 decimals, 0.001..9999999), numeric «مدة الانتظار بين هديتين (أيام)» (1–365). Behavior:
   - In **edit** mode: fill from `OrganizationDto`; on Save call `UpdateAsync` (existing) **then** `SetPolicyAsync` with the new values (both inside the existing busy/try-catch pattern; if the second fails show the mapped error and keep the dialog open).
   - In **create** mode: call `CreateAsync`, then `SetPolicyAsync` using the returned `Id`.
   - Gift fields are disabled while «تفعيل الهدايا» is unchecked.
3. **EditAgeCategoryDialog:** add checkbox «لا يُحتسب ضمن حد المرافقين (مجاني دائماً)». Pass `IsCompanionLimitExempt` in Create/Update requests.
4. Show the new values in the Organizations grid (max companions, gift on/off, gift value, interval) and the exempt flag in the AgeCategories grid (`AutoGenerateColumns = false`, Arabic headers).
5. **Resources:** add every new label/message to `Strings.resx` and a matching property in `Strings.Designer.cs` — see §8.3. Add `Val_` keys for the T-G3 validator codes and `Error_Gift_*`, `Error_CheckIn_ExceedsFreeCompanionLimit` keys (error key = `"Error_" + code.Replace('.', '_')`, resolved by `ErrorMessages.GetMessage` — read it; add explicit fallbacks only if the file's pattern requires).

**Tests (Architecture test project, same style as existing form tests):** dialogs construct with parameterless ctor and are RTL; saving a new org calls `CreateAsync` then `SetPolicyAsync` with the returned id; edit mode calls `UpdateAsync` then `SetPolicyAsync`; gift controls disabled when gifts off; exempt checkbox flows into the request. `ErrorMessagesTests`: every new error code has a non-empty Arabic message.

**Verify:** build + unit tests. **Done when** green. Manually open both dialogs in the VS designer if possible and report whether they load.

---

### T-G8 — WinForms: desk integration (companion limit + gift panel + QR dialog)

**Files (create):** `Controls/GiftOfferPanel.cs` + `.Designer.cs`, `Forms/GiftQrDialog.cs` + `.Designer.cs`.
**Files (modify):** `Controls/CheckInDeskControl.cs` (+ `.Designer.cs` only to host the panel), `DependencyInjection.cs` (register `GiftQrDialog` transient), `Strings.resx`/`Strings.Designer.cs`.

**Do:**
1. **Companion limit on the desk** (small, in `CheckInDeskControl.cs`): keep `_orgService`; after a holder is found, read the holder's organization from the already-loaded org list (`OrganizationDto.MaxFreeCompanions`) and the age categories (`AgeCategoryDto.IsCompanionLimitExempt`) already loaded for counters. Compute `counted` exactly like BR-25 and: show a label «المرافقون المجانيون: x من y» next to the total label; when `counted > limit` **disable** Record Entry and Override, show the warning text (use the existing warning color `#B26A00` and keep banner text stating the result). Server-side validation remains authoritative; also map `CheckIn.ExceedsFreeCompanionLimit` from the service result via `ErrorMessages`.
2. **`GiftOfferPanel : UserControl`** (own designer; parameterless ctor for design time + ctor with `IGiftVoucherService?`, `ICurrentUser?`, `IFormFactory?`, `INotifyService?`, `ILogger<GiftOfferPanel>?` — follow `CheckInDeskControl` style: nullable deps, `InitializeComponent()`, `WireEvents()`). Content: title, status line (eligible / not eligible + localized reason + next eligible local date-time via the existing `DateTimeDisplayHelper`), gift value, button «إصدار QR الهدية (F6)», button «إصدار بمدة مبكرة — مشرف (F7)».
   - Public API: `Task LoadAsync(Guid beneficiaryId)` (calls `GetEligibilityAsync`; **any failure or Forbidden hides the panel and must never block entry recording**; log with `ILogger`), `void Clear()`.
   - F6 → `IssueAsync` → open `GiftQrDialog` with the result. F7 → show `VoidReasonDialog`-like reason prompt? **Do not create a new dialog**: reuse `SupervisorAuthDialog` for approval when the current user lacks `OverrideGiftInterval` (exactly how `CheckInDeskControl.OverrideEntryAsync` does it — read lines ~922–960) and reuse the same reason-entry mechanism that override uses; then `IssueOverrideAsync`.
   - Buttons disabled while busy (`BusyScope`), re-enabled in `finally`. After a successful issue, call `LoadAsync` again so the panel immediately shows "not eligible until …".
3. **`GiftQrDialog : RtlFormBase`** (do **not** copy `IssueQrDialog` internals): shows holder name, org, value, receipt no., expiry date, the QR image rendered through `IQrImageRenderer` (clone PNG bytes into an independent `Bitmap`, as the S15 fix did), buttons Print / Save PNG / Close. Printing: A6/receipt friendly layout via `PrintDocument` with RTL text: title, receipt no., holder, value, QR, "valid until dd/MM/yyyy". All text from resx. Never log or show the raw digits as text except a small `Tooltip`-free caption is **not** needed.
4. **Desk hook:** in `CheckInDeskControl`, after a successful `LookupCardAsync` (holder shown) call `_giftPanel.LoadAsync(holder.Id)` fire-and-forget inside try/catch; call `_giftPanel.Clear()` in `ResetDesk()`/`ClearPreviousHolderData()`. Key handling in `ProcessCmdKey`: `Keys.F6` and `Keys.F7` forward to the panel's public `IssueAsync()`/`IssueOverrideAsync()` when enabled (only if Osama approved A8; otherwise skip the shortcuts and keep buttons only).
5. Constructor wiring: add the panel as a child created through the control tree (designer) and pass dependencies using a method `AttachGiftServices(...)` **or** resolve the panel via `IFormFactory.CreateControl<GiftOfferPanel>()` — read how `MainForm` creates controls and pick the existing pattern. Do not add 5 more constructor parameters to `CheckInDeskControl` without updating its existing parameterless/test call sites.

**Tests (Architecture test project, style of `CheckInDeskControlTests`):** limit label and Record-button disabling when counted > limit; infants do not count; panel hidden when service returns Forbidden or throws; panel shows eligible/not eligible texts; F6 triggers issue when enabled; issue success opens the QR dialog (use `IFormFactory` fake); override path asks for supervisor when user lacks permission and not when user is Supervisor; `GiftQrDialog` parameterless ctor works and is RTL.

**Verify:** build + unit tests; **run the app locally** against LocalDB (see `docs/TEST_SCRIPT.md`) and click through: eligible → issue → panel switches to not eligible. Report what you actually saw. **Done when** green and walked through.

---

### T-G9 — WinForms: Gift redemption screen (rides window)

**Files (create):** `Controls/GiftRedeemControl.cs` + `.Designer.cs`.
**Files (modify):** `Forms/MainForm.cs` + designer (menu item + show method, mirror `ShowBeneficiaries` / `ShowOrganizations` pattern), `DependencyInjection.cs` (register control transient), `Strings.resx`/`Designer`.

**Do:**
1. Layout (large touch-friendly, Cairo fonts per §5 rules): one text box «امسح كيوركود الهدية» (focus always here; Enter = redeem; Esc = clear; text normalized with `DigitNormalizer.NormalizeDigits` — read how the desk does it), a **`StatusBanner`** (≥ 20 pt) that shows result text + color (green granted / red denied; text always states the result), a details block (receipt number, holder, organization, value in large font, issued time), and a grid «آخر الهدايا المصروفة اليوم» (last 20 redeemed by this workstation: time, receipt no., holder, value) + running total for the day at this window.
2. Behavior: on Enter call `IGiftVoucherService.RedeemAsync(text)`. Success → green banner «هدية صالحة — قيمتها X د.ل — سلّم الكوبون»; show details; refresh grid; clear the text box and return focus after `AutoResetDelayMs` like the desk. Failure → red banner with the `ErrorMessages` text for the `Gift.*` error. Disable the text box while the call runs; ignore empty input; protect against double Enter (busy flag).
3. The "today at this window" list/total uses `IGiftReportService.GetReceiptsAsync` filtered `Redeemed`, today's business date, current workstation id (needs `ViewReports`; if the user lacks it hide the list, but still allow redeeming — redeeming needs only `RedeemGift`). Business date comes from the report service/DB — do not use the PC clock for rules; the PC clock may be used only for UI display.
4. Menu: top-level or under an existing menu, item «شباك الألعاب – صرف الهدايا», visible iff `ICurrentUser.Has(Permission.RedeemGift)`; Operators log in at the rides window and use it as their main screen (no need to auto-open).

**Tests (Architecture project):** control constructs parameterless; Arabic-Indic digits normalized; success shows details and green text; each error code shows red banner with mapped message; busy guard; menu visibility by permission (extend `MainFormTests`).

**Verify:** build + tests; manual run: scan (type) a QR issued in T-G8 → success → scan again → «سبق صرف هذه الهدية». **Done when** green and walked through.

---

### T-G10 — WinForms: gift receipts report + Excel export

**Files (create):** `Forms/GiftReceiptsForm.cs` + `.Designer.cs` (or `Controls/…` if `ReportsControl` hosting is simpler — read `ReportsControl.cs` and `MainForm.ShowReports` first and choose the lighter integration), `Application/Ports/Reports/IGiftExcelExportService.cs`, `Infrastructure/Reports/ClosedXmlGiftExcelExportService.cs`.
**Files (modify):** `MainForm` (menu «تقرير الواصلات», visible with `ViewReports`), `DependencyInjection` files, resx.

**Do:** date range (from/to), organization filter, status filter (مصروفة/صادرة/ملغاة/الكل), workstation filter optional, grid with columns from T-G6, totals bar (count + total value per status), buttons Refresh / Export Excel / Print preview (reuse the existing print-preview helper if it is generic; otherwise skip printing and say so). Excel: RTL sheet via ClosedXML, mirroring `ClosedXmlExcelExportService` styling; **new port `IGiftExcelExportService`** so existing fakes of `IExcelExportService` are not broken. Dates shown `dd/MM/yyyy`, times `hh:mm tt` park time, Western digits.

**Tests:** Application/Infrastructure test that the exported workbook opens (ClosedXML load) and contains header + N rows + totals; form test: refresh calls the service with the chosen filters; Forbidden shows the localized message.

**Verify:** build + tests; manual export opened in Excel. **Done when** green.

---

### T-G11 — Hardening, docs, handoff

1. Full run: `dotnet build -c Debug`, all unit tests, **integration tests**, architecture tests. Zero warnings in Domain/Application/Infrastructure.
2. Re-run both parallel tests 20× (as in T-G5) to catch flakiness.
3. Manual end-to-end script (add to `docs/TEST_SCRIPT.md`, Arabic like the rest of the file): admin configures an organization (limit 3, gift on, value, 30 days) → desk lookup → companions 3 adults + 2 infants allowed, 4 adults blocked → issue gift → panel shows not eligible → second issue blocked → supervisor early issue with reason → rides window redeems → second scan blocked → void flow → report shows receipts → Excel export.
4. Update `docs/USER_GUIDE_AR.md` (new sections: gift at the gate, redemption at the rides window, supervisor early issue, org settings, report).
5. Update `docs/SESSION_HANDOFF.md` (snapshot, next session, log entry), tick tasks in `docs/ROADMAP.md`, append decisions to `docs/DECISIONS.md`.
6. Commit `docs: update session handoff` and open the PR to `main` using `.github/pull_request_template.md`. **Osama reviews/merges.**

---

## 8. Cross-cutting UI rules (apply to T-G7..T-G10)

1. Every form derives from `RtlFormBase` (RTL, Cairo, DPI scaling, `KeyPreview`). User controls follow `CheckInDeskControl`/`BeneficiariesControl`.
2. Layout in `*.Designer.cs` `InitializeComponent()` only: no logic, lambdas or DI there; use `TableLayoutPanel`/`FlowLayoutPanel`, docking, no absolute positions; add `#nullable disable` at the top as other designers do. Every form/control must open in the VS designer, so keep a **parameterless constructor** passing `null` dependencies (pattern already used).
3. **Adding a string:** (a) add `<data name="Key" xml:space="preserve"><value>…</value></data>` to `Strings.resx` following neighbors; (b) add the matching property to `Strings.Designer.cs` using the file's real pattern, e.g. `internal static string Btn_RecordEntry => GetString(nameof(Btn_RecordEntry));` (error/validation keys that are looked up dynamically through `Strings.GetOptionalString(...)` need only the resx entry). Never use Arabic literals in `.cs`.
4. Colors: granted `#1B873F`, denied `#C62828`, warning `#B26A00`, neutral `#455A64`; banner text always states the result.
5. All I/O handlers: `async void` only at event level, try/catch → logger + friendly message, disable triggering controls while busy, never `.Result`/`.Wait()`.
6. Digits: Western 0-9 in display; normalize Arabic-Indic digits on every numeric/scan input.
7. Message boxes only through `INotifyService`.
8. Times: store/compare UTC; display park local time via the existing helper.

---

## 9. Items needing Osama's approval before coding starts

1. **SPEC.md** — add BR-19..BR-26 and FRs (T-G0).
2. **DATABASE.md + migration** — new columns on released tables `Organizations` and `AgeCategories`, new table `GiftVouchers` (AGENTS §7/§11).
3. **ADR-020** (T-G0).
4. **AGENTS.md §5.11** — shortcuts F6/F7 (A8). If refused, buttons only.
5. Assumptions A1–A8 in §3 (any veto changes the matching task).
6. **Current dirty working tree** (≈30 uncommitted files on `phase/f4-cloud-dashboard`): must be committed/resolved by Osama before T-G0 (AGENTS §1.1).

## 10. Out of scope (do not do; ask first)

- Cloud sync of vouchers / web dashboard for gifts (A7).
- Printing the physical rides coupons or integrating with a rides ticketing system.
- Different gift values by age/category, or multiple gift types per organization.
- Checking card expiry for gifts (A5).
- Persisting failed redemption scans in a log table.
- Reloading the QR verifier's trusted keys at runtime (see risk R1).
- Fixing existing issues: `IssueQrDialog` uses `QrImageRenderer` from Infrastructure and Arabic literals in `.cs`; `CardScanService`'s misleading "forged" message when a *gift* QR is scanned at the entry desk. Note them in `SESSION_HANDOFF.md` "Known issues".

## 11. Risks and review checklist (for Claude's review after implementation)

| # | Risk | What to check |
|---|---|---|
| R1 | `QrTokenVerifier` trusts keys loaded **at app start**. A workstation that registers its signing key *after* the rides-window app started will have its gift QRs rejected (`UnknownKey`) until the rides app restarts. | Document in USER_GUIDE and TEST_SCRIPT: start every workstation once before opening the park; consider a follow-up `ReloadVerifierAsync`. |
| R2 | Existing printed entry cards must still verify. | Regression test on `MessageToSign(EntryCard)` bytes; run the old `EntryCardsTests` untouched. |
| R3 | Positional-record changes silently breaking callers or resetting policy. | `CreateOrganizationRequest`/`UpdateOrganizationRequest` unchanged; only trailing defaults elsewhere. |
| R4 | Concurrency correctness. | Parallel issue and parallel redeem integration tests actually run (not skipped) and pass 20×. |
| R5 | Time source violations. | `DateTime.Now/UtcNow` absent from Domain/Application; business date from DB time + `Park.TimeZoneId`. |
| R6 | Layer violations. | Architecture tests green; WinForms uses `IQrImageRenderer`, not Infrastructure types. |
| R7 | Sensitive data in logs/audit. | No QR digits, tokens, or full national IDs in logs or `DetailsJson`. |
| R8 | Outbox payload now carries new Organization/AgeCategory fields. | Web ingestion tolerant; SyncAgent untouched; `SyncOutboxIntegrationTests` still green. |
| R9 | Migration quality. | Hand-review `Up/Down`, check names, CHECKs, the «رضيع» UPDATE; run on a fresh LocalDB and on a DB at the previous migration. |
| R10 | UI not verifiable by tests only. | Executor reports actual manual walkthrough results for T-G8/T-G9/T-G10; Claude re-runs the app. |
| R11 | Agent drift. | Diff touches only files listed per task; no unrelated reformatting; commits one per task with task IDs. |
