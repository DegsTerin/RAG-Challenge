// Purpose: Acquires narrow atomic resource locks without stealing stale or foreign ownership records.
import { createHash } from "node:crypto";
import { mkdir, open, readdir, rename, unlink } from "node:fs/promises";
import { dirname } from "node:path";
import { canonicalJson } from "../core/canonical-json.js";
import { OrchestratorStop } from "../core/errors.js";
import { assertIdentifier } from "../core/validation.js";
import { assertNoExistingReparseBoundary, readBoundedRegularFile, resolveContained } from "../security/path-policy.js";
import type { PreparedResourceLock, ResourceLockInspection, ResourceLockOwner, ResourceLocks } from "../ports/resource-locks.js";
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

function recordDigest(record: LockRecord): string {
  return createHash("sha256").update(canonicalJson(record), "utf8").digest("hex");
}

function preparedName(record: LockRecord, action: PreparedResourceLock["action"]): string {
  return `${lockName(record.resource)}.${record.runId}.${record.attemptId}.${action === "RELEASE" ? "release" : "reconcile"}`;
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

  public async release(resource: string, runId: string, attemptId: string): Promise<PreparedResourceLock> {
    const record = this.held.get(resource);
    if (record === undefined || record.runId !== runId || record.attemptId !== attemptId) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' is not owned by this attempt.`);
    }
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    const path = resolveContained(this.lockRoot, lockName(resource));
    const text = await readBoundedRegularFile(this.authorityRoot, path, maximumLockBytes, "Resource lock", "SHARED_RESOURCE_COLLISION", record.taskId);
    const persisted = parseLock(parseSecureJson(text, "Resource lock", "SHARED_RESOURCE_COLLISION"));
    if (canonicalJson(persisted) !== canonicalJson(record)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' lock ownership changed.`);
    }
    const tombstoneName = preparedName(record, "RELEASE");
    const tombstone = resolveContained(this.lockRoot, tombstoneName);
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    await rename(path, tombstone);
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    const movedText = await readBoundedRegularFile(this.authorityRoot, tombstone, maximumLockBytes, "Released resource lock", "SHARED_RESOURCE_COLLISION", record.taskId);
    const moved = parseLock(parseSecureJson(movedText, "Released resource lock", "SHARED_RESOURCE_COLLISION"));
    if (canonicalJson(moved) !== canonicalJson(persisted)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", `Resource '${resource}' changed during release.`);
    }
    this.held.delete(resource);
    return { lockId: tombstoneName, resource, action: "RELEASE", recordDigest: recordDigest(moved) };
  }

  public async inspect(): Promise<readonly string[]> {
    return (await this.inspectRecords()).map((record) => record.lockId);
  }

  public async inspectRecords(): Promise<readonly ResourceLockInspection[]> {
    try {
      await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
      const names = (await readdir(this.lockRoot)).sort();
      await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
      return await Promise.all(names.map(async (name): Promise<ResourceLockInspection> => {
        try {
          const canonical = /^[0-9a-f]{64}\.lock$/.test(name);
          const prepared = name.endsWith(".release") ? "RELEASE" : name.endsWith(".reconcile") ? "RECONCILE" : null;
          if (!canonical && prepared === null) throw new Error("invalid");
          const path = resolveContained(this.lockRoot, name);
          const text = await readBoundedRegularFile(this.authorityRoot, path, maximumLockBytes, "Resource lock", "SHARED_RESOURCE_COLLISION");
          const record = parseLock(parseSecureJson(text, "Resource lock", "SHARED_RESOURCE_COLLISION"));
          const digest = recordDigest(record);
          if ((canonical && lockName(record.resource) !== name) || (prepared !== null && preparedName(record, prepared) !== name)) throw new Error("invalid");
          let active = true;
          try { process.kill(record.processId, 0); }
          catch (error) { active = (error as NodeJS.ErrnoException).code !== "ESRCH"; }
          if (prepared !== null) {
            if (active) return { lockId: name, resource: record.resource, status: "ACTIVE", runId: record.runId, taskId: record.taskId, attemptId: record.attemptId, acquiredAt: record.acquiredAt, recordDigest: digest };
            return { lockId: name, resource: record.resource, status: prepared === "RELEASE" ? "RELEASE_PREPARED" : "RECONCILE_PREPARED", runId: record.runId, taskId: record.taskId, attemptId: record.attemptId, acquiredAt: record.acquiredAt, recordDigest: digest };
          }
          return { lockId: name, resource: record.resource, status: active ? "ACTIVE" : "OWNER_PROCESS_ABSENT", runId: record.runId, taskId: record.taskId, attemptId: record.attemptId, acquiredAt: record.acquiredAt, recordDigest: digest };
        } catch {
          return { lockId: name, resource: null, status: "INVALID", runId: null, taskId: null, attemptId: null, acquiredAt: null, recordDigest: null };
        }
      }));
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") {
        return [];
      }
      throw error;
    }
  }

  public async reconcileAbsentOwner(lockId: string, expected: ResourceLockOwner): Promise<PreparedResourceLock> {
    if (!/^[0-9a-f]{64}\.lock$/.test(lockId)) throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Lock reconciliation requires an exact lock identity.");
    assertIdentifier(expected.runId, "reconcile.runId");
    assertIdentifier(expected.taskId, "reconcile.taskId");
    assertIdentifier(expected.attemptId, "reconcile.attemptId");
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    const path = resolveContained(this.lockRoot, lockId);
    const text = await readBoundedRegularFile(this.authorityRoot, path, maximumLockBytes, "Reconciled resource lock", "SHARED_RESOURCE_COLLISION", expected.taskId);
    const record = parseLock(parseSecureJson(text, "Reconciled resource lock", "SHARED_RESOURCE_COLLISION"));
    if (lockName(record.resource) !== lockId || record.runId !== expected.runId || record.taskId !== expected.taskId ||
        record.attemptId !== expected.attemptId || record.acquiredAt !== expected.acquiredAt) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "The reconciled lock does not match the exact interrupted owner.", expected.taskId);
    }
    try {
      process.kill(record.processId, 0);
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "The lock owner process is still active.", expected.taskId);
    } catch (error) {
      if (error instanceof OrchestratorStop) throw error;
      if ((error as NodeJS.ErrnoException).code !== "ESRCH") throw error;
    }
    const tombstoneName = preparedName(record, "RECONCILE");
    const tombstone = resolveContained(this.lockRoot, tombstoneName);
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    await rename(path, tombstone);
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    const movedText = await readBoundedRegularFile(this.authorityRoot, tombstone, maximumLockBytes, "Reconciled lock tombstone", "SHARED_RESOURCE_COLLISION", expected.taskId);
    const moved = parseLock(parseSecureJson(movedText, "Reconciled lock tombstone", "SHARED_RESOURCE_COLLISION"));
    if (canonicalJson(moved) !== canonicalJson(record)) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "The lock changed during reconciliation.", expected.taskId);
    }
    return { lockId: tombstoneName, resource: record.resource, action: "RECONCILE", recordDigest: recordDigest(moved) };
  }

  public async finalise(prepared: PreparedResourceLock, expected: ResourceLockOwner): Promise<void> {
    assertIdentifier(expected.runId, "finalise.runId");
    assertIdentifier(expected.taskId, "finalise.taskId");
    assertIdentifier(expected.attemptId, "finalise.attemptId");
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    const path = resolveContained(this.lockRoot, prepared.lockId);
    const text = await readBoundedRegularFile(this.authorityRoot, path, maximumLockBytes, "Prepared resource lock", "SHARED_RESOURCE_COLLISION", expected.taskId);
    const record = parseLock(parseSecureJson(text, "Prepared resource lock", "SHARED_RESOURCE_COLLISION"));
    if (preparedName(record, prepared.action) !== prepared.lockId || record.resource !== prepared.resource || record.runId !== expected.runId ||
        record.taskId !== expected.taskId || record.attemptId !== expected.attemptId || record.acquiredAt !== expected.acquiredAt ||
        recordDigest(record) !== prepared.recordDigest) {
      throw new OrchestratorStop("SHARED_RESOURCE_COLLISION", "Prepared lock ownership changed before finalisation.", expected.taskId);
    }
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
    await unlink(path);
    await assertNoExistingReparseBoundary(this.authorityRoot, this.lockRoot);
  }
}
