// Purpose: Loads the coordinator-owned en-GB policy and rejects non-compliant candidate commit prose before evidence is trusted.
import { createHash } from "node:crypto";
import { OrchestratorStop } from "../core/errors.js";
import { readBoundedRegularFile, resolveContained } from "./path-policy.js";
import { parseSecureJson } from "./secure-json.js";

export interface TrustedLanguagePolicy {
  readonly policyId: "rag-challenge-language-policy-v1";
  readonly technicalLanguage: "en-GB";
  readonly bannedAmericanSpellings: readonly Readonly<{ american: string; british: string }>[];
  readonly portugueseTechnicalMarkers: readonly string[];
}

function canonicalJson(value: unknown): string {
  if (Array.isArray(value)) return `[${value.map(canonicalJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    const record = value as Readonly<Record<string, unknown>>;
    return `{${Object.keys(record).sort().map((key) => `${JSON.stringify(key)}:${canonicalJson(record[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function exactKeys(value: Readonly<Record<string, unknown>>, expected: readonly string[]): boolean {
  const actual = Object.keys(value).sort();
  const wanted = [...expected].sort();
  return actual.length === wanted.length && actual.every((key, index) => key === wanted[index]);
}

function asRecord(value: unknown): Readonly<Record<string, unknown>> {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy is invalid.");
  }
  return value as Readonly<Record<string, unknown>>;
}

export function parseTrustedLanguagePolicy(value: unknown, expectedSchemaDigest: string): TrustedLanguagePolicy {
  const document = asRecord(value);
  if (!exactKeys(document, ["$schema", "schemaDigest", "payload", "digest"]) || document.$schema !== "./language-policy.schema.json" ||
      document.schemaDigest !== expectedSchemaDigest || typeof document.digest !== "string" || !/^sha256:[0-9a-f]{64}$/.test(document.digest)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy envelope is invalid.");
  }
  const payload = asRecord(document.payload);
  if (!exactKeys(payload, ["schemaVersion", "policyId", "technicalLanguage", "ownerLanguage", "bannedAmericanSpellings",
    "portugueseTechnicalMarkers", "scannedExtensions", "excludedPaths", "excludedRegions", "appendOnlyPrefixes"]) ||
      payload.schemaVersion !== 1 || payload.policyId !== "rag-challenge-language-policy-v1" || payload.technicalLanguage !== "en-GB") {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy identity is invalid.");
  }
  if (!Array.isArray(payload.bannedAmericanSpellings) || !Array.isArray(payload.portugueseTechnicalMarkers) ||
      payload.bannedAmericanSpellings.length === 0 || payload.portugueseTechnicalMarkers.length === 0) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language rules are missing.");
  }
  const spellings = payload.bannedAmericanSpellings.map((entry) => {
    const record = asRecord(entry);
    if (!exactKeys(record, ["american", "british"]) || typeof record.american !== "string" || typeof record.british !== "string" ||
        !/^[a-z]+$/.test(record.american) || !/^[a-z]+$/.test(record.british)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator spelling rule is invalid.");
    }
    return { american: record.american, british: record.british };
  });
  if (payload.portugueseTechnicalMarkers.some((marker) => typeof marker !== "string" || marker.length < 2)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator Portuguese marker is invalid.");
  }
  const digest = createHash("sha256").update(canonicalJson(payload)).digest("hex");
  if (document.digest !== `sha256:${digest}`) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy digest is invalid.");
  }
  return {
    policyId: "rag-challenge-language-policy-v1",
    technicalLanguage: "en-GB",
    bannedAmericanSpellings: spellings,
    portugueseTechnicalMarkers: payload.portugueseTechnicalMarkers as readonly string[],
  };
}

export async function loadTrustedLanguagePolicy(repositoryRoot: string): Promise<TrustedLanguagePolicy> {
  try {
    const schemaPath = resolveContained(repositoryRoot, "eng", "language-policy.schema.json");
    const schemaText = await readBoundedRegularFile(repositoryRoot, schemaPath, 1_048_576, "Trusted language schema", "OUT_OF_SCOPE_CHANGE_REQUIRED");
    if (schemaText.includes("\uFFFD")) throw new Error("invalid UTF-8");
    const schema = asRecord(parseSecureJson(schemaText, "Trusted language schema", "OUT_OF_SCOPE_CHANGE_REQUIRED"));
    if (schema.$schema !== "https://json-schema.org/draft/2020-12/schema" ||
        schema.$id !== "https://rag-challenge.invalid/schemas/language-policy-v1.json" || schema.additionalProperties !== false) {
      throw new Error("invalid schema identity");
    }
    const expectedSchemaDigest = `sha256:${createHash("sha256").update(schemaText).digest("hex")}`;
    const policyPath = resolveContained(repositoryRoot, "eng", "language-policy.json");
    const policyText = await readBoundedRegularFile(repositoryRoot, policyPath, 8_388_608, "Trusted language policy", "OUT_OF_SCOPE_CHANGE_REQUIRED");
    if (policyText.includes("\uFFFD")) throw new Error("invalid UTF-8");
    return parseTrustedLanguagePolicy(
      parseSecureJson(policyText, "Trusted language policy", "OUT_OF_SCOPE_CHANGE_REQUIRED"),
      expectedSchemaDigest,
    );
  } catch (error) {
    if (error instanceof OrchestratorStop) throw error;
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy is missing or unreadable.");
  }
}

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

export function assertBritishCommitMessage(message: string, policy: TrustedLanguagePolicy, taskId?: string): void {
  if (message.includes("\uFFFD")) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The candidate commit message is not valid UTF-8.", taskId);
  }
  const normalised = message.normalize("NFC")
    .replace(/https?:\/\/\S+/gi, " ")
    .replace(/`[^`]*`/g, " ")
    .replace(/\b[0-9a-f]{12,}\b/gi, " ");
  for (const entry of policy.bannedAmericanSpellings) {
    if (new RegExp(`(^|[^A-Za-z])${escapeRegex(entry.american)}([^A-Za-z]|$)`, "i").test(normalised)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The candidate commit message is not compliant British English.", taskId);
    }
  }
  const lower = normalised.toLocaleLowerCase("pt-BR");
  if (policy.portugueseTechnicalMarkers.some((marker) =>
    new RegExp(`(^|[^\\p{L}])${escapeRegex(marker)}([^\\p{L}]|$)`, "iu").test(lower))) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The candidate commit message contains Portuguese technical prose.", taskId);
  }
}
