# Session Handoff

> **Read at the start of every session. Update at the end of every session** (AGENTS.md §1).
> Keep "Snapshot" and "Next session" accurate — they are overwritten each time. The log keeps the last 10 sessions only.

---

## Snapshot

| Field | Value |
|---|---|
| Last updated | 2026-11-13 |
| Last session | S26 — Phase R0 (partial, cloud session, no shell): git baseline by static analysis, roadmap Phase R, web CI + baseline script staged |
| Current phase | Phase R — Remediation (`docs/REMEDIATION_PLAN.md`). R0 in progress: R0.1 and R0.3 done, R0.4 drafted, R0.2 pending |
| Working branch | `main` (dirty) → `phase/r0-baseline` still to be created |
| Build | Not re-verified in S26 (no shell). Last known: clean, 0 errors; 16 pre-existing NuGet vulnerability warnings for `System.Security.Cryptography.Pkcs` 9.0.11 |
| .NET Unit tests | 588 passed as of S25 (202 domain + 169 application + 154 architecture + 63 infrastructure); not re-run in S26 |
| .NET Integration tests | 34 passed on LocalDB as of S25; not re-run in S26 |
| Total tests | 622 as of S25. **`php artisan test` in `web/` has never been baselined — R0.2** |
| Latest migration | `20261102125339_AddGiftVouchersAndCompanionLimit` (Applied on SQLEXPRESS and LocalDB) |

## State of the work

**Completed in this session (S26):**

1. **R0.1 — baseline git state (done, by static analysis).** The cloud session had no shell on the dev
   machine, so `git` could not be run. Instead `.git/index`, `.git/refs` and `.git/logs/HEAD` were read
   directly and all 566 tracked files were compared against their recorded blob (size, mtime, then SHA-1
   for every candidate). Result:
   - `main` = `13ec3c60dbd10f996b31b4e8b1a11e48ac8b7b32`, identical to `origin/main` as last fetched
     2026-11-05 — **nothing unpushed**.
   - Modified, not staged: `docs/SESSION_HANDOFF.md` only.
   - Untracked: `docs/REMEDIATION_PLAN.md` only. Everything else untracked is git-ignored
     (`artifacts/`, `dist/`, `graphify-out/`, `.vs/`).
   - **Correction to REMEDIATION_PLAN.md §1:** the plan expects uncommitted S25 changes in
     `IssueQrDialog.cs`, `GiftQrDialog.cs` and `Strings.resx`. They are **not** modified. Their mtimes are
     24 minutes newer than the commit, but their contents are byte-identical to the committed blobs (a
     build/restore touched them). The same holds for `Strings.Designer.cs`. All S25 work is committed and
     pushed in `13ec3c6`.
   - Caveat: `origin/main` is the last-fetched value; no `git fetch` was possible. Confirm with a fetch.
   - Osama's decision: commit `docs/SESSION_HANDOFF.md` and `docs/REMEDIATION_PLAN.md` on
     `phase/r0-baseline`, not on `main`.
2. **R0.3 — Phase R added to `docs/ROADMAP.md`** (R0–R9, all tasks, R6.7 struck through as cancelled by
   Q-R5). The plan's §2.2 questions were put to Osama; the plan's documented defaults apply to any he
   leaves unanswered.
3. **R0.4 — `web/` CI workflow written**, but parked at `docs/_R0.4-web-workflow.yml` because
   `.github/workflows/` is write-protected for remote file tools. It is a separate `web.yml` (ubuntu-latest,
   PHP 8.3 with pdo_sqlite, composer install with a vendor cache, `npm ci && npm run build` so Inertia
   feature tests resolve the Vite manifest, `php artisan test`, Laravel logs uploaded on failure). It
   skips cleanly if `web/composer.json` is absent, mirroring `build.yml`'s solution check.
   **First action next session:** `git mv`/move it to `.github/workflows/web.yml` and delete the parked copy.
