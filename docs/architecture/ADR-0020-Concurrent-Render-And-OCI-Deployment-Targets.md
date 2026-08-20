# ADR-0020: Concurrent Render and OCI Deployment Targets

## Status

`accepted`

## Date

2026-08-20

## Accepted

2026-08-20

## Owners

- Product owner: RAG-Challenge owner
- Architecture owner: RAG-Challenge
- Technical owners: deployment, security and runtime operations

## Decision authority and baseline

- Decision authority: the owner's explicit 2026-08-20 instruction,
  `Então apenas documente a decisão de usar Render e OCI`.
- Branch: `main`
- Commit: `eccffff56abbd23d37378a5bde7a76d2a1d06bc9`
- Prompt corpus before this decision: `4.17.6`
- Lifecycle position: `STATE-07 TESTING_HOMOLOGATION`; unchanged
- Runtime preflight: `NOT_APPLICABLE` for this documentary decision
- Protected OpenAPI v1 SHA-256:
  `d6a686b94c926914beb28b437f464430a01de6560c2e2d476cf5c36025813e34`
- Protected OpenAPI v2 SHA-256:
  `f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733`

## Context

[ADR-0005](ADR-0005-MVP-Providers-Persistence-And-OCI-Deployment.md)
selects OCI as the conditional durable MVP deployment target. Separately
authorised `STATE-07` evidence records one public Render Hobby/Free deployment
with one instance, no persistent disk and no Render database. That Render
deployment is public homologation evidence, not production or satisfaction of
the OCI requirement.

The owner has decided to retain both hosting providers and intends them to be
available concurrently after an independently authorised OCI deployment. This
decision records the target operating shape without claiming that OCI has been
inspected, configured or deployed.

## Decision

1. Retain Render as a secondary public deployment and homologation surface.
2. Retain OCI as the durable MVP deployment target selected by ADR-0005.
3. Permit both independently deployed instances to remain available at the
   same time once the OCI instance has been separately authorised, implemented
   and verified.
4. Keep each instance isolated: separate runtime configuration, secrets,
   storage, mutable state, operational evidence and deployment lifecycle.
5. Do not share, concurrently mount or live-replicate SQLite databases,
   content-store objects or vector-store files between Render and OCI.
6. Do not infer active-active writes, automatic failover, traffic management,
   cross-provider replication or a single shared recovery set. Any such
   capability requires a later ADR and its own implementation and operational
   evidence.
7. Present the two endpoints according to their evidenced roles. Availability
   or health in one environment does not prove equivalent data, readiness,
   recovery, security controls or provider authority in the other.

## Relationship to existing decisions

This ADR supplements ADR-0005; it does not supersede its conditional OCI
compute, durable storage, backup, IAM, secret, egress or cost controls. Render
does not replace the OCI requirement. ADR-0006 security controls and
ADR-0018/ADR-0019 corrective boundaries remain unchanged.

The OCI region remains unresolved for execution. ADR-0005 records
`sa-saopaulo-1` as a conditional candidate. Merely opening an authenticated OCI
console in another region neither changes that candidate nor proves tenancy
capacity, entitlement, IAM, billing or deployment readiness. Region selection
and tenancy inspection require separate authority and evidence.

## Consequences

- The Render and OCI instances may intentionally expose different operational
  state until an independently designed synchronisation mechanism exists.
- A deployment, update, rollback or incident in one provider does not
  automatically mutate the other.
- Each environment requires its own sanitised readiness, security, persistence,
  backup, restore, cost and rollback evidence before its role can be expanded.
- Public documentation must distinguish the already observed Render deployment
  from the still-unverified OCI deployment.
- `SEC-CORR-002` visual containment remains the first recorded corrective
  implementation candidate. This deployment decision neither postpones nor
  authorises it.

## Alternatives considered

### Replace Render with OCI

Not selected. The owner chose to retain the existing Render surface while OCI
is added under separate authority.

### Replace OCI with Render

Not selected. Render does not replace the OCI delivery requirement or the
durable target selected by ADR-0005.

### Shared active-active state

Not selected. The current SQLite, filesystem content store and local vector
store do not provide a safe cross-provider concurrent-write design.

## Verification and negative scope

This decision was recorded from the clean local documentary baseline. No OCI
or Render account, console, API, service, deployment, network endpoint, secret,
billing surface or runtime was accessed or changed. No code, test, schema,
migration, dependency, workflow, OpenAPI contract, branch or worktree changed.
No Automatic Quality Gate, Human Gate or lifecycle transition was executed.
