// Purpose: Defines narrow mutable-resource lock ownership independently from its filesystem implementation.
export interface ResourceLockOwner {
  readonly runId: string;
  readonly taskId: string;
  readonly attemptId: string;
  readonly acquiredAt: string;
}

export interface ResourceLockInspection {
  readonly lockId: string;
  readonly status: "ACTIVE" | "OWNER_PROCESS_ABSENT" | "INVALID";
  readonly runId: string | null;
  readonly taskId: string | null;
  readonly attemptId: string | null;
  readonly acquiredAt: string | null;
}

export interface ResourceLocks {
  acquire(resource: string, owner: ResourceLockOwner): Promise<void>;
  release(resource: string, runId: string, attemptId: string): Promise<void>;
  inspect(): Promise<readonly string[]>;
  inspectRecords(): Promise<readonly ResourceLockInspection[]>;
}
