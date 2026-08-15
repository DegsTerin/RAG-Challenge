// Purpose: Acquires narrow atomic resource locks without stealing stale or foreign ownership records.
import { createHash } from "node:crypto";
import { mkdir, open, readFile, readdir, unlink } from "node:fs/promises";
import { canonicalJson } from "../core/canonical-json.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";
import { assertNoExistingReparseBoundary, resolveContained } from "../security/path-policy.js";
import type { ResourceLocks } from "../ports/resource-locks.js";

interface LockRecord {
  readonly schemaVersion: 1;
  readonly resource: string;
  readonly runId: string;
  readonly taskId: string;
  readonly attemptId: string;
  readonly acquiredAt: string;
}

function lockName(resource: string): string {
  return `${createHash("sha256").update(resource, "utf8").digest("hex")}.lock`;
}

export class FileResourceLocks implements ResourceLocks {
  private readonly held = new Map<string, LockRecord>();

  public constructor(private readonly lockRoot: string) {}

  public async acquire(resource: string, record: Omit<LockRecord, "schemaVersion" | "resource">): Promise<void> {
    if (resource.length === 0 || resource.length > 128 || /[\u0000-\u001f]/.test(resource)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Resource identity is invalid.");
    }
    assertIdentifier(record.runId, "runId");
    assertIdentifier(record.taskId, "taskId");
    assertIdentifier(record.attemptId, "attemptId");
    await assertNoExistingReparseBoundary(this.lockRoot, this.lockRoot);
    await mkdir(this.lockRoot, { recursive: true });
    const path = resolveContained(this.lockRoot, lockName(resource));
    const value: LockRecord = { schemaVersion: 1, resource, ...record };
    let handle;
    try {
      handle = await open(path, "wx", 0o600);
      await handle.writeFile(canonicalJson(value), { encoding: "utf8" });
      await handle.sync();
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EEXIST") {
        throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' is already locked.`, record.taskId);
      }
      throw error;
    } finally {
      await handle?.close();
    }
    this.held.set(resource, value);
  }

  public async release(resource: string, runId: string, attemptId: string): Promise<void> {
    const record = this.held.get(resource);
    if (record === undefined || record.runId !== runId || record.attemptId !== attemptId) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' is not owned by this attempt.`);
    }
    const path = resolveContained(this.lockRoot, lockName(resource));
    const persisted = JSON.parse(await readFile(path, "utf8")) as LockRecord;
    if (persisted.runId !== runId || persisted.attemptId !== attemptId || persisted.resource !== resource) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' lock ownership changed.`);
    }
    await unlink(path);
    this.held.delete(resource);
  }

  public async inspect(): Promise<readonly string[]> {
    try {
      return (await readdir(this.lockRoot)).filter((name) => name.endsWith(".lock")).sort();
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") {
        return [];
      }
      throw error;
    }
  }
}
