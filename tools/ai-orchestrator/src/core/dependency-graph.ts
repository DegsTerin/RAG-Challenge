// Purpose: Resolves task dependencies deterministically and rejects missing, circular, contradictory or impossible graphs.
import type { TaskDefinition, TaskStatus } from "./contracts.js";
import { OrchestratorStop } from "./errors.js";

export interface TaskGroups {
  readonly ready: readonly TaskDefinition[];
  readonly blocked: readonly TaskDefinition[];
  readonly running: readonly TaskDefinition[];
  readonly completed: readonly TaskDefinition[];
  readonly failed: readonly TaskDefinition[];
}

const runningStatuses = new Set<TaskStatus>([
  "ASSIGNED", "RUNNING", "TESTING", "REVIEW", "INTEGRATION_READY", "INTEGRATING", "VALIDATING",
]);
const terminalFailure = new Set<TaskStatus>(["FAIL", "CANCELLED", "HUMAN_REVIEW_REQUIRED"]);

function isSuccessful(task: TaskDefinition): boolean {
  return task.status === "PASS" || (task.taskKind === "IMPLEMENTATION" && task.status === "IMPLEMENTED");
}

function sorted(tasks: readonly TaskDefinition[]): TaskDefinition[] {
  return [...tasks].sort((left, right) => right.priority - left.priority || left.taskId.localeCompare(right.taskId, "en"));
}

export class DependencyGraph {
  private readonly byId: ReadonlyMap<string, TaskDefinition>;

  public constructor(private readonly tasks: readonly TaskDefinition[]) {
    this.byId = new Map(tasks.map((task) => [task.taskId, task]));
    this.validate();
  }

  public task(taskId: string): TaskDefinition {
    const task = this.byId.get(taskId);
    if (task === undefined) {
      throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `Task '${taskId}' does not exist.`);
    }
    return task;
  }

  public dependenciesSatisfied(task: TaskDefinition): boolean {
    return task.dependencies.every((dependency) => isSuccessful(this.task(dependency)));
  }

  public hasFailedDependency(task: TaskDefinition): boolean {
    return task.dependencies.some((dependency) => terminalFailure.has(this.task(dependency).status));
  }

  public groups(): TaskGroups {
    const ready = this.tasks.filter((task) =>
      (task.status === "DISCOVERED" || task.status === "READY") &&
      task.blockedBy.length === 0 && this.dependenciesSatisfied(task));
    const blocked = this.tasks.filter((task) =>
      task.status === "BLOCKED" || task.blockedBy.length > 0 ||
      (!this.dependenciesSatisfied(task) && !this.hasFailedDependency(task)));
    const running = this.tasks.filter((task) => runningStatuses.has(task.status));
    const completed = this.tasks.filter(isSuccessful);
    const failed = this.tasks.filter((task) => terminalFailure.has(task.status) || this.hasFailedDependency(task));
    return {
      ready: sorted(ready),
      blocked: sorted(blocked),
      running: sorted(running),
      completed: sorted(completed),
      failed: sorted(failed),
    };
  }

  private validate(): void {
    for (const task of this.tasks) {
      const references = [...task.dependencies, ...task.blockedBy];
      if (references.includes(task.taskId)) {
        throw new OrchestratorStop("CONFLICTING_REQUIREMENTS", `Task '${task.taskId}' refers to itself.`, task.taskId);
      }
      for (const dependency of task.dependencies) {
        if (!this.byId.has(dependency)) {
          throw new OrchestratorStop(
            "CONFLICTING_REQUIREMENTS",
            `Task '${task.taskId}' has missing dependency '${dependency}'.`,
            task.taskId,
          );
        }
      }
      const contradiction = task.dependencies.find((dependency) => task.blockedBy.includes(dependency));
      if (contradiction !== undefined) {
        throw new OrchestratorStop(
          "CONFLICTING_REQUIREMENTS",
          `Task '${task.taskId}' both depends on and is blocked by '${contradiction}'.`,
          task.taskId,
        );
      }
    }

    const visiting = new Set<string>();
    const visited = new Set<string>();
    const visit = (taskId: string, path: readonly string[]): void => {
      if (visiting.has(taskId)) {
        throw new OrchestratorStop(
          "CONFLICTING_REQUIREMENTS",
          `Dependency cycle detected: ${[...path, taskId].join(" -> ")}.`,
          taskId,
        );
      }
      if (visited.has(taskId)) {
        return;
      }
      visiting.add(taskId);
      for (const dependency of this.task(taskId).dependencies) {
        visit(dependency, [...path, taskId]);
      }
      visiting.delete(taskId);
      visited.add(taskId);
    };

    for (const task of this.tasks) {
      visit(task.taskId, []);
    }
  }
}
