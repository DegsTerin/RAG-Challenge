// Purpose: Provides exhaustive product-owned interface copy for the two approved visual languages without translating source-derived evidence.
import type { InterfaceLanguage, Theme } from "./preferences";

export interface DashboardCopy {
  brandEyebrow: string;
  brandName: string;
  documentTitle: string;
  pageTitle: string;
  pageIntroduction: string;
  interfaceLanguageLabel: string;
  themeLabel: string;
  languageNames: Readonly<Record<InterfaceLanguage, string>>;
  themeNames: Readonly<Record<Theme, string>>;
  workspaceLabel: string;
  queryHeading: string;
  queryIntroduction: string;
  questionLanguageLabel: string;
  questionLabel: string;
  questionPlaceholder: string;
  questionHint: string;
  questionByteCount: (used: number, maximum: number) => string;
  askAction: string;
  askingAction: string;
  clearAction: string;
  resultHeading: string;
  initialResultTitle: string;
  initialResultBody: string;
  loadingTitle: string;
  loadingBody: string;
  answeredLabel: string;
  insufficientTitle: string;
  insufficientBody: string;
  coverageHeading: string;
  coverageIntroduction: string;
  activeDatabasesLabel: string;
  eligibleDatabasesLabel: string;
  activeDocumentsLabel: string;
  eligibleDocumentsLabel: string;
  degradedSourcesHeading: string;
  noDegradedSources: string;
  sourceStateUnknown: string;
  citationsHeading: string;
  citationLabel: (index: number) => string;
  sourceLocal: string;
  sourceOfficial: string;
  sourceFreshnessLabel: string;
  sourceSnapshotLabel: string;
  revalidatedAtLabel: string;
  sourceUrlLabel: string;
  documentLabel: string;
  generationLabel: string;
  contentLanguageLabel: string;
  pdfLocation: (start: number | null, end: number | null) => string;
  csvLocation: (start: number | null, end: number | null) => string;
  columnsLabel: string;
  technicalDetailsSummary: string;
  correlationLabel: string;
  errorHeading: string;
  validationEmpty: string;
  validationTooLong: string;
  validationControlCharacter: string;
  clientFailures: Readonly<Record<string, string>>;
  problemMessages: Readonly<Record<string, string>>;
  retryAfter: (seconds: number) => string;
  unsupportedProblem: string;
  scopeNote: string;
  privacyNote: string;
}

