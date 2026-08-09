// Purpose: Composes the accessible Dashboard shell and owns only device-local visual preferences; query execution remains in the API client boundary.
import {
  useEffect,
  useReducer,
  useRef,
  useState,
  type ReactNode,
} from "react";

import {
  ContractValidationError,
  createPageImageUrl,
  isSafeHttpsUrl,
  isSourceFreshness,
  maximumQuestionBytes,
  utf8ByteCount,
  validateQuestion,
  type CitationV2,
  type EvidenceCoverageV2,
  type PageImageEvidenceV1,
  type ProblemDetailsV1,
  type QuestionValidationFailure,
  type QueryResponseV2,
  type SupportedLanguage,
} from "./contracts/api-v2";
import { dashboardCopy, knownSourceStates, type DashboardCopy } from "./i18n";
import {
  persistPreference,
  preferenceKeys,
  resolveInterfaceLanguage,
  resolveQuestionLanguage,
  resolveTheme,
  type InterfaceLanguage,
  type PreferenceStorage,
  type Theme,
} from "./preferences";
import { askQuestion } from "./query-client";
import { initialQueryState, queryReducer, type QueryState } from "./state/query-state";

export interface DashboardShellProperties {
  interfaceLanguage: InterfaceLanguage;
  theme: Theme;
  onInterfaceLanguageChange: (language: InterfaceLanguage) => void;
  onThemeChange: (theme: Theme) => void;
  workspace?: ReactNode;
}

interface SkipLinkActivationEvent {
  preventDefault(): void;
}

export function moveFocusToMainContent(
  event: SkipLinkActivationEvent,
  target: HTMLElement | null,
): void {
  event.preventDefault();
  target?.focus();
}

export function DashboardShell({
  interfaceLanguage,
  theme,
  onInterfaceLanguageChange,
  onThemeChange,
  workspace,
}: DashboardShellProperties): JSX.Element {
  const copy = dashboardCopy[interfaceLanguage];
  const mainContent = useRef<HTMLElement>(null);

  return (
    <div className="site-shell">
      <a
        className="skip-link"
        href="#main-content"
        onClick={(event: SkipLinkActivationEvent) =>
          moveFocusToMainContent(event, mainContent.current)
        }
      >
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

      <main ref={mainContent} id="main-content" className="main-content" tabIndex={-1}>
        <section className="hero" aria-labelledby="page-title">
          <h1 id="page-title">{copy.pageIntroduction}</h1>
        </section>

        {workspace ?? <WorkspacePlaceholder interfaceLanguage={interfaceLanguage} />}
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
  const [questionLanguage, setQuestionLanguage] = useState<SupportedLanguage>(() =>
    resolveQuestionLanguage(storage),
  );
  const [question, setQuestion] = useState("");
  const [validationFailure, setValidationFailure] =
    useState<QuestionValidationFailure | null>(null);
  const [queryState, dispatch] = useReducer(queryReducer, initialQueryState);
  const requestSequence = useRef(0);
  const activeController = useRef<AbortController>(null);
  const resultHeading = useRef<HTMLElement>(null);

  useEffect(() => {
    applyInterfaceLanguageMetadata(document, interfaceLanguage);
  }, [interfaceLanguage]);

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    document.documentElement.style.colorScheme = theme.toLowerCase();
  }, [theme]);

  useEffect(() => {
    if (queryState.phase === "completed" || queryState.phase === "failed") {
      resultHeading.current?.focus();
    }
  }, [queryState.phase]);

  useEffect(
    () => () => {
      activeController.current?.abort();
    },
    [],
  );

  function changeInterfaceLanguage(language: InterfaceLanguage): void {
    setInterfaceLanguage(language);
    persistPreference(storage, preferenceKeys.interfaceLanguage, language);
  }

  function changeTheme(selectedTheme: Theme): void {
    setTheme(selectedTheme);
    persistPreference(storage, preferenceKeys.theme, selectedTheme);
  }

  function changeQuestionLanguage(language: SupportedLanguage): void {
    setQuestionLanguage(language);
    persistPreference(storage, preferenceKeys.questionLanguage, language);
  }

  async function submitQuestion(event: { preventDefault(): void }): Promise<void> {
    event.preventDefault();
    const failure = validateQuestion(question);
    setValidationFailure(failure);

    if (failure !== null) {
      return;
    }

    activeController.current?.abort();
    const controller = new AbortController();
    activeController.current = controller;
    requestSequence.current = (requestSequence.current ?? 0) + 1;
    const requestId = requestSequence.current;
    dispatch({ type: "begin", requestId });

    try {
      const result = await askQuestion(question, questionLanguage, controller.signal);
      if (result.kind === "completed") {
        dispatch({ type: "complete", requestId, response: result.response });
      } else {
        dispatch({ type: "failProblem", requestId, problem: result.problem });
      }
    } catch (error) {
      const failureKind = error instanceof ContractValidationError
        ? "ResponseIncompatible"
        : isAbortError(error)
          ? "RequestCancelled"
          : "NetworkUnavailable";
      dispatch({ type: "failClient", requestId, failure: failureKind });
    }
  }

  function clearQuery(): void {
    activeController.current?.abort();
    activeController.current = null;
    setQuestion("");
    setValidationFailure(null);
    dispatch({ type: "reset" });
  }

  return (
    <DashboardShell
      interfaceLanguage={interfaceLanguage}
      theme={theme}
      onInterfaceLanguageChange={changeInterfaceLanguage}
      onThemeChange={changeTheme}
      workspace={
        <QueryWorkspace
          interfaceLanguage={interfaceLanguage}
          questionLanguage={questionLanguage}
          question={question}
          validationFailure={validationFailure}
          queryState={queryState}
          resultHeading={resultHeading}
          onQuestionLanguageChange={changeQuestionLanguage}
          onQuestionChange={(value) => {
            setQuestion(value);
            if (validationFailure !== null) {
              setValidationFailure(validateQuestion(value));
            }
          }}
          onSubmit={submitQuestion}
          onClear={clearQuery}
        />
      }
    />
  );
}

