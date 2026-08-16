# en-GB Repository Completion Report

This report records the bounded repository-completion implementation, its
independent reviews, protected identities and gate status. It is factual
evidence only and grants no lifecycle, provider, external-action or Human Gate
authority.

## 1. Authority and baseline

```text
Conversation: RAG-Challenge — EN-GB — REPOSITORY COMPLETION
Authority: AUTH-ENGB-REPOSITORY-COMPLETION-IMPL-001
Mandatory baseline: main@8882ab8a58e1db58fb0148b967894f1b8388adc2
Coordinator branch: codex/en-gb-repository-completion
Initial working tree: clean
Lifecycle: STATE-07 TESTING_HOMOLOGATION active and unchanged
Parallel-work classification: SEQUENTIAL_ONLY for mutations, integration and gates; independent reviews remained read-only
```

The initial branch, full HEAD and clean tree matched the mandatory baseline
before any worktree or branch was created. The two writable lanes used the
exclusive branches and worktrees named by the owner. The enforcement lane was
created only after the internal-identifier candidate had been reviewed and
integrated. The coordinator integrated one commit at a time.

No network, product provider, product credential identifier or value, billing,
online audit, installation, OCI, GitHub, push, pull request, merge, release,
deploy, Human Gate or lifecycle action was used or performed.

## 2. Binding canonical exceptions

The exact identifiers, literals and four public script paths enumerated by the
owner remain canonical and unchanged. The closed executable manifest records
their existing paths, values, counts and contexts; this report deliberately
does not duplicate their legacy-spelled values into a new path. They remain
contracts, persisted or hash-bound identities, canonical literals or existing
public names, not precedent for a new private identifier or project-owned
prose.

## 3. Internal-identifier lane

The internal lane changed only six implementation and unit-test files. It
applied 54 lexical substitutions to private test names, helpers, local
parameters and local variables after confirming that they were editable,
non-serialised, non-persisted, not bound to a digest and not part of a public
contract. Public members, constructor parameters, persistence names and every
binding canonical exception remained unchanged.

| Evidence | Result |
|---|---|
| Source candidate | `f7b51dd9ec2739bfe4be11335170e38f5ab431e8` |
| Integrated commit | `172575da006e652b105d8bf5e0c70c139ee1e0cb` |
| Unit tests | 21 passed, 0 failed |
| Integration tests | 3 passed, 0 failed |
| Independent review | PASS, zero findings |

## 4. Enforcement-coverage lane

The enforcement policy now classifies every regular tracked blob. Two exact
binary paths are bound by purpose and SHA-256. Every other tracked blob must
decode as strict UTF-8 text irrespective of extension. Whole-file text
classifications are exact by path, closed reason and digest. Editable
localisation, owner-facing examples, preserved historical text and synthetic
rejection fixtures are bounded by unique markers and region digests inside
otherwise scanned files.

The reviewed technical candidate contained 419 tracked blobs: 417 text files
and two binaries. Its closed exceptional inventory contained 100 immutable
text paths, 18 exact regions and 102 exact canonical-identifier allowances.
The remaining editable text stayed under lexical and identifier inspection.
The addition of this tracked report makes the documentary candidate contain
420 tracked blobs without adding an exception.

Canonical identifier allowances bind the exact path, classification, kind,
value, occurrence count and three-line context hash. Legacy-spelled filenames
also require exact allowances. New private identifiers, unmatched filenames,
count drift, context substitution and legacy spelling embedded in a new
identifier fail closed. New commit messages are inspected without rewriting
history; backticked canonical literals remain narrowly recognised technical
exceptions.

The ordinary checker has no baseline writer, wildcard classification,
authority switch or reusable control-update bypass. Ordinary exact and range
checks reject changes to the complete protected enforcement boundary. This
manually reviewed bootstrap remains an exceptional control update and does not
authorise a later candidate to bypass that rejection.

| Evidence | Result |
|---|---|
| Source implementation candidate | `1ae50b3b4955eb3270a17fcf5a73278fa4008241` |
| Source localisation corrective candidate | `db57d68f2154f979567bf8b5ec821573cd93610b` |
| Source commit-literal corrective candidate | `ac4f7e5b413519d63c7562fefb7e04b9af55c19e` |
| Integrated implementation commit | `b9031d5eb30ce81736a5bc0f0ad4cc66e9ff77ec` |
| Integrated localisation corrective commit | `fe7f9f02770d38ca76f109709f1b2537d7b9ac63` |
| Integrated commit-literal corrective commit | `08a2c960bbf3e46e9b74276da73f8cb5a56157cc` |
| Language-policy tests | 105 passed, 0 failed |
| Repository inspection | 419 tracked blobs, zero findings on the reviewed technical candidate |
| Orchestrator package | lint and typecheck passed; 105 of 107 tests passed, 0 failed and 2 host symlink-permission skips |
| Independent corrective reviews | PASS after each attributable P1; zero residual P0–P3 findings |

## 5. Independent-review corrections

The first enforcement security review found one blocking P1. The code-region
extractor skipped any line containing a `"pt-BR"` dictionary key, which could
hide unclassified technical prose outside an exact localisation region. The
candidate was not integrated.

