// Purpose: Proves the en-GB policy, migration debt and commit-history boundaries with synthetic local fixtures only.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdtemp, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import test from "node:test";
import {
  assertDebtMatches,
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

function digestDocument(schema, payload) {
  return { $schema: schema, payload, digest: `sha256:${sha256(canonicalJson(payload))}` };
}

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

function baselinePayload(policyDigest, findings = [], status = "IN_PROGRESS") {
  return { schemaVersion: 1, baselineId: "rag-challenge-en-gb-migration-v1", status, policyDigest, findings };
}

function git(root, ...arguments_) {
  const result = spawnSync("git", arguments_, { cwd: root, encoding: "utf8", shell: false });
  if (result.status !== 0) throw new Error(`Synthetic Git command failed: git ${arguments_.join(" ")}\n${result.stderr}`);
  return result.stdout.trim();
}

async function createRepository(initialMessage = "test(language): initialise synthetic policy") {
  const root = await mkdtemp(join(tmpdir(), "rag-challenge-language-policy-"));
  await mkdir(join(root, "eng"), { recursive: true });
  await writeFile(join(root, "history.md"), "history\n", "utf8");
  await writeFile(join(root, "technical.md"), "British technical prose is authorised.\n", "utf8");
  await writeFile(join(root, "preserved.md"), "## Historical\nImplementação preservada.\n", "utf8");
  await writeFile(join(root, "locale.ts"), "export const label = 'Alteração do arquivo';\n", "utf8");
  await writeFile(join(root, "citation.json"), '{"text":"Alteração citada pela fonte"}\n', "utf8");
  await writeFile(join(root, "eng", "language-policy.schema.json"), await readFile(join(sourceRoot, "language-policy.schema.json"), "utf8"), "utf8");
  await writeFile(join(root, "eng", "language-migration-baseline.schema.json"), await readFile(join(sourceRoot, "language-migration-baseline.schema.json"), "utf8"), "utf8");
  const policy = digestDocument("./language-policy.schema.json", policyPayload());
  await writeFile(join(root, "eng", "language-policy.json"), `${JSON.stringify(policy, null, 2)}\n`, "utf8");
  git(root, "init", "--initial-branch=main");
  git(root, "config", "user.name", "Language Fixture");
  git(root, "config", "user.email", "language@example.invalid");
  git(root, "add", ".");
  git(root, "commit", "-m", initialMessage);
  const findings = await inspectRepository(root, policy.payload);
  const baseline = digestDocument("./language-migration-baseline.schema.json", baselinePayload(policy.digest, findings));
  await writeFile(join(root, "eng", "language-migration-baseline.json"), `${JSON.stringify(baseline, null, 2)}\n`, "utf8");
  git(root, "add", "eng/language-migration-baseline.json");
  git(root, "commit", "-m", "test(language): record synthetic migration baseline");
  await writeFile(join(root, "history.md"), "history\n", "utf8");
  return root;
}

test("policy and baseline manifests reject unknown fields, invalid schemas and digest mismatch", () => {
  const policy = digestDocument("./language-policy.schema.json", policyPayload());
  assert.doesNotThrow(() => validatePolicyDocument(policy));
  assert.throws(() => validatePolicyDocument({ ...policy, unexpected: true }), /missing or unexpected/);
  assert.throws(() => validatePolicyDocument({ ...policy, digest: `sha256:${"0".repeat(64)}` }), /digest does not match/);
  const baseline = digestDocument("./language-migration-baseline.schema.json", baselinePayload(policy.digest));
  assert.doesNotThrow(() => validateBaselineDocument(baseline, policy.digest));
  assert.throws(() => validateBaselineDocument({ ...baseline, $schema: "wrong" }, policy.digest), /schema reference/);
  assert.throws(() => validateBaselineDocument({ ...baseline, digest: `sha256:${"0".repeat(64)}` }, policy.digest), /digest does not match/);
});

test("baseline regeneration is not exposed by the tracked checker", () => {
  const result = spawnSync(process.execPath, [join(sourceRoot, "check-language.mjs"), "--write-baseline"], {
    cwd: sourceRoot,
    encoding: "utf8",
    shell: false,
  });
  assert.equal(result.status, 1);
  assert.match(result.stderr, /baseline regeneration is disabled/i);
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

test("identifiers, protocols, paths, hashes and canonical literals are not prose findings", () => {
  const payload = policyPayload();
  const text = "OpenAI_API_KEY CH_INDEX_UNAVAILABLE https://example.invalid/color /src/color.ts 0123456789abcdef0123456789abcdef pt-BR en-GB";
  assert.deepEqual(inspectRegions("fixture.md", extractProse("fixture.md", text), payload), []);
});

test("owner-facing pt-BR fenced payload is excluded but Portuguese technical prose fails", () => {
  const payload = policyPayload();
  const ownerPayload = "```text\nAutorizo a alteração do arquivo.\n```\n";
  assert.deepEqual(inspectRegions("handoff.md", extractProse("handoff.md", ownerPayload), payload), []);
  const findings = inspectRegions("technical.md", extractProse("technical.md", "A implementação exige validação técnica."), payload);
  assert.equal(findings.some((finding) => finding.ruleId === "PORTUGUESE_TECHNICAL_PROSE"), true);
});

test("localisation and source or citation exclusions are closed exact paths", () => {
  const payload = policyPayload();
  assert.deepEqual(payload.excludedPaths.map((entry) => [entry.path, entry.classification]), [
    ["locale.ts", "FUNCTIONAL_LOCALISATION"],
    ["citation.json", "SOURCE_OR_CITATION_DATA"],
  ]);
  assert.equal(payload.excludedPaths.some((entry) => /[*?]/.test(entry.path)), false);
});

test("identical migration debt passes, changed or new debt fails, and COMPLETE cannot retain debt", () => {
  const policy = digestDocument("./language-policy.schema.json", policyPayload());
  const finding = inspectRegions("technical.md", extractProse("technical.md", "A implementação exige validação técnica."), policy.payload)[0];
  const baseline = digestDocument("./language-migration-baseline.schema.json", baselinePayload(policy.digest, [finding]));
  assert.doesNotThrow(() => assertDebtMatches([finding], baseline));
  const changed = { ...finding, line: finding.line + 1 };
  assert.throws(() => assertDebtMatches([changed], baseline), /new or changed/);
  const complete = digestDocument("./language-migration-baseline.schema.json", baselinePayload(policy.digest, [finding], "COMPLETE"));
  assert.throws(() => validateBaselineDocument(complete, policy.digest), /COMPLETE.*cannot retain debt/);
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

test("commit base fails closed when zero, missing, non-ancestor or an empty range", async () => {
  const root = await createRepository();
  try {
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: "0".repeat(40) }), /non-zero full SHA-1/);
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: "f".repeat(40) }), /Git 'rev-parse' failed closed/);
    const head = git(root, "rev-parse", "HEAD");
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: head }), /contains no new commit/);
    git(root, "checkout", "--orphan", "unrelated");
    await writeFile(join(root, "unrelated.md"), "British prose.\n", "utf8");
    git(root, "add", "unrelated.md");
    git(root, "commit", "-m", "docs(language): create unrelated history");
    await assert.rejects(runCheck({ repositoryRoot: root, commitBase: head }), /Git 'merge-base' failed closed/);
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

test("synthetic helpers use deterministic SHA-256 identities", () => {
  assert.equal(sha256("fixture"), createHash("sha256").update("fixture").digest("hex"));
});
