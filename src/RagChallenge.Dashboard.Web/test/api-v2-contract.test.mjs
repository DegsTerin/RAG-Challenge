// Purpose: Pins the frozen API v2 artefact and verifies fail-closed language, visual-selector and notice-obligation decoding with synthetic data only.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import test from "node:test";

import {
  ContractValidationError,
  createPageImageUrl,
  decodeQueryResponse,
} from "../src/contracts/api-v2.ts";
import {
  answeredResponse,
  noticeBearingAnsweredResponse,
} from "./fixtures/query-v2.mjs";

test("pins the separately versioned OpenAPI v2 snapshot", async () => {
  const bytes = await readFile(new URL("../../../docs/api/openapi-v2.json", import.meta.url));
  const document = JSON.parse(bytes.toString("utf8"));

  assert.equal(
    createHash("sha256").update(bytes).digest("hex"),
    "f4dca8db7fb7bd453e580495bb1bb7760812d954344931063e8549ed8f036733",
  );
  assert.deepEqual(Object.keys(document.paths).sort(), [
    "/api/v2/evidence/page-images/{indexGenerationId}/{renderManifestId}/{pageNumber}/{imageContentObjectId}",
    "/api/v2/questions",
  ]);
  assert.equal(document.components.schemas.CitationV2.additionalProperties, false);
  assert.equal(document.components.schemas.PageImageEvidenceV1.additionalProperties, false);
  assert.equal(
    document.components.schemas.DerivativeObligationPresentationV1.additionalProperties,
    false,
  );
  assert.equal(
    document.components.schemas.ObligationSetId.pattern,
    "^obligationset-[a-f0-9]{64}$",
  );
});

test("preserves observed source language and constructs only the frozen relative route", () => {
  const response = decodeQueryResponse(answeredResponse, "pt-BR");
  const citation = response.citations[0];
  const page = citation.pageImages[0];
  const url = createPageImageUrl(response.indexGenerationId, page);

  assert.equal(citation.contentLanguage, "en-GB");
  assert.equal(citation.sourceDeclaredLanguage, "EN-gb");
  assert.equal(
    url,
    `/api/v2/evidence/page-images/${response.indexGenerationId}/${page.renderManifestId}/142/${page.imageContentObjectId}`,
  );
  assert.equal(url.startsWith("/"), true);
  assert.equal(url.includes("://"), false);
  assert.equal(page.obligationSetId, null);
  assert.equal(citation.derivativeObligationPresentation, null);
});

test("accepts one exact obligation set for every notice-bearing page", () => {
  const response = decodeQueryResponse(noticeBearingAnsweredResponse, "pt-BR");
  const citation = response.citations[0];
  const presentation = citation.derivativeObligationPresentation;

  assert.notEqual(presentation, null);
  assert.equal(citation.pageImages[0].obligationSetId, presentation.obligationSetId);
  assert.equal(presentation.contentLanguage, citation.contentLanguage);
  assert.deepEqual(presentation.orderedDisclaimers, [
    "Synthetic first disclaimer.",
    "Synthetic second disclaimer.",
  ]);
});

test("rejects coerced languages, cross-citation pages, duplicate pages and mismatched hashes", () => {
  const pdf = answeredResponse.citations[0];
  const csv = answeredResponse.citations[1];
  const invalidResponses = [
    { ...answeredResponse, citations: [{ ...pdf, contentLanguage: "EN-gb" }, csv] },
    { ...answeredResponse, citations: [{ ...pdf, sourceDeclaredLanguage: "en_GB" }, csv] },
    {
      ...answeredResponse,
      citations: [{ ...pdf, pageImages: [{ ...pdf.pageImages[0], pageNumber: 143 }] }, csv],
    },
    {
      ...answeredResponse,
      citations: [{ ...pdf, pageImages: [pdf.pageImages[0], pdf.pageImages[0]] }, csv],
    },
    {
      ...answeredResponse,
      citations: [{
        ...pdf,
        pageImages: [{ ...pdf.pageImages[0], contentSha256: "d".repeat(64) }],
      }, csv],
    },
    {
      ...answeredResponse,
      citations: [pdf, { ...csv, pageImages: [pdf.pageImages[0]] }],
    },
    { ...answeredResponse, internalAuthority: "must-not-be-accepted" },
  ];

  for (const response of invalidResponses) {
    assert.throws(() => decodeQueryResponse(response, "pt-BR"), ContractValidationError);
  }
});

test("rejects incomplete, mixed or mismatched notice-bearing obligations", () => {
  const pdf = noticeBearingAnsweredResponse.citations[0];
  const csv = noticeBearingAnsweredResponse.citations[1];
  const presentation = pdf.derivativeObligationPresentation;
  const invalidResponses = [
    {
      ...noticeBearingAnsweredResponse,
      citations: [{ ...pdf, derivativeObligationPresentation: null }, csv],
    },
    {
      ...noticeBearingAnsweredResponse,
      citations: [{
        ...pdf,
        pageImages: [{ ...pdf.pageImages[0], obligationSetId: null }],
      }, csv],
    },
    {
      ...noticeBearingAnsweredResponse,
      citations: [{
        ...pdf,
        pageImages: [{
          ...pdf.pageImages[0],
          obligationSetId: `obligationset-${"e".repeat(64)}`,
        }],
      }, csv],
    },
    {
      ...noticeBearingAnsweredResponse,
      citations: [{
        ...pdf,
        derivativeObligationPresentation: {
          ...presentation,
          contentLanguage: "pt-BR",
        },
      }, csv],
    },
    {
      ...noticeBearingAnsweredResponse,
      citations: [{
        ...pdf,
        derivativeObligationPresentation: {
          ...presentation,
          unexpectedAuthority: "must-not-be-accepted",
        },
      }, csv],
    },
    {
      ...noticeBearingAnsweredResponse,
      citations: [{
        ...pdf,
        derivativeObligationPresentation: {
          ...presentation,
          copyrightNotice: "",
        },
      }, csv],
    },
    {
      ...noticeBearingAnsweredResponse,
      citations: [{
        ...pdf,
        derivativeObligationPresentation: {
          ...presentation,
          orderedDisclaimers: Array.from({ length: 17 }, (_, index) =>
            `Synthetic disclaimer ${index + 1}.`),
        },
      }, csv],
    },
    {
      ...noticeBearingAnsweredResponse,
      citations: [pdf, {
        ...csv,
        derivativeObligationPresentation: presentation,
      }],
    },
  ];

  for (const response of invalidResponses) {
    assert.throws(() => decodeQueryResponse(response, "pt-BR"), ContractValidationError);
  }
});
