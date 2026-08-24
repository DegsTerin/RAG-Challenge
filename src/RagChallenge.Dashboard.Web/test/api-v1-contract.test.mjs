// Purpose: Verifies request bounds, response invariants, provenance, and sanitised API v1 failures with synthetic data only.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import test from "node:test";

import {
  ContractValidationError,
  createQueryRequest,
  decodeProblemDetails,
  decodeQueryResponse,
  utf8ByteCount,
} from "../src/contracts/api-v1.ts";
import {
  answeredResponse,
  answeredResponseEnGb,
  insufficientEvidenceResponse,
  rateLimitedProblem,
} from "./fixtures/query-v1.mjs";

test("pins the repository-owned OpenAPI v1 snapshot", async () => {
  const openApiUrl = new URL("../../../docs/api/openapi-v1.json", import.meta.url);
  const bytes = await readFile(openApiUrl);
  const document = JSON.parse(bytes.toString("utf8"));

  assert.equal(
    createHash("sha256").update(bytes).digest("hex").toUpperCase(),
    "D6A686B94C926914BEB28B437F464430A01DE6560C2E2D476CF5C36025813E34",
  );
  assert.deepEqual(Object.keys(document.paths).sort(), [
    "/api/v1/health/live",
    "/api/v1/health/ready",
    "/api/v1/questions",
  ]);
  assert.equal(document.components.schemas.QueryRequestV1.additionalProperties, false);
  assert.equal(document.components.schemas.QueryResponseV1.additionalProperties, false);
});

test("normalises and bounds an API v1 request by UTF-8 bytes", () => {
  const { request, body } = createQueryRequest("  configuração  ", "pt-BR");

  assert.equal(request.question, "configuração");
  assert.equal(request.corpusId, "rag-challenge-product");
  assert.ok(utf8ByteCount(body) <= 8192);
});

test("rejects empty, oversized, and control-character questions", () => {
  assert.throws(() => createQueryRequest("   ", "pt-BR"), ContractValidationError);
  assert.throws(() => createQueryRequest("a".repeat(4097), "en-GB"), ContractValidationError);
  assert.throws(() => createQueryRequest("unsafe\u0000text", "en-GB"), ContractValidationError);
});

test("decodes answered and insufficient-evidence completions", () => {
  const answered = decodeQueryResponse(answeredResponse, "pt-BR");
  const answeredEnGb = decodeQueryResponse(answeredResponseEnGb, "en-GB");
  const insufficient = decodeQueryResponse(insufficientEvidenceResponse, "pt-BR");

  assert.equal(answered.answerLanguage, "pt-BR");
  assert.equal(answeredEnGb.answerLanguage, "en-GB");
  assert.equal(answered.citations[0].sourceTrustClass, "OfficialExternal");
  assert.equal(answered.citations[0].contentLanguage, "en-GB");
  assert.match(answered.citations[0].canonicalUrl, /^https:\/\//);
  assert.equal(answered.citations[1].sourceTrustClass, "LocalAuthorised");
  assert.equal(answered.citations[1].canonicalUrl, null);
  assert.equal(answered.citations[1].sourceFreshness, "Local");
  assert.equal(insufficient.outcome, "InsufficientEvidence");
  assert.equal(insufficient.answer, null);
});

test("rejects completed responses in a language other than the requested language", () => {
  assert.throws(
    () => decodeQueryResponse(answeredResponseEnGb, "pt-BR"),
    ContractValidationError,
  );
  assert.throws(
    () => decodeQueryResponse(answeredResponse, "en-GB"),
    ContractValidationError,
  );
});

test("fails closed on cross-class citation freshness", () => {
  const localCitationIndex = 1;
  const officialCitationIndex = 0;

  assert.throws(
    () =>
      decodeQueryResponse({
        ...answeredResponse,
        citations: answeredResponse.citations.map((citation, index) =>
          index === localCitationIndex
            ? { ...citation, sourceFreshness: "Current" }
            : citation),
      }, "pt-BR"),
    ContractValidationError,
  );
  assert.throws(
    () =>
      decodeQueryResponse({
        ...answeredResponse,
        citations: answeredResponse.citations.map((citation, index) =>
          index === officialCitationIndex
            ? { ...citation, sourceFreshness: "Local" }
            : citation),
      }, "pt-BR"),
    ContractValidationError,
  );
  assert.throws(
    () =>
      decodeQueryResponse({
        ...answeredResponse,
        citations: answeredResponse.citations.map((citation, index) =>
          index === officialCitationIndex
            ? { ...citation, sourceFreshness: "Future" }
            : citation),
      }, "pt-BR"),
    ContractValidationError,
  );
});

test("rejects inconsistent completion and provenance identities", () => {
  assert.throws(
    () => decodeQueryResponse(
      { ...insufficientEvidenceResponse, answer: "unsupported" },
      "pt-BR",
    ),
    ContractValidationError,
  );
  assert.throws(
    () =>
      decodeQueryResponse({
        ...answeredResponse,
        citations: [{ ...answeredResponse.citations[0], indexGenerationId: "other" }],
      }, "pt-BR"),
    ContractValidationError,
  );
  assert.throws(
    () =>
      decodeQueryResponse({
        ...answeredResponse,
        citations: [{ ...answeredResponse.citations[0], canonicalUrl: "javascript:alert(1)" }],
      }, "pt-BR"),
    ContractValidationError,
  );
  assert.throws(
    () =>
      decodeQueryResponse({
        ...answeredResponse,
        citations: answeredResponse.citations.map((citation, index) => index === 1
          ? { ...citation, canonicalUrl: "javascript:alert(1)" }
          : citation),
      }, "pt-BR"),
    ContractValidationError,
  );
  assert.throws(
    () =>
      decodeQueryResponse({
        ...answeredResponse,
        citations: answeredResponse.citations.map((citation, index) => index === 1
          ? { ...citation, canonicalUrl: "https://local.invalid/document.csv" }
          : citation),
      }, "pt-BR"),
    ContractValidationError,
  );
});

test("decodes only canonical sanitised Problem Details", () => {
  assert.deepEqual(decodeProblemDetails(rateLimitedProblem), rateLimitedProblem);
  assert.throws(
    () => decodeProblemDetails({ ...rateLimitedProblem, code: "not-canonical" }),
    ContractValidationError,
  );
  assert.throws(
    () => decodeProblemDetails({ ...rateLimitedProblem, correlationId: "unsafe value" }),
    ContractValidationError,
  );
});
