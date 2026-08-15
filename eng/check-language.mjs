// Purpose: Enforces project-owned en-GB prose while preserving narrowly classified historical, localisation and external content.
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { extname, isAbsolute, relative, resolve, sep } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const policySchemaId = "https://rag-challenge.invalid/schemas/language-policy-v1.json";
const baselineSchemaId = "https://rag-challenge.invalid/schemas/language-migration-baseline-v1.json";
const digestPattern = /^sha256:[0-9a-f]{64}$/;
const hashPattern = /^[0-9a-f]{64}$/;
const proseJsonKeys = new Set([
  "description", "displayName", "help", "label", "message", "note", "objective",
  "purpose", "reason", "summary", "text", "title",
]);

export function canonicalJson(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

export function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function assertObject(value, label) {
  if (value === null || typeof value !== "object" || Array.isArray(value)) throw new Error(`${label} must be an object.`);
}

function assertExactKeys(value, expected, label) {
  assertObject(value, label);
  const actual = Object.keys(value).sort();
  const wanted = [...expected].sort();
  if (actual.length !== wanted.length || actual.some((key, index) => key !== wanted[index])) {
    throw new Error(`${label} contains missing or unexpected fields.`);
  }
}

function assertRepositoryPath(path, label) {
  if (typeof path !== "string" || path.length === 0 || path.includes("\\") || path.includes("\0") ||
      path.startsWith("/") || /^[A-Za-z]:/.test(path) || path.split("/").some((part) => part === "" || part === "." || part === "..")) {
    throw new Error(`${label} is not a closed repository-relative path.`);
  }
}

function assertUnique(values, key, label) {
  const seen = new Set();
  for (const value of values) {
    const identity = key(value);
    if (seen.has(identity)) throw new Error(`${label} contains duplicate identity '${identity}'.`);
    seen.add(identity);
  }
}

async function parseJsonFile(path, label) {
  let parsed;
  try { parsed = JSON.parse(await readFile(path, "utf8")); }
  catch (error) { throw new Error(`${label} is missing or invalid JSON: ${error instanceof Error ? error.message : String(error)}`); }
  return parsed;
}

async function assertSchemaIdentity(path, expectedId, label) {
  const schema = await parseJsonFile(path, `${label} schema`);
  assertObject(schema, `${label} schema`);
  if (schema.$schema !== "https://json-schema.org/draft/2020-12/schema" || schema.$id !== expectedId || schema.additionalProperties !== false) {
    throw new Error(`${label} schema identity or closed-object contract is invalid.`);
  }
}

export function validatePolicyDocument(document) {
  assertExactKeys(document, ["$schema", "payload", "digest"], "Language policy");
  if (document.$schema !== "./language-policy.schema.json" || !digestPattern.test(document.digest ?? "")) {
    throw new Error("Language policy schema reference or digest format is invalid.");
  }
  const payload = document.payload;
  assertExactKeys(payload, [
    "schemaVersion", "policyId", "technicalLanguage", "ownerLanguage", "bannedAmericanSpellings",
    "portugueseTechnicalMarkers", "scannedExtensions", "excludedPaths", "excludedRegions", "appendOnlyPrefixes",
  ], "Language policy payload");
  if (payload.schemaVersion !== 1 || payload.policyId !== "rag-challenge-language-policy-v1" ||
      payload.technicalLanguage !== "en-GB" || payload.ownerLanguage !== "pt-BR") {
    throw new Error("Language policy identity and language boundary are invalid.");
  }
  for (const [name, values] of [["bannedAmericanSpellings", payload.bannedAmericanSpellings],
    ["portugueseTechnicalMarkers", payload.portugueseTechnicalMarkers], ["scannedExtensions", payload.scannedExtensions],
    ["excludedPaths", payload.excludedPaths], ["excludedRegions", payload.excludedRegions], ["appendOnlyPrefixes", payload.appendOnlyPrefixes]]) {
    if (!Array.isArray(values) || (name !== "excludedRegions" && values.length === 0)) throw new Error(`Language policy '${name}' must be an array with the required entries.`);
  }
  for (const entry of payload.bannedAmericanSpellings) {
    assertExactKeys(entry, ["american", "british"], "Spelling entry");
    if (!/^[a-z]+$/.test(entry.american) || !/^[a-z]+$/.test(entry.british) || entry.american === entry.british) {
      throw new Error("Language policy spelling entries must be distinct lower-case words.");
    }
  }
  assertUnique(payload.bannedAmericanSpellings, (entry) => entry.american, "Spelling entries");
  if (payload.portugueseTechnicalMarkers.some((marker) => typeof marker !== "string" || marker.length < 2)) {
    throw new Error("Portuguese markers must be bounded non-empty strings.");
  }
  assertUnique(payload.portugueseTechnicalMarkers, (entry) => entry.toLocaleLowerCase("pt-BR"), "Portuguese markers");
  if (payload.scannedExtensions.some((extension) => !/^\.[a-z0-9]+$/.test(extension))) {
    throw new Error("Scanned extensions must be explicit lower-case suffixes.");
  }
  assertUnique(payload.scannedExtensions, (entry) => entry, "Scanned extensions");
  const classes = new Set(["FROZEN_PUBLIC_CONTRACT", "GENERATED_MIGRATION", "FUNCTIONAL_LOCALISATION",
    "OWNER_FACING_PT_BR", "SOURCE_OR_CITATION_DATA", "EXTERNAL_CANONICAL_FORMAT", "ACCEPTED_ARCHITECTURE_HISTORY",
    "HISTORICAL_EVIDENCE", "FROZEN_EVALUATION_DATA"]);
  for (const entry of payload.excludedPaths) {
    assertExactKeys(entry, ["path", "classification", "reason"], "Excluded path");
    assertRepositoryPath(entry.path, "Excluded path");
    if (!classes.has(entry.classification) || typeof entry.reason !== "string" || entry.reason.length === 0) {
      throw new Error("Excluded paths require a closed classification and reason.");
    }
  }
  assertUnique(payload.excludedPaths, (entry) => entry.path, "Excluded paths");
  for (const entry of payload.excludedRegions) {
    assertExactKeys(entry, ["path", "classification", "startMarker", "endMarker", "sha256"], "Excluded region");
    assertRepositoryPath(entry.path, "Excluded region path");
    if (entry.classification !== "PRESERVED_HISTORICAL_REGION" || typeof entry.startMarker !== "string" || entry.startMarker.length === 0 ||
        !(entry.endMarker === null || (typeof entry.endMarker === "string" && entry.endMarker.length > 0)) || !hashPattern.test(entry.sha256)) {
      throw new Error("Excluded regions require exact markers, classification and digest.");
    }
  }
  assertUnique(payload.excludedRegions, (entry) => `${entry.path}\0${entry.startMarker}`, "Excluded regions");
  for (const entry of payload.appendOnlyPrefixes) {
    assertExactKeys(entry, ["path", "prefixBytes", "sha256"], "Append-only prefix");
    assertRepositoryPath(entry.path, "Append-only prefix path");
    if (!Number.isSafeInteger(entry.prefixBytes) || entry.prefixBytes < 1 || !hashPattern.test(entry.sha256)) {
      throw new Error("Append-only prefix identity is invalid.");
    }
  }
  assertUnique(payload.appendOnlyPrefixes, (entry) => entry.path, "Append-only prefixes");
  const actualDigest = `sha256:${sha256(canonicalJson(payload))}`;
  if (actualDigest !== document.digest) throw new Error("Language policy digest does not match its canonical payload.");
  return document;
}

export function validateBaselineDocument(document, expectedPolicyDigest) {
  assertExactKeys(document, ["$schema", "payload", "digest"], "Language migration baseline");
  if (document.$schema !== "./language-migration-baseline.schema.json" || !digestPattern.test(document.digest ?? "")) {
    throw new Error("Language migration baseline schema reference or digest format is invalid.");
  }
  const payload = document.payload;
  assertExactKeys(payload, ["schemaVersion", "baselineId", "status", "policyDigest", "findings"], "Language migration payload");
  if (payload.schemaVersion !== 1 || payload.baselineId !== "rag-challenge-en-gb-migration-v1" ||
      !["IN_PROGRESS", "COMPLETE"].includes(payload.status) || payload.policyDigest !== expectedPolicyDigest || !Array.isArray(payload.findings)) {
    throw new Error("Language migration baseline identity, status or policy binding is invalid.");
  }
  for (const finding of payload.findings) {
    assertExactKeys(finding, ["path", "line", "ruleId", "token", "contextHash", "regionHash"], "Migration finding");
    assertRepositoryPath(finding.path, "Migration finding path");
    if (!Number.isSafeInteger(finding.line) || finding.line < 1 || !["US_SPELLING", "PORTUGUESE_TECHNICAL_PROSE"].includes(finding.ruleId) ||
        typeof finding.token !== "string" || finding.token.length === 0 || !hashPattern.test(finding.contextHash) || !hashPattern.test(finding.regionHash)) {
      throw new Error("Language migration finding is invalid.");
    }
  }
  assertUnique(payload.findings, findingIdentity, "Migration findings");
  const actualDigest = `sha256:${sha256(canonicalJson(payload))}`;
  if (actualDigest !== document.digest) throw new Error("Language migration baseline digest does not match its canonical payload.");
  if (payload.status === "COMPLETE" && payload.findings.length !== 0) {
    throw new Error("A COMPLETE language migration baseline cannot retain debt.");
  }
  return document;
}

function normaliseRegion(value) {
  return value.normalize("NFC").trim().replace(/\s+/g, " ");
}

function cleanTechnicalText(value) {
  return value
    .replace(/`[^`]*`/g, " ")
    .replace(/https?:\/\/\S+/gi, " ")
    .replace(/(?:^|\s)[A-Za-z0-9_.-]*(?:\/[A-Za-z0-9_.-]+)+(?=\s|$)/g, " ")
    .replace(/\b[A-Za-z0-9]+(?:[-_][A-Za-z0-9]+)+\b/g, " ")
    .replace(/\b[0-9a-f]{12,}\b/gi, " ")
    .replace(/\[[^\]]*\]\([^)]*\)/g, " ")
    .replace(/\b[A-Z][A-Za-z0-9]*(?:[._/:+-][A-Za-z0-9]+)+\b/g, " ");
}

function markdownRegions(text) {
  const result = [];
  let fenced = false;
  for (const [index, line] of text.split(/\r?\n/).entries()) {
    if (/^\s*(```|~~~)/.test(line)) { fenced = !fenced; continue; }
    if (!fenced && line.trim().length > 0) result.push({ line: index + 1, text: cleanTechnicalText(line) });
  }
  return result;
}

