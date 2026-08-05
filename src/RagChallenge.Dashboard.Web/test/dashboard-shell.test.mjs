// Purpose: Component-tests the localised, themed Dashboard shell through the existing Vite and React server-rendering boundary.
import assert from "node:assert/strict";
import test from "node:test";
import { createElement } from "react";
import { renderToStaticMarkup } from "react-dom/server";
import { createServer } from "vite";

const vite = await createServer({
  logLevel: "silent",
  server: { middlewareMode: true },
  appType: "custom",
});

test.after(async () => {
  await vite.close();
});

test("renders complete pt-BR and en-GB shells in both themes", async () => {
  const { DashboardShell } = await vite.ssrLoadModule("/src/App.tsx");

  for (const interfaceLanguage of ["pt-BR", "en-GB"]) {
    for (const theme of ["Light", "Dark"]) {
      const html = renderToStaticMarkup(
        createElement(DashboardShell, {
          interfaceLanguage,
          theme,
          onInterfaceLanguageChange() {},
          onThemeChange() {},
        }),
      );

      assert.match(html, /<main id="main-content"/);
      assert.match(html, /aria-pressed="true"/);
      assert.match(html, /RAG-Challenge/);
      assert.equal(html.includes("dangerouslySetInnerHTML"), false);
      assert.equal(html.includes(theme === "Light" ? ">Claro<" : ">Escuro<"), interfaceLanguage === "pt-BR");
    }
  }
});

test("keeps source and query concepts out of visual-preference controls", async () => {
  const { DashboardShell } = await vite.ssrLoadModule("/src/App.tsx");
  const html = renderToStaticMarkup(
    createElement(DashboardShell, {
      interfaceLanguage: "en-GB",
      theme: "Dark",
      onInterfaceLanguageChange() {},
      onThemeChange() {},
    }),
  );

  assert.match(html, /Interface language/);
  assert.match(html, /Visual theme/);
  assert.match(html, /does not administer sources, providers, or the catalogue/);
});