interface InterfaceLanguageDocumentTarget {
  title: string;
  documentElement: { lang: string };
}

export function applyInterfaceLanguageMetadata(
  target: InterfaceLanguageDocumentTarget,
  interfaceLanguage: InterfaceLanguage,
): void {
  target.documentElement.lang = interfaceLanguage;
  target.title = dashboardCopy[interfaceLanguage].documentTitle;
}

interface QueryWorkspaceProperties {
  interfaceLanguage: InterfaceLanguage;
  questionLanguage: SupportedLanguage;
  question: string;
  validationFailure: QuestionValidationFailure | null;
  queryState: QueryState;
  resultHeading: { current: HTMLElement | null };
  onQuestionLanguageChange: (language: SupportedLanguage) => void;
  onQuestionChange: (value: string) => void;
  onSubmit: (event: { preventDefault(): void }) => void;
  onClear: () => void;
}

export function QueryWorkspace({
  interfaceLanguage,
  questionLanguage,
  question,
  validationFailure,
  queryState,
  resultHeading,
  onQuestionLanguageChange,
  onQuestionChange,
  onSubmit,
  onClear,
}: QueryWorkspaceProperties): JSX.Element {
  const copy = dashboardCopy[interfaceLanguage];
  const questionBytes = utf8ByteCount(question.normalize("NFC"));
  const validationMessage = getValidationMessage(copy, validationFailure);
  const isSubmitting = queryState.phase === "submitting";

  return (
    <section className="workspace-grid" aria-label={copy.workspaceLabel}>
      <div className="panel query-panel">
        <span className="panel-number" aria-hidden="true">01</span>
        <p className="section-kicker">{copy.queryHeading}</p>
        <h2>{copy.queryIntroduction}</h2>

        <form className="query-form" onSubmit={onSubmit} noValidate>
          <fieldset className="language-fieldset">
            <legend>{copy.questionLanguageLabel}</legend>
            <div className="segmented-control">
              {(["pt-BR", "en-GB"] as const).map((language) => (
                <label key={language} className="radio-segment">
                  <input
                    type="radio"
                    name="question-language"
                    value={language}
                    checked={questionLanguage === language}
                    onChange={() => onQuestionLanguageChange(language)}
                  />
                  <span>{copy.languageNames[language]}</span>
                </label>
              ))}
            </div>
          </fieldset>

          <div className="question-field">
            <label htmlFor="question">{copy.questionLabel}</label>
            <textarea
              id="question"
              rows={6}
              value={question}
              placeholder={copy.questionPlaceholder}
              aria-describedby="question-hint question-count question-error"
              aria-invalid={validationFailure !== null}
              disabled={isSubmitting}
              onChange={(event: { currentTarget: { value: string } }) =>
                onQuestionChange(event.currentTarget.value)}
            />
            <div className="field-support">
              <span id="question-hint">{copy.questionHint}</span>
              <span
                id="question-count"
                className={questionBytes > maximumQuestionBytes ? "byte-count byte-count-invalid" : "byte-count"}
              >
                {copy.questionByteCount(questionBytes, maximumQuestionBytes)}
              </span>
            </div>
            <p id="question-error" className="field-error" aria-live="polite">
              {validationMessage}
            </p>
          </div>

          <div className="form-actions">
            <button className="primary-action" type="submit" disabled={isSubmitting}>
              {isSubmitting ? copy.askingAction : copy.askAction}
            </button>
            <button className="secondary-action" type="button" onClick={onClear} disabled={isSubmitting && question.length === 0}>
              {copy.clearAction}
            </button>
          </div>
        </form>
      </div>

      <QueryResultPanel
        interfaceLanguage={interfaceLanguage}
        queryState={queryState}
        resultHeading={resultHeading}
      />
    </section>
  );
}

