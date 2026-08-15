// Purpose: Defines narrow mutable-resource lock ownership independently from its filesystem implementation.
export interface ResourceLockOwner {
  readonly runId: string;
  readonly taskId: string;
  readonly attemptId: string;
  readonly acquiredAt: string;
}

export interface ResourceLockInspection {
  readonly lockId: string;
  readonly resource: string | null;
  readonly status: "ACTIVE" | "OWNER_PROCESS_ABSENT" | "RELEASE_PREPARED" | "RECONCILE_PREPARED" | "INVALID";
  readonly runId: string | null;
  readonly taskId: string | null;
  readonly attemptId: string | null;
  readonly acquiredAt: string | null;
  readonly recordDigest: string | null;
}

export interface PreparedResourceLock {
  readonly lockId: string;
  readonly resource: string;
  readonly action: "RELEASE" | "RECONCILE";
  readonly recordDigest: string;
}

export interface ResourceLocks {
  acquire(resource: string, owner: ResourceLockOwner): Promise<void>;
  release(resource: string, runId: string, attemptId: string): Promise<PreparedResourceLock>;
  inspect(): Promise<readonly string[]>;
  inspectRecords(): Promise<readonly ResourceLockInspection[]>;
  reconcileAbsentOwner(lockId: string, expected: ResourceLockOwner): Promise<PreparedResourceLock>;
  finalise(prepared: PreparedResourceLock, expected: ResourceLockOwner): Promise<void>;
}
