# STATE-04 Backend Implementation Report

## Purpose and boundary

This report is the durable, sanitised record for `STATE-04
BACKEND_IMPLEMENTATION`. It records the dependency assessment completed by
`S04-A0`, the selected parsing candidates, the accepted limitations and the
precondition that stopped the first implementation increment. It is evidence,
not an independent authority to install a package, execute a later state or
perform an external action.

Temporary packages, raw catalogue and registration responses, restored
assemblies, caches, launchers, raw logs and local paths remain outside the
repository. They must remain preserved until the owner separately authorises
their cleanup.

## Baseline and authority

- Initial baseline: branch `main`, commit
  `fe6c9028f061a7f0a98fc3debecffb0de3ad69bc`, corpus `4.9.2`, clean working
  tree.
- State position: `STATE-04 BACKEND_IMPLEMENTATION` active.
- Owner authority dated 2026-08-04: close `S04-A0`, pin the selected parsers,
  execute `S04-A` through `S04-D` sequentially and then execute the
  `STATE-04` Automatic Quality Gate.
- Negative authority: no network, external provider, account, secret, real
  corpus, real official product source, GitHub, OCI, Dashboard, DB-Notifier,
  publication, deployment, Human Gate or entry into a later lifecycle state.
- Runtime preflight for this documentation-only increment:
  `NOT_APPLICABLE`; no product process or listener was inspected or started.

## S04-A0 outcome

`S04-A0` is closed for local exploratory development with the following
selection:

- `PdfPig` `0.1.15`: selected PDF parser candidate.
- `CsvHelper` `33.1.0`: selected and sole CSV parser candidate.
- `Sylvan.Data.Csv` `1.4.4`: retained only as a non-selected fallback; it must
  not be referenced, restored or installed under the current authority.
- OpenAI integration: direct HTTP adapter as permitted by ADR-0005; no
  `OpenAI` or `System.ClientModel` package is selected.

The selection is conditional on the independent gates below. It is suitable
only for local `STATE-04` work and is not a production approval.

## Package identity and hash record

| Package | Bytes | SHA-256 of nupkg | Published/raw nupkg SHA-512 (Base64) | Signed/lock content hash (Base64) |
| --- | ---: | --- | --- | --- |
| `PdfPig` `0.1.15` | 11,867,960 | `D35768B69F86CD06CA14B849B8DF3673FD9D2DE3014C391E7C1E69E6B21C59C8` | `M5PHyQHujFuKMOuLQhyp9LNQz36E6r/qrCl86B/YCwM7gCxjW4IFQTxzOnbdoi1BVuvA2LFCJV93+TjjfEtKpg==` | `Bf0NO4o2ZSVnemMyj21KDU1sfSgmamFC0pD4aAv19CDNXQsoO1Z6O99gNqpiK/XWWZ5d5i7JkQoT7PUBxEPz5g==` |
| `CsvHelper` `33.1.0` | 903,000 | `A447D7E28EB7C6EA48EEEBB07BAA9AD393A77B4A49ED46F3CC9A8B8784A18EEF` | `1ZVz2+JluZlqNhJWK2K5CkFKv2MVUmCmmwbZl1XkB6sqdtRoyEJlM+HjT5SGD5IzbUjUew0ZLD2PM5HnebSdeA==` | `kqfTOZGrn7NarNeXgjh86JcpTHUoeQDMB8t9NVa/ZtlSYiV1rxfRnQ49WaJsob4AiGrbK0XDzpyKkBwai4F8eg==` |
| `Sylvan.Data.Csv` `1.4.4` (fallback only) | 154,727 | `809816C0E83533255EE1CA99E177C038537F657ACB933380D2BDB5814D32A814` | `/HQ5JgAeNtlu+dRYJINl+wF1SBw1FzEy6scxQZSO/9CinAZ83b1fT3bYzDaS8CeMdh0O1MfsR/gLOW4QcZ6Bnw==` | `1xZkDQ29uW2Y37NMN2nNkD59pFv8icaiu1WGmbE98m9n6jNL3IO3aM3rK4Uf6RgI+BFKbf7S5DdZA6nu5g9COw==` |

The nupkg sizes and raw hashes were recalculated from the preserved inputs
before this report was written. The independent operational gates are:

1. `RAW_NUPKG_HASH`: the SHA-512 of all nupkg bytes must match the published
   package hash.
2. `CACHE_NUPKG_HASH`: the isolated cache `.nupkg.sha512` value must match
   `RAW_NUPKG_HASH`.
