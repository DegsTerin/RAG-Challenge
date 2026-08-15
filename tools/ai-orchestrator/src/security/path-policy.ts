// Purpose: Resolves orchestrator-owned paths fail-closed and rejects traversal, alternate streams and existing reparse boundaries.
import { constants } from "node:fs";
import { lstat, open, realpath } from "node:fs/promises";
import { isAbsolute, relative, resolve, sep } from "node:path";
import type { StopCode } from "../core/contracts.js";
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
  let physicalRoot: string;
  try {
    if ((await lstat(absoluteRoot)).isSymbolicLink()) {
      throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The authorised root is a symbolic-link boundary.");
    }
    physicalRoot = await realpath(absoluteRoot);
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === "ENOENT") throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "The authorised root does not exist.");
    throw error;
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
      const physical = await realpath(current);
      const physicalRelation = relative(physicalRoot, physical);
      if (physicalRelation === ".." || physicalRelation.startsWith(`..${sep}`) || isAbsolute(physicalRelation)) {
        throw new OrchestratorStop("OUT_OF_SCOPE_CHANGE_REQUIRED", "An existing path escapes its authorised physical root.");
      }
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") {
        return;
      }
      throw error;
    }
  }
}

export async function readBoundedRegularFile(
  authorityRoot: string,
  path: string,
  maximumBytes: number,
  label: string,
  stopCode: StopCode = "TEST_BASELINE_BROKEN",
  taskId?: string,
): Promise<string> {
  await assertNoExistingReparseBoundary(authorityRoot, path);
  const before = await lstat(path, { bigint: true });
  if (before.isSymbolicLink() || !before.isFile() || before.size > BigInt(maximumBytes)) {
    throw new OrchestratorStop(stopCode, `${label} is not a bounded regular file.`, taskId);
  }
  const flags = constants.O_RDONLY | (process.platform === "win32" ? 0 : constants.O_NOFOLLOW);
  const handle = await open(path, flags);
  let text: string;
  try {
    const opened = await handle.stat({ bigint: true });
    if (!opened.isFile() || opened.dev !== before.dev || opened.ino !== before.ino || opened.size !== before.size) {
      throw new OrchestratorStop(stopCode, `${label} changed before it could be opened safely.`, taskId);
    }
    const content = await handle.readFile();
    const after = await handle.stat({ bigint: true });
    if (after.dev !== opened.dev || after.ino !== opened.ino || after.size !== opened.size ||
        after.mtimeMs !== opened.mtimeMs || content.byteLength > maximumBytes) {
      throw new OrchestratorStop(stopCode, `${label} changed while it was being read.`, taskId);
    }
    text = content.toString("utf8");
  } finally {
    await handle.close();
  }
  await assertNoExistingReparseBoundary(authorityRoot, path);
  const current = await lstat(path, { bigint: true });
  if (current.isSymbolicLink() || current.dev !== before.dev || current.ino !== before.ino || current.size !== before.size || current.mtimeMs !== before.mtimeMs) {
    throw new OrchestratorStop(stopCode, `${label} changed after it was read.`, taskId);
  }
  return text;
}

export function resolveRunRoot(stateRoot: string, runId: string): string {
  assertIdentifier(runId, "runId");
  return resolveContained(stateRoot, runId);
}