4. **Convenience script `scripts/r0-baseline.ps1`** — runs the whole AGENTS.md §1.1 step 3 sequence
   (fetch, status, log, build, unit tests, integration tests, `php artisan test`) and writes everything to
   `docs/_r0-baseline.log` (git-ignored by `*.log`). Read-only; it commits nothing. Keep it if useful,
   delete it otherwise — it is not referenced by any doc.

**Uncommitted state of the tree as S26 ended** (nothing was committed; `main` is still at `13ec3c6`):

| File | State | From |
|---|---|---|
| `docs/REMEDIATION_PLAN.md` | untracked | review session 2026-11-12 |
| `docs/SESSION_HANDOFF.md` | modified | review session + S26 |
| `docs/ROADMAP.md` | modified | S26 — Phase R task list (R0.3) |
| `docs/_R0.4-web-workflow.yml` | untracked | S26 — move to `.github/workflows/web.yml` (R0.4) |
| `scripts/r0-baseline.ps1` | untracked | S26 — optional helper |

Osama approved committing these on `phase/r0-baseline`, **not** on `main`.

**In progress**
- R0 is not finished. Outstanding: R0.2 (needs a shell), R0.4 file placement, the commit, and the PR.

**Not started / Future**
- F2 Electronic gate (deferred by client; SPEC §9 F2).

## Next session — start here

**Run Phase R sessions on the dev machine (`claude` in `C:\dev\ParkGate`), not from the cloud.**
A cloud session reaches the folder through the desktop app's file tools only: it cannot run `git`, `dotnet`
or `php`, and the platform grants terminals in click-only mode, so it cannot drive one through computer use
either. `.github/` is write-protected for remote tools as well. S26 hit all three limits.

Finish R0, in this order:

1. `git fetch origin` and confirm `main` still equals `origin/main` (S26 could not fetch).
2. `git checkout -b phase/r0-baseline`, then commit the two files Osama approved:
   `docs/SESSION_HANDOFF.md`, `docs/REMEDIATION_PLAN.md`, plus `docs/ROADMAP.md` (changed in S26).
   Suggested subject: `docs: add remediation plan and phase R roadmap [R0.1][R0.3]`.
3. **R0.2** — record the real baseline in the Snapshot table above:
   `dotnet build ParkGate.sln -c Debug`, `dotnet test ParkGate.sln --filter "Category!=Integration"`,
   `dotnet test ParkGate.sln --filter "Category=Integration"`, and `cd web && php artisan test`.
   The web suite has never been baselined; if it is red, fix it before R1 (AGENTS.md §1.1.3).
4. **R0.4** — move `docs/_R0.4-web-workflow.yml` to `.github/workflows/web.yml` (delete the parked copy),
   then confirm both workflows go green on the branch. If the web suite needs no built assets, the
   Node and "Build assets" steps can be dropped to save about a minute per run.
5. Open the PR for `phase/r0-baseline` → `main` using `.github/pull_request_template.md`. Do not merge.
6. Then R1 (web security) on `phase/r1-web-security`.

Carried over from S25 (do it when a printer is available; it does not block Phase R):
- Verify QR printing on the user machine with a connected printer or "Microsoft Print to PDF", and confirm
  with Osama that customer and gift QR printing works without closing the app.

## Blockers / waiting on the user

- **R0.2 and every commit/PR in Phase R need a shell on the dev machine.** Run Phase R sessions locally.
- Open confirmations (the plan's documented defaults apply if unanswered): §2.2 — Q-R3 (who creates web
  dashboard users), Q-R4b (remaining glossary terms), Q-R6 (desk PC count, workgroup vs domain),
  Q-R7 (lowest desk resolution). None of them block R0; they block R1.1, R7.1, R6.5 and R5.8.
- Q-R2b (keep `Notes` / `StatusReason` local — recommended: yes) is needed before R2.6.
- Decisions already made by Osama are in the plan's §2.1 — do not re-ask them.

---

## Session Log (last 10 sessions)

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