3. `SIGNED_CONTENT_HASH`: the value preserved by offline signature
   verification remains a distinct domain.
4. `LOCK_CONTENT_HASH`: `packages.lock.json` `contentHash` must match
   `SIGNED_CONTENT_HASH`.
5. `SIGNATURE`: the result remains
   `CONDITIONAL_REVOCATION_NOT_CURRENT` because revocation freshness was not
   available offline.
6. `GRAPH`: package identity, exact version, selected target-framework asset
   and applicable dependency graph must remain exact, with no applicable
   transitive package for either selected parser.

## Evidence summary

### Observed locally

- Structural inspection found bounded, safe archive paths and the expected
  NuSpec identities and versions. Licence metadata was reviewed; the
  packaged Sylvan licence was separately checked only because it remained a
  candidate at that time.
- Offline `dotnet nuget verify --all` completed with exit code zero for all
  three candidates. Current revocation status was not established offline.
- Disposable locked restores completed in the corrected `S1-R` evidence.
  The selected assets were `lib/net8.0` for PdfPig, `lib/net9.0` for
  CsvHelper and `lib/net6.0` for the non-selected Sylvan fallback; no
  applicable transitive package was observed.
- Metadata-only inspection, Release `net10.0` compilation and
  framework-dependent `linux-arm64` cross-publish completed in `S1-C1` for
  the exploratory harnesses. No unexpected native import or module
  initialiser was observed. This did not execute the parser assemblies on
  Linux ARM64.
- Controlled runtime testing in `S1-C2` did not reach package loading. The
  attempts were blocked before package resolution by
  `NuGet.Configuration.ConfigurationDefaults` when the child environment was
  cleared too aggressively. This is an orchestration limitation, not an
  observed parser incompatibility.

### Primary-source research and accepted limitation

The four authorised Microsoft Learn pages were retrieved successfully and
reviewed within the fixed allowlist. They documented catalogue and
registration package hashes and PackageReference/cache behaviour, but did not
fully define the relationship among `packages.lock.json` `contentHash`, the
signed content hash and `.nupkg.sha512` for the local observations.

The owner accepted that incomplete primary-source definition exclusively for
local `STATE-04` development. This acceptance does not establish normative
NuGet semantics, does not approve production use and does not remove
`CONDITIONAL_REVOCATION_NOT_CURRENT`.

## First implementation gate

Before parser adapters can be accepted in `S04-A`, deterministic synthetic
runtime tests must cover:

- one-page and two-page valid PDFs, a truncated PDF and a pre-parser
  oversized PDF;
- quoted UTF-8 CSV, the literal cell `=1+1`, an unterminated quote and a
  pre-parser oversized CSV;
- bounded input before parser invocation, sanitised failures, no arbitrary
  path access, literal preservation and no unexpected assembly, dependency or
  side effect.

A valid-case failure requires rollback of the pin and stops the sequence. If
a malformed input is accepted, the owning adapter requires an explicit guard.
CsvHelper must not be replaced automatically by Sylvan.

## Blocking precondition discovered before pin

The preserved offline source contains the three parser nupkgs and their
metadata, but it does not contain the repository's already locked .NET
dependency set. An isolated product restore would therefore require at least
one of the following currently unauthorised actions:

- read-only seeding of a task-isolated cache from the existing global NuGet
  cache; or
- an exact, separately authorised offline feed or download operation.

Network use and global-cache access are both prohibited by the current
authority. Consequently:

- no parser PackageReference or central version pin was added;
- no product lockfile was changed;
- no `dotnet` restore, build, test or parser loading was started;
- no implementation in `S04-A`, `S04-B`, `S04-C` or `S04-D` was started;
- the `STATE-04` Automatic Quality Gate remains pending.

Attempting the restore with the known incomplete source would not produce
useful evidence and could not meet the owner's isolation gate. Execution must
resume only after the offline dependency source is complete under explicit
authority and the initial baseline is reconciled.

## Retention and risk

- Preserve all temporary `S04-A0` evidence until separate cleanup authority.
- Never version nupkgs, raw catalogues, raw registrations, caches, restored
  assemblies, launchers, temporary logs or local paths.
- The retained evidence is exploratory and workstation-local. It does not
  prove current online revocation, Linux ARM64 runtime behaviour, production
  suitability, parser quality over a real corpus or provider behaviour.
- The consolidated `S04-A` to `S04-D` authority remains unconsumed. Its
  sequence may resume after the dependency-source precondition is resolved;
  no later state or Human Gate is implied.