interface QueryResultPanelProperties {
  interfaceLanguage: InterfaceLanguage;
  queryState: QueryState;
  resultHeading: { current: HTMLElement | null };
}

export function QueryResultPanel({
  interfaceLanguage,
  queryState,
  resultHeading,
}: QueryResultPanelProperties): JSX.Element {
  const copy = dashboardCopy[interfaceLanguage];
  const isInitial = queryState.phase === "idle";
  const isLoading = queryState.phase === "submitting";
  const isFailed = queryState.phase === "failed";

  return (
    <div
      className={`panel result-panel${isInitial ? " result-panel-empty" : ""}`}
      aria-live={isLoading ? "polite" : "off"}
      aria-busy={isLoading}
    >
      <span className="panel-number" aria-hidden="true">02</span>
      <p className="section-kicker">{copy.resultHeading}</p>
      <h2 ref={resultHeading} tabIndex={-1}>
        {isInitial
          ? copy.initialResultTitle
          : isLoading
            ? copy.loadingTitle
            : isFailed
              ? copy.errorHeading
              : queryState.response?.outcome === "Answered"
                ? copy.answeredLabel
                : copy.insufficientTitle}
      </h2>
      {isInitial && <p>{copy.initialResultBody}</p>}
      {isLoading && <p>{copy.loadingBody}</p>}
      {isFailed && (
        <FailureSummary
          copy={copy}
          problem={queryState.problem}
          clientFailure={queryState.clientFailure}
        />
      )}
      {queryState.phase === "completed" && queryState.response !== null && (
        <CompletedResult
          copy={copy}
          interfaceLanguage={interfaceLanguage}
          response={queryState.response}
        />
      )}
    </div>
  );
}

function FailureSummary({
  copy,
  problem,
  clientFailure,
}: {
  copy: DashboardCopy;
  problem: ProblemDetailsV1 | null;
  clientFailure: QueryState["clientFailure"];
}): JSX.Element {
  const message = problem === null
    ? copy.clientFailures[clientFailure ?? ""] ?? copy.unsupportedProblem
    : copy.problemMessages[problem.code] ?? copy.unsupportedProblem;

  return (
    <div className="failure-summary" role="alert">
      <p>{message}</p>
      {problem?.retryAfterSeconds !== undefined && (
        <p>{copy.retryAfter(problem.retryAfterSeconds)}</p>
      )}
      {problem !== null && (
        <p className="correlation-line">
          <span>{copy.correlationLabel}</span>
          <code>{problem.correlationId}</code>
        </p>
      )}
    </div>
  );
}

