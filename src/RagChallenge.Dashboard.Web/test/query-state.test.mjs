// Purpose: Verifies deterministic query-state transitions and refusal of stale asynchronous completions.
import assert from "node:assert/strict";
import test from "node:test";

import { initialQueryState, queryReducer } from "../src/state/query-state.ts";
import { answeredResponse, rateLimitedProblem } from "./fixtures/query-v1.mjs";

test("moves from idle through submission to completion", () => {
  const submitting = queryReducer(initialQueryState, { type: "begin", requestId: 1 });
  const completed = queryReducer(submitting, {
    type: "complete",
    requestId: 1,
    response: answeredResponse,
  });

  assert.equal(submitting.phase, "submitting");
  assert.equal(completed.phase, "completed");
  assert.equal(completed.response.outcome, "Answered");
});

test("keeps the newest request when an older completion arrives", () => {
  const first = queryReducer(initialQueryState, { type: "begin", requestId: 1 });
  const second = queryReducer(first, { type: "begin", requestId: 2 });
  const staleCompletion = queryReducer(second, {
    type: "complete",
    requestId: 1,
    response: answeredResponse,
  });

  assert.equal(staleCompletion, second);
  assert.equal(staleCompletion.activeRequestId, 2);
});

test("records canonical and client-side failures separately", () => {
  const submitting = queryReducer(initialQueryState, { type: "begin", requestId: 1 });
  const problem = queryReducer(submitting, {
    type: "failProblem",
    requestId: 1,
    problem: rateLimitedProblem,
  });
  const resubmitting = queryReducer(problem, { type: "begin", requestId: 2 });
  const clientFailure = queryReducer(resubmitting, {
    type: "failClient",
    requestId: 2,
    failure: "ResponseIncompatible",
  });

  assert.equal(problem.problem.code, "CH_QUERY_RATE_LIMITED");
  assert.equal(clientFailure.clientFailure, "ResponseIncompatible");
  assert.equal(clientFailure.problem, null);
});