function codeRegions(text) {
  const result = [];
  let block = false;
  for (const [index, raw] of text.split(/\r?\n/).entries()) {
    let line = raw;
    if (block) {
      const end = line.indexOf("*/");
      if (end < 0) { result.push({ line: index + 1, text: cleanTechnicalText(line) }); continue; }
      result.push({ line: index + 1, text: cleanTechnicalText(line.slice(0, end)) });
      line = line.slice(end + 2); block = false;
    }
    const blockStart = line.indexOf("/*");
    if (blockStart >= 0) {
      const end = line.indexOf("*/", blockStart + 2);
      result.push({ line: index + 1, text: cleanTechnicalText(line.slice(blockStart + 2, end < 0 ? undefined : end)) });
      block = end < 0;
      line = line.slice(0, blockStart);
    }
    const comment = line.match(/^\s*(?:\/\/|#)\s?(.*)$/);
    if (comment) result.push({ line: index + 1, text: cleanTechnicalText(comment[1]) });
    if (/["']pt-BR["']\s*:/.test(line)) continue;
    for (const match of line.matchAll(/(["'])([^"'\r\n]{4,})\1/g)) {
      const candidate = match[2];
      if (/\s/.test(candidate) && !/^[A-Za-z0-9_./:+-]+$/.test(candidate)) {
        result.push({ line: index + 1, text: cleanTechnicalText(candidate) });
      }
    }
  }
  return result;
}

function jsonRegions(text) {
  let value;
  try { value = JSON.parse(text); } catch { throw new Error("Scanned JSON is invalid."); }
  const result = [];
  const visit = (current, key = "") => {
    if (Array.isArray(current)) current.forEach((item) => visit(item, key));
    else if (current !== null && typeof current === "object") Object.entries(current).forEach(([childKey, child]) => visit(child, childKey));
    else if (typeof current === "string" && proseJsonKeys.has(key) && /\s/.test(current)) result.push({ line: 1, text: cleanTechnicalText(current) });
  };
  visit(value);
  return result;
}

function yamlTomlRegions(text) {
  const result = [];
  for (const [index, line] of text.split(/\r?\n/).entries()) {
    const comment = line.match(/^\s*#\s?(.*)$/);
    const owned = line.match(/^\s*(?:description|display_name|help|message|objective|purpose|summary|title)\s*[:=]\s*["']?(.+?)["']?\s*$/i);
    if (comment) result.push({ line: index + 1, text: cleanTechnicalText(comment[1]) });
    else if (owned) result.push({ line: index + 1, text: cleanTechnicalText(owned[1]) });
  }
  return result;
}

export function extractProse(path, text) {
  const extension = extname(path).toLowerCase();
  if (extension === ".md") return markdownRegions(text);
  if (extension === ".json") return jsonRegions(text);
  if ([".yaml", ".yml", ".toml"].includes(extension)) return yamlTomlRegions(text);
  return codeRegions(text);
}

export function inspectRegions(path, regions, payload) {
  const findings = [];
  for (const region of regions) {
    const normalised = normaliseRegion(region.text);
    if (normalised.length === 0) continue;
    const lower = normalised.toLocaleLowerCase("en-GB");
    for (const entry of payload.bannedAmericanSpellings) {
      const pattern = new RegExp(`(^|[^A-Za-z])${entry.american}([^A-Za-z]|$)`, "i");
      if (pattern.test(normalised)) findings.push(makeFinding(path, region.line, "US_SPELLING", entry.american, normalised));
    }
    const marker = payload.portugueseTechnicalMarkers.find((entry) =>
      new RegExp(`(^|[^\\p{L}])${escapeRegex(entry)}([^\\p{L}]|$)`, "iu").test(lower));
    if (marker !== undefined) findings.push(makeFinding(path, region.line, "PORTUGUESE_TECHNICAL_PROSE", marker, normalised));
  }
  return findings.sort(compareFindings);
}

export function inspectCommitMessage(message, payload) {
  return inspectRegions("<commit-message>", message.split(/\r?\n/).map((text, index) => ({ line: index + 1, text: cleanTechnicalText(text) })), payload);
}

function escapeRegex(value) { return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&"); }

function makeFinding(path, line, ruleId, token, region) {
  const context = normaliseRegion(region);
  return { path, line, ruleId, token: token.toLocaleLowerCase("en-GB"), contextHash: sha256(context.toLocaleLowerCase("en-GB")), regionHash: sha256(`${line}:${context}`) };
}

function compareFindings(left, right) {
  return left.path.localeCompare(right.path) || left.line - right.line || left.ruleId.localeCompare(right.ruleId) || left.token.localeCompare(right.token);
}

function findingIdentity(finding) {
  return [finding.path, finding.line, finding.ruleId, finding.token, finding.contextHash, finding.regionHash].join("\0");
}

function git(repositoryRoot, arguments_) {
  const environment = {};
  for (const name of ["PATH", "SystemRoot", "WINDIR"]) if (process.env[name] !== undefined) environment[name] = process.env[name];
  const result = spawnSync("git", ["-c", "core.hooksPath=", "-c", "credential.helper=", "-c", "protocol.file.allow=never", ...arguments_], {
    cwd: repositoryRoot, encoding: "utf8", env: environment, shell: false, maxBuffer: 16 * 1024 * 1024,
  });
  if (result.status !== 0) throw new Error(`Git '${arguments_[0]}' failed closed with exit code ${result.status ?? "unknown"}.`);
  return result.stdout;
}

function assertInside(root, path) {
  const relation = relative(root, path);
  if (relation === ".." || relation.startsWith(`..${sep}`) || isAbsolute(relation)) throw new Error("A language-policy path escapes the repository root.");
}

async function verifyAppendOnlyPrefixes(repositoryRoot, payload) {
  for (const entry of payload.appendOnlyPrefixes) {
    const path = resolve(repositoryRoot, entry.path); assertInside(repositoryRoot, path);
    const bytes = await readFile(path);
    if (bytes.length < entry.prefixBytes || sha256(bytes.subarray(0, entry.prefixBytes)) !== entry.sha256) {
      throw new Error(`Append-only prefix identity changed for '${entry.path}'.`);
    }
  }
}

export async function inspectRepository(repositoryRoot, payload) {
  const tracked = git(repositoryRoot, ["ls-files", "-z"]).split("\0").filter(Boolean).sort();
  const excluded = new Set(payload.excludedPaths.map((entry) => entry.path));
  const appendOnly = new Map(payload.appendOnlyPrefixes.map((entry) => [entry.path, entry]));
  const extensions = new Set(payload.scannedExtensions);
  const findings = [];
  for (const path of tracked) {
    if (excluded.has(path) || !extensions.has(extname(path).toLowerCase())) continue;
    const absolute = resolve(repositoryRoot, path); assertInside(repositoryRoot, absolute);
    const bytes = await readFile(absolute);
    const prefix = appendOnly.get(path);
    let text;
    if (prefix === undefined) {
      text = bytes.toString("utf8");
    } else {
      const suffixBytes = bytes.subarray(prefix.prefixBytes);
      const suffix = suffixBytes.toString("utf8");
      if (!Buffer.from(suffix, "utf8").equals(suffixBytes)) throw new Error(`Append-only suffix is not valid UTF-8 for '${path}'.`);
      const prefixText = bytes.subarray(0, prefix.prefixBytes).toString("utf8");
      text = `${prefixText.replace(/[^\n]/g, " ")}${suffix}`;
    }
    for (const region of payload.excludedRegions.filter((entry) => entry.path === path)) {
      const start = text.indexOf(region.startMarker);
      if (start < 0 || text.indexOf(region.startMarker, start + region.startMarker.length) >= 0) {
        throw new Error(`Excluded region marker is missing or ambiguous for '${path}'.`);
      }
      const end = region.endMarker === null ? text.length : text.indexOf(region.endMarker, start + region.startMarker.length);
      if (end < 0 || (region.endMarker !== null && text.indexOf(region.endMarker, end + region.endMarker.length) >= 0)) {
        throw new Error(`Excluded region end marker is missing or ambiguous for '${path}'.`);
      }
      const regionText = text.slice(start, region.endMarker === null ? end : end + region.endMarker.length);
      if (sha256(regionText) !== region.sha256) throw new Error(`Excluded region identity changed for '${path}'.`);
      text = `${text.slice(0, start)}${regionText.replace(/[^\n]/g, " ")}${text.slice(region.endMarker === null ? end : end + region.endMarker.length)}`;
    }
    findings.push(...inspectRegions(path, extractProse(path, text), payload));
  }
  return findings.sort(compareFindings);
}

export function assertDebtMatches(findings, baseline) {
  const actual = new Set(findings.map(findingIdentity));
  const accepted = new Set(baseline.payload.findings.map(findingIdentity));
  const unaccepted = [...actual].filter((identity) => !accepted.has(identity));
  if (unaccepted.length > 0) throw new Error(`Language enforcement found ${unaccepted.length} new or changed item(s).`);
  if (baseline.payload.status === "COMPLETE" && findings.length > 0) throw new Error("Language migration is COMPLETE but repository debt remains.");
}

function commitMessages(repositoryRoot, base, expectedHead) {
  const head = git(repositoryRoot, ["rev-parse", "--verify", "HEAD"]).trim();
  if (!/^[0-9a-f]{40}$/.test(head)) throw new Error("Repository HEAD is not a full commit identity.");
  if (expectedHead !== null) {
    if (!/^[0-9a-f]{40}$/.test(expectedHead) || /^0{40}$/.test(expectedHead) || expectedHead !== head) {
      throw new Error("Commit head must be the exact non-zero repository HEAD.");
    }
    if (base !== null) throw new Error("Commit base and explicit commit head are mutually exclusive.");
    return [git(repositoryRoot, ["show", "-s", "--format=%B", head])];
  }
  if (base === null) return [git(repositoryRoot, ["show", "-s", "--format=%B", head])];
  if (!/^[0-9a-f]{40}$/.test(base) || /^0{40}$/.test(base)) throw new Error("Commit base must be a non-zero full SHA-1.");
  git(repositoryRoot, ["rev-parse", "--verify", `${base}^{commit}`]);
  git(repositoryRoot, ["merge-base", "--is-ancestor", base, "HEAD"]);
  const commits = git(repositoryRoot, ["rev-list", "--reverse", `${base}..HEAD`]).trim().split(/\r?\n/).filter(Boolean);
  if (commits.length === 0) throw new Error("Commit range contains no new commit.");
  return commits.map((commit) => git(repositoryRoot, ["show", "-s", "--format=%B", commit]));
}

export async function loadPolicy(repositoryRoot) {
  const policyPath = resolve(repositoryRoot, "eng/language-policy.json");
  const schemaPath = resolve(repositoryRoot, "eng/language-policy.schema.json");
  await assertSchemaIdentity(schemaPath, policySchemaId, "Language policy");
  return validatePolicyDocument(await parseJsonFile(policyPath, "Language policy"));
}

export async function runCheck({ repositoryRoot, commitBase = null, commitHead = null }) {
  const policy = await loadPolicy(repositoryRoot);
  await assertSchemaIdentity(resolve(repositoryRoot, "eng/language-migration-baseline.schema.json"), baselineSchemaId, "Language migration baseline");
  await verifyAppendOnlyPrefixes(repositoryRoot, policy.payload);
  const findings = await inspectRepository(repositoryRoot, policy.payload);
  const baselinePath = resolve(repositoryRoot, "eng/language-migration-baseline.json");
  const baseline = validateBaselineDocument(await parseJsonFile(baselinePath, "Language migration baseline"), policy.digest);
  assertDebtMatches(findings, baseline);
  const messages = commitMessages(repositoryRoot, commitBase, commitHead);
  const commitFindings = messages.flatMap((message) => inspectCommitMessage(message, policy.payload));
  if (commitFindings.length > 0) throw new Error(`Language enforcement rejected ${commitFindings.length} commit-message item(s).`);
  return { files: git(repositoryRoot, ["ls-files"]).trim().split(/\r?\n/).filter(Boolean).length, findings: findings.length, commits: messages.length };
}

function valueAfter(arguments_, name) {
  const index = arguments_.indexOf(name);
  if (index < 0) return null;
  const value = arguments_[index + 1];
  if (value === undefined || value.startsWith("--")) throw new Error(`Argument '${name}' requires a value.`);
  return value;
}

async function main() {
  const arguments_ = process.argv.slice(2);
  if (arguments_.includes("--write-baseline")) throw new Error("Migration baseline regeneration is disabled; changes require an explicit reviewed edit.");
  const valueOptions = new Set(["--repository-root", "--commit-base", "--commit-head"]);
  for (let index = 0; index < arguments_.length; index += 1) {
    const argument = arguments_[index];
    if (!valueOptions.has(argument)) throw new Error(`Unknown language-policy argument '${argument}'.`);
    index += 1;
    if (index >= arguments_.length || arguments_[index].startsWith("--")) throw new Error(`Argument '${argument}' requires a value.`);
  }
  const repositoryRoot = resolve(valueAfter(arguments_, "--repository-root") ?? resolve(fileURLToPath(import.meta.url), "../.."));
  const result = await runCheck({ repositoryRoot, commitBase: valueAfter(arguments_, "--commit-base"), commitHead: valueAfter(arguments_, "--commit-head") });
  process.stdout.write(`Language policy PASS: ${result.files} files, ${result.findings} accepted migration findings, ${result.commits} commit message(s).\n`);
}

if (process.argv[1] !== undefined && resolve(process.argv[1]) === resolve(fileURLToPath(import.meta.url))) {
  main().catch((error) => { process.stderr.write(`Language policy FAIL: ${error instanceof Error ? error.message : String(error)}\n`); process.exitCode = 1; });
}
