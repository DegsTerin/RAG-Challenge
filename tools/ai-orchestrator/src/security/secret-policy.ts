// Purpose: Applies one non-echoing secret-material boundary to plans, prompts, results, arguments, environments and runner protocols before persistence or spawn.
import { OrchestratorStop } from "../core/errors.js";

const authorityReferencePattern = /^AUTH-[A-Z0-9][A-Z0-9-]{2,122}$/;
const secretTokenPatterns = [
  /\b(?:sk|sk-proj)-[A-Za-z0-9_-]{8,}\b/,
  /-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----/,
  /\bBearer\s+[A-Za-z0-9._~+/-]{12,}={0,2}\b/i,
  /(?:KEY|SECRET|TOKEN|PASSWORD|CREDENTIAL|CONNECTION_STRING)\s*[=:]\s*["']?[^\s,"'}]{4,}/i,
] as const;
const sensitiveFieldPattern = /(?:api[_-]?key|secret|token|password|credential|connection[_-]?string)/i;

function reject(label: string): never {
  throw new OrchestratorStop(
    "SECRET_REQUIRED",
    `${label} contains prohibited secret-shaped material.`,
  );
}

export function assertNoSecretShapedText(value: string, label: string): void {
  if (secretTokenPatterns.some((pattern) => pattern.test(value))) reject(label);
}

export function assertNoSecretShapedMaterial(value: unknown, label: string): void {
  const pending: unknown[] = [value];
  while (pending.length > 0) {
    const current = pending.pop();
    if (typeof current === "string") {
      assertNoSecretShapedText(current, label);
      continue;
    }
    if (current === null || typeof current !== "object") continue;
    if (Array.isArray(current)) {
      pending.push(...current);
      continue;
    }
    for (const [key, item] of Object.entries(current as Record<string, unknown>)) {
      if (sensitiveFieldPattern.test(key) && typeof item === "string" && item.length > 0) {
        reject(label);
      }
      pending.push(item);
    }
  }
}

export function assertAuthorityReference(value: string, label: string): void {
  assertNoSecretShapedText(value, label);
  if (!authorityReferencePattern.test(value)) {
    throw new OrchestratorStop(
      "HUMAN_DECISION_REQUIRED",
      `${label} must be a bounded non-secret AUTH-* reference.`,
    );
  }
}

export function assertClosedEnvironment(
  environment: Readonly<Record<string, string>>,
  permittedNames: ReadonlySet<string>,
  label: string,
): void {
  for (const [name, value] of Object.entries(environment)) {
    if (!permittedNames.has(name) || sensitiveFieldPattern.test(name) || value.length > 32_768 || /[\u0000\r\n]/.test(value)) {
      throw new OrchestratorStop(
        "SECRET_REQUIRED",
        `${label} contains a variable outside the closed allowlist.`,
      );
    }
    assertNoSecretShapedText(value, label);
  }
}

export function assertArgumentsContainNoSecretMaterial(
  arguments_: readonly string[],
  label: string,
): void {
  for (const argument of arguments_) assertNoSecretShapedText(argument, label);
}
