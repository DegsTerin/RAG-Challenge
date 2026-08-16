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

// SYNTHETIC_LANGUAGE_POLICY_PAYLOAD_START
function policyPayload(overrides = {}) {
  return {
    schemaVersion: 1,
    policyId: "rag-challenge-language-policy-v1",
    technicalLanguage: "en-GB",
    ownerLanguage: "pt-BR",
    bannedAmericanSpellings,
    portugueseTechnicalMarkers: ["alteração", "arquivo", "implementação", "segurança", "validação"],
    scannedExtensions: [".json", ".md", ".mjs", ".ts", ".yml"],
    excludedPaths: [
      { path: "locale.ts", classification: "FUNCTIONAL_LOCALISATION", reason: "Synthetic bilingual interface data." },
      { path: "citation.json", classification: "SOURCE_OR_CITATION_DATA", reason: "Synthetic source-language citation data." },
    ],
    excludedRegions: [{
      path: "preserved.md",
      classification: "PRESERVED_HISTORICAL_REGION",
      startMarker: "## Historical",
      endMarker: null,
      sha256: sha256("## Historical\nImplementação preservada.\n"),
    }],
    appendOnlyPrefixes: [{ path: "history.md", prefixBytes: 1, sha256: sha256("h") }],
    ...overrides,
  };
}
// SYNTHETIC_LANGUAGE_POLICY_PAYLOAD_END

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
async function createRepository(initialMessage = "test(language): initialise synthetic policy") {
  const root = await guidTempDirectory("rag-challenge-language-policy-");
  await mkdir(join(root, "eng"), { recursive: true });
  await writeFile(join(root, "history.md"), "history\n", "utf8");
  await writeFile(join(root, "technical.md"), "British technical prose is authorised.\n", "utf8");
  await writeFile(join(root, "preserved.md"), "## Historical\nImplementação preservada.\n", "utf8");
  await writeFile(join(root, "locale.ts"), "export const label = 'Alteração do arquivo';\n", "utf8");
  await writeFile(join(root, "citation.json"), '{"text":"Alteração citada pela fonte"}\n', "utf8");
  await writeFile(join(root, "eng", "language-policy.schema.json"), await readFile(join(sourceRoot, "language-policy.schema.json"), "utf8"), "utf8");
  await writeFile(join(root, "eng", "language-migration-baseline.schema.json"), await readFile(join(sourceRoot, "language-migration-baseline.schema.json"), "utf8"), "utf8");
  const policy = digestDocument("./language-policy.schema.json", policySchemaDigest, policyPayload());
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
  await writeFile(join(root, "history.md"), "history\n", "utf8");
  return root;
}
// SYNTHETIC_LANGUAGE_REPOSITORY_END

test("policy and baseline manifests reject unknown fields, invalid schemas and digest mismatch", () => {
  const policy = digestDocument("./language-policy.schema.json", policySchemaDigest, policyPayload());
  assert.doesNotThrow(() => validatePolicyDocument(policy, policySchemaDigest));
  assert.throws(() => validatePolicyDocument({ ...policy, unexpected: true }, policySchemaDigest), /missing or unexpected/);
  assert.throws(() => validatePolicyDocument({ ...policy, digest: `sha256:${"0".repeat(64)}` }, policySchemaDigest), /digest does not match/);
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
      /new or changed/,
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
  const token = "sk-proj-synthetic-not-a-real-secret";
  for (const value of [identifier, token, "invalid \uFFFD message"]) {
    assert.throws(() => assertSafeCommitMessage(value), (error) =>
      error instanceof Error && !error.message.includes(value));
  }
  const text = "CH_INDEX_UNAVAILABLE https://example.invalid/color /src/color.ts 0123456789abcdef0123456789abcdef pt-BR en-GB";
  assert.deepEqual(inspectRegions("fixture.md", extractProse("fixture.md", text), payload), []);
});
// SYNTHETIC_LANGUAGE_SECRET_MESSAGES_END

// SYNTHETIC_LANGUAGE_OWNER_PAYLOAD_START
test("owner-facing pt-BR fenced payload is excluded but Portuguese technical prose fails", () => {
  const payload = policyPayload();
  const ownerPayload = "```text\nAutorizo a alteração do arquivo.\n```\n";
  assert.deepEqual(inspectRegions("handoff.md", extractProse("handoff.md", ownerPayload), payload), []);
  const findings = inspectRegions("technical.md", extractProse("technical.md", "A implementação exige validação técnica."), payload);
  assert.equal(findings.some((finding) => finding.ruleId === "PORTUGUESE_TECHNICAL_PROSE"), true);
});
// SYNTHETIC_LANGUAGE_OWNER_PAYLOAD_END

test("localisation and source or citation exclusions are closed exact paths", () => {
  const payload = policyPayload();
  assert.deepEqual(payload.excludedPaths.map((entry) => [entry.path, entry.classification]), [
    ["locale.ts", "FUNCTIONAL_LOCALISATION"],
    ["citation.json", "SOURCE_OR_CITATION_DATA"],
  ]);
  assert.equal(payload.excludedPaths.some((entry) => /[*?]/.test(entry.path)), false);
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
    await writeFile(join(root, "technical.md"), "British technical prose remains authorised.\n", "utf8");
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
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: base }), /exceptional manual review/);
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
