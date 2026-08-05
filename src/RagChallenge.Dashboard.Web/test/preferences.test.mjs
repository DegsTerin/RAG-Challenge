// Purpose: Verifies bounded local preferences and deterministic language/theme fallbacks without browser or external state.
import assert from "node:assert/strict";
import test from "node:test";

import {
  persistPreference,
  preferenceKeys,
  resolveInterfaceLanguage,
  resolveQuestionLanguage,
  resolveTheme,
} from "../src/preferences.ts";

function createStorage(initialValues = {}) {
  const values = new Map(Object.entries(initialValues));
  return {
    getItem(key) {
      return values.get(key) ?? null;
    },
    setItem(key, value) {
      values.set(key, value);
    },
  };
}

test("uses approved defaults and independent stored preferences", () => {
  const storage = createStorage({
    [preferenceKeys.interfaceLanguage]: "en-GB",
    [preferenceKeys.questionLanguage]: "pt-BR",
    [preferenceKeys.theme]: "Dark",
  });

  assert.equal(resolveInterfaceLanguage(storage), "en-GB");
  assert.equal(resolveQuestionLanguage(storage), "pt-BR");
  assert.equal(resolveTheme(storage, false), "Dark");
  assert.equal(resolveInterfaceLanguage(null), "pt-BR");
  assert.equal(resolveTheme(null, false), "Light");
  assert.equal(resolveTheme(null, true), "Dark");
});

test("falls back safely when stored values or storage are invalid", () => {
  const invalidStorage = createStorage({
    [preferenceKeys.interfaceLanguage]: "fr-FR",
    [preferenceKeys.theme]: "HighContrast",
  });
  const failingStorage = {
    getItem() {
      throw new Error("unavailable");
    },
    setItem() {
      throw new Error("unavailable");
    },
  };

  assert.equal(resolveInterfaceLanguage(invalidStorage), "pt-BR");
  assert.equal(resolveTheme(invalidStorage, false), "Light");
  assert.equal(resolveInterfaceLanguage(failingStorage), "pt-BR");
  assert.equal(persistPreference(failingStorage, preferenceKeys.theme, "Dark"), false);
});

test("persists only an explicit bounded preference value", () => {
  const storage = createStorage();

  assert.equal(persistPreference(storage, preferenceKeys.theme, "Dark"), true);
  assert.equal(resolveTheme(storage, false), "Dark");
});
