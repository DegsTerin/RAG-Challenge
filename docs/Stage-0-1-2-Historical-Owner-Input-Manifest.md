# Stage 0, Stage 1 and Stage 2 historical owner-input provenance

## Status and authority

This manifest records the provenance and preservation identity of three files
supplied by the RAG-Challenge owner as inputs to the Stage 0, Stage 1 and
Stage 2 development work. It is an evidence and traceability document only.

The three source files are classified as `HISTORICAL_OWNER_INPUT`, `INACTIVE`,
`NON_NORMATIVE` and `LOCAL_ONLY`. They are not instructions for a current
agent, do not grant execution authority, do not change the lifecycle or a
Human Gate, and cannot override current governance, accepted ADRs or the
factual state. The current authorities routed by
[`AGENTS.md`](../AGENTS.md),
[`Start-Here.md`](../prompts/Start-Here.md) and
[`Current-State.md`](../prompts/state/Current-State.md) always prevail.

The original bodies remain outside Git. No tracked full translation, body
copy or second normative version exists.

## Preserved local objects

The files were verified at the repository root, moved sequentially on
2026-08-15 to the ignored `reference-materials/governance-inputs/` directory,
and verified again at their destinations. The move did not change their
bytes.

| Stage | Original owner-supplied filename | Preserved local filename | Language | Bytes | SHA-256 |
|---|---|---|---|---:|---|
| Stage 0 | `Stage 0 - Instructions developing AI agents.md` | `Stage-0-Instructions-Developing-AI-Agents.pt-BR.md` | `pt-BR` | 5,717 | `d06dbf297253499af50ae1c9f464228f9100d2a0c5860f36cc624512d8873032` |
| Stage 1 | `Stage 1 - Governance and Multi-Agent Readiness Audit.md` | `Stage-1-Governance-And-Multi-Agent-Readiness-Audit.pt-BR.md` | `pt-BR` | 23,925 | `f093db3b4212dd6a6986904d62e5ebe7324c8e3fea6a3c581c507986def2bd5d` |
| Stage 2 | `Stage 2 - Multi-Agent Orchestrator Implementation.md` | `Stage-2-Multi-Agent-Orchestrator-Implementation.pt-BR.md` | `pt-BR` | 26,072 | `0b0fb003ff9e8dfae3911b98aaebb6a1c69c1869f169ddb50fda644d03cb7a92` |

The preserved names are relative to
`reference-materials/governance-inputs/`. That directory is excluded by
`.gitignore`; public documentation and project checks do not depend on the
local files being present.

## Implementation lineage

The owner inputs informed a selective implementation that started from
`main@9f309e1b6a21a33cbd24b4b6498e840dd26585c9` and was present on
`main@d9968cac893f70989553fe9b8ae07ad7a3dbdaae`. The complete original
implementation set is:

| Commit | Subject |
|---|---|
| `3c3907661c24` | `feat(governance): prepare safe multi-agent development` |
| `48d5ddd97900` | `test(integration): serialise shared SQLite state` |
| `9495825c16ce` | `docs(governance): record stage 1 readiness audit` |
| `be274907472f` | `docs(governance): require en-GB commit messages` |
| `7c1cc73bbe6a` | `docs(governance): record Stage 2 authorisation` |
| `2009c1be7307` | `docs(governance): record Stage 2 dependency condition` |
| `45b35dd46353` | `feat(orchestrator): add deterministic development coordination` |
| `c781fb437b32` | `build(ci): validate the orchestrator toolchain` |
| `e1385717d1be` | `test(orchestrator): reject unsafe environment names` |
| `26b52645cd40` | `fix(orchestrator): enforce deterministic trust boundaries` |
| `1e0aa65909d6` | `fix(orchestrator): harden deterministic recovery and integration` |
| `8b75d424581a` | `fix(orchestrator): close durable isolation boundaries` |
| `283912c20b97` | `docs(orchestrator): record Stage 2 implementation` |
| `0854d4671721` | `docs(governance): record fake-only architecture disposition` |
| `f150b2d0523a` | `docs(orchestrator): propose pre-turn Codex runner` |
| `a7c64d5d16e2` | `docs(orchestrator): record ADR-0017 acceptance` |
| `583c3b40ba0b` | `feat(orchestrator): enable pre-turn Codex runner` |
| `9512d6ebceb6` | `fix(orchestrator): use stable App Server fields` |
| `d9968cac893f` | `docs(orchestrator): record ADR-0017 activation` |

The later bounded implementation under
`AUTH-STAGE012-GOV-SEC-ENGB-IMPL-001` is recorded on
`codex/stage012-integration` from `197296365a8e` through the pre-manifest
integration head `1226ca4429a3`. It adds the permanent hand-off rule, product
credential isolation, trusted en-GB enforcement and the approved language
migration. That later range does not modify or track the three original
owner-input bodies.

Detailed implementation and review evidence remains in:

- [`Multi-Agent-Readiness-Audit.md`](Multi-Agent-Readiness-Audit.md);
- [`Stage-2-Multi-Agent-Orchestrator-Report.md`](Stage-2-Multi-Agent-Orchestrator-Report.md);
- [`ADR-0016`](architecture/ADR-0016-Deterministic-Development-Orchestrator-And-Codex-Runner-Boundary.md);
- [`ADR-0017`](architecture/ADR-0017-Codex-App-Server-Pre-Turn-Checkpoint-Runner.md).

## Verification and recovery rule

When local provenance must be checked, verify the exact byte length and
SHA-256 value in this manifest. A missing or mismatched local file must not be
reconstructed from a translation, report or current governance document. It
requires the original owner-supplied bytes or a separately authorised
provenance correction.
