// Purpose: Verifies the complete interface-language, question-language, and theme matrix plus token-level text contrast.
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import { createElement } from "react";
import { renderToStaticMarkup } from "react-dom/server";
import { createServer } from "vite";

import { answeredResponse } from "./fixtures/query-v1.mjs";

const vite = await createServer({
  logLevel: "silent",
  server: { middlewareMode: true },
  appType: "custom",
});

test.after(async () => {
  await vite.close();
});

test("keeps all eight interface, question-language, and theme combinations shrinkable at the narrow breakpoint", async () => {
  const css = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");
  const narrowLayoutStart = css.indexOf("@media (max-width: 52rem)");
  const narrowLayoutEnd = css.indexOf("@media (max-width: 32rem)", narrowLayoutStart);
  const narrowLayout = css.slice(narrowLayoutStart, narrowLayoutEnd);
  const compactLayoutEnd = css.indexOf("@media (prefers-reduced-motion: reduce)", narrowLayoutEnd);
  const compactLayout = css.slice(narrowLayoutEnd, compactLayoutEnd);
  const { DashboardShell, QueryWorkspace } = await vite.ssrLoadModule("/src/App.tsx");
  const { decodeQueryResponse } = await vite.ssrLoadModule("/src/contracts/api-v1.ts");
  const longTokens = {
    answer: `Answer${"a".repeat(512)}`,
    citationTitle: `Title${"t".repeat(512)}`,
    citationExcerpt: `Excerpt${"e".repeat(512)}`,
  };
  let combinations = 0;

  assert.notEqual(narrowLayoutStart, -1);
  assert.notEqual(narrowLayoutEnd, -1);
  assert.notEqual(compactLayoutEnd, -1);
  assert.match(narrowLayout, /\.hero,[\s\S]*grid-template-columns:\s*minmax\(0, 1fr\)/);
  assert.match(compactLayout, /\.hero h1\s*{[\s\S]*font-size:\s*clamp\(2\.25rem, 11vw, 3\.4rem\)/);
  assert.match(
    css,
    /\.answer-copy,\s*\.citation-card h4,\s*\.citation-card blockquote\s*{\s*overflow-wrap:\s*anywhere;/,
  );

  for (const interfaceLanguage of ["pt-BR", "en-GB"]) {
    for (const questionLanguage of ["pt-BR", "en-GB"]) {
      for (const theme of ["Light", "Dark"]) {
        const response = decodeQueryResponse({
          ...answeredResponse,
          answerLanguage: questionLanguage,
          answer: longTokens.answer,
          citations: answeredResponse.citations.map((citation, index) => index === 0
            ? {
                ...citation,
                title: longTokens.citationTitle,
                excerpt: longTokens.citationExcerpt,
              }
            : citation),
        }, questionLanguage);
        const workspace = createElement(QueryWorkspace, {
          interfaceLanguage,
          questionLanguage,
          question: questionLanguage === "pt-BR" ? "Pergunta sintética?" : "Synthetic question?",
          validationFailure: null,
          queryState: {
            phase: "completed",
            activeRequestId: null,
            response,
            problem: null,
            clientFailure: null,
          },
          resultHeading: { current: null },
          onQuestionLanguageChange() {},
          onQuestionChange() {},
          onSubmit() {},
          onClear() {},
        });
        const html = renderToStaticMarkup(createElement(DashboardShell, {
          interfaceLanguage,
          theme,
          onInterfaceLanguageChange() {},
          onThemeChange() {},
          workspace,
        }));

        assert.equal(countMatches(html, /aria-pressed="true"/g), 2);
        assert.equal(countMatches(html, /checked=""/g), 1);
        assert.match(html, new RegExp(`lang="${questionLanguage}"`));
        assert.match(html, interfaceLanguage === "pt-BR" ? /Resposta fundamentada/ : /Grounded answer/);
        assert.match(html, theme === "Light" ? /aria-pressed="true">Claro|aria-pressed="true">Light/ : /aria-pressed="true">Escuro|aria-pressed="true">Dark/);
        assert.ok(html.includes(longTokens.answer));
        assert.ok(html.includes(longTokens.citationTitle));
        assert.ok(html.includes(longTokens.citationExcerpt));
        combinations += 1;
      }
    }
  }

  assert.equal(combinations, 8);
});

test("localises the initial and switched document title only from interface language", async () => {
  const indexHtml = await readFile(new URL("../index.html", import.meta.url), "utf8");
  const { applyInterfaceLanguageMetadata } = await vite.ssrLoadModule("/src/App.tsx");
  const expectedTitles = {
    "pt-BR": "RAG-Challenge — Documentação de bancos de dados",
    "en-GB": "RAG-Challenge — Database documentation",
  };
  const target = { title: "", documentElement: { lang: "" } };
  let combinations = 0;

  assert.match(indexHtml, new RegExp(`<title>${expectedTitles["pt-BR"]}</title>`));

  for (const interfaceLanguage of ["pt-BR", "en-GB"]) {
    for (const questionLanguage of ["pt-BR", "en-GB"]) {
      for (const theme of ["Light", "Dark"]) {
        applyInterfaceLanguageMetadata(target, interfaceLanguage);
        assert.equal(target.documentElement.lang, interfaceLanguage);
        assert.equal(target.title, expectedTitles[interfaceLanguage]);
        combinations += 1;
      }
    }
  }

  assert.equal(combinations, 8);
});

test("owns localised messages for every API v1 failure code", async () => {
  const { dashboardCopy } = await vite.ssrLoadModule("/src/i18n.ts");
  const codes = [
    "CH_QUERY_INVALID_INPUT",
    "CH_CORPUS_UNAVAILABLE",
    "CH_SOURCE_UNAVAILABLE",
    "CH_SOURCE_STALE",
    "CH_SOURCE_POLICY_VIOLATION",
    "CH_EMBEDDING_UNAVAILABLE",
    "CH_INDEX_UNAVAILABLE",
    "CH_LANGUAGE_MODEL_UNAVAILABLE",
    "CH_QUERY_RATE_LIMITED",
    "CH_CONFIGURATION_INVALID",
    "CH_OPERATION_CANCELLED",
    "CH_UNEXPECTED_FAILURE",
  ];

  for (const interfaceLanguage of ["pt-BR", "en-GB"]) {
    assert.deepEqual(Object.keys(dashboardCopy[interfaceLanguage].problemMessages), codes);
    for (const code of codes) {
      assert.ok(dashboardCopy[interfaceLanguage].problemMessages[code].length > 20);
    }
  }
});

test("keeps primary text tokens above the WCAG AA normal-text contrast floor", async () => {
  const css = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");
  const light = readTheme(css, ":root");
  const dark = readTheme(css, ':root[data-theme="Dark"]');

  for (const [themeName, tokens] of [["Light", light], ["Dark", dark]]) {
    for (const [foreground, background] of [
      ["text", "canvas"],
      ["text-muted", "canvas"],
      ["text", "surface"],
      ["accent", "canvas"],
      ["danger", "canvas"],
      ["accent-contrast", "accent-strong"],
    ]) {
      const ratio = contrastRatio(tokens[foreground], tokens[background]);
      assert.ok(ratio >= 4.5, `${themeName} ${foreground}/${background} contrast was ${ratio.toFixed(2)}.`);
    }
  }
});

function countMatches(value, expression) {
  return [...value.matchAll(expression)].length;
}

function readTheme(css, selector) {
  const start = css.indexOf(`${selector} {`);
  assert.notEqual(start, -1, `${selector} was not found.`);
  const end = css.indexOf("}", start);
  const block = css.slice(start, end);
  return Object.fromEntries(
    [...block.matchAll(/--([a-z-]+):\s*(#[0-9a-f]{6});/gi)].map((match) => [match[1], match[2]]),
  );
}

function contrastRatio(foreground, background) {
  assert.ok(foreground && background, "Both contrast tokens must resolve to hexadecimal colours.");
  const lighter = Math.max(relativeLuminance(foreground), relativeLuminance(background));
  const darker = Math.min(relativeLuminance(foreground), relativeLuminance(background));
  return (lighter + 0.05) / (darker + 0.05);
}

function relativeLuminance(hex) {
  const channels = hex.slice(1).match(/.{2}/g).map((channel) => Number.parseInt(channel, 16) / 255);
  const [red, green, blue] = channels.map((channel) => channel <= 0.04045
    ? channel / 12.92
    : ((channel + 0.055) / 1.055) ** 2.4);
  return (0.2126 * red) + (0.7152 * green) + (0.0722 * blue);
}
