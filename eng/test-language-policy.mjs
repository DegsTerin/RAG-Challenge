// Purpose: Proves the en-GB policy, migration debt and commit-history boundaries with synthetic local fixtures only.
import assert from "node:assert/strict";
import { createHash, randomUUID } from "node:crypto";
import { mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import test from "node:test";
import {
  assertDebtMatches,
  assertSafeCommitMessage,
  canonicalJson,
  extractProse,
  inspectCommitMessage,
  inspectRepository,
  inspectRegions,
  isProtectedLanguageControlPath,
  runCheck,
  sha256,
  validateBaselineDocument,
  validatePolicyDocument,
} from "./check-language.mjs";

const sourceRoot = resolve(dirname(fileURLToPath(import.meta.url)));
const repositoryPolicyDocument = JSON.parse(await readFile(join(sourceRoot, "language-policy.json"), "utf8"));
const bannedAmericanSpellings = repositoryPolicyDocument.payload.bannedAmericanSpellings;
const policySchemaDigest = `sha256:${sha256(await readFile(join(sourceRoot, "language-policy.schema.json")))}`;
const baselineSchemaDigest = `sha256:${sha256(await readFile(join(sourceRoot, "language-migration-baseline.schema.json")))}`;

function digestDocument(schema, schemaDigest, payload) {
  return { $schema: schema, schemaDigest, payload, digest: `sha256:${sha256(canonicalJson(payload))}` };
}

function contextHashesForValue(text, value, kind, dominantLiterals = []) {
  const dominantRanges = dominantLiterals.flatMap((literal) => {
    const ranges = [];
    let offset = 0;
    while ((offset = text.indexOf(literal, offset)) >= 0) {
      ranges.push({ start: offset, end: offset + literal.length });
      offset += literal.length;
    }
    return ranges;
  });
  const ranges = [];
  let offset = 0;
  while ((offset = text.indexOf(value, offset)) >= 0) {
    const range = { start: offset, end: offset + value.length };
    const before = range.start === 0 ? "" : text[range.start - 1];
    const after = range.end === text.length ? "" : text[range.end];
    const exactIdentifier = kind !== "IDENTIFIER" || (!/[A-Za-z0-9_]/.test(before) && !/[A-Za-z0-9_]/.test(after));
    if (exactIdentifier && !dominantRanges.some((dominant) =>
      range.start >= dominant.start && range.end <= dominant.end)) ranges.push(range);
    offset = range.end;
  }
  return ranges.map((range) => {
    const currentStart = text.lastIndexOf("\n", Math.max(0, range.start - 1)) + 1;
    const currentEndIndex = text.indexOf("\n", range.end);
    const currentEnd = currentEndIndex < 0 ? text.length : currentEndIndex;
    const previousStart = currentStart === 0 ? 0 : text.lastIndexOf("\n", Math.max(0, currentStart - 2)) + 1;
    const nextEndIndex = text.indexOf("\n", currentEnd + 1);
    const nextEnd = currentEndIndex < 0 ? currentEnd : (nextEndIndex < 0 ? text.length : nextEndIndex);
    return sha256(text.slice(previousStart, nextEnd));
  });
}

// SYNTHETIC_LANGUAGE_POLICY_PAYLOAD_START
function policyPayload(overrides = {}) {
  const canonicalText = canonicalFixture();
  return {
    schemaVersion: 2,
    policyId: "rag-challenge-language-policy-v2",
    technicalLanguage: "en-GB",
    ownerLanguage: "pt-BR",
    bannedAmericanSpellings,
    portugueseTechnicalMarkers: ["alteração", "arquivo", "implementação", "segurança", "validação"],
    binaryPaths: [{
      path: "image.bin",
      classification: "HISTORICAL_VISUAL_EVIDENCE",
      reason: "Synthetic binary classification.",
      sha256: sha256(new Uint8Array([0xff, 0xfe, 0xfd])),
    }],
    immutableTextPaths: [
      {
        path: "citation.json",
        classification: "SOURCE_OR_CITATION_DATA",
        reason: "Synthetic source-language citation data.",
        sha256: sha256('{"text":"Alteração citada pela fonte"}\n'),
      },
      {
        path: "historical.md",
        classification: "HISTORICAL_EVIDENCE",
        reason: "Synthetic protected historical data.",
        sha256: sha256("Historical OPENAI_API_KEY identifier.\n"),
      },
    ],
    productCredentialIdentifierAllowances: [
      { path: "security.md", classification: "SECURITY_POLICY", sha256: null },
      {
        path: "historical.md",
        classification: "PRESERVED_HISTORICAL_DOCUMENT",
        sha256: sha256("Historical OPENAI_API_KEY identifier.\n"),
      },
    ],
    canonicalIdentifierAllowances: [
      {
        path: "canonical.ts",
        classification: "CANONICAL_CONTRACT_IDENTIFIER",
        kind: "IDENTIFIER",
        value: "LogicalArtifactDigest",
        occurrences: 1,
        contextHashes: contextHashesForValue(canonicalText, "LogicalArtifactDigest", "IDENTIFIER"),
      },
      {
        path: "canonical.ts",
        classification: "PUBLIC_API_IDENTIFIER",
        kind: "IDENTIFIER",
        value: "artifacts",
        occurrences: 2,
        contextHashes: contextHashesForValue(canonicalText, "artifacts", "IDENTIFIER",
          ["task-owned-artifacts-local-v1", "artifacts-local"]),
      },
      {
        path: "canonical.ts",
        classification: "CANONICAL_STORAGE_LITERAL",
        kind: "LITERAL",
        value: "artifacts-local",
        occurrences: 1,
        contextHashes: contextHashesForValue(canonicalText, "artifacts-local", "LITERAL",
          ["task-owned-artifacts-local-v1"]),
      },
      {
        path: "canonical.ts",
        classification: "CANONICAL_POLICY_LITERAL",
        kind: "LITERAL",
        value: "task-owned-artifacts-local-v1",
        occurrences: 1,
        contextHashes: contextHashesForValue(canonicalText, "task-owned-artifacts-local-v1", "LITERAL"),
      },
      {
        path: "eng/language-policy.json",
        classification: "SYNTHETIC_ENFORCEMENT_IDENTIFIER",
        kind: "IDENTIFIER",
        value: "artifact",
        occurrences: 1,
        contextHashes: ["0".repeat(64)],
      },
      {
        path: "eng/language-policy.json",
        classification: "SYNTHETIC_ENFORCEMENT_IDENTIFIER",
        kind: "IDENTIFIER",
        value: "artifacts",
        occurrences: 1,
        contextHashes: ["0".repeat(64)],
      },
    ],
    excludedRegions: [
      {
        path: "eng/language-policy.json",
        classification: "SYNTHETIC_ENFORCEMENT_REGION",
        startMarker: '    "canonicalIdentifierAllowances": [',
        endMarker: '    ],\n    "excludedRegions": [',
        sha256: "0".repeat(64),
      },
      {
        path: "locale.ts",
        classification: "FUNCTIONAL_LOCALISATION_REGION",
        startMarker: '  "pt-BR": {',
        endMarker: '  "en-GB": {',
        sha256: sha256('  "pt-BR": { label: "Alteração do arquivo" },\n  "en-GB": {'),
      },
      {
        path: "handoff.md",
        classification: "OWNER_FACING_PT_BR_REGION",
        startMarker: "## Owner hand-off\n```text",
        endMarker: "Autorizo a alteração do arquivo.\n```",
        sha256: sha256("## Owner hand-off\n```text\nAutorizo a alteração do arquivo.\n```"),
      },
      {
        path: "preserved.md",
        classification: "PRESERVED_HISTORICAL_REGION",
        startMarker: "## Historical",
        endMarker: null,
        sha256: sha256("## Historical\nImplementação preservada.\n"),
      },
    ],
    appendOnlyPrefixes: [{ path: "history.md", prefixBytes: 1, sha256: sha256("h") }],
    ...overrides,
  };
}
// SYNTHETIC_LANGUAGE_POLICY_PAYLOAD_END

function policyDocument(payload) {
  const selfRegion = payload.excludedRegions.find((entry) => entry.path === "eng/language-policy.json");
  let draft = digestDocument("./language-policy.schema.json", policySchemaDigest, payload);
  let text = `${JSON.stringify(draft, null, 2)}\n`;
  let start = text.indexOf(selfRegion.startMarker);
  let end = text.indexOf(selfRegion.endMarker, start + selfRegion.startMarker.length) + selfRegion.endMarker.length;
  const masked = `${text.slice(0, start)}${text.slice(start, end).replace(/[^\n]/g, " ")}${text.slice(end)}`;
  for (const entry of payload.canonicalIdentifierAllowances.filter((allowance) =>
    allowance.path === "eng/language-policy.json" && allowance.kind !== "PATH")) {
    entry.contextHashes = contextHashesForValue(masked, entry.value, entry.kind);
  }
  draft = digestDocument("./language-policy.schema.json", policySchemaDigest, payload);
  text = `${JSON.stringify(draft, null, 2)}\n`;
  start = text.indexOf(selfRegion.startMarker);
  end = text.indexOf(selfRegion.endMarker, start + selfRegion.startMarker.length) + selfRegion.endMarker.length;
  selfRegion.sha256 = sha256(text.slice(start, end));
  return digestDocument("./language-policy.schema.json", policySchemaDigest, payload);
}

function boundPolicyPayload(overrides = {}) {
  return policyDocument(policyPayload(overrides)).payload;
}

function baselinePayload(policyDigest, findings = [], status = "IN_PROGRESS") {
  return { schemaVersion: 1, baselineId: "rag-challenge-en-gb-migration-v1", status, policyDigest, findings };
}

function childEnvironment() {
  const nullDevice = process.platform === "win32" ? "NUL" : "/dev/null";
  const environment = {
    GIT_CONFIG_NOSYSTEM: "1", GIT_CONFIG_GLOBAL: nullDevice, GIT_ATTR_NOSYSTEM: "1",
    GIT_TERMINAL_PROMPT: "0", GCM_INTERACTIVE: "Never", GIT_PAGER: "cat",
  };
  for (const name of ["PATH", "SystemRoot", "WINDIR", "TEMP", "TMP"]) {
    if (process.env[name] !== undefined) environment[name] = process.env[name];
  }
  return environment;
}

function git(root, ...arguments_) {
  const nullDevice = process.platform === "win32" ? "NUL" : "/dev/null";
  const fixedArguments = [
    "--no-optional-locks", "-c", `core.hooksPath=${nullDevice}`, "-c", `core.attributesFile=${nullDevice}`,
    "-c", `init.templateDir=${nullDevice}`, "-c", "core.fsmonitor=false", "-c", "credential.helper=", "-c", "core.askPass=", "-c", "protocol.allow=never",
    ...arguments_,
  ];
  const result = spawnSync("git", fixedArguments, {
    cwd: root, encoding: "utf8", env: childEnvironment(), shell: false, windowsHide: true, timeout: 30_000,
  });
  if (result.status !== 0) throw new Error("Synthetic Git command failed closed.");
  return result.stdout.trim();
}

async function guidTempDirectory(prefix) {
  const root = join(tmpdir(), `${prefix}${randomUUID()}`);
  await mkdir(root, { recursive: false });
  return root;
}

// SYNTHETIC_LANGUAGE_REPOSITORY_START
function canonicalFixture(extra = "") {
  return `export interface Contract { LogicalArtifactDigest: string; }\nexport function collect(artifacts: string[]) { return artifacts; }\nexport const root = "artifacts-local";\nexport const policy = "task-owned-artifacts-local-v1";\n${extra}`;
}

async function createRepository(initialMessage = "test(language): initialise synthetic policy") {
  const root = await guidTempDirectory("rag-challenge-language-policy-");
  await mkdir(join(root, "eng"), { recursive: true });
  await writeFile(join(root, "history.md"), "history\n", "utf8");
  await writeFile(join(root, "technical.md"), "British technical prose is authorised.\n", "utf8");
  await writeFile(join(root, "technical.config"), "# British configuration behaviour is authorised.\n", "utf8");
  await writeFile(join(root, "canonical.ts"), canonicalFixture(), "utf8");
  await writeFile(join(root, "image.bin"), new Uint8Array([0xff, 0xfe, 0xfd]));
  await writeFile(join(root, "security.md"), "The OPENAI_API_KEY identifier is synthetic policy data.\n", "utf8");
  await writeFile(join(root, "historical.md"), "Historical OPENAI_API_KEY identifier.\n", "utf8");
  await writeFile(join(root, "preserved.md"), "## Historical\nImplementação preservada.\n", "utf8");
  await writeFile(join(root, "handoff.md"), "## Owner hand-off\n```text\nAutorizo a alteração do arquivo.\n```\n", "utf8");
  await writeFile(join(root, "locale.ts"), 'export const copy = {\n  "pt-BR": { label: "Alteração do arquivo" },\n  "en-GB": { label: "File change" },\n};\n', "utf8");
  await writeFile(join(root, "citation.json"), '{"text":"Alteração citada pela fonte"}\n', "utf8");
  await writeFile(join(root, "eng", "language-policy.schema.json"), await readFile(join(sourceRoot, "language-policy.schema.json"), "utf8"), "utf8");
  await writeFile(join(root, "eng", "language-migration-baseline.schema.json"), await readFile(join(sourceRoot, "language-migration-baseline.schema.json"), "utf8"), "utf8");
  const policy = policyDocument(policyPayload());
  await writeFile(join(root, "eng", "language-policy.json"), `${JSON.stringify(policy, null, 2)}\n`, "utf8");
  git(root, "init", "--initial-branch=main");
  git(root, "config", "user.name", "Language Fixture");
  git(root, "config", "user.email", "language@example.invalid");
  git(root, "add", ".");
  git(root, "commit", "-m", initialMessage);
  const findings = await inspectRepository(root, policy.payload);
  const baseline = digestDocument("./language-migration-baseline.schema.json", baselineSchemaDigest, baselinePayload(policy.digest, findings));
  await writeFile(join(root, "eng", "language-migration-baseline.json"), `${JSON.stringify(baseline, null, 2)}\n`, "utf8");
  git(root, "add", "eng/language-migration-baseline.json");
  git(root, "commit", "-m", "test(language): record synthetic migration baseline");
  await writeFile(join(root, "technical.md"), "British technical prose remains authorised.\n", "utf8");
  git(root, "add", "technical.md");
  git(root, "commit", "-m", "test(language): establish ordinary synthetic head");
  await writeFile(join(root, "history.md"), "history\n", "utf8");
  return root;
}
// SYNTHETIC_LANGUAGE_REPOSITORY_END

test("policy and baseline manifests reject unknown fields, invalid schemas and digest mismatch", () => {
  const policy = digestDocument("./language-policy.schema.json", policySchemaDigest, policyPayload());
  assert.doesNotThrow(() => validatePolicyDocument(policy, policySchemaDigest));
  assert.throws(() => validatePolicyDocument({ ...policy, unexpected: true }, policySchemaDigest), /missing or unexpected/);
  assert.throws(() => validatePolicyDocument({ ...policy, digest: `sha256:${"0".repeat(64)}` }, policySchemaDigest), /digest does not match/);
  const unknownAllowancePayload = policyPayload({
    productCredentialIdentifierAllowances: [
      { path: "security.md", classification: "UNKNOWN", sha256: null },
    ],
  });
  assert.throws(() => validatePolicyDocument(
    digestDocument("./language-policy.schema.json", policySchemaDigest, unknownAllowancePayload),
    policySchemaDigest,
  ), /closed classification/);
  const wrongIdentifierKindPayload = policyPayload({
    canonicalIdentifierAllowances: [{
      path: "canonical.ts",
      classification: "CANONICAL_CONTRACT_IDENTIFIER",
      kind: "LITERAL",
      value: ["Logical", "Arti", "fact", "Digest"].join(""),
      occurrences: 1,
      contextHashes: ["0".repeat(64)],
    }],
  });
  assert.throws(() => validatePolicyDocument(
    digestDocument("./language-policy.schema.json", policySchemaDigest, wrongIdentifierKindPayload),
    policySchemaDigest,
  ), /closed classification, kind/);
  const unhashedHistoricalPayload = policyPayload({
    productCredentialIdentifierAllowances: [
      { path: "preserved.md", classification: "PRESERVED_HISTORICAL_DOCUMENT", sha256: null },
    ],
  });
  assert.throws(() => validatePolicyDocument(
    digestDocument("./language-policy.schema.json", policySchemaDigest, unhashedHistoricalPayload),
    policySchemaDigest,
  ), /exact digest/);
  const baseline = digestDocument("./language-migration-baseline.schema.json", baselineSchemaDigest, baselinePayload(policy.digest));
  assert.doesNotThrow(() => validateBaselineDocument(baseline, policy.digest, baselineSchemaDigest));
  assert.throws(() => validateBaselineDocument({ ...baseline, $schema: "wrong" }, policy.digest, baselineSchemaDigest), /schema reference/);
  assert.throws(() => validateBaselineDocument({ ...baseline, digest: `sha256:${"0".repeat(64)}` }, policy.digest, baselineSchemaDigest), /digest does not match/);
});

test("trusted schema bytes reject a preserved-identity constraint mutation", async () => {
  const root = await createRepository();
  try {
    const schemaPath = join(root, "eng", "language-policy.schema.json");
    const schema = JSON.parse(await readFile(schemaPath, "utf8"));
    schema.properties.schemaDigest.pattern = "^invalid$";
    await writeFile(schemaPath, `${JSON.stringify(schema, null, 2)}\n`, "utf8");
    await assert.rejects(runCheck({ repositoryRoot: root }), /schema reference|digest format/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

// SYNTHETIC_LANGUAGE_TRUST_TAMPER_START
test("trusted policy root defeats coordinated candidate policy, baseline and schema tampering", async () => {
  const trustedRoot = await createRepository();
  const candidateRoot = join(dirname(trustedRoot), `rag-challenge-language-candidate-${randomUUID()}`);
  try {
    git(trustedRoot, "worktree", "add", "-b", `candidate-${randomUUID()}`, candidateRoot, "HEAD");
    const candidatePolicyPath = join(candidateRoot, "eng", "language-policy.json");
    const candidatePolicy = JSON.parse(await readFile(candidatePolicyPath, "utf8"));
    candidatePolicy.payload.bannedAmericanSpellings = [{ american: "unused", british: "unusedb" }];
    candidatePolicy.digest = `sha256:${sha256(canonicalJson(candidatePolicy.payload))}`;
    await writeFile(candidatePolicyPath, `${JSON.stringify(candidatePolicy, null, 2)}\n`, "utf8");
    const candidateBaselinePath = join(candidateRoot, "eng", "language-migration-baseline.json");
    const candidateBaseline = JSON.parse(await readFile(candidateBaselinePath, "utf8"));
    candidateBaseline.payload.policyDigest = candidatePolicy.digest;
    candidateBaseline.payload.findings = [];
    candidateBaseline.digest = `sha256:${sha256(canonicalJson(candidateBaseline.payload))}`;
    await writeFile(candidateBaselinePath, `${JSON.stringify(candidateBaseline, null, 2)}\n`, "utf8");
    const candidateSchemaPath = join(candidateRoot, "eng", "language-policy.schema.json");
    const candidateSchema = JSON.parse(await readFile(candidateSchemaPath, "utf8"));
    candidateSchema.properties.payload = {};
    await writeFile(candidateSchemaPath, `${JSON.stringify(candidateSchema, null, 2)}\n`, "utf8");
    await writeFile(join(candidateRoot, "technical.md"), "This candidate changes behavior.\n", "utf8");
    await assert.rejects(
      runCheck({ repositoryRoot: candidateRoot, trustedPolicyRoot: trustedRoot }),
      /new or changed|canonical identifier allowance count/,
    );
  } finally {
    try { git(trustedRoot, "worktree", "remove", "--force", candidateRoot); } catch { /* The synthetic root removal remains authoritative. */ }
    await rm(candidateRoot, { recursive: true, force: true });
    await rm(trustedRoot, { recursive: true, force: true });
  }
});
// SYNTHETIC_LANGUAGE_TRUST_TAMPER_END

test("baseline regeneration is not exposed by the tracked checker", () => {
  const result = spawnSync(process.execPath, [join(sourceRoot, "check-language.mjs"), "--write-baseline"], {
    cwd: sourceRoot,
    encoding: "utf8",
    env: childEnvironment(),
    shell: false,
    windowsHide: true,
    timeout: 30_000,
  });
  assert.equal(result.status, 1);
  assert.match(result.stderr, /baseline regeneration is disabled/i);
});

test("CLI failures sanitise absolute paths and do not emit Git stderr", async () => {
  const root = await guidTempDirectory("rag-challenge-language-missing-");
  try {
    await writeFile(join(root, "fixture.md"), "British fixture prose.\n", "utf8");
    git(root, "init", "--initial-branch=main");
    git(root, "config", "user.name", "Language Fixture");
    git(root, "config", "user.email", "language@example.invalid");
    git(root, "add", "fixture.md");
    git(root, "commit", "-m", "test(language): create missing-policy fixture");
    const result = spawnSync(process.execPath, [join(sourceRoot, "check-language.mjs"), "--repository-root", root], {
      cwd: sourceRoot, encoding: "utf8", env: childEnvironment(), shell: false, windowsHide: true, timeout: 30_000,
    });
    assert.equal(result.status, 1);
    assert.equal(result.stderr.includes(root), false);
    assert.match(result.stderr, /Language policy FAIL/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("ambiguous catalog and license forms are reserved for semantic review", () => {
  assert.equal(bannedAmericanSpellings.some((entry) => ["catalog", "license"].includes(entry.american)), false);
});

test("every direct and transitive language-enforcement dependency is protected", () => {
  for (const path of [
    "tools/ai-orchestrator/src/security/secret-policy.ts",
    "tools/ai-orchestrator/src/security/path-policy.ts",
    "tools/ai-orchestrator/src/security/secure-json.ts",
    "tools/ai-orchestrator/src/adapters/bounded-process.ts",
    "tools/ai-orchestrator/src/security/git-process-policy.ts",
    "tools/ai-orchestrator/src/security/git-repository-policy.ts",
    "tools/ai-orchestrator/src/core/contracts.ts",
    "tools/ai-orchestrator/src/core/errors.ts",
    "tools/ai-orchestrator/src/core/validation.ts",
    "tools/ai-orchestrator/src/ports/candidate-inspector.ts",
    "tools/ai-orchestrator/src/ports/process-executor.ts",
    "tools/ai-orchestrator/test/adapters.test.ts",
    "tools/ai-orchestrator/test/cli.test.ts",
    "tools/ai-orchestrator/test/codex-app-server.test.ts",
    "tools/ai-orchestrator/test/core.test.ts",
    "tools/ai-orchestrator/test/security-boundaries.test.ts",
    "prompts/governance/Security-And-Access.md",
  ]) assert.equal(isProtectedLanguageControlPath(path), true, path);
});

for (const entry of bannedAmericanSpellings) {
  test(`American spelling '${entry.american}' fails and British '${entry.british}' passes`, () => {
    const payload = policyPayload();
    assert.equal(inspectCommitMessage(`docs(language): ${entry.american} technical prose`, payload).some((finding) => finding.token === entry.american), true);
    assert.equal(inspectCommitMessage(`docs(language): ${entry.british} technical prose`, payload).length, 0);
  });
}

// SYNTHETIC_LANGUAGE_SECRET_MESSAGES_START
test("raw commit messages reject credential identifiers, secret shapes and invalid decoding without echoing", () => {
  const payload = policyPayload();
  const identifier = "OPENAI_API_KEY";
  const syntheticSecret = "sk-proj-synthetic-not-a-real-secret";
  for (const value of [
    identifier,
    syntheticSecret,
    "Bearer synthetic-token-value",
    "-----BEGIN PRIVATE KEY-----",
    "PASSWORD=synthetic-value",
    "ghp_abcdefghijklmnopqrstuvwxyz123456",
    "invalid \uFFFD message",
  ]) {
    assert.throws(() => assertSafeCommitMessage(value), (error) =>
      error instanceof Error && !error.message.includes(value));
  }
  const text = "CH_INDEX_UNAVAILABLE https://example.invalid/color /src/color.ts 0123456789abcdef0123456789abcdef pt-BR en-GB";
  assert.deepEqual(inspectRegions("fixture.md", extractProse("fixture.md", text), payload), []);
});

test("CLI unknown-argument failures never echo synthetic secret-shape families", () => {
  for (const synthetic of [
    "OPENAI_API_KEY",
    "sk-proj-synthetic-not-a-real-secret",
    "Bearer synthetic-token-value",
    "-----BEGIN PRIVATE KEY-----",
    "PASSWORD=synthetic-value",
    "ghp_abcdefghijklmnopqrstuvwxyz123456",
  ]) {
    const result = spawnSync(process.execPath, [join(sourceRoot, "check-language.mjs"), synthetic], {
      cwd: sourceRoot, encoding: "utf8", env: childEnvironment(), shell: false, windowsHide: true, timeout: 30_000,
    });
    assert.equal(result.status, 1);
    assert.equal(result.stderr.includes(synthetic), false);
    assert.match(result.stderr, /Unknown language-policy argument/);
  }
});

test("product credential identifier allowances are exact, canonical and fail closed", async () => {
  const root = await createRepository();
  const identifier = ["OPENAI", "API", "KEY"].join("_");
  try {
    await assert.doesNotReject(inspectRepository(root, boundPolicyPayload()));
    await writeFile(join(root, "technical.md"), `The ${identifier} identifier is outside policy.\n`, "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), (error) =>
      error instanceof Error && /outside its closed allowlist/.test(error.message) && !error.message.includes(identifier));
    await writeFile(join(root, "technical.md"), "British technical prose remains authorised.\n", "utf8");
    await writeFile(join(root, "security.md"), "The openai_api_key identifier is not canonical.\n", "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /non-canonical product credential identifier/);
    await writeFile(join(root, "security.md"), "British security policy contains no identifier.\n", "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /unused path/);
    await writeFile(join(root, "security.md"), "The OPENAI_API_KEY identifier is synthetic policy data.\n", "utf8");
    await writeFile(join(root, "historical.md"), "Historical OPENAI_API_KEY identifier changed.\n", "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /protected historical credential document changed/);
  } finally { await rm(root, { recursive: true, force: true }); }
});
// SYNTHETIC_LANGUAGE_SECRET_MESSAGES_END

// SYNTHETIC_LANGUAGE_OWNER_PAYLOAD_START
test("unclassified owner-facing text fences and Portuguese technical prose fail", () => {
  const payload = policyPayload();
  const ownerPayload = "```text\nAutorizo a alteração do arquivo.\n```\n";
  assert.equal(inspectRegions("handoff.md", extractProse("handoff.md", ownerPayload), payload)
    .some((finding) => finding.ruleId === "PORTUGUESE_TECHNICAL_PROSE"), true);
  const findings = inspectRegions("technical.md", extractProse("technical.md", "A implementação exige validação técnica."), payload);
  assert.equal(findings.some((finding) => finding.ruleId === "PORTUGUESE_TECHNICAL_PROSE"), true);
});
// SYNTHETIC_LANGUAGE_OWNER_PAYLOAD_END

test("localisation regions and immutable source data are closed exact classifications", () => {
  const payload = policyPayload();
  assert.deepEqual(payload.immutableTextPaths.map((entry) => [entry.path, entry.classification]), [
    ["citation.json", "SOURCE_OR_CITATION_DATA"],
    ["historical.md", "HISTORICAL_EVIDENCE"],
  ]);
  assert.equal(payload.excludedRegions.some((entry) =>
    entry.path === "locale.ts" && entry.classification === "FUNCTIONAL_LOCALISATION_REGION"), true);
  assert.equal([...payload.immutableTextPaths, ...payload.excludedRegions].some((entry) => /[*?]/.test(entry.path)), false);
});

// SYNTHETIC_LANGUAGE_MIGRATION_DEBT_START
test("identical migration debt passes, changed or new debt fails, and COMPLETE cannot retain debt", () => {
  const policy = digestDocument("./language-policy.schema.json", policySchemaDigest, policyPayload());
  const finding = inspectRegions("technical.md", extractProse("technical.md", "A implementação exige validação técnica."), policy.payload)[0];
  const baseline = digestDocument("./language-migration-baseline.schema.json", baselineSchemaDigest, baselinePayload(policy.digest, [finding]));
  assert.doesNotThrow(() => assertDebtMatches([finding], baseline));
  const changed = { ...finding, line: finding.line + 1 };
  assert.throws(() => assertDebtMatches([changed], baseline), /new or changed/);
  const complete = digestDocument("./language-migration-baseline.schema.json", baselineSchemaDigest, baselinePayload(policy.digest, [finding], "COMPLETE"));
  assert.throws(() => validateBaselineDocument(complete, policy.digest, baselineSchemaDigest), /COMPLETE.*cannot retain debt/);
});
// SYNTHETIC_LANGUAGE_MIGRATION_DEBT_END

// SYNTHETIC_LANGUAGE_HISTORY_AND_COMMITS_START
test("all tracked text is inspected independently of filename extension", async () => {
  const root = await createRepository();
  try {
    await writeFile(join(root, "technical.config"), "# This configuration documents behavior.\n", "utf8");
    await assert.rejects(runCheck({ repositoryRoot: root }), /new or changed/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("binary classifications require an exact path, digest and non-text identity", async () => {
  const root = await createRepository();
  try {
    await writeFile(join(root, "image.bin"), new Uint8Array([0xff, 0xfe, 0xfc]));
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /Classified binary identity changed/);
    await writeFile(join(root, "image.bin"), "British text.\n", "utf8");
    const textBytes = await readFile(join(root, "image.bin"));
    const payload = boundPolicyPayload({
      binaryPaths: [{ ...policyPayload().binaryPaths[0], sha256: sha256(textBytes) }],
    });
    await assert.rejects(inspectRepository(root, payload), /contains valid UTF-8 text/);
    await writeFile(join(root, "image.bin"), new Uint8Array([0xff, 0xfe, 0xfd]));
    await writeFile(join(root, "unknown.bin"), new Uint8Array([0xff, 0xfe, 0xfb]));
    git(root, "add", "unknown.bin");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /not valid UTF-8/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("canonical identifier allowances reject new internal names, count drift and unclassified paths", async () => {
  const root = await createRepository();
  try {
    await writeFile(join(root, "technical.md"), "British prose names PrivateArtifact as a private helper.\n", "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /outside the canonical allowlist/);
    await writeFile(join(root, "technical.md"), "British technical prose remains authorised.\n", "utf8");
    await writeFile(join(root, "canonical.ts"), canonicalFixture().replace("function collect", "function gather"), "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /allowance context changed/);
    await writeFile(join(root, "canonical.ts"), "export interface Contract { Digest: string; }\n", "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /allowance count changed/);
    await writeFile(join(root, "canonical.ts"), canonicalFixture(), "utf8");
    await writeFile(join(root, "PrivateArtifact.cs"), "// British test prose.\n", "utf8");
    git(root, "add", "PrivateArtifact.cs");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /tracked path is outside/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("commit messages reject embedded legacy identifier spelling but preserve backticked canonical literals", () => {
  const payload = policyPayload();
  assert.equal(inspectCommitMessage("refactor(language): rename PrivateArtifact helper", payload).length > 0, true);
  assert.equal(inspectCommitMessage("refactor(language): rename private-artifact-helper", payload).length > 0, true);
  assert.deepEqual(inspectCommitMessage("docs(language): preserve `LogicalArtifactDigest` contract", payload), []);
});

test("append-only enforcement scans new suffixes and detects prefix mutation", async () => {
  const root = await createRepository();
  try {
    await assert.doesNotReject(runCheck({ repositoryRoot: root }));
    await writeFile(join(root, "history.md"), "history\nBritish suffix is authorised.\n", "utf8");
    await assert.doesNotReject(runCheck({ repositoryRoot: root }));
    await writeFile(join(root, "history.md"), "history\nThis suffix documents behavior.\n", "utf8");
    await assert.rejects(runCheck({ repositoryRoot: root }), /new or changed/);
    await writeFile(join(root, "history.md"), "history\nA implementação exige validação.\n", "utf8");
    await assert.rejects(runCheck({ repositoryRoot: root }), /new or changed/);
    await writeFile(join(root, "history.md"), "changed\n", "utf8");
    await assert.rejects(runCheck({ repositoryRoot: root }), /Append-only prefix identity changed/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("immutable commit mode ignores adversarial worktree content and prefix mutations", async () => {
  const root = await createRepository();
  try {
    const head = git(root, "rev-parse", "HEAD");
    await writeFile(join(root, "technical.md"), "This dirty worktree changes behavior.\n", "utf8");
    await assert.doesNotReject(runCheck({ repositoryRoot: root, commitHead: head }));
    await assert.rejects(runCheck({ repositoryRoot: root }), /new or changed/);
    await writeFile(join(root, "technical.md"), "British technical prose remains authorised.\n", "utf8");
    await writeFile(join(root, "history.md"), "changed\n", "utf8");
    await assert.doesNotReject(runCheck({ repositoryRoot: root, commitHead: head }));
    await assert.rejects(runCheck({ repositoryRoot: root }), /Append-only prefix identity changed/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("commit range scans immutable HEAD bytes despite adversarial worktree content", async () => {
  const root = await createRepository();
  try {
    const base = git(root, "rev-parse", "HEAD");
    await writeFile(join(root, "technical.md"), "Committed British behaviour remains authorised.\n", "utf8");
    git(root, "add", "technical.md");
    git(root, "commit", "-m", "docs(language): preserve committed behaviour");
    await writeFile(join(root, "technical.md"), "This dirty worktree introduces behavior.\n", "utf8");
    await assert.doesNotReject(runCheck({ repositoryRoot: root, commitBase: base }));
    await assert.rejects(runCheck({ repositoryRoot: root }), /new or changed/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("immutable text classifications require exact regular tracked files and digests", async () => {
  const root = await createRepository();
  try {
    const payload = boundPolicyPayload({
      immutableTextPaths: [
        ...policyPayload().immutableTextPaths,
        {
          path: "missing.md",
          classification: "HISTORICAL_EVIDENCE",
          reason: "Synthetic missing immutable text.",
          sha256: "a".repeat(64),
        },
      ],
    });
    await assert.rejects(inspectRepository(root, payload), /classified whole-file path/);
    await writeFile(join(root, "citation.json"), '{"text":"Changed source"}\n', "utf8");
    await assert.rejects(inspectRepository(root, boundPolicyPayload()), /Immutable text identity changed/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("identity-bound historical region fails closed on marker or digest drift", async () => {
  const root = await createRepository();
  try {
    await writeFile(join(root, "preserved.md"), "## Historical\nImplementação alterada.\n", "utf8");
    await assert.rejects(runCheck({ repositoryRoot: root }), /Excluded region identity changed/);
    await writeFile(join(root, "preserved.md"), "## Changed\nImplementação preservada.\n", "utf8");
    await assert.rejects(runCheck({ repositoryRoot: root }), /marker is missing or ambiguous/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("commit range accepts British prose and rejects American subject or body", async () => {
  const root = await createRepository();
  try {
    const base = git(root, "rev-parse", "HEAD");
    await writeFile(join(root, "technical.md"), "British technical prose continues to be authorised.\n", "utf8");
    git(root, "add", "technical.md");
    git(root, "commit", "-m", "docs(language): clarify authorised prose");
    await assert.doesNotReject(runCheck({ repositoryRoot: root, commitBase: base }));
    const badBase = git(root, "rev-parse", "HEAD");
    await writeFile(join(root, "new.md"), "Further British prose.\n", "utf8");
    git(root, "add", "new.md");
    git(root, "commit", "-m", "docs(language): normalize subject", "-m", "This body documents behavior.");
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: badBase }), /commit-message item/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("commit range rejects secret material without echoing it", async () => {
  const root = await createRepository();
  try {
    const base = git(root, "rev-parse", "HEAD");
    await writeFile(join(root, "new.md"), "Further British prose.\n", "utf8");
    git(root, "add", "new.md");
    const identifier = "OPENAI_API_KEY";
    git(root, "commit", "-m", `docs(language): preserve ${identifier}`);
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: base }), (error) =>
      error instanceof Error && /secret-shaped/.test(error.message) && !error.message.includes(identifier));
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("ordinary commit ranges reject language-control changes unconditionally", async () => {
  const root = await createRepository();
  try {
    const base = git(root, "rev-parse", "HEAD");
    const policyPath = join(root, "eng", "language-policy.json");
    await writeFile(policyPath, `\n${await readFile(policyPath, "utf8")}`, "utf8");
    git(root, "add", "eng/language-policy.json");
    git(root, "commit", "-m", "test(language): preserve reviewed policy bytes");
    const head = git(root, "rev-parse", "HEAD");
    await assert.rejects(runCheck({ repositoryRoot: root }), /exceptional manual review/);
    await assert.rejects(runCheck({ repositoryRoot: root, commitHead: head }), /exceptional manual review/);
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: base }), /exceptional manual review/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("commit ranges reject an intermediate protected edit that a later commit restores", async () => {
  const root = await createRepository();
  try {
    const base = git(root, "rev-parse", "HEAD");
    const policyPath = join(root, "eng", "language-policy.json");
    const original = await readFile(policyPath, "utf8");
    await writeFile(policyPath, `\n${original}`, "utf8");
    git(root, "add", "eng/language-policy.json");
    git(root, "commit", "-m", "test(language): alter reviewed policy bytes");
    await writeFile(policyPath, original, "utf8");
    git(root, "add", "eng/language-policy.json");
    git(root, "commit", "-m", "test(language): restore reviewed policy bytes");
    assert.equal(git(root, "diff", "--name-only", base, "HEAD"), "");
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: base }), /exceptional manual review/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("exact and range checks reject merge commits", async () => {
  const root = await createRepository();
  try {
    const base = git(root, "rev-parse", "HEAD");
    git(root, "checkout", "-b", "synthetic-side");
    await writeFile(join(root, "side.md"), "British side-branch prose.\n", "utf8");
    git(root, "add", "side.md");
    git(root, "commit", "-m", "docs(language): add side prose");
    git(root, "checkout", "main");
    await writeFile(join(root, "main.md"), "British main-branch prose.\n", "utf8");
    git(root, "add", "main.md");
    git(root, "commit", "-m", "docs(language): add main prose");
    git(root, "merge", "--no-ff", "synthetic-side", "-m", "docs(language): merge synthetic histories");
    const head = git(root, "rev-parse", "HEAD");
    await assert.rejects(runCheck({ repositoryRoot: root, commitHead: head }), /Merge commits are not accepted/);
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: base }), /Merge commits are not accepted/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("root commit semantics protect language-control paths", async () => {
  const root = await guidTempDirectory("rag-challenge-language-root-");
  try {
    await mkdir(join(root, "eng"), { recursive: true });
    await writeFile(join(root, "eng", "check-language.mjs"), "// Synthetic protected control.\n", "utf8");
    git(root, "init", "--initial-branch=main");
    git(root, "config", "user.name", "Language Fixture");
    git(root, "config", "user.email", "language@example.invalid");
    git(root, "add", ".");
    git(root, "commit", "-m", "test(language): create protected root commit");
    const head = git(root, "rev-parse", "HEAD");
    await assert.rejects(runCheck({ repositoryRoot: root, commitHead: head }), /exceptional manual review/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("Git index modes and invalid UTF-8 fail closed before prose acceptance", async () => {
  const root = await createRepository();
  try {
    const target = join(root, "target.txt");
    await writeFile(target, "technical.md\n", "utf8");
    const blob = git(root, "hash-object", "-w", target);
    git(root, "update-index", "--add", "--cacheinfo", `120000,${blob},synthetic-link.md`);
    await assert.rejects(runCheck({ repositoryRoot: root }), /non-regular/);
    git(root, "update-index", "--force-remove", "synthetic-link.md");
    await writeFile(join(root, "technical.md"), new Uint8Array([0xff, 0xfe, 0xfd]));
    await assert.rejects(runCheck({ repositoryRoot: root }), /valid UTF-8/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("commit base fails closed when zero, missing, non-ancestor or an empty range", async () => {
  const root = await createRepository();
  try {
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: "0".repeat(40) }), /non-zero full SHA-1/);
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: "f".repeat(40) }), /Git operation failed closed/);
    const head = git(root, "rev-parse", "HEAD");
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: head }), /contains no new commit/);
    git(root, "checkout", "--orphan", "unrelated");
    await writeFile(join(root, "unrelated.md"), "British prose.\n", "utf8");
    git(root, "add", "unrelated.md");
    git(root, "commit", "-m", "docs(language): create unrelated history");
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: head }), /Git operation failed closed/);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("workflow-dispatch head must equal the exact non-zero checked-out HEAD", async () => {
  const root = await createRepository();
  try {
    const head = git(root, "rev-parse", "HEAD");
    await assert.doesNotReject(runCheck({ repositoryRoot: root, commitHead: head }));
    await assert.rejects(runCheck({ repositoryRoot: root, commitHead: "0".repeat(40) }), /exact non-zero repository HEAD/);
    await assert.rejects(runCheck({ repositoryRoot: root, commitHead: "f".repeat(40) }), /exact non-zero repository HEAD/);
  } finally { await rm(root, { recursive: true, force: true }); }
});
// SYNTHETIC_LANGUAGE_HISTORY_AND_COMMITS_END

test("synthetic helpers use deterministic SHA-256 identities", () => {
  assert.equal(sha256("fixture"), createHash("sha256").update("fixture").digest("hex"));
});
