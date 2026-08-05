// Purpose: Owns deterministic query-view transitions so late or cancelled requests cannot overwrite newer Dashboard state.
import type { ProblemDetailsV1, QueryResponseV1 } from "../contracts/api-v1";

export type ClientFailureKind =
  | "InvalidQuestion"
  | "NetworkUnavailable"
  | "RequestCancelled"
  | "ResponseIncompatible";

export type QueryPhase = "idle" | "submitting" | "completed" | "failed";

export interface QueryState {
  phase: QueryPhase;
  activeRequestId: number | null;
  response: QueryResponseV1 | null;
  problem: ProblemDetailsV1 | null;
  clientFailure: ClientFailureKind | null;
}

export type QueryAction =
  | { type: "begin"; requestId: number }
  | { type: "complete"; requestId: number; response: QueryResponseV1 }
  | { type: "failProblem"; requestId: number; problem: ProblemDetailsV1 }
  | { type: "failClient"; requestId: number; failure: ClientFailureKind }
  | { type: "reset" };

export const initialQueryState: QueryState = {
  phase: "idle",
  activeRequestId: null,
  response: null,
  problem: null,
  clientFailure: null,
};

export function queryReducer(state: QueryState, action: QueryAction): QueryState {
  switch (action.type) {
    case "begin":
      return {
        phase: "submitting",
        activeRequestId: action.requestId,
        response: null,
        problem: null,
        clientFailure: null,
      };
    case "complete":
      if (state.activeRequestId !== action.requestId) {
        return state;
      }

      return {
        phase: "completed",
        activeRequestId: null,
        response: action.response,
        problem: null,
        clientFailure: null,
      };
    case "failProblem":
      if (state.activeRequestId !== action.requestId) {
        return state;
      }

      return {
        phase: "failed",
        activeRequestId: null,
        response: null,
        problem: action.problem,
        clientFailure: null,
      };
    case "failClient":
      if (state.activeRequestId !== action.requestId) {
        return state;
      }

      return {
        phase: "failed",
        activeRequestId: null,
        response: null,
        problem: null,
        clientFailure: action.failure,
      };
    case "reset":
      return initialQueryState;
  }
}