The corrective commit removed that generic skip and added a negative test that
requires the former reproduction to emit both
`PORTUGUESE_TECHNICAL_PROSE` and `US_SPELLING`. Removing the skip exposed two
existing Dashboard localisation fixtures. They are now represented by two
additional exact, digest-bound functional-localisation regions rather than a
generic exemption. Together with the two existing `i18n.ts` regions, all four
functional regions have unique markers and matching SHA-256 identities.

The same read-only reviewer then confirmed the reproduction, the four region
digests, the immutable 419-blob inspection, both candidate messages and the
absence of any residual generic `pt-BR` skip. The closing result was PASS with
zero P0–P3 findings.

The first integrated result review then found a separate blocking P1 in commit
message handling. Both the repository checker and the orchestrator removed
every inline code span before checking legacy-spelled identifier stems. This
allowed an arbitrary private identifier or American prose to evade the rule by
using backticks, while the tests proved only the intended positive canonical
case. The gates remained stopped.

The second corrective commit derives one closed set from the exact values in
the validated canonical-identifier allowances. Only a complete, case-exact
member of that set receives the inline technical-literal exception. Every
other backticked value is returned to lexical and identifier inspection. The
orchestrator now validates the allowance records’ envelope, path, class, kind,
value, count, context hashes and uniqueness before deriving the same set.
Negative tests cover a private identifier and American prose between
backticks; the exact canonical case remains positive. The originating
read-only reviewer reproduced all three outcomes and approved `ac4f7e5` with
zero residual P0–P3 findings before its integration as `08a2c96`.

## 6. Protected identity evidence

The integrated candidate preserves:

- OpenAPI v1 SHA-256
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
  and Git blob `a5fb3602fbab33bda6aa56cc4caaa9fdc37c8160`;
- OpenAPI v2 SHA-256
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`
  and Git blob `5ed6a47631653dd0c137b6ea1e979ae2c14bf8a8`;
- migrations tree `8ab6d3dbb632487d8ac5ea3be580118e58e82570`;
- evaluation tree `16a7a68f87af2704632487d2f50fedda0f12a980`;
- the four owner-listed public script paths and every binding canonical value
  described in section 2.

No OpenAPI contract, accepted ADR, product requirement, schema, migration,
dataset, source, citation, localisation decision, hash-bound region or
append-only prefix was rewritten.

## 7. Documentary reconciliation

Corpus `4.17.0` records the repository-completion capability without changing
the product lifecycle or any accepted architectural disposition. Current
State records the integrated candidate; the append-only State Transition Log
records the authority, implementation, completed independent reviews, passed
documentary gate and failed canonical execution; and the instruction-system
ledger records the new capability newest-first. Historical
entries and protected prefixes remain byte-for-byte.

## 8. Gate results

```text
Focused lane checks: PASS
Independent corrective reviews: PASS after both attributable P1 corrections
Integrated final result review: PASS, zero P0–P3 findings
Integrated final security review: PASS, zero P0–P3 findings
Documentary gate: PASS for 420 non-ignored files
Canonical ./eng/ci.ps1 -Offline: FAIL; exactly one execution consumed on 2c2b80c106be6a9b69884e2267c3d7a84d7c11f9
```

Runtime preflight found no process or listener proved to belong to
RAG-Challenge. The canonical command ran once in a closed child environment,
with MSBuild node reuse disabled and task-owned offline package caches and
temporaries. It exited `1` after 6,085 ms during the first language-policy
stage. Of 105 tests, 84 passed and 21 failed with the same sanitised
`Synthetic Git command failed closed` outcome at synthetic `git add .` calls.
Restore, formatting, build, .NET tests, Dashboard checks, orchestrator checks,
coverage aggregation and the repository-audit stage were therefore not
reached. The command was not retried.

A bounded local diagnostic attributed the observed stage failure to the
coordinator’s temporary-path selection. The task-specific temporary base was
153 characters; the generated repository root reached 220 characters and its
longest fixture and Git lock paths reached 264. Under the same closed Git
environment, `git init` succeeded in both locations, `git add .` returned
`128` with a path-length error in the long location and the identical add
succeeded in a shorter task-owned location. This is an execution-envelope
failure rather than evidence of a candidate regression, but the mandatory
gate remains failed.

Disposition is `TEST_BASELINE_BROKEN`. The single execution authorised by
`AUTH-ENGB-REPOSITORY-COMPLETION-IMPL-001` has been consumed. A corrected
canonical run requires new explicit authority, a new exact clean baseline and
a shorter isolated task-specific temporary root. No result from the passing
focused suites substitutes for that missing canonical PASS.

## 9. Limitations

- Offline validation cannot substitute for an online dependency audit.
- The canonical offline gate did not pass; all stages after the first
  language-policy stage remain unexecuted on the integrated baseline.
- Two orchestrator file-symlink tests remain conditional on a Windows host
  permission not available in the focused lane checks; all other relevant
  boundary tests passed.
- Lexical enforcement is necessary but not sufficient. Independent semantic
  review remains the control for audience, source, canonical and idiomatic
  language classification.
- The manually reviewed policy bootstrap is not reusable authority for any
  later control update.

## 10. Current Git state

```text
Branch: codex/en-gb-repository-completion
Integrated technical HEAD: 08a2c960bbf3e46e9b74276da73f8cb5a56157cc
Failed canonical baseline: 2c2b80c106be6a9b69884e2267c3d7a84d7c11f9
Final factual documentation commit: reported in the owner hand-off because a commit cannot truthfully embed its own object identity
Push, pull request, merge, release and deployment: NOT_PERFORMED
```
