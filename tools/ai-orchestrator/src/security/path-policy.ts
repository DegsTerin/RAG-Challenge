// Purpose: Resolves orchestrator-owned paths fail-closed and rejects traversal, alternate streams and existing reparse boundaries.
import { lstat } from "node:fs/promises";
import { isAbsolute, relative, resolve, sep } from "node:path";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";

export function resolveContained(root: string, ...segments: readonly string[]): string {
  const absoluteRoot = resolve(root);
  for (const segment of segments) {
    if (segment.length === 0 || isAbsolute(segment) || segment.includes(":") || /[\u0000-\u001f]/.test(segment)) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "A filesystem segment is absolute or ambiguous.");
    }
  }
  const candidate = resolve(absoluteRoot, ...segments);
  const relation = relative(absoluteRoot, candidate);
  if (relation === "" || relation === ".." || relation.startsWith(`..${sep}`) || isAbsolute(relation)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "A filesystem path escapes its authorised root.");
  }
  return candidate;
}

export async function assertNoExistingReparseBoundary(root: string, candidate: string): Promise<void> {
  const absoluteRoot = resolve(root);
  const relation = relative(absoluteRoot, resolve(candidate));
  if (relation === ".." || relation.startsWith(`..${sep}`) || isAbsolute(relation)) {
    throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "A filesystem path escapes its authorised root.");
  }
  try {
    if ((await lstat(absoluteRoot)).isSymbolicLink()) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The authorised root is a symbolic-link boundary.");
    }
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code !== "ENOENT") {
      throw error;
    }
  }
  const parts = relation === "" ? [] : relation.split(sep);
  let current = absoluteRoot;
  for (const part of parts) {
    current = resolve(current, part);
    try {
      const metadata = await lstat(current);
      if (metadata.isSymbolicLink()) {
        throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "An existing path crosses a symbolic-link boundary.");
      }
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") {
        return;
      }
      throw error;
    }
  }
}

export function resolveRunRoot(stateRoot: string, runId: string): string {
  assertIdentifier(runId, "runId");
  return resolveContained(stateRoot, runId);
}
