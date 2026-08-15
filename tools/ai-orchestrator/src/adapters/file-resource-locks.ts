// Purpose: Acquires narrow atomic resource locks without stealing stale or foreign ownership records.
import { createHash } from "node:crypto";
import { mkdir, open, readFile, readdir, stat, unlink } from "node:fs/promises";
import { dirname } from "node:path";
import { canonicalJson } from "../core/canonical-json.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";
import { assertNoExistingReparseBoundary, resolveContained } from "../security/path-policy.js";
import type { ResourceLockInspection, ResourceLockOwner, ResourceLocks } from "../ports/resource-locks.js";
import { parseSecureJson } from "../security/secure-json.js";

interface LockRecord {
  readonly schemaVersion: 1;
  readonly resource: string;
  readonly runId: string;
  readonly taskId: string;
  readonly attemptId: string;
  readonly acquiredAt: string;
  readonly processId: number;
}

const maximumLockBytes = 8_192;

function parseLock(value: unknown): LockRecord {
  if (value === null || typeof value !== "object" || Array.isArray(value)) throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "A lock record is invalid.");
  const source = value as Record<string, unknown>;
  if (Object.keys(source).sort().join(",") !== "acquiredAt,attemptId,processId,resource,runId,schemaVersion,taskId" || source.schemaVersion !== 1 ||
      typeof source.resource !== "string" || typeof source.runId !== "string" || typeof source.taskId !== "string" ||
      typeof source.attemptId !== "string" || typeof source.acquiredAt !== "string" || !Number.isInteger(source.processId) || (source.processId as number) <= 0 ||
      !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?Z$/.test(source.acquiredAt) || Number.isNaN(Date.parse(source.acquiredAt))) {
    throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "A lock record failed its closed contract.");
  }
  if (source.resource.length === 0 || source.resource.length > 128 || /[\u0000-\u001f]/.test(source.resource)) {
    throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "A lock resource identity is invalid.");
  }
  assertIdentifier(source.runId, "lock.runId");
  assertIdentifier(source.taskId, "lock.taskId");
  assertIdentifier(source.attemptId, "lock.attemptId");
  return source as unknown as LockRecord;
}

function lockName(resource: string): string {
  return `${createHash("sha256").update(resource, "utf8").digest("hex")}.lock`;
}

export class FileResourceLocks implements ResourceLocks {
  private readonly held = new Map<string, LockRecord>();

  public constructor(private readonly lockRoot: string, private readonly authorityRoot: string = dirname(lockRoot)) {}

  public async acquire(resource: string, record: ResourceLockOwner): Promise<void> {
    if (resource.length === 0 || resource.length > 128 || /[\u0000-\u001f]/.test(resource)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Resource identity is invalid.");
    }
    assertIdentifier(record.runId, "runId");
    assertIdentifier(record.taskId, "taskId");
    assertIdentifier(record.attemptId, "attemptId");
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    await mkdir(this.lockRoot, { recursive: true });
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    const path = resolveContained(this.lockRoot, lockName(resource));
    const value: LockRecord = { schemaVersion: 1, resource, ...record, processId: process.pid };
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
    const metadata = await stat(path);
    if (!metadata.isFile() || metadata.size > maximumLockBytes) throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' lock is oversized.`);
    const persisted = parseLock(parseSecureJson(await readFile(path, "utf8"), "Resource lock", "SHARED_RESOURCE_COLLISION"));
    if (persisted.runId !== runId || persisted.attemptId !== attemptId || persisted.resource !== resource) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' lock ownership changed.`);
    }
    await unlink(path);
    this.held.delete(resource);
  }

  public async inspect(): Promise<readonly string[]> {
    return (await this.inspectRecords()).map((record) => record.lockId);
  }

  public async inspectRecords(): Promise<readonly ResourceLockInspection[]> {
    try {
      const names = (await readdir(this.lockRoot)).sort();
      return await Promise.all(names.map(async (name): Promise<ResourceLockInspection> => {
        try {
          if (!/^[0-9a-f]{64}\.lock$/.test(name)) throw new Error("invalid");
          const path = resolveContained(this.lockRoot, name);
          const metadata = await stat(path);
          if (!metadata.isFile() || metadata.size > maximumLockBytes) throw new Error("invalid");
          const record = parseLock(parseSecureJson(await readFile(path, "utf8"), "Resource lock", "SHARED_RESOURCE_COLLISION"));
          let active = true;
          try { process.kill(record.processId, 0); }
          catch (error) { active = (error as NodeJS.ErrnoException).code !== "ESRCH"; }
          return { lockId: name, status: active ? "ACTIVE" : "OWNER_PROCESS_ABSENT", runId: record.runId, taskId: record.taskId, attemptId: record.attemptId, acquiredAt: record.acquiredAt };
        } catch {
          return { lockId: name, status: "INVALID", runId: null, taskId: null, attemptId: null, acquiredAt: null };
        }
      }));
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") {
        return [];
      }
      throw error;
    }
  }
}
