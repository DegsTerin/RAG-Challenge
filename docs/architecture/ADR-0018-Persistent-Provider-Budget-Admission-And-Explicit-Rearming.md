# ADR-0018: Persistent Provider Budget Admission and Explicit Rearming

## Status

`accepted`

## Date

2026-08-16

## Accepted

2026-08-16

## Owners

- Product owner: RAG-Challenge owner
- Architecture owner: RAG-Challenge
- Technical owners: security, operations governance and provider adapters

## Preparation and decision authority and baseline

- Preparation authority: `SEC-CORR-ADR-PREP-01`
- Prior read-only design authority: `SEC-CORR-DESIGN-01`
- Permanent corrective identity: `SEC-CORR-001`
- Branch: `main`
- Commit: `334053e0101ce882767ccba29c69da7882917280`
- Prompt corpus before preparation: `4.17.1`
- Decision authority: explicit product-owner decision
  `ADR-0018: ACEITAR.` on clean
  `main@89be70aba4de556611c9bdda8da62d1d4f9a1e41`, corpus `4.17.2`
- Lifecycle position: `STATE-07 TESTING_HOMOLOGATION`; unchanged
- Runtime preflight: `NOT_APPLICABLE` for documentary preparation and
  acceptance recording
- Protected OpenAPI v1 SHA-256:
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
- Protected OpenAPI v2 SHA-256:
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`

## Identity and authority boundary

`SEC-CORR-001` is assigned by this decision to the persistent provider-budget
boundary. It does not reuse `SEC-001`, which already identifies the distinct
NuGet vulnerability-gate finding in the
[Multi-Agent Readiness Audit](../Multi-Agent-Readiness-Audit.md). The existing
identifier and its historical test fixtures remain unchanged.

This decision selects no price, provider account, spend limit or operational
budget. No external price source was consulted. The operational provider
budget remains exactly zero and disarmed. Preparing or accepting this ADR
would not arm a budget, validate a credential, enable egress or authorise a
provider request.

The decision is an internal security and operations boundary. It does not
change OpenAPI, public HTTP behaviour, the product credential policy, an
existing campaign freeze, billing or lifecycle. Acceptance establishes
architecture authority only. Persistence design, schema, migration,
implementation, testing, operational values and any provider execution would
retain separate authorities.

## Context

[ADR-0006](ADR-0006-Security-Egress-Administration-And-HTTP-Contracts.md)
requires a monetary circuit breaker and states that retries must not bypass
rate limits or budgets. The
[security policy](../../prompts/governance/Security-And-Access.md) also requires
distinct operation-specific authority for administrative indexing, query
embedding and grounded generation immediately before credential lookup or
provider egress.

The threat model keeps these boundaries open:

- `THR-S02-014`: external embedding can disclose and charge for the complete
  authorised corpus over many bounded requests;
- `THR-S02-017`: anonymous query can exhaust provider budget; and
- `THR-S02-035`: catalogue growth can exhaust provider capacity or budget.

The frozen provider-candidate campaign recorded an operational limit and an
absolute ceiling for that one unexecuted evaluation design. Those values are
historical campaign inputs. They are not current prices, an account control, a
runtime ledger, a reusable budget or standing provider authority.

An in-memory counter is insufficient for the product boundary. A crash,
restart, parallel instance or uncertain timeout can discard reservations,
repeat an ambiguous request or restore an apparently unused budget. A
provider-side account limit is also insufficient because it is external,
coarser than the three operation authorities and cannot prove local admission
before credential access or egress.

## Decision drivers

- Fail closed before credential lookup and before provider egress.
- Preserve consumed and uncertain budget across process crashes and restarts.
- Prevent concurrent instances from admitting more than the authorised total.
- Keep indexing, query embedding and grounded generation separately bounded.
- Make retry and replay idempotent under one stable provider-request identity.
- Treat an uncertain remote outcome conservatively without inventing a refund.
- Require explicit rearming after every runtime-session change or trip.
- Keep prices and operational limits outside this documentary preparation.
- Keep secrets, questions, passages, answers and provider payloads out of the
  budget ledger and audit.
- Preserve public contracts and existing campaign evidence byte for byte.

## Decision

### Persistent budget envelope

Introduce an internal, versioned `ProviderBudgetEnvelopeV1` owned by
Application policy and persisted through an Application-owned fail-closed port
implemented in Infrastructure against the durable control plane. The envelope
is not public configuration and is
never selected by a browser, question, retrieved document, model output or
provider response.

Each envelope binds at least:

- an opaque budget-envelope ID and monotonically increasing revision;
- the exact environment, provider and non-secret billing-scope reference;
- the closed operation classes `AdministrativeIndexEmbedding`,
  `QueryEmbedding` and `GroundedGeneration`;
- exact permitted provider and model identifiers;
- one immutable cost-schedule ID and SHA-256 supplied under later authority;
- one aggregate authorised limit and strict per-operation allocations;
- currency and a fixed integer accounting unit defined by that schedule;
- committed, reserved and indeterminate amounts;
- effective and expiry instants;
- current state, runtime-session binding and rearm revision; and
- creation, arm, trip, reconciliation and closure authority references.

All amounts use non-negative integer arithmetic. Binary floating-point and
rounding after admission are prohibited. Overflow, an unknown currency, a
missing cost schedule, schedule drift, an unsupported model or an amount that
cannot be bounded before egress makes admission unavailable.

The aggregate limit is authoritative across all operation classes. Each
operation also has a strict allocation, so one capability cannot consume
another capability's authority. Moving an allocation or increasing the
aggregate limit requires a new explicitly authorised envelope revision; it is
not a rearm operation.

### Default and persistent state machine

The closed states are:

```text
Disarmed
Armed
Tripped
Exhausted
ReconciliationRequired
Expired
```

Absence, unreadability, corruption, incompatible version, scope mismatch,
expired authority or an unknown state is equivalent to `Disarmed` and rejects
admission. It never creates a replacement record automatically.

`Armed` is valid only for the exact runtime-session identity named by the
latest durable rearm record. A new process or service session has a new
identity and cannot inherit an earlier session's armed state. Process restart,
clean or abnormal, therefore requires explicit rearming before the first
provider admission.

`Tripped` records a policy violation or bounded failure. `Exhausted` records
that no further maximum reservation fits. `ReconciliationRequired` records at
least one uncertain remote outcome. `Expired` records expiry of the authority
window. None of these states returns to `Armed` automatically.

### Admission transaction

Every provider attempt uses one stable, opaque `providerRequestId` bound to
the exact operation authority reference. Before credential lookup or network
egress, the Application boundary must:

1. validate the bounded request plan, operation class and trusted in-memory
   operational grant;
2. resolve the exact immutable cost schedule and calculate a conservative
   maximum charge from request and response limits;
3. open one serialisable control-plane transaction for the aggregate envelope
   and its operation allocation;
4. verify `Armed`, runtime-session identity, revision, expiry, model, provider,
   currency and schedule digest;
5. reject unless both aggregate and operation remaining amounts can cover the
   maximum charge;
6. persist an idempotent reservation keyed by `providerRequestId`, advance the
   revision and commit;
7. read back the reservation and envelope through the durable port; and
8. revalidate the matching operational grant immediately before credential
   lookup and egress.

Only a successful durable readback permits credential lookup and exactly one
provider attempt. An existing identical reservation is a replay and returns
its recorded state. The same ID with different request, authority, schedule or
maximum-charge digests is a conflict and causes `Tripped` without egress.

The transaction uses database-enforced uniqueness and serialisable or
compare-and-swap semantics. A process-local lock, cache or provider dashboard
is not admission evidence. Concurrent instances cannot reserve against a
stale remaining amount.

### Completion, uncertainty and retry

After an observed response, a second durable transaction records the bounded
usage evidence, calculated charge, provider outcome and sanitised timing. It
converts the reservation to a committed amount without exceeding the maximum
reservation. A reported charge or usage that exceeds the admitted bound is a
policy violation: the envelope becomes `Tripped`, the full reservation remains
committed and further egress stops.

A failure proved to occur locally before any request bytes cross the provider
boundary may release its reservation through a durable, audited transition.
Lack of proof is not proof of non-consumption.

Timeout, cancellation after send, connection loss after a request may have
been accepted, provider response parse failure, process crash during an
attempt or missing completion readback makes the reservation indeterminate.
The complete maximum amount is conservatively committed, the envelope becomes
`ReconciliationRequired`, and the same operation is not retried. Any later
reconciliation requires separate authority and independent evidence; it may
record a correction but may not rewrite the historical admission or silently
restore capacity.

Retry is allowed only by the owning provider policy for a transient,
idempotent failure and only when durable evidence proves that the original
attempt did not cross the chargeable provider boundary. It reuses the same
`providerRequestId`; it never creates another reservation or bypasses a trip.

### Explicit rearming

Rearming is a local administrative control, not a public endpoint and not a
side effect of application start-up, readiness, credential availability,
provider recovery or elapsed time. A rearm request must include:

- an exact rearm authority reference;
- authenticated local actor identity and bounded reason;
- expected envelope and ledger revisions;
- the new runtime-session identity;
- the unchanged aggregate limit, allocations, cost schedule and expiry; and
- acknowledgement of committed, reserved and indeterminate totals.

The control persists and reads back one rearm record before it can return
success. Rearming never increases a limit, changes a model or provider, resets
committed use, releases a reservation, resolves an uncertain attempt, extends
expiry or edits history. `ReconciliationRequired` cannot be rearmed until each
indeterminate attempt receives an authorised disposition. `Exhausted` needs a
new budget-envelope authority rather than a rearm.

No operational envelope is implemented. Its absence is therefore treated as
`Disarmed`, with an effective aggregate limit of zero and zero operation
allocations. Neither preparation nor acceptance of this ADR changes
that value.

### Readiness, audit and disclosure

Liveness remains independent of the ledger. Readiness performs no provider
call and exposes only a sanitised capability state such as `Disarmed`,
`Armed`, `Tripped`, `Exhausted`, `ReconciliationRequired` or `Expired`. It does
not expose limits, remaining amount, account scope, price schedule, actor,
request IDs or cost details publicly.

The protected audit may record envelope/revision IDs, operation class,
authority reference, request digest, maximum reservation, state transition,
sanitised outcome and UTC instants. It must not contain credential material,
questions, passages, answers, provider payloads, local paths or personal
identifiers beyond the approved local actor reference.

### Compatibility and implementation sequence

The boundary is internal. OpenAPI v1 and v2, query payloads and public response
semantics remain unchanged. The decision does not reinterpret the frozen
provider-candidate campaign or any provider-side account control.

Separate sequential authorities remain required for:

1. the exact internal persistence schema and migration, including crash-safe
   uniqueness and transaction semantics;
2. typed Application contracts and a fake deterministic ledger;
3. Infrastructure persistence, administrative rearming and provider-adapter
   integration;
4. focused failure, replay, concurrency, crash and restart tests;
5. an independent security review and Automatic Quality Gate;
6. exact cost-schedule and zero-to-nonzero budget decisions; and
7. any credential lookup, provider egress, billing or real execution.

Acceptance does not authorise any item in this sequence.

## Alternatives considered

### Process-local counter

Rejected. Restart, crash or multiple instances can reset or diverge the
remaining amount, and an uncertain request cannot be recovered safely.

### Provider-side spend limit only

Rejected as the product admission boundary. It is external, may be delayed,
does not partition the three operation authorities and is not durable local
evidence before credential lookup.

### Record cost only after a response

Rejected. Concurrent requests can exceed the budget before any response is
recorded, and a lost response creates unbounded uncertainty.

### Automatically rearm after restart or cooldown

Rejected. It recreates spending authority without a current human action and
can repeat an attempt whose remote outcome is unknown.

### Shared pool without operation allocations

Rejected. A high-volume indexing operation could consume the authority
intended for query embedding or grounded generation. Strict allocations and
one aggregate limit preserve both least privilege and a complete ceiling.

### Keep all operational budgets at zero

Retained as the current safe fallback until a nonzero operational envelope is
separately authorised and implemented. Provider capability stays disarmed and
no provider request is admitted.

## Consequences and risks

### Positive

- Budget cannot reset silently across a crash or restart.
- Concurrent instances share one durable admission authority.
- Unknown remote outcomes reduce capacity and stop further spend.
- Operation-specific provider authority remains enforceable.
- Readiness can report capability without a billable probe.

### Costs and residual risks

- Admission adds a durable transaction and readback before every provider
  attempt.
- Availability of the provider path now depends on control-plane integrity.
- A conservative maximum commitment can underuse the authorised budget after
  an ambiguous failure.
- Exact charge calculation still depends on a separately verified immutable
  cost schedule and bounded provider usage semantics.
- Persistence corruption, clock drift and restore consistency require their
  own tests and operational recovery evidence.

## Acceptance record and implementation stop conditions

The owner accepted the complete recorded boundary through the exact decision
`ADR-0018: ACEITAR.` on clean
`main@89be70aba4de556611c9bdda8da62d1d4f9a1e41`, corpus `4.17.2`, with both
protected OpenAPI identities unchanged. The accepted architecture includes:

- strict per-operation allocations under one aggregate limit;
- conservative maximum reservation before egress;
- maximum commitment plus `ReconciliationRequired` for uncertain outcomes;
- explicit rearming for every new runtime session; and
- the invariant that rearming does not restore or increase budget.

This acceptance establishes architecture authority only. It accepts no risk,
arms no budget and authorises no price, account fact, nonzero operational
value, credential, provider call, public contract change, persistence schema,
migration, implementation or executable test. The operational provider budget
remains exactly zero and disarmed. No external price was consulted.

Any later design or implementation must stop if it would require an inferred
price or account fact, automatic rearm, floating-point money, unbounded retry
or weaker treatment of an uncertain outcome.

Implementation must later stop if atomic reservation cannot be proved across
the supported concurrency and restore boundaries, or if any path can reach
credential lookup or provider egress without a durable admitted reservation
and a matching operation-specific grant.
