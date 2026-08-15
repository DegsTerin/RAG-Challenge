// Purpose: Restricts automatic retries to explicitly classified transient failures and preserves all other failure histories.
import type { RetryClass } from "./contracts.js";

export class ClassifiedFailure extends Error {
  public constructor(public readonly retryClass: RetryClass, message: string) {
    super(message);
    this.name = "ClassifiedFailure";
  }
}

export function mayRetry(retryClass: RetryClass, attemptNumber: number, maximumAttempts: number): boolean {
  return retryClass === "TRANSIENT_FAILURE" && attemptNumber < maximumAttempts;
}
