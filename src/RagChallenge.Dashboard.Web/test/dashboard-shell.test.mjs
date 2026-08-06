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
  const expectedHeroHeadings = {
    "pt-BR": "Consulte a documentação ativa de bancos de dados e acompanhe a cobertura, a origem e a localização de cada evidência.",
    "en-GB": "Query the active database documentation and inspect the coverage, origin, and location of every piece of evidence.",
  };

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

      assert.match(html, /<main id="main-content" class="main-content" tabindex="-1"/);
      assert.match(html, /aria-pressed="true"/);
      assert.match(html, /RAG-Challenge/);
      assert.match(html, new RegExp(`<h1 id="page-title">${expectedHeroHeadings[interfaceLanguage]}</h1>`));
      assert.equal(html.includes("Respostas fundamentadas, com a fonte à vista."), false);
      assert.equal(html.includes("Grounded answers, with the source in view."), false);
      assert.equal(html.includes('class="hero-orbit"'), false);
      assert.equal(html.includes("dangerouslySetInnerHTML"), false);
      assert.equal(html.includes(theme === "Light" ? ">Claro<" : ">Escuro<"), interfaceLanguage === "pt-BR");
    }
  }
});

test("moves skip-link focus to main content before the next main control", async () => {
  const { DashboardShell, moveFocusToMainContent } = await vite.ssrLoadModule("/src/App.tsx");
  const events = [];

  moveFocusToMainContent(
    {
      preventDefault() {
        events.push("navigation-prevented");
      },
    },
    {
      focus() {
        events.push("main-focused");
      },
    },
  );

  assert.deepEqual(events, ["navigation-prevented", "main-focused"]);

  const html = renderToStaticMarkup(
    createElement(DashboardShell, {
      interfaceLanguage: "en-GB",
      theme: "Light",
      onInterfaceLanguageChange() {},
      onThemeChange() {},
      workspace: createElement("button", { id: "first-main-control" }, "First main control"),
    }),
  );
  const skipLinkPosition = html.indexOf('class="skip-link"');
  const mainPosition = html.indexOf(
    '<main id="main-content" class="main-content" tabindex="-1">',
  );
  const firstMainControlPosition = html.indexOf('id="first-main-control"');

  assert.ok(skipLinkPosition >= 0);
  assert.ok(mainPosition > skipLinkPosition);
  assert.ok(firstMainControlPosition > mainPosition);
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
