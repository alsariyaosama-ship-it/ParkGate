# Session Handoff

> **Read at the start of every session. Update at the end of every session** (AGENTS.md §1).
> Keep "Snapshot" and "Next session" accurate — they are overwritten each time. The log keeps the last 10 sessions only.

---

## Snapshot

| Field | Value |
|---|---|
| Last updated | 2026-10-04 |
| Last session | S00 — Planning |
| Current phase | Phase 0 — Foundation (not started) |
| Working branch | `main` → create `phase/00-foundation` |
| Build | n/a (no solution yet) |
| Unit tests | n/a |
| Integration tests | n/a |
| CI | Workflow present; skips until `ParkGate.sln` exists |
| Last commit | `docs: initial specification and agent rules` |

## State of the work

**Done**
- Specification, database design, roadmap, ADRs, agent rules and GitHub scaffolding committed.

**In progress**
- Nothing.

**Not started**
- All code.

## Next session — start here

1. Create branch `phase/00-foundation` from `main`.
2. T0.1 — create the solution and projects (AGENTS.md §3, ROADMAP Phase 0).
3. T0.2 — references + `Directory.Packages.props`.
4. Continue T0.3 → T0.6 in order; push and open the Phase 0 PR when all are ticked.

## Blockers / waiting on the user

- None for Phases 0–6.
- None. Remaining open questions use the documented defaults in SPEC §11.

## Known issues / tech debt

- None.

## Environment notes

- Dev DB: LocalDB `(localdb)\MSSQLLocalDB`, database `ParkGate_Dev` (default in `appsettings.json`).
- Integration tests create and drop `ParkGate_Test_<guid>` databases on LocalDB.

---

## Session log (newest first, max 10)

### S00 — 2026-10-04 — Planning
- Created SPEC, DATABASE, ROADMAP, DECISIONS, AGENTS, CLAUDE, README, CI workflow, PR template, `.gitignore`, `.gitattributes`, `.editorconfig`, `Directory.Build.props`.
- Decisions: ADR-001 … ADR-016 — **approved by the user**.
- User answers: Q2 holder counted separately · Q3 rolling window in hours · Q8 no financial claims, per-organization statistics important · Q11 gate deferred.
- No code yet.

<!--
Template for a new log entry (copy above the previous entry):

### S<NN> — YYYY-MM-DD — <short title>
- Tasks: T<x.y> done, T<x.z> partial (what's left)
- Commits: <short hashes or "see branch">
- Decisions: ADR-0XX (if any)
- Notes for next session: <one or two lines>
-->
