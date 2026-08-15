// Purpose: Persists an agent thread identity before a turn can be interrupted so recovery can resume fail-closed.
import type { ThreadCheckpoint } from "../core/contracts.js";

export interface ThreadCheckpointStore {
  save(checkpoint: ThreadCheckpoint): Promise<void>;
  load(runId: string, taskId: string): Promise<ThreadCheckpoint | null>;
  remove(expected: ThreadCheckpoint): Promise<void>;
  inspect(runId: string): Promise<readonly ThreadCheckpoint[]>;
  inspectPreparedRemovals(runId: string): Promise<readonly ThreadCheckpoint[]>;
  finalisePreparedRemoval(expected: ThreadCheckpoint): Promise<void>;
}
