// Purpose: Defines recoverable orchestrator state persistence independently from the filesystem adapter.
import type { PersistedRunState } from "../core/contracts.js";

export interface StateStore {
  save(state: PersistedRunState): Promise<void>;
  load(runId: string): Promise<PersistedRunState>;
}
