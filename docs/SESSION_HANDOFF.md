# Session Handoff

> **Read at the start of every session. Update at the end of every session** (AGENTS.md §1).
> Keep "Snapshot" and "Next session" accurate — they are overwritten each time. The log keeps the last 10 sessions only.

---

## Snapshot

| Field | Value |
|---|---|
| Last updated | 2026-11-13 |
| Last session | S27 — Phase R0 completed locally: baseline recorded (R0.2), web CI placed (R0.4), PR opened |
| Current phase | Phase R — Remediation (`docs/REMEDIATION_PLAN.md`). R0 done, awaiting PR review; next R1 |
| Working branch | `phase/r0-baseline` (PR to `main`, not merged) |
| Build | `dotnet build ParkGate.sln -c Debug` (S27): 0 errors, 16 warnings — all NU1902/NU1903 for `System.Security.Cryptography.Pkcs` 9.0.11 (R8.2) |
| .NET Unit tests | 588 passed (S27): 202 domain + 169 application + 63 infrastructure + 154 architecture |
| .NET Integration tests | 34 passed on LocalDB (S27) |
| Web tests | `php artisan test` (PHP 8.4.0 local, SQLite): 33 passed, 82 assertions (S27) — first web baseline |
| Total tests | 655 (622 .NET + 33 web), all green |
| Latest migration | `20261102125339_AddGiftVouchersAndCompanionLimit` (Applied on SQLEXPRESS and LocalDB) |

## State of the work

**Completed in this session (S27, local):**

1. R0.1 confirmed by a real `git fetch`: `main` == `origin/main` == `13ec3c6`.
2. R0.2 baseline recorded in the Snapshot (655 tests green, 16 known NuGet audit warnings).
3. R0.3 (done in S26) and the plan/handoff committed on `phase/r0-baseline`.
4. R0.4 `.github/workflows/web.yml` added. Its first CI run is on the PR.
5. PR `phase/r0-baseline` → `main` opened. Osama reviews and merges.

S26 (cloud, no shell) did R0.1 by static analysis and R0.3; see the S26 log entry.

**In progress**
- None. R0 is complete pending PR review and green CI on both workflows.
**Not started / Future**
- F2 Electronic gate (deferred by client; SPEC §9 F2).

## Next session — start here

**Run Phase R sessions on the dev machine (`claude` in `C:\dev\ParkGate`), not from the cloud.**
A cloud session reaches the folder through the desktop app's file tools only: it cannot run `git`, `dotnet`
or `php`, and the platform grants terminals in click-only mode, so it cannot drive one through computer use
either. `.github/` is write-protected for remote tools as well. S26 hit all three limits.

1. Check the R0 PR: both `build` and `web` workflows green. If `web` is red, fix it on `phase/r0-baseline`.
   If the web suite needs no built assets, the Node and "Build assets" steps can be dropped.
2. After Osama merges R0: R1 (web security) on `phase/r1-web-security`, starting with R1.1.
3. Line endings (see S27 log): renormalize `IssueQrDialog.cs` and the three `w/mixed` files in a
   dedicated `chore` commit, or do it in R9.

Carried over from S25 (do it when a printer is available; it does not block Phase R):
- Verify QR printing on the user machine with a connected printer or "Microsoft Print to PDF", and confirm
  with Osama that customer and gift QR printing works without closing the app.

## Blockers / waiting on the user

- R0 PR review and merge (Osama).
- Untracked `Claude outputs/web.yml` (duplicate) and `scripts/r0-baseline.ps1`: delete or keep?
- Open confirmations (the plan's documented defaults apply if unanswered): §2.2 — Q-R3 (who creates web
  dashboard users), Q-R4b (remaining glossary terms), Q-R6 (desk PC count, workgroup vs domain),
  Q-R7 (lowest desk resolution). None of them block R0; they block R1.1, R7.1, R6.5 and R5.8.
- Q-R2b (keep `Notes` / `StatusReason` local — recommended: yes) is needed before R2.6.
- Decisions already made by Osama are in the plan's §2.1 — do not re-ask them.

---

## Session Log (last 10 sessions)

### S27 — 2026-11-13 — Phase R0 completed (local session)
- `git fetch origin`: `main` == `origin/main` == `13ec3c6`, nothing unpushed. S26's static result confirmed.
- Created `phase/r0-baseline`; committed the remediation plan, roadmap Phase R and handoff [R0.1][R0.3].
- R0.2 baseline (all green): build 0 errors / 16 NU190x warnings; 588 unit + 34 integration; web 33.
- R0.4: moved `docs/_R0.4-web-workflow.yml` to `.github/workflows/web.yml` (the parked file was untracked,
  so a plain move + `git add`, not `git mv`).
- **Line-ending trap for manual verification.** `GiftQrDialog.cs`, `IssueQrDialog.cs`, `Strings.resx` and
  `Strings.Designer.cs` have the same content as their blobs, but SHA-1 over the raw bytes shows a false
  difference in three of them. Cause: `.gitattributes` sets `*.cs`/`*.resx` `text eol=crlf`, so the blob
  is normalized to LF while the working file is CRLF, and the index records the working-file size, not the
  blob size. Any future manual hash check must normalize CRLF→LF before hashing (or use
  `git hash-object`, which applies the filters).
