// Purpose: Component-tests safe, localised presentation of API v1 results, coverage, provenance, citations, and failures.
import assert from "node:assert/strict";
import test from "node:test";
import { createElement } from "react";
import { renderToStaticMarkup } from "react-dom/server";
import { createServer } from "vite";

import {
  answeredResponse,
  insufficientEvidenceResponse,
  rateLimitedProblem,
} from "./fixtures/query-v1.mjs";

const vite = await createServer({
  logLevel: "silent",
  server: { middlewareMode: true },
  appType: "custom",
});

test.after(async () => {
  await vite.close();
});

test("presents coverage and both official PDF and authorised local CSV citations", async () => {
  const { QueryResultPanel } = await vite.ssrLoadModule("/src/App.tsx");

  for (const interfaceLanguage of ["pt-BR", "en-GB"]) {
    const html = renderResult(QueryResultPanel, interfaceLanguage, {
      phase: "completed",
      activeRequestId: null,
      response: answeredResponse,
      problem: null,
      clientFailure: null,
    });

    assert.match(html, /lang="pt-BR"/);
    assert.match(html, /lang="en-GB"/);
    assert.match(html, /Official external source|Fonte oficial externa/);
    assert.match(html, /Authorised local document|Documento local autorizado/);
    assert.match(html, /Pages?: 142|Páginas?: 142/);
    assert.match(html, /Records?: 7–9|Registros?: 7–9/);
    assert.match(html, /recurso, estado/);
    assert.match(html, /rel="noopener noreferrer"/);
    assert.match(html, /target="_blank"/);
    assert.match(html, /source-synthetic-stale/);
    assert.match(html, /correlation_synthetic_001/);
  }
});

test("keeps insufficient evidence distinct from an empty answer and still reports coverage", async () => {
  const { QueryResultPanel } = await vite.ssrLoadModule("/src/App.tsx");
  const html = renderResult(QueryResultPanel, "en-GB", {
    phase: "completed",
    activeRequestId: null,
    response: insufficientEvidenceResponse,
    problem: null,
    clientFailure: null,
  });

  assert.match(html, /Insufficient evidence/);
  assert.match(html, /Evaluated coverage/);
  assert.equal(html.includes("Cited evidence"), false);
  assert.equal(html.includes("<blockquote"), false);
});

test("maps canonical failures to owned copy and never exposes server prose", async () => {
  const { QueryResultPanel } = await vite.ssrLoadModule("/src/App.tsx");
  const hostileProblem = {
    ...rateLimitedProblem,
    title: "<script>server title</script>",
    detail: "secret server detail",
  };
  const html = renderResult(QueryResultPanel, "en-GB", {
    phase: "failed",
    activeRequestId: null,
    response: null,
    problem: hostileProblem,
    clientFailure: null,
  });

  assert.match(html, /temporary query limit has been reached/);
  assert.match(html, /Try again after 3 seconds/);
  assert.match(html, /correlation_synthetic_002/);
  assert.equal(html.includes("server title"), false);
  assert.equal(html.includes("secret server detail"), false);
});

test("escapes answer and evidence strings without an HTML or Markdown rendering sink", async () => {
  const { QueryResultPanel } = await vite.ssrLoadModule("/src/App.tsx");
  const hostileResponse = {
    ...answeredResponse,
    answer: "<img src=x onerror=alert(1)> **not Markdown**",
    citations: answeredResponse.citations.map((citation, index) => index === 0
      ? {
          ...citation,
          title: "<script>alert(1)</script>",
          excerpt: "<a href='javascript:alert(1)'>hostile evidence</a>",
        }
      : citation),
  };
  const html = renderResult(QueryResultPanel, "pt-BR", {
    phase: "completed",
    activeRequestId: null,
    response: hostileResponse,
    problem: null,
    clientFailure: null,
  });

  assert.match(html, /&lt;img src=x onerror=alert\(1\)&gt; \*\*not Markdown\*\*/);
  assert.match(html, /&lt;script&gt;alert\(1\)&lt;\/script&gt;/);
  assert.match(html, /&lt;a href=&#x27;javascript:alert\(1\)&#x27;&gt;hostile evidence&lt;\/a&gt;/);
  assert.equal(html.includes("dangerouslySetInnerHTML"), false);
  assert.equal(html.includes("<script>"), false);
});

test("never presents a local citation URL as an interactive link", async () => {
  const { QueryResultPanel } = await vite.ssrLoadModule("/src/App.tsx");
  const hostileResponse = {
    ...answeredResponse,
    citations: answeredResponse.citations.map((citation, index) => index === 1
      ? { ...citation, canonicalUrl: "javascript:alert(document.domain)" }
      : citation),
  };
  const html = renderResult(QueryResultPanel, "en-GB", {
    phase: "completed",
    activeRequestId: null,
    response: hostileResponse,
    problem: null,
    clientFailure: null,
  });

  assert.equal(html.includes("javascript:"), false);
  assert.equal([...html.matchAll(/href=/g)].length, 1);
  assert.match(html, /href="https:\/\/www\.postgresql\.org\//);
});

test("presents a bounded client failure without fabricated server metadata", async () => {
  const { QueryResultPanel } = await vite.ssrLoadModule("/src/App.tsx");
  const html = renderResult(QueryResultPanel, "pt-BR", {
    phase: "failed",
    activeRequestId: null,
    response: null,
    problem: null,
    clientFailure: "ResponseIncompatible",
  });

  assert.match(html, /incompatível com o contrato v1/);
  assert.equal(html.includes("Identificador da solicitação"), false);
});

function renderResult(component, interfaceLanguage, queryState) {
  return renderToStaticMarkup(createElement(component, {
    interfaceLanguage,
    queryState,
    resultHeading: { current: null },
  }));
}