export const dashboardCopy: Readonly<Record<InterfaceLanguage, DashboardCopy>> = {
  "pt-BR": {
    brandEyebrow: "Catálogo de documentação",
    brandName: "RAG-Challenge",
    documentTitle: "RAG-Challenge — Documentação de bancos de dados",
    pageTitle: "Respostas fundamentadas, com a fonte à vista.",
    pageIntroduction:
      "Consulte a documentação ativa de bancos de dados e acompanhe a cobertura, a origem e a localização de cada evidência.",
    interfaceLanguageLabel: "Idioma da interface",
    themeLabel: "Tema visual",
    languageNames: { "pt-BR": "Português (Brasil)", "en-GB": "English (UK)" },
    themeNames: { Light: "Claro", Dark: "Escuro" },
    workspaceLabel: "Área de consulta",
    queryHeading: "Faça uma pergunta",
    queryIntroduction:
      "Escolha o idioma da pergunta. A resposta usará o mesmo idioma; as citações permanecerão no idioma original.",
    questionLanguageLabel: "Idioma da pergunta",
    questionLabel: "Pergunta sobre bancos de dados",
    questionPlaceholder: "Ex.: Como o PostgreSQL implementa controle de concorrência?",
    questionHint: "Não inclua informações pessoais, confidenciais ou secretas.",
    questionByteCount: (used, maximum) => `${used} de ${maximum} bytes UTF-8`,
    askAction: "Consultar documentação",
    askingAction: "Consultando…",
    clearAction: "Limpar consulta",
    resultHeading: "Resultado",
    initialResultTitle: "A resposta aparecerá aqui",
    initialResultBody:
      "Além da resposta, você verá a cobertura avaliada e as citações reconstruídas pelo servidor.",
    loadingTitle: "Consultando a geração ativa",
    loadingBody: "Aguarde enquanto as evidências elegíveis são recuperadas e validadas.",
    answeredLabel: "Resposta fundamentada",
    insufficientTitle: "Evidência insuficiente",
    insufficientBody:
      "A documentação elegível não sustenta uma resposta segura para esta pergunta. Tente reformular sem ampliar o escopo da fonte.",
    coverageHeading: "Cobertura avaliada",
    coverageIntroduction:
      "Estes números descrevem o conjunto ativo e elegível avaliado; não representam uma lista de fontes citadas.",
    activeDatabasesLabel: "Bancos ativos",
    eligibleDatabasesLabel: "Bancos elegíveis",
    activeDocumentsLabel: "Documentos ativos",
    eligibleDocumentsLabel: "Documentos elegíveis",
    degradedSourcesHeading: "Cobertura degradada",
    noDegradedSources: "Nenhuma fonte degradada foi informada.",
    sourceStateUnknown: "Estado não reconhecido",
    citationsHeading: "Evidências citadas",
    citationLabel: (index) => `Citação ${index}`,
    sourceLocal: "Documento local autorizado",
    sourceOfficial: "Fonte oficial externa",
    sourceFreshnessLabel: "Frescor",
    sourceSnapshotLabel: "Snapshot",
    revalidatedAtLabel: "Revalidada em",
    sourceUrlLabel: "Documento oficial",
    documentLabel: "Documento",
    generationLabel: "Geração",
    contentLanguageLabel: "Idioma da evidência",
    pdfLocation: (start, end) => formatRange("Página", "Páginas", start, end, "não informada"),
    csvLocation: (start, end) => formatRange("Registro", "Registros", start, end, "não informado"),
    columnsLabel: "Colunas",
    technicalDetailsSummary: "Detalhes técnicos reproduzíveis",
    correlationLabel: "Identificador da solicitação",
    errorHeading: "Não foi possível concluir a consulta",
    validationEmpty: "Digite uma pergunta antes de consultar.",
    validationTooLong: "A pergunta excede o limite de 4.096 bytes UTF-8.",
    validationControlCharacter: "A pergunta contém um caractere de controle não permitido.",
    clientFailures: {
      InvalidQuestion: "Revise a pergunta e tente novamente.",
      NetworkUnavailable: "A API não pôde ser alcançada neste ambiente.",
      RequestCancelled: "A consulta foi cancelada antes da conclusão.",
      ResponseIncompatible: "A API retornou uma resposta incompatível com o contrato v1.",
    },
    problemMessages: {
      CH_QUERY_INVALID_INPUT: "A API recusou a pergunta por formato ou limite inválido.",
      CH_CORPUS_UNAVAILABLE: "O corpus configurado não pode atender consultas agora.",
      CH_SOURCE_UNAVAILABLE: "Nenhuma fonte ativa elegível está disponível.",
      CH_SOURCE_STALE: "Nenhuma evidência oficial elegível permanece atual.",
      CH_SOURCE_POLICY_VIOLATION: "A capacidade de fonte falhou de forma segura por política.",
      CH_EMBEDDING_UNAVAILABLE: "O embedding da consulta está indisponível.",
      CH_INDEX_UNAVAILABLE: "O índice ativo compatível está indisponível.",
      CH_LANGUAGE_MODEL_UNAVAILABLE: "A geração fundamentada está indisponível.",
      CH_QUERY_RATE_LIMITED: "O limite temporário de consultas foi atingido.",
      CH_CONFIGURATION_INVALID: "A capacidade está desativada por configuração inválida.",
      CH_OPERATION_CANCELLED: "A operação foi cancelada pelo servidor.",
      CH_UNEXPECTED_FAILURE: "O servidor não conseguiu concluir a solicitação.",
    },
    retryAfter: (seconds) => `Tente novamente após ${seconds} segundo${seconds === 1 ? "" : "s"}.`,
    unsupportedProblem: "O servidor retornou uma falha não reconhecida.",
    scopeNote: "Consulta somente leitura. O Dashboard não administra fontes, providers ou catálogo.",
    privacyNote: "Preferências locais guardam somente idioma e tema neste dispositivo.",
  },
  "en-GB": {
    brandEyebrow: "Documentation catalogue",
    brandName: "RAG-Challenge",
    documentTitle: "RAG-Challenge — Database documentation",
    pageTitle: "Grounded answers, with the source in view.",
    pageIntroduction:
      "Query the active database documentation and inspect the coverage, origin, and location of every piece of evidence.",
    interfaceLanguageLabel: "Interface language",
    themeLabel: "Visual theme",
    languageNames: { "pt-BR": "Português (Brasil)", "en-GB": "English (UK)" },
    themeNames: { Light: "Light", Dark: "Dark" },
    workspaceLabel: "Query workspace",
    queryHeading: "Ask a question",
    queryIntroduction:
      "Choose the question language. The answer will use the same language; citations will remain in their original language.",
    questionLanguageLabel: "Question language",
    questionLabel: "Database documentation question",
    questionPlaceholder: "For example: How does PostgreSQL implement concurrency control?",
    questionHint: "Do not include personal, confidential, or secret information.",
    questionByteCount: (used, maximum) => `${used} of ${maximum} UTF-8 bytes`,
    askAction: "Query documentation",
    askingAction: "Querying…",
    clearAction: "Clear query",
    resultHeading: "Result",
    initialResultTitle: "The answer will appear here",
    initialResultBody:
      "Alongside the answer, you will see the evaluated coverage and the citations reconstructed by the server.",
    loadingTitle: "Querying the active generation",
    loadingBody: "Please wait while eligible evidence is retrieved and validated.",
    answeredLabel: "Grounded answer",
    insufficientTitle: "Insufficient evidence",
    insufficientBody:
      "The eligible documentation does not support a safe answer to this question. Try rephrasing it without broadening the source scope.",
    coverageHeading: "Evaluated coverage",
    coverageIntroduction:
      "These figures describe the active and eligible set evaluated; they are not a list of cited sources.",
    activeDatabasesLabel: "Active databases",
    eligibleDatabasesLabel: "Eligible databases",
    activeDocumentsLabel: "Active documents",
    eligibleDocumentsLabel: "Eligible documents",
    degradedSourcesHeading: "Degraded coverage",
    noDegradedSources: "No degraded source was reported.",
    sourceStateUnknown: "Unrecognised state",
    citationsHeading: "Cited evidence",
    citationLabel: (index) => `Citation ${index}`,
    sourceLocal: "Authorised local document",
    sourceOfficial: "Official external source",
    sourceFreshnessLabel: "Freshness",
    sourceSnapshotLabel: "Snapshot",
    revalidatedAtLabel: "Revalidated at",
    sourceUrlLabel: "Official document",
    documentLabel: "Document",
    generationLabel: "Generation",
    contentLanguageLabel: "Evidence language",
    pdfLocation: (start, end) => formatRange("Page", "Pages", start, end, "not provided"),
    csvLocation: (start, end) => formatRange("Record", "Records", start, end, "not provided"),
    columnsLabel: "Columns",
    technicalDetailsSummary: "Reproducible technical details",
    correlationLabel: "Request identifier",
    errorHeading: "The query could not be completed",
    validationEmpty: "Enter a question before querying.",
    validationTooLong: "The question exceeds the 4,096-byte UTF-8 limit.",
    validationControlCharacter: "The question contains a disallowed control character.",
    clientFailures: {
      InvalidQuestion: "Review the question and try again.",
      NetworkUnavailable: "The API could not be reached in this environment.",
      RequestCancelled: "The query was cancelled before completion.",
      ResponseIncompatible: "The API returned a response incompatible with contract v1.",
    },
    problemMessages: {
      CH_QUERY_INVALID_INPUT: "The API rejected the question because its format or bounds are invalid.",
      CH_CORPUS_UNAVAILABLE: "The configured corpus cannot serve queries at present.",
      CH_SOURCE_UNAVAILABLE: "No eligible active source is available.",
      CH_SOURCE_STALE: "No eligible official evidence remains current.",
      CH_SOURCE_POLICY_VIOLATION: "The source capability failed closed because of policy.",
      CH_EMBEDDING_UNAVAILABLE: "Query embedding is unavailable.",
      CH_INDEX_UNAVAILABLE: "The compatible active index is unavailable.",
      CH_LANGUAGE_MODEL_UNAVAILABLE: "Grounded answer generation is unavailable.",
      CH_QUERY_RATE_LIMITED: "The temporary query limit has been reached.",
      CH_CONFIGURATION_INVALID: "The capability is disabled by invalid configuration.",
      CH_OPERATION_CANCELLED: "The operation was cancelled by the server.",
      CH_UNEXPECTED_FAILURE: "The server could not complete the request.",
    },
    retryAfter: (seconds) => `Try again after ${seconds} second${seconds === 1 ? "" : "s"}.`,
    unsupportedProblem: "The server returned an unrecognised failure.",
    scopeNote: "Read-only query. The Dashboard does not administer sources, providers, or the catalogue.",
    privacyNote: "Local preferences store only language and theme on this device.",
  },
};

export const knownSourceStates: Readonly<Record<InterfaceLanguage, Readonly<Record<string, string>>>> = {
  "pt-BR": {
    Current: "Atual",
    Stale: "Desatualizada",
    Unavailable: "Indisponível",
    Withdrawn: "Retirada",
    Deactivated: "Desativada",
  },
  "en-GB": {
    Current: "Current",
    Stale: "Stale",
    Unavailable: "Unavailable",
    Withdrawn: "Withdrawn",
    Deactivated: "Deactivated",
  },
};

function formatRange(
  singular: string,
  plural: string,
  start: number | null,
  end: number | null,
  missing: string,
): string {
  if (start === null) {
    return `${singular}: ${missing}`;
  }

  return end === null || end === start
    ? `${singular}: ${start}`
    : `${plural}: ${start}–${end}`;
}
