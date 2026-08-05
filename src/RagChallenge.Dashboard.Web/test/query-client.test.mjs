// Purpose: Flow-tests the bounded same-origin API client with fake fetch responses and no listener or external network.
import assert from "node:assert/strict";
import test from "node:test";

import { ContractValidationError } from "../src/contracts/api-v1.ts";
import { askQuestion } from "../src/query-client.ts";
import { answeredResponse, rateLimitedProblem } from "./fixtures/query-v1.mjs";

test("posts the frozen request contract without credentials or redirects", async () => {
  let observedInput;
  let observedInit;
  const fakeFetch = async (input, init) => {
    observedInput = input;
    observedInit = init;
    return new Response(JSON.stringify(answeredResponse), {
      status: 200,
      headers: { "content-type": "application/json; charset=utf-8" },
    });
  };

  const result = await askQuestion(
    "Como funciona o MVCC?",
    "pt-BR",
    new AbortController().signal,
    fakeFetch,
  );

  assert.equal(observedInput, "/api/v1/questions");
  assert.equal(observedInit.method, "POST");
  assert.equal(observedInit.credentials, "omit");
  assert.equal(observedInit.redirect, "error");
  assert.equal(observedInit.mode, "same-origin");
  assert.equal(JSON.parse(observedInit.body).questionLanguage, "pt-BR");
  assert.equal(result.kind, "completed");
});

test("returns canonical Problem Details without exposing server prose as UI copy", async () => {
  const fakeFetch = async () =>
    new Response(JSON.stringify(rateLimitedProblem), {
      status: 429,
      headers: { "content-type": "application/problem+json" },
    });

  const result = await askQuestion(
    "What is MVCC?",
    "en-GB",
    new AbortController().signal,
    fakeFetch,
  );

  assert.equal(result.kind, "problem");
  assert.equal(result.problem.code, "CH_QUERY_RATE_LIMITED");
  assert.equal(result.problem.retryAfterSeconds, 3);
});

test("fails closed on incompatible media type, JSON, or body size", async () => {
  const textResponse = async () => new Response("plain text", { status: 200 });
  const invalidJson = async () =>
    new Response("{", { status: 200, headers: { "content-type": "application/json" } });
  const oversized = async () =>
    new Response(JSON.stringify({ padding: "x".repeat(262_145) }), {
      status: 200,
      headers: { "content-type": "application/json" },
    });

  for (const fakeFetch of [textResponse, invalidJson, oversized]) {
    await assert.rejects(
      askQuestion("What is MVCC?", "en-GB", new AbortController().signal, fakeFetch),
      ContractValidationError,
    );
  }
});
