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
  readonly productCredentialIdentifierAllowances: readonly ProductCredentialIdentifierAllowance[];
}

export type ProductCredentialIdentifierAllowanceClassification =
  | "PRODUCT_RUNTIME_OR_DEPLOYMENT_CONFIGURATION"
  | "SECURITY_POLICY"
  | "EXECUTABLE_POLICY_ENFORCEMENT"
  | "SYNTHETIC_ENFORCEMENT"
  | "PRESERVED_HISTORICAL_DOCUMENT";

export interface ProductCredentialIdentifierAllowance {
  readonly path: string;
  readonly classification: ProductCredentialIdentifierAllowanceClassification;
  readonly sha256: string | null;
}

const productCredentialIdentifierAllowanceClassifications = new Set<string>([
  "PRODUCT_RUNTIME_OR_DEPLOYMENT_CONFIGURATION",
  "SECURITY_POLICY",
  "EXECUTABLE_POLICY_ENFORCEMENT",
  "SYNTHETIC_ENFORCEMENT",
  "PRESERVED_HISTORICAL_DOCUMENT",
]);

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

function isClosedRepositoryFilePath(value: string): boolean {
  if (value.length === 0 || value.includes("\\") || value.includes("\0") || value.includes("*") || value.includes("?") ||
      value.startsWith("/") || value.endsWith("/") || /^[A-Za-z]:/.test(value)) {
    return false;
  }
  return value.split("/").every((segment) => segment.length > 0 && segment !== "." && segment !== "..");
}

export function parseTrustedLanguagePolicy(value: unknown, expectedSchemaDigest: string): TrustedLanguagePolicy {
  const document = asRecord(value);
  if (!exactKeys(document, ["$schema", "schemaDigest", "payload", "digest"]) || document.$schema !== "./language-policy.schema.json" ||
      document.schemaDigest !== expectedSchemaDigest || typeof document.digest !== "string" || !/^sha256:[0-9a-f]{64}$/.test(document.digest)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy envelope is invalid.");
  }
  const payload = asRecord(document.payload);
  if (!exactKeys(payload, ["schemaVersion", "policyId", "technicalLanguage", "ownerLanguage", "bannedAmericanSpellings",
    "portugueseTechnicalMarkers", "scannedExtensions", "productCredentialIdentifierAllowances", "excludedPaths",
    "excludedRegions", "appendOnlyPrefixes"]) ||
      payload.schemaVersion !== 1 || payload.policyId !== "rag-challenge-language-policy-v1" || payload.technicalLanguage !== "en-GB") {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy identity is invalid.");
  }
  if (!Array.isArray(payload.bannedAmericanSpellings) || !Array.isArray(payload.portugueseTechnicalMarkers) ||
      !Array.isArray(payload.productCredentialIdentifierAllowances) || payload.bannedAmericanSpellings.length === 0 ||
      payload.portugueseTechnicalMarkers.length === 0 || payload.productCredentialIdentifierAllowances.length === 0) {
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
  const allowancePaths = new Set<string>();
  const productCredentialIdentifierAllowances = payload.productCredentialIdentifierAllowances.map((entry) => {
    const record = asRecord(entry);
    if (!exactKeys(record, ["path", "classification", "sha256"]) || typeof record.path !== "string" ||
        !isClosedRepositoryFilePath(record.path) || typeof record.classification !== "string" ||
        !productCredentialIdentifierAllowanceClassifications.has(record.classification) || allowancePaths.has(record.path)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The product credential identifier allowance is invalid.");
    }
    const isHistorical = record.classification === "PRESERVED_HISTORICAL_DOCUMENT";
    if ((isHistorical && (typeof record.sha256 !== "string" || !/^[0-9a-f]{64}$/.test(record.sha256))) ||
        (!isHistorical && record.sha256 !== null)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The product credential identifier allowance digest is invalid.");
    }
    allowancePaths.add(record.path);
    return {
      path: record.path,
      classification: record.classification as ProductCredentialIdentifierAllowanceClassification,
      sha256: record.sha256 as string | null,
    };
  });
  const digest = createHash("sha256").update(canonicalJson(payload)).digest("hex");
  if (document.digest !== `sha256:${digest}`) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The trusted coordinator language policy digest is invalid.");
  }
  return {
    policyId: "rag-challenge-language-policy-v1",
    technicalLanguage: "en-GB",
    bannedAmericanSpellings: spellings,
    portugueseTechnicalMarkers: payload.portugueseTechnicalMarkers as readonly string[],
    productCredentialIdentifierAllowances,
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
