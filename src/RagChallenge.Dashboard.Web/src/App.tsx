// Purpose: Composes the accessible Dashboard shell and owns only device-local visual preferences; query execution remains in the API client boundary.
import { useEffect, useState } from "react";

import { dashboardCopy } from "./i18n";
import {
  persistPreference,
  preferenceKeys,
  resolveInterfaceLanguage,
  resolveTheme,
  type InterfaceLanguage,
  type PreferenceStorage,
  type Theme,
} from "./preferences";

export interface DashboardShellProperties {
  interfaceLanguage: InterfaceLanguage;
  theme: Theme;
  onInterfaceLanguageChange: (language: InterfaceLanguage) => void;
  onThemeChange: (theme: Theme) => void;
}

export function DashboardShell({
  interfaceLanguage,
  theme,
  onInterfaceLanguageChange,
  onThemeChange,
}: DashboardShellProperties): JSX.Element {
  const copy = dashboardCopy[interfaceLanguage];

  return (
    <div className="site-shell">
      <a className="skip-link" href="#main-content">
        {interfaceLanguage === "pt-BR" ? "Pular para o conteúdo" : "Skip to content"}
      </a>
      <header className="site-header">
        <div className="brand-lockup" aria-label={copy.brandName}>
          <span className="brand-mark" aria-hidden="true">R</span>
          <span>
            <span className="brand-eyebrow">{copy.brandEyebrow}</span>
            <span className="brand-name">{copy.brandName}</span>
          </span>
        </div>
        <div className="preference-panel" aria-label={interfaceLanguage === "pt-BR" ? "Preferências visuais" : "Visual preferences"}>
          <PreferenceSelector
            label={copy.interfaceLanguageLabel}
            values={["pt-BR", "en-GB"]}
            selectedValue={interfaceLanguage}
            getLabel={(value) => copy.languageNames[value]}
            onChange={onInterfaceLanguageChange}
          />
          <PreferenceSelector
            label={copy.themeLabel}
            values={["Light", "Dark"]}
            selectedValue={theme}
            getLabel={(value) => copy.themeNames[value]}
            onChange={onThemeChange}
          />
        </div>
      </header>

      <main id="main-content" className="main-content">
        <section className="hero" aria-labelledby="page-title">
          <div className="hero-copy">
            <p className="section-kicker">{copy.workspaceLabel}</p>
            <h1 id="page-title">{copy.pageTitle}</h1>
            <p className="hero-introduction">{copy.pageIntroduction}</p>
          </div>
          <div className="hero-orbit" aria-hidden="true">
            <span className="orbit-core">RAG</span>
            <span className="orbit-ring orbit-ring-one" />
            <span className="orbit-ring orbit-ring-two" />
          </div>
        </section>

        <section className="workspace-grid" aria-label={copy.workspaceLabel}>
          <div className="panel query-panel">
            <span className="panel-number" aria-hidden="true">01</span>
            <p className="section-kicker">{copy.queryHeading}</p>
            <h2>{copy.queryIntroduction}</h2>
            <div className="pending-workspace" aria-hidden="true">
              <span />
              <span />
              <span />
            </div>
          </div>
          <div className="panel result-panel result-panel-empty">
            <span className="panel-number" aria-hidden="true">02</span>
            <p className="section-kicker">{copy.resultHeading}</p>
            <h2>{copy.initialResultTitle}</h2>
            <p>{copy.initialResultBody}</p>
          </div>
        </section>
      </main>

      <footer className="site-footer">
        <p>{copy.scopeNote}</p>
        <p>{copy.privacyNote}</p>
      </footer>
    </div>
  );
}

export function App(): JSX.Element {
  const storage = getStorage();
  const [interfaceLanguage, setInterfaceLanguage] = useState<InterfaceLanguage>(() =>
    resolveInterfaceLanguage(storage),
  );
  const [theme, setTheme] = useState<Theme>(() =>
    resolveTheme(storage, getSystemThemePreference()),
  );

  useEffect(() => {
    document.documentElement.lang = interfaceLanguage;
  }, [interfaceLanguage]);

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.colorScheme = theme.toLowerCase();
  }, [theme]);

  function changeInterfaceLanguage(language: InterfaceLanguage): void {
    setInterfaceLanguage(language);
    persistPreference(storage, preferenceKeys.interfaceLanguage, language);
  }

  function changeTheme(selectedTheme: Theme): void {
    setTheme(selectedTheme);
    persistPreference(storage, preferenceKeys.theme, selectedTheme);
  }

  return (
    <DashboardShell
      interfaceLanguage={interfaceLanguage}
      theme={theme}
      onInterfaceLanguageChange={changeInterfaceLanguage}
      onThemeChange={changeTheme}
    />
  );
}

interface PreferenceSelectorProperties<T extends string> {
  label: string;
  values: readonly T[];
  selectedValue: T;
  getLabel: (value: T) => string;
  onChange: (value: T) => void;
}

function PreferenceSelector<T extends string>({
  label,
  values,
  selectedValue,
  getLabel,
  onChange,
}: PreferenceSelectorProperties<T>): JSX.Element {
  return (
    <div className="preference-group">
      <span className="preference-label">{label}</span>
      <div className="segmented-control" role="group" aria-label={label}>
        {values.map((value) => (
          <button
            key={value}
            className="segment-button"
            type="button"
            aria-pressed={selectedValue === value}
            onClick={() => onChange(value)}
          >
            {getLabel(value)}
          </button>
        ))}
      </div>
    </div>
  );
}

function getStorage(): PreferenceStorage | null {
  if (typeof window === "undefined") {
    return null;
  }

  try {
    return window.localStorage;
  } catch {
    return null;
  }
}

function getSystemThemePreference(): boolean {
  return typeof window !== "undefined" &&
    typeof window.matchMedia === "function" &&
    window.matchMedia("(prefers-color-scheme: dark)").matches;
}
