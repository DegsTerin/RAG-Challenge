// Purpose: Pins the frozen API v2 artefact and verifies fail-closed BCP 47 and composite visual-selector decoding with synthetic data only.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import test from "node:test";

import {
  ContractValidationError,
  createPageImageUrl,
  decodeQueryResponse,
} from "../src/contracts/api-v2.ts";
import { answeredResponse } from "./fixtures/query-v2.mjs";

test("pins the separately versioned OpenAPI v2 snapshot", async () => {
  const bytes = await readFile(new URL("../../../docs/api/openapi-v2.json", import.meta.url));
  const document = JSON.parse(bytes.toString("utf8"));

  assert.equal(
    createHash("sha256").update(bytes).digest("hex"),
    "01ab26ae8066971af2e5ae83ec828fae556951d5ce6c335b42f6e7cf7b062640",
  );
  assert.deepEqual(Object.keys(document.paths).sort(), [
    "/api/v2/evidence/page-images/{indexGenerationId}/{renderManifestId}/{pageNumber}/{imageContentObjectId}",
    "/api/v2/questions",
  ]);
  assert.equal(document.components.schemas.CitationV2.additionalProperties, false);
  assert.equal(document.components.schemas.PageImageEvidenceV1.additionalProperties, false);
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