function CompletedResult({
  copy,
  interfaceLanguage,
  response,
}: {
  copy: DashboardCopy;
  interfaceLanguage: InterfaceLanguage;
  response: QueryResponseV2;
}): JSX.Element {
  return (
    <div className="completed-result">
      {response.outcome === "Answered" ? (
        <p className="answer-copy" lang={response.answerLanguage}>{response.answer}</p>
      ) : (
        <p className="insufficient-copy">{copy.insufficientBody}</p>
      )}

      <CoveragePanel
        copy={copy}
        interfaceLanguage={interfaceLanguage}
        coverage={response.evidenceCoverage}
      />

      {response.outcome === "Answered" && (
        <section className="citations-section" aria-labelledby="citations-heading">
          <h3 id="citations-heading">{copy.citationsHeading}</h3>
          <ol className="citation-list">
            {response.citations.map((citation, index) => (
              <li key={citation.chunkId}>
                <CitationCard
                  citation={citation}
                  copy={copy}
                  interfaceLanguage={interfaceLanguage}
                  index={index + 1}
                  generationId={response.indexGenerationId}
                />
              </li>
            ))}
          </ol>
        </section>
      )}

      <p className="correlation-line">
        <span>{copy.correlationLabel}</span>
        <code>{response.correlationId}</code>
      </p>
    </div>
  );
}

