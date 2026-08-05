// Purpose: Flow-tests the bounded same-origin API client with fake fetch responses and no listener or external network.
import assert from "node:assert/strict";
import test from "node:test";

import { ContractValidationError } from "../src/contracts/api-v1.ts";
import { askQuestion } from "../src/query-client.ts";
import {
  answeredResponse,
  answeredResponseEnGb,
  rateLimitedProblem,
} from "./fixtures/query-v1.mjs";

const maximumResponseBytes = 262_144;

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

test("binds completed responses to the question language sent", async () => {
  for (const [questionLanguage, response] of [
    ["pt-BR", answeredResponse],
    ["en-GB", answeredResponseEnGb],
  ]) {
    const fakeFetch = async () =>
      new Response(JSON.stringify(response), {
        status: 200,
        headers: { "content-type": "application/json" },
      });

    const result = await askQuestion(
      "Synthetic question",
      questionLanguage,
      new AbortController().signal,
      fakeFetch,
    );

    assert.equal(result.kind, "completed");
    assert.equal(result.response.answerLanguage, questionLanguage);
  }

  for (const [questionLanguage, response] of [
    ["pt-BR", answeredResponseEnGb],
    ["en-GB", answeredResponse],
  ]) {
    const fakeFetch = async () =>
      new Response(JSON.stringify(response), {
        status: 200,
        headers: { "content-type": "application/json" },
      });

    await assert.rejects(
      askQuestion(
        "Synthetic question",
        questionLanguage,
        new AbortController().signal,
        fakeFetch,
      ),
      ContractValidationError,
    );
  }
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

test("accepts a valid response at the exact byte ceiling", async () => {
  const responseText = createPaddedResponse(maximumResponseBytes, answeredResponseEnGb);
  const fakeFetch = async () =>
    new Response(responseText, {
      status: 200,
      headers: { "content-type": "application/json" },
    });

  const result = await askQuestion(
    "What is MVCC?",
    "en-GB",
    new AbortController().signal,
    fakeFetch,
  );

  assert.equal(new TextEncoder().encode(responseText).byteLength, maximumResponseBytes);
  assert.equal(result.kind, "completed");
});

test("cancels incremental reading at the first byte beyond the ceiling", async () => {
  let pulls = 0;
  let cancelled = false;
  const body = new ReadableStream(
    {
      pull(controller) {
        pulls += 1;
        controller.enqueue(
          pulls === 1
            ? new Uint8Array(maximumResponseBytes).fill(0x20)
            : Uint8Array.of(0x7b),
        );
      },
      cancel() {
        cancelled = true;
      },
    },
    { highWaterMark: 0 },
  );
  const fakeFetch = async () =>
    new Response(body, {
      status: 200,
      headers: { "content-type": "application/json" },
    });

  await assert.rejects(
    askQuestion("What is MVCC?", "en-GB", new AbortController().signal, fakeFetch),
    ContractValidationError,
  );

  assert.equal(pulls, 2);
  assert.equal(cancelled, true);
});

test("rejects an oversized declared length before acquiring the body reader", async () => {
  let readerAcquired = false;
  let cancelled = false;
  const fakeFetch = async () => ({
    ok: true,
    headers: new Headers({
      "content-length": String(maximumResponseBytes + 1),
      "content-type": "application/json",
    }),
    body: {
      getReader() {
        readerAcquired = true;
        throw new Error("The body reader must not be acquired.");
      },
      async cancel() {
        cancelled = true;
      },
    },
  });

  await assert.rejects(
    askQuestion("What is MVCC?", "en-GB", new AbortController().signal, fakeFetch),
    ContractValidationError,
  );

  assert.equal(readerAcquired, false);
  assert.equal(cancelled, true);
});

test("preserves cancellation while streaming the response body", async () => {
  const abortController = new AbortController();
  const fakeFetch = async (_input, init) => {
    const body = new ReadableStream({
      start(controller) {
        init.signal.addEventListener(
          "abort",
          () => controller.error(new DOMException("The operation was aborted.", "AbortError")),
          { once: true },
        );
      },
    });

    return new Response(body, {
      status: 200,
      headers: { "content-type": "application/json" },
    });
  };
  const pendingQuery = askQuestion(
    "What is MVCC?",
    "en-GB",
    abortController.signal,
    fakeFetch,
  );

  abortController.abort();

  await assert.rejects(
    pendingQuery,
    (error) => error instanceof DOMException && error.name === "AbortError",
  );
});

function createPaddedResponse(byteLength, response = answeredResponse) {
  const emptyResponse = JSON.stringify({ ...response, padding: "" });
  const paddingLength = byteLength - new TextEncoder().encode(emptyResponse).byteLength;
  assert.ok(paddingLength >= 0);
  return JSON.stringify({ ...response, padding: "x".repeat(paddingLength) });
}