- `git ls-files --eol` (S27) shows more than S26 assumed: `IssueQrDialog.cs` is `w/lf` (pure LF, 193 lines)
  despite `eol=crlf`; `GiftQrDialog.cs` (16 LF), `Strings.resx` (3 LF) and `Strings.Designer.cs` (1 LF)
  are `w/mixed`. Git reports them clean now, but the first save from Visual Studio will likely show a
  whole-file diff for `IssueQrDialog.cs`. Fix: `git rm --cached` + `git checkout` of these files (or
  `git add --renormalize .`) in a dedicated commit; added to R9 if not done earlier.
- Untracked, left out of the commit: `Claude outputs/web.yml` (byte-identical duplicate of the workflow)
  and `scripts/r0-baseline.ps1` (optional helper). Osama to decide: delete or keep.

### S26 — 2026-11-13 — Phase R0 (partial): git baseline, roadmap, web CI
- No shell available (cloud session). Derived `git status` from `.git/index` + SHA-1 of all 566 tracked
  files instead of running git.
- Found: `main` == `origin/main` (`13ec3c6`), nothing unpushed; only `docs/SESSION_HANDOFF.md` modified
  and only `docs/REMEDIATION_PLAN.md` untracked.
- Disproved REMEDIATION_PLAN §1's assumption of uncommitted S25 changes in `IssueQrDialog.cs`,
  `GiftQrDialog.cs`, `Strings.resx`, `Strings.Designer.cs` — touched, identical content.
- R0.3 done: Phase R (R0–R9) added to `docs/ROADMAP.md`; §2.2 questions asked.
- R0.4 written but parked at `docs/_R0.4-web-workflow.yml` (`.github/` is protected for remote tools).
- Added `scripts/r0-baseline.ps1` so one command produces the R0.2 numbers.
- Confirmed the cloud session cannot reach a shell: no `device_bash`, and the device grants `Terminal`
  in `click` tier only ("cannot type, press keys, or paste"). Phase R moves to local sessions.
- Nothing committed. No build or test run.

### Review — 2026-11-12 — Full code review and remediation plan (no code changes)
- Reviewed web, sync agent, WinForms UI, Application/Infrastructure and database design.
- Added `docs/REMEDIATION_PLAN.md` (Phase R). Osama answered Q-R1 to Q-R7 (plan §2.1).
- No source files changed. Nothing committed.

### S25 — 2026-11-05 — Bugfix: PrintDialog COM crash & GDI+ Font rendering on QR card print
- Fixed `IssueQrDialog` and `GiftQrDialog` print crash (`0xc000041d` in `combase.dll`) by disabling `UseEXDialog`.
- Replaced private font collection usage in printer graphics with standard font constructor.
- Added printer count pre-check and invalid printer fallback.
- Added `Msg_NoPrinterInstalled` to `Strings.resx`.
- Commits:
  - `fix(winforms): fix print dialog combase crash and font rendering on qr card print`
- All 588 unit/arch tests pass (100% green).

### S24 — 2026-11-05 — Database migration on SQLEXPRESS & Beneficiaries Window Fix
- Applied pending migration `20261102125339_AddGiftVouchersAndCompanionLimit` on SQLEXPRESS instance.
- Fixed `BeneficiariesControl` row tag lifecycle and selection event handling to ensure customer details populate and buttons enable.
- Commits:
  - `fix(winforms): ensure beneficiary details populate and buttons enable on grid load`
- All 588 unit/arch tests and 34 integration tests pass (100% green).

### S23 — 2026-11-04 — Phase F5 Completion (T-G8..T-G11)
- Tasks: T-G8, T-G9, T-G10, T-G11 completed on `phase/f5-gift-vouchers`.
- Commits:
  - `feat(gift): desk companion limit, gift panel, and qr dialog [T-G8]`
  - `feat(gift): rides gift redemption screen and main menu [T-G9]`
  - `feat(gift): gift receipts report and excel export [T-G10]`
- Highlights:
  - Full end-to-end gift voucher flow implemented: Desk eligibility check & issue (F6/F7) -> Rides window redemption (F3) -> Comprehensive report & ClosedXML Excel export.
  - Free-companion limit calculation live on desk with exemption for infants.
  - Zero warnings, 621 tests passing (587 unit/arch + 34 LocalDB integration).
  - Documentation and test script updated.

### S22 — 2026-11-04 — Organization gift policy & companion limit WinForms screens [T-G7]
- Tasks: T-G7 completed on `phase/f5-gift-vouchers`.
- Commits: `feat(gift): organization gift policy and age category exempt winforms screens [T-G7]`.
- Decisions: Extended `EditOrganizationDialog`, `EditAgeCategoryDialog`, `OrganizationsForm`, and `AgeCategoriesForm` with new gift settings and companion limit exemption controls.

### S21 — 2026-11-04 — Gift report queries, QR image adapter, DI [T-G6]
- Tasks: T-G6 completed on `phase/f5-gift-vouchers`.
- Commits: `feat(gift): report queries, QR image adapter and DI registrations [T-G6]`.

### S20 — 2026-11-02 — Persistence & Migrations [T-G5]
- Tasks: T-G5 completed on `phase/f5-gift-vouchers`.
