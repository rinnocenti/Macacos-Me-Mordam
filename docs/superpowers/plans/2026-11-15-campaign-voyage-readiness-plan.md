# Campaign Voyage Loop Implementation Plan

**Execução:** As regras da Task 1, incluindo Continue após uma chegada abaixo do limiar, foram aprovadas pelo usuário. A auditoria da Task 2 está concluída; o usuário aceitou as APIs Experimentais de câmera exigidas para o MVP. A compatibilidade de importação/compilação ainda requer verificação no Unity.

> **For agentic workers:** Use superpowers:executing-plans or superpowers:subagent-driven-development to carry out this plan after the user selects an execution approach. Steps use checkbox syntax for tracking.

**Goal:** Produce an implementation-ready voyage rules specification and a verified Immersive Framework boot contract before planning gameplay code.

**Architecture:** One campaign Route and Activity contain repeatable voyages. The game owns voyage and campaign state; the Framework owns application and navigation. This plan resolves the gameplay rules and verifies the actual boot requirements before any gameplay scripts or authored application assets are planned.

**Tech Stack:** Unity 6000.6.9f1, C#, local Immersive Framework declared as 1.2.0 in the resolved package.json (local Git revision and release maturity not confirmed), NUnit intended for later gameplay logic tests.

**Spec:** [Campaign Activity design](../specs/2026-11-15-campaign-activity-design.md)

## Global Constraints

- Unity version: 6000.6.9f1.
- Target: Android (Mobile); primary input: Touch; touch support: Full; gamepad: None.
- The project currently resolves Immersive Framework from a machine-local absolute path. Keep that working-tree change local; do not commit it as portable setup.
- Prefer Stable Framework contracts. Do not rely on Experimental behavior unless the boot audit proves it is required and the user accepts the risk.
- Keep monkey, ship, voyage and reward rules in the game. Do not modify Framework package source or create gameplay authority inside the package.
- Do not implement code or author scenes/assets until the separate implementation plan is reviewed and approved.
- Unity compilation, tests, Play Mode, smoke and batchmode are outside this plan's execution environment; record them as pending and provide manual Editor steps.

## Review Focus

The voyage rules document must define an exact expected result for each case below. These are design acceptance scenarios; they are not executable tests yet.

1. **Arrival and sinking in the same update:** exactly one terminal result wins, and the reward cannot be granted twice.
2. **Touch near a spawn or overlapping targets:** the rule selects at most one intended monkey and does not leave duplicate or negative load.
3. **Removing a monkey near the sinking limit:** the rule states whether load removal slows, pauses or reverses sinking.
4. **Application pause or mobile backgrounding:** voyage time, spawn schedule and sinking follow one explicit policy.
5. **Retry or next voyage after an outcome:** previous monkeys and results cannot leak into the next attempt, and campaign progress changes exactly once.

---

### Task 1: Specify the campaign voyage rules

**Files:**
- Create: design/gdd/voyage-system.md
- Read: design/game-brief.md
- Read: docs/superpowers/specs/2026-11-15-campaign-activity-design.md

**Interfaces:**
- Consumes: the approved campaign Activity boundary and the existing game brief.
- Produces: a user-approved game rules contract with state transitions, formulas, tuning values, edge cases and deterministic acceptance scenarios.

- [x] **Step 1: Record the user-approved voyage rules**

Record the chosen voyage terminal states and precedence, mobile pause/background timing, monkey classes and spawn schedule, touch target selection, weight-to-sinking formula and recovery behavior, reward threshold, schedule order, and explicit Continue trigger.

- [x] **Resolve the next-voyage trigger**

The user chose explicit Continue after success below the curse threshold. Retry repeats the same schedule; each success advances exactly once to the next one.

- [x] **Step 2: Write the rules in design/gdd/voyage-system.md**

Include Overview, Core Mechanics, State Transitions, Formulas, Edge Cases, Dependencies, Tuning Knobs and Acceptance Criteria. Keep all runtime rules game-owned and independent from Unity scene objects.

- [x] **Step 3: Add deterministic acceptance scenarios**

Include a row for every Review Focus case plus normal arrival, normal sinking, defeating a light monkey, defeating a heavy monkey, and reaching the curse-breaking threshold. Each row must specify initial state, input or elapsed time, expected final state and reward/progress effect.

- [x] **Step 4: Check the document against the brief and request approval**

Confirm it preserves the one-route MVP and excludes persistent campaign saves and extra regions. The user approved the rules and every acceptance row has a concrete expected result.

**Check:** A designer can determine the result of each acceptance scenario without inventing a value, formula or transition.

---

### Task 2: Audit the installed Framework boot contract

**Files:**
- Create: design/architecture/framework-application-boot-contract.md
- Read: local Framework Documentation~/Guides/Getting-Started.md
- Read: local Framework Documentation~/Guides/Game-Flow.md
- Read: local Framework Documentation~/API/Public-API.md
- Read: the installed boot, validator and call-site source needed to trace each gate

**Interfaces:**
- Consumes: the current Unity project, its resolved local Framework package and the approved voyage rules document.
- Produces: a gate table that distinguishes runtime prerequisites, authoring/readiness checks, conditional behavior, API maturity and manual Editor validation.

- [x] **Step 1: Inventory the current application assets and package resolution**

Record the Unity project root/version, manifest and lock resolution, local Framework package identity, and whether any GameApplication, Route or Activity assets already exist. Preserve the current manifest and lockfile edits.

- [x] **Step 2: Trace every required startup gate**

Follow the installed runtime boot path and every invoked validator through its call sites. Inspect source defaults separately from serialized project assets. Do not infer optionality from an empty setting or from the first validator alone.

- [x] **Step 3: Record the minimal campaign composition and validation**

Write the gate table with condition, required value/reference, failure result, source symbol and maturity. Name the precise Unity Editor import/compile and startup checks that remain manual. Do not generate or modify Framework assets during this audit.

**Check:** Every recommended Application, Route, Activity or Persistent Content requirement is backed by installed documentation and a traced runtime or validator path; unverified gates are explicitly marked pending.

---

## Handoff Gate

Handoff complete: the separate code-level implementation plan is [ready for user review](2026-11-15-campaign-implementation-plan.md). Do not implement gameplay code until that plan is approved.

