---
type: product-outcome
status: discovery
date: 2026-08-08
owner: "Grove Product"
roadmap: "[[../../roadmaps/Migration roadmap]]"
---

# Run Grove as its own app

## Purpose

Give Grove a desktop home: an installable app whose vault lives durably on
the person's own disk, ready to host agents and exchange files.

## Background

Grove runs today from a browser with its vault in browser storage — working,
but tied to a browser profile and unable to host subprocesses or hold
credentials. The accepted [[../../decisions/Desktop stack]] keeps the entire
frontend and wraps it in a shell with a Rust core: same application, durable
home. The migration is deliberately an adapter swap, not a rewrite, because
persistence and assets land behind interfaces first (T-PS01).

## Outcome

> I can open Grove as its own app on my computer, with my vault stored safely on my own disk, so my work lives with me instead of inside a browser.

## Behavior scenarios

### Install and open

When I install Grove and open it, I get the same application I know — same
Grid, same surfaces, same feel — in its own window.

### My vault, my disk

When I work, everything commits to a vault on my own disk; deleting a
browser or its profile costs me nothing.

### Bring my browser vault

When I first open the app with an existing browser vault, Grove migrates it
losslessly and shows me it did.

### Ready for what's next

When the app runs, the capabilities the browser could not hold — agents,
credentials, file exchange — have their home, arriving through their own
outcomes.

## Context

Stack, sequencing, and platform order are owned by
[[../../decisions/Desktop stack]] and [[../../roadmaps/Migration roadmap]].
Persistence granularity is T-PS01's contract. File exchange remains
[[../../discovery/Vault folder and file onboarding]].

## Decisions

- The frontend is preserved, not rewritten; entering the app tier authorizes
  no visual or behavioral drift.
- Browser-vault migration is lossless and atomic, with the original left
  readable until commit.
- Windows ships first; macOS and Linux each gate on the full QA journey
  suite.

## Non-goals

- Any rendering or interaction change as part of migration.
- Cloud accounts, sync, or telemetry.
- File-folder onboarding (its own discovery).

## Evidence

- Decision: [[../../decisions/Desktop stack]]
- Discovery: [[../../discovery/Vault folder and file onboarding]]
- QA: the full existing journey suite run inside the shell per platform.
