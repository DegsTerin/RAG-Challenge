// Purpose: Defines coordinator-owned worktree operations independently from task and agent output contracts.
export interface WorktreeRecord {
  readonly path: string;
  readonly head: string;
  readonly branch: string | null;
  readonly prunable: boolean;
}

export interface WorktreeManager {
  list(): Promise<readonly WorktreeRecord[]>;
  create(taskId: string, path: string, branch: string, baseline: string): Promise<WorktreeRecord>;
  validate(path: string, branch: string, baseline: string): Promise<WorktreeRecord>;
  removeManaged(taskId: string, path: string): Promise<void>;
}
