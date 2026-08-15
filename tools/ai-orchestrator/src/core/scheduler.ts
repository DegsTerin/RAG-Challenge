// Purpose: Selects a deterministic, bounded set of dependency-ready and mutually isolated tasks for each execution wave.
import type { TaskDefinition } from "./contracts.js";
import { taskConflict, assertFrozenContracts } from "./conflicts.js";
import { DependencyGraph } from "./dependency-graph.js";

export interface ScheduleWave {
  readonly tasks: readonly TaskDefinition[];
  readonly deferred: readonly { readonly taskId: string; readonly conflictsWith: string; readonly reasons: readonly string[] }[];
}

export function scheduleWave(tasks: readonly TaskDefinition[], maximumConcurrency: number): ScheduleWave {
  const candidates = new DependencyGraph(tasks).groups().ready;
  const selected: TaskDefinition[] = [];
  const deferred: { taskId: string; conflictsWith: string; reasons: readonly string[] }[] = [];

  for (const candidate of candidates) {
    assertFrozenContracts(candidate);
    if (selected.length >= maximumConcurrency) {
      break;
    }
    const conflict = selected.map((task) => taskConflict(candidate, task)).find((value) => value !== null);
    if (conflict === undefined) {
      selected.push(candidate);
    } else {
      deferred.push({
        taskId: candidate.taskId,
        conflictsWith: conflict.rightTaskId,
        reasons: conflict.reasons,
      });
    }
  }
  return { tasks: selected, deferred };
}