function CoveragePanel({
  copy,
  interfaceLanguage,
  coverage,
}: {
  copy: DashboardCopy;
  interfaceLanguage: InterfaceLanguage;
  coverage: EvidenceCoverageV2;
}): JSX.Element {
  const degradedSources = Object.entries(coverage.degradedSources);
  const metrics = [
    [copy.activeDatabasesLabel, coverage.activeDatabaseCount],
    [copy.eligibleDatabasesLabel, coverage.eligibleDatabaseCount],
    [copy.activeDocumentsLabel, coverage.activeDocumentCount],
    [copy.eligibleDocumentsLabel, coverage.eligibleDocumentCount],
  ] as const;

  return (
    <section className="coverage-section" aria-labelledby="coverage-heading">
      <h3 id="coverage-heading">{copy.coverageHeading}</h3>
      <p>{copy.coverageIntroduction}</p>
      <dl className="coverage-metrics">
        {metrics.map(([label, value]) => (
          <div key={label}>
            <dt>{label}</dt>
            <dd>{value}</dd>
          </div>
        ))}
      </dl>
      <h4>{copy.degradedSourcesHeading}</h4>
      {degradedSources.length === 0 ? (
        <p>{copy.noDegradedSources}</p>
      ) : (
        <ul className="degraded-source-list">
          {degradedSources.map(([sourceId, state]) => (
            <li key={sourceId}>
              <code>{sourceId}</code>
              <span>
                {isSourceFreshness(state)
                  ? knownSourceStates[interfaceLanguage][state]
                  : copy.sourceStateUnknown}
              </span>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

function CitationCard({
  citation,
  copy,
  interfaceLanguage,
  index,
  generationId,
}: {
  citation: CitationV2;
  copy: DashboardCopy;
  interfaceLanguage: InterfaceLanguage;
  index: number;
  generationId: string;
}): JSX.Element {
  const freshness = knownSourceStates[interfaceLanguage][citation.sourceFreshness] ??
    copy.sourceStateUnknown;
  const location = citation.documentFormat === "Pdf"
    ? copy.pdfLocation(citation.pageStart, citation.pageEnd)
    : copy.csvLocation(citation.recordStart, citation.recordEnd);

  return (
    <article className="citation-card" aria-labelledby={`citation-${index}-title`}>
      <div className="citation-heading-row">
        <p className="citation-index">{copy.citationLabel(index)}</p>
        <span className={`trust-badge trust-${citation.sourceTrustClass.toLowerCase()}`}>
          {citation.sourceTrustClass === "LocalAuthorised" ? copy.sourceLocal : copy.sourceOfficial}
        </span>
      </div>
      <h4 id={`citation-${index}-title`} lang={citation.contentLanguage}>
        {citation.title ?? citation.documentId}
      </h4>
      <blockquote lang={citation.contentLanguage}>{citation.excerpt}</blockquote>
      {citation.pageImages.map((pageImage) => (
        <VisualEvidenceImage
          key={`${citation.documentId}-${citation.documentVersion}-${pageImage.pageNumber}`}
          citation={citation}
          pageImage={pageImage}
          generationId={generationId}
          copy={copy}
        />
      ))}
      <dl className="citation-summary">
        <div><dt>{copy.contentLanguageLabel}</dt><dd>{citation.contentLanguage}</dd></div>
        {citation.sourceDeclaredLanguage !== null && (
          <div>
            <dt>{copy.sourceDeclaredLanguageLabel}</dt>
            <dd>{citation.sourceDeclaredLanguage}</dd>
          </div>
        )}
        <div><dt>{copy.sourceFreshnessLabel}</dt><dd>{freshness}</dd></div>
        <div><dt>{copy.documentLabel}</dt><dd>{citation.documentId} v{citation.documentVersion}</dd></div>
        <div><dt>{location}</dt><dd>{citation.documentFormat}</dd></div>
        {citation.documentFormat === "Csv" && citation.columns.length > 0 && (
          <div><dt>{copy.columnsLabel}</dt><dd>{citation.columns.join(", ")}</dd></div>
        )}
      </dl>
      {citation.sourceTrustClass === "OfficialExternal" &&
        citation.canonicalUrl !== null &&
        isSafeHttpsUrl(citation.canonicalUrl) && (
        <p>
          <a href={citation.canonicalUrl} rel="noopener noreferrer" target="_blank">
            {copy.sourceUrlLabel}
          </a>
        </p>
        )}
      <details>
        <summary>{copy.technicalDetailsSummary}</summary>
        <dl className="technical-details">
          <div><dt>{copy.generationLabel}</dt><dd><code>{generationId}</code></dd></div>
          <div><dt>{copy.sourceSnapshotLabel}</dt><dd><code>{citation.sourceSnapshotId ?? "—"}</code></dd></div>
          <div><dt>{copy.revalidatedAtLabel}</dt><dd>{formatDateTime(citation.revalidatedAt, interfaceLanguage)}</dd></div>
        </dl>
      </details>
    </article>
  );
}

function VisualEvidenceImage({
  citation,
  pageImage,
  generationId,
  copy,
}: {
  key?: string;
  citation: CitationV2;
  pageImage: PageImageEvidenceV1;
  generationId: string;
  copy: DashboardCopy;
}): JSX.Element {
  const [loadFailed, setLoadFailed] = useState(false);
  const sourceTitle = citation.title ?? citation.documentId;
  const description = copy.pageImageDescription(
    sourceTitle,
    citation.documentVersion,
    pageImage.pageNumber,
  );

  return (
    <figure className="visual-evidence">
      {!loadFailed && (
        <img
          src={createPageImageUrl(generationId, pageImage)}
          width={pageImage.widthPixels}
          height={pageImage.heightPixels}
          alt={description}
          loading="lazy"
          decoding="async"
          onError={() => setLoadFailed(true)}
        />
      )}
      <figcaption>
        <span>{description}</span>
        {loadFailed && <span role="status">{copy.pageImageUnavailable}</span>}
      </figcaption>
    </figure>
  );
}

function formatDateTime(value: string | null, language: InterfaceLanguage): string {
  if (value === null) {
    return "—";
  }

  return new Intl.DateTimeFormat(language, {
    dateStyle: "medium",
    timeStyle: "short",
    timeZone: "UTC",
  }).format(new Date(value));
}

function WorkspacePlaceholder({ interfaceLanguage }: { interfaceLanguage: InterfaceLanguage }): JSX.Element {
  const copy = dashboardCopy[interfaceLanguage];
  return (
    <section className="workspace-grid" aria-label={copy.workspaceLabel}>
      <div className="panel query-panel">
        <span className="panel-number" aria-hidden="true">01</span>
        <p className="section-kicker">{copy.queryHeading}</p>
        <h2>{copy.queryIntroduction}</h2>
      </div>
      <div className="panel result-panel result-panel-empty">
        <span className="panel-number" aria-hidden="true">02</span>
        <p className="section-kicker">{copy.resultHeading}</p>
        <h2>{copy.initialResultTitle}</h2>
        <p>{copy.initialResultBody}</p>
      </div>
    </section>
  );
}

function getValidationMessage(
  copy: (typeof dashboardCopy)[InterfaceLanguage],
  failure: QuestionValidationFailure | null,
): string {
  switch (failure) {
    case "Empty":
      return copy.validationEmpty;
    case "TooLong":
      return copy.validationTooLong;
    case "ControlCharacter":
      return copy.validationControlCharacter;
    case null:
      return "";
  }
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError";
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
