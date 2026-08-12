---
type: product-outcome
status: implemented-verified
date: 2026-08-07
owner: "Grove Product"
roadmap: "[[../../roadmaps/Product Roadmap]]"
---

# Quick Note capture

## Purpose

Define the lightweight capture function for recording thoughts before deciding
what to do with them next.

## Background

A person may know what they want to record before they know how they will use
it. Quick Note must let them continue recording, revisit Notes made during the
same session, and leave each saved Memory available for a later action.

## Outcome

> I can capture several thoughts without deciding what to do with them, return to any Note created in the current capture session to refine it, and keep capturing so ideas are recorded before I have to organize them.

## Behavior scenarios

### Capture without a location

I open Quick Note, write a multiline Note, and save it without choosing a
destination. The Note becomes a durable Memory without requiring another
surface to be open.

### Continue capturing

After Save, the composer is ready for the next Note and the saved Note remains
visible in the current session feed.

### Refine a session Note

I reopen a Note from the feed, edit it, and save the current revision without
creating a second Memory.

### Place one Note

I choose a later action for a saved Note, complete that action, and return to
the same capture session after commit or cancellation.

### Close or cancel

Cancelling an empty draft creates nothing. Closing the session clears only the
transient feed; saved Memories remain durable.

## Context

Quick Note is an application function. It creates Memories; it does not own
Memory Slate, Content viewers, Placement, or any later operation performed on
the saved record.

## Decisions

- The visible product name is Quick Note.
- Enter inserts a line break; Shift+Enter saves the current Note and starts the
  next Note; Ctrl+Enter saves and exposes the available next action.
- The actions are Cancel, Save, and a later-action handoff.

## Non-goals

- Batch Placement.
- Memory Slate browsing or context-based Memory recall.
- Rich text, Documents, Images, Anchors, or spatial retrieval.

## Evidence

- Wireframe: [[../../ux/wireframes/Quick Note capture]]
- Interaction contract: [[../../decisions/Quick Note interaction contract]]
- Model: [[../../reference/Content and Memory model]]
- Browser journey: `qa/scenarios/capture-notes-rapidly.json`
- Implementation evidence: [[../../engineering/evidence/T-C01 Memory-backed rapid Note capture]]
