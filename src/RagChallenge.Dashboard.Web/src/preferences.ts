// Purpose: Resolves and stores only bounded device-local language and theme preferences, with deterministic fail-safe defaults.
import type { SupportedLanguage } from "./contracts/api-v1";

export type InterfaceLanguage = SupportedLanguage;
export type Theme = "Light" | "Dark";

export interface PreferenceStorage {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
}

export const preferenceKeys = {
  interfaceLanguage: "rag-challenge.interface-language.v1",
  questionLanguage: "rag-challenge.question-language.v1",
  theme: "rag-challenge.theme.v1",
} as const;

export function resolveInterfaceLanguage(storage: PreferenceStorage | null): InterfaceLanguage {
  return readPreference(storage, preferenceKeys.interfaceLanguage, ["pt-BR", "en-GB"]) ?? "pt-BR";
}

export function resolveQuestionLanguage(storage: PreferenceStorage | null): SupportedLanguage {
  return readPreference(storage, preferenceKeys.questionLanguage, ["pt-BR", "en-GB"]) ?? "pt-BR";
}

export function resolveTheme(storage: PreferenceStorage | null, systemPrefersDark: boolean): Theme {
  return readPreference(storage, preferenceKeys.theme, ["Light", "Dark"]) ??
    (systemPrefersDark ? "Dark" : "Light");
}

export function persistPreference(
  storage: PreferenceStorage | null,
  key: (typeof preferenceKeys)[keyof typeof preferenceKeys],
  value: InterfaceLanguage | Theme,
): boolean {
  if (storage === null) {
    return false;
  }

  try {
    storage.setItem(key, value);
    return true;
  } catch {
    return false;
  }
}

function readPreference<const T extends readonly string[]>(
  storage: PreferenceStorage | null,
  key: string,
  acceptedValues: T,
): T[number] | null {
  if (storage === null) {
    return null;
  }

  try {
    const value = storage.getItem(key);
    return value !== null && acceptedValues.includes(value) ? value : null;
  } catch {
    return null;
  }
}
