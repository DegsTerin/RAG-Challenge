# ADR-0003 — Product and Technical Naming

- Status: accepted
- Date: 2026-07-30
- Accepted: 2026-07-30
- Decision authority: product-owner request, `Gostaria de mudar o nome do
  projeto de Challenge para RAG-Challenge`
- Owners: RAG-Challenge architecture / product owner
- Supersedes:
  [ADR-0001 — Runtime Stack and Modular Monolith](ADR-0001-Runtime-Stack-And-Modular-Monolith.md)

## Context

The product owner explicitly requested that the project name change from
`Challenge` to `RAG-Challenge`. ADR-0001 had already accepted `Challenge.*`
as the .NET project and namespace family during bootstrap.

That request is the explicit human decision for the canonical project
identity. `RagChallenge` is the syntax-safe, PascalCase implementation of the
same identity in C# and configuration, not a second public product name or a
separate lifecycle decision.

The public name contains a hyphen, which is not valid in a C# namespace.
The repository therefore needs an explicit mapping between public identity,
filesystem artefacts and language-specific identifiers. The change occurs
while `STATE-01 PROJECT_SETUP` is active, before a functional API, public
release or external consumer contract exists.

## Decision

- Supersede ADR-0001 as the current bootstrap decision record. Incorporate
  every non-naming decision from ADR-0001 by reference without modification,
  including the runtime stack, modular-monolith boundary, dependency
  direction, toolchains, tests, administration mode and negative scope.
- Use `RAG-Challenge` as the canonical product and repository name.
- Use `RAG-Challenge.sln` as the solution filename.
- Use `RagChallenge` as the .NET project, assembly, root namespace and
  configuration-section prefix.
- Use these physical project names:

  ```text
  RagChallenge.Domain
  RagChallenge.Application
  RagChallenge.Infrastructure
  RagChallenge.Server.Api
  RagChallenge.Dashboard.Web

  RagChallenge.UnitTests
  RagChallenge.Architecture.Tests
  RagChallenge.IntegrationTests
  ```

- Use `rag-challenge-dashboard-web` as the private npm package name.
- Use `<rag-challenge-root>` as the current non-secret documentation
  placeholder for the repository root.
- Preserve the stable `CH-MOD-*` module identifiers and `CH_*` error-code
  family. They are established contract identifiers, not display names.
- Preserve historical uses of `Challenge` in accepted evidence, append-only
  history and references to the Alura/ONE Challenge materials.
- Preserve `reference-materials/challenge-original/` as local-only
  provenance. Its path and contents are not renamed.
- Replace only ADR-0001's product, solution, project, assembly, namespace and
  configuration naming provisions. Its other decisions remain current
  through their incorporation above.
- Treat the physical checkout directory and any future GitHub or OCI resource
  name as external environment concerns. This decision does not rename or
  create an external resource.

## Alternatives

### Keep `Challenge.*` as the internal prefix

Rejected because the setup has not reached its Human Gate and no published
binary or API compatibility obligation requires a permanent mismatch between
the public identity and the technical surface.

### Use `RAGChallenge.*`

Rejected in favour of `RagChallenge.*`, which applies .NET PascalCase
conventions to an acronym longer than two characters.

### Use `RAG-Challenge.*` everywhere

Rejected because the hyphen is invalid in C# namespaces and would require
inconsistent overrides across projects and source files.

## Consequences

- Existing local project, assembly, namespace and configuration names change
  incompatibly, but no released consumer contract requires a compatibility
  shim.
- ADR-0001 becomes `superseded`; it remains the historical source for all
  incorporated non-naming decisions.
- The solution, project references, tests, setup scripts, lockfiles and
  onboarding documentation must migrate together.
- Historical evidence continues to show the names that were true when that
  evidence was recorded.
- A later public repository or deployment should use the canonical product
  identity, but each external mutation still requires its own authority.

## Security and operations

- The rename introduces no secret, credential, external network destination
  or data flow.
- Runtime preflight remains based on verified process ownership rather than a
  generic old or new process name.
- Generated outputs from the old assembly names must not be treated as source
  evidence and should be recreated from the renamed baseline.

## Compatibility and migration

- Rename the solution, project directories and `.csproj` files.
- Rename .NET namespaces, assembly expectations, `InternalsVisibleTo`,
  configuration keys and repository-root discovery.
- Regenerate the .NET lockfiles without changing third-party package
  versions.
- Rename the private npm package and verify its existing lockfile.
- Repeat the complete offline setup gate and a clean-clone reproduction
  before presenting the `STATE-01` Human Gate.

## Acceptance checks

- `RAG-Challenge.sln` contains exactly the seven approved .NET projects.
- Production references still point inwards according to ADR-0001.
- All compiled product assemblies and namespaces use `RagChallenge.*`.
- Configuration fails closed under
  `RagChallenge:Setup:AllowExternalServices`.
- Dashboard lint, tests, typecheck and build pass with the renamed package.
- Historical evidence, stable `CH-MOD-*`/`CH_*` identifiers and local-only
  reference materials remain unchanged.
- The repository audit, offline CI gate, health smoke and clean-clone
  reproduction pass without external product services.
