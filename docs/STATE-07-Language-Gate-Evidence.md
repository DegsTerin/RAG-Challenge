# STATE-07 Integrated Language Gate Evidence

## Purpose and boundary

This report records the exceptional manual dispositions, preserved mechanical
failures, focused corrections and single approved offline execution of
`H-GATE-LANGUAGE-001`.

It does not replace or reclassify any earlier execution. It does not constitute
the `STATE-07` Human Gate, product homologation, a lifecycle transition,
publication, deployment or provider authority.

## Sealed candidate identity

| Fact | Value |
| --- | --- |
| Integration baseline | `e2ae23f8870ac2f447a9bfa09812288c7dcf3f66` |
| Corrective baseline | `4b6f9fc04bcd418eb5394f6f5f35c08a74bf1deb` |
| Corrective commit | `fcbab59ec2063a3dcbc7bb12ad8d869b230c66d3` |
| Sealing commit | `e80d10a29738fa7a042286c549687d08b2fe1dea` |
| Sealed tree | `3bd61746ee14f4c594e421a8f419a6506b117f4b` |
| Tracked files | `445` |
| Virtual manifest SHA-256 | `8ec9e2b98e5e6db0365282b57fd993afcf31d248c841ecb817b9853a869d7a57` |
| Branch | `codex/lang-mr-001-integrate-e2ae23f` |
| Gate mode | local, sequential and offline |

## Preserved execution history

1. The first gate against the earlier sealed candidate passed all 107 language
   tests and exited `1` because explicit commit-base and commit-head
   configuration were supplied together. This remains a configuration failure.
2. The separately authorised corrected retest of that earlier candidate used
   only its commit base and exited `0`.
3. The first integrated-candidate identity aggregator encountered a PowerShell
   `ParserError`. It did not classify the candidate and was not repeated.
4. Independent review of
   `e2ae23f8870ac2f447a9bfa09812288c7dcf3f66..4b6f9fc04bcd418eb5394f6f5f35c08a74bf1deb`
   returned `MANUAL_REVIEW_PASS` with zero P0-P3.
5. The first integrated gate passed all 107 language tests and exited `1`
   because a private identifier in
   `eng/Invoke-LocalArtefactRetention.ps1` used an unallowlisted legacy
   spelling.
6. Focused correction exposed and preserved two further checker failures: one
   truncated context hash and one current canonical storage literal in
   `prompts/state/Current-State.md` without an exact allowance.
7. The private identifier was corrected to British English. Canonical storage
   literals remained unchanged and were bound by exact path, classification,
   value, occurrence count and context hash.
8. Independent review of
   `4b6f9fc04bcd418eb5394f6f5f35c08a74bf1deb..e80d10a29738fa7a042286c549687d08b2fe1dea`
   returned `MANUAL_REVIEW_PASS` with zero P0-P3.
9. The two non-overlapping manual dispositions together cover the complete
   integrated candidate through the sealing commit. They accept no risk and do
   not erase any earlier result.

## Approved canonical execution

The command

`pwsh -NoProfile -File .\eng\ci.ps1 -Offline`

ran exactly once with
`RAG_LANGUAGE_COMMIT_BASE=fcbab59ec2063a3dcbc7bb12ad8d869b230c66d3`
and without `RAG_LANGUAGE_COMMIT_HEAD`. It exited `0`.

Observed results:

- language-policy tests: 107 passed;
- immutable language checker: 445 tracked files, zero migration findings and
  one commit message;
- Release build: zero warnings and zero errors;
- .NET tests: 571 passed and zero failed;
- merged .NET coverage: 95.74% lines and 67.30% branches;
- Dashboard: lint, typecheck, 45 tests and production build passed;
- orchestrator: 105 passed, zero failed and two environment-bound symlink
  cases skipped;
- orchestrator coverage: 82.12% lines, 76.71% branches and 88.78% functions;
- repository audit: 445 non-ignored files passed; and
- Dashboard, orchestrator and .NET online dependency audits remained
  `NOT_RUN` because registry metadata is unavailable in offline mode.

The final post-check confirmed the same branch, HEAD, tree and manifest, a
clean worktree, absent commit-boundary environment variables and no remaining
RAG-Challenge-owned process.

## Disposition

`H-GATE-LANGUAGE-001` is `APPROVED` for the sealed candidate identified above.
The candidate has not been integrated into `main`.

Real provider-backed homologation, online dependency audits, the `STATE-07`
Human Gate, publication, deployment and lifecycle transition remain separate
and unexecuted.
