// Purpose: Freezes and validates the Dashboard-facing API v1 transport without granting client-side authority over corpus, providers, or sources.
export const queryEndpointV1 = "/api/v1/questions";
export const mvpCorpusId = "database-systems-catalogue-mvp";
export const maximumQuestionBytes = 4096;
export const maximumRequestBytes = 8192;

export const supportedLanguages = ["pt-BR", "en-GB"] as const;
export const queryOutcomes = ["Answered", "InsufficientEvidence"] as const;
export const documentFormats = ["Pdf", "Csv"] as const;
export const sourceTrustClasses = ["LocalAuthorised", "OfficialExternal"] as const;
export const sourceFreshnessStates = [
  "Local",
  "Current",
  "Stale",
  "Withdrawn",
  "Deactivated",
  "Unavailable",
] as const;

export type SupportedLanguage = (typeof supportedLanguages)[number];
export type QueryOutcome = (typeof queryOutcomes)[number];
export type DocumentFormat = (typeof documentFormats)[number];
export type SourceTrustClass = (typeof sourceTrustClasses)[number];
export type SourceFreshness = (typeof sourceFreshnessStates)[number];

export function isSourceFreshness(value: string): value is SourceFreshness {
  return sourceFreshnessStates.some((state) => state === value);
}

export interface QueryRequestV1 {
  corpusId: string;
  questionLanguage: SupportedLanguage;
  question: string;
}

export interface EvidenceCoverageV1 {
  activeDatabaseCount: number;
  activeDocumentCount: number;
  eligibleDatabaseCount: number;
  eligibleDocumentCount: number;
  degradedSources: Readonly<Record<string, string>>;
}

export interface CitationV1 {
  corpusId: string;
  indexGenerationId: string;
  databaseProductId: string;
  databaseProductRevision: number;
  documentId: string;
  documentVersion: number;
  documentFormat: DocumentFormat;
  contentLanguage: SupportedLanguage;
  chunkId: string;
  sourceAdapterId: string;
  sourceTrustClass: SourceTrustClass;
  excerpt: string;
  title: string | null;
  pageStart: number | null;
  pageEnd: number | null;
  recordStart: number | null;
  recordEnd: number | null;
  columns: readonly string[];
  canonicalUrl: string | null;
  sourceSnapshotId: string | null;
  revalidatedAt: string | null;
  sourceFreshness: SourceFreshness;
}

export interface LanguageModelDescriptorV1 {
  providerId: string;
  modelId: string;
  modelRevision: string;
}

export interface QueryResponseV1 {
  outcome: QueryOutcome;
  answerLanguage: SupportedLanguage;
  answer: string | null;
  citations: readonly CitationV1[];
  evidenceCoverage: EvidenceCoverageV1;
  indexGenerationId: string;
  retrievalPolicyVersion: string;
  promptVersion: string;
  languageModelDescriptor: LanguageModelDescriptorV1;
  correlationId: string;
}

export interface ProblemDetailsV1 {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance?: string;
  code: string;
  correlationId: string;
  retryAfterSeconds?: number;
}

export class ContractValidationError extends Error {
  public constructor(message: string) {
    super(message);
    this.name = "ContractValidationError";
  }
}

export type QuestionValidationFailure = "Empty" | "TooLong" | "ControlCharacter";

export function normaliseQuestion(question: string): string {
  return question.trim().normalize("NFC");
}

export function utf8ByteCount(value: string): number {
  return new TextEncoder().encode(value).byteLength;
}

export function validateQuestion(question: string): QuestionValidationFailure | null {
  const normalisedQuestion = normaliseQuestion(question);
  const questionBytes = utf8ByteCount(normalisedQuestion);

  if (questionBytes < 1) {
    return "Empty";
  }

  if (questionBytes > maximumQuestionBytes) {
    return "TooLong";
  }

  for (const character of normalisedQuestion) {
    const codePoint = character.codePointAt(0) ?? 0;
    const isAllowedWhitespace = character === "\r" || character === "\n" || character === "\t";

    if ((codePoint <= 0x1f || codePoint === 0x7f) && !isAllowedWhitespace) {
      return "ControlCharacter";
    }
  }

  return null;
}

export function createQueryRequest(
  question: string,
  questionLanguage: SupportedLanguage,
): { request: QueryRequestV1; body: string } {
  const normalisedQuestion = normaliseQuestion(question);
  const validationFailure = validateQuestion(normalisedQuestion);

  if (validationFailure !== null) {
    throw new ContractValidationError(`Question validation failed: ${validationFailure}.`);
  }

  const request: QueryRequestV1 = {
    corpusId: mvpCorpusId,
    questionLanguage,
    question: normalisedQuestion,
  };
  const body = JSON.stringify(request);

  if (utf8ByteCount(body) > maximumRequestBytes) {
    throw new ContractValidationError("Request body is outside the API v1 bounds.");
  }

  return { request, body };
}

export function decodeQueryResponse(value: unknown): QueryResponseV1 {
  const object = requireObject(value, "response");
  const outcome = requireEnum(object.outcome, queryOutcomes, "outcome");
  const answerLanguage = requireEnum(object.answerLanguage, supportedLanguages, "answerLanguage");
  const answer = requireNullableString(object.answer, "answer");
  const citations = requireArray(object.citations, "citations").map(decodeCitation);
  const evidenceCoverage = decodeEvidenceCoverage(object.evidenceCoverage);
  const indexGenerationId = requireNonEmptyString(object.indexGenerationId, "indexGenerationId");
  const response: QueryResponseV1 = {
    outcome,
    answerLanguage,
    answer,
    citations,
    evidenceCoverage,
    indexGenerationId,
    retrievalPolicyVersion: requireNonEmptyString(
      object.retrievalPolicyVersion,
      "retrievalPolicyVersion",
    ),
    promptVersion: requireNonEmptyString(object.promptVersion, "promptVersion"),
    languageModelDescriptor: decodeLanguageModelDescriptor(object.languageModelDescriptor),
    correlationId: requireCorrelationId(object.correlationId),
  };

  if (outcome === "Answered" && (answer === null || answer.trim().length === 0)) {
    throw new ContractValidationError("Answered response must contain an answer.");
  }

  if (outcome === "InsufficientEvidence" && (answer !== null || citations.length !== 0)) {
    throw new ContractValidationError(
      "InsufficientEvidence response must not contain an answer or citations.",
    );
  }

  for (const citation of citations) {
    if (
      citation.corpusId !== mvpCorpusId ||
      citation.indexGenerationId !== indexGenerationId
    ) {
      throw new ContractValidationError("Citation identity does not match the response.");
    }

    if (citation.canonicalUrl !== null && !isSafeHttpsUrl(citation.canonicalUrl)) {
      throw new ContractValidationError("Citation URL does not use the approved HTTPS scheme.");
    }

    if (
      citation.sourceTrustClass === "LocalAuthorised" &&
      (citation.sourceFreshness !== "Local" || citation.canonicalUrl !== null)
    ) {
      throw new ContractValidationError(
        "Local citation must use Local freshness and no canonical URL.",
      );
    }

    if (
      citation.sourceTrustClass === "OfficialExternal" &&
      (citation.sourceFreshness === "Local" ||
        citation.canonicalUrl === null ||
        citation.sourceSnapshotId === null ||
        citation.revalidatedAt === null)
    ) {
      throw new ContractValidationError(
        "Official citation is missing required provenance metadata.",
      );
    }
  }

  return response;
}

export function decodeProblemDetails(value: unknown): ProblemDetailsV1 {
  const object = requireObject(value, "problem details");
  const problem: ProblemDetailsV1 = {
    type: requireNonEmptyString(object.type, "type"),
    title: requireString(object.title, "title"),
    status: requireInteger(object.status, "status", 100),
    detail: requireString(object.detail, "detail"),
    code: requireNonEmptyString(object.code, "code"),
    correlationId: requireCorrelationId(object.correlationId),
  };

  if (!/^CH_[A-Z0-9_]+$/.test(problem.code)) {
    throw new ContractValidationError("Problem code is not canonical.");
  }

  if (object.instance !== undefined) {
    problem.instance = requireString(object.instance, "instance");
  }

  if (object.retryAfterSeconds !== undefined) {
    problem.retryAfterSeconds = requireInteger(
      object.retryAfterSeconds,
      "retryAfterSeconds",
      1,
    );
  }

  return problem;
}

export function isSafeHttpsUrl(value: string): boolean {
  try {
    const url = new URL(value);
    return url.protocol === "https:" && url.username === "" && url.password === "";
  } catch {
    return false;
  }
}

function decodeCitation(value: unknown, index: number): CitationV1 {
  const object = requireObject(value, `citations[${index}]`);
  const columns = requireArray(object.columns, "columns").map((column, columnIndex) =>
    requireString(column, `columns[${columnIndex}]`),
  );

  if (columns.length > 64) {
    throw new ContractValidationError("Citation contains too many CSV columns.");
  }

  return {
    corpusId: requireNonEmptyString(object.corpusId, "corpusId"),
    indexGenerationId: requireNonEmptyString(object.indexGenerationId, "indexGenerationId"),
    databaseProductId: requireNonEmptyString(object.databaseProductId, "databaseProductId"),
    databaseProductRevision: requireInteger(
      object.databaseProductRevision,
      "databaseProductRevision",
      1,
    ),
    documentId: requireNonEmptyString(object.documentId, "documentId"),
    documentVersion: requireInteger(object.documentVersion, "documentVersion", 1),
    documentFormat: requireEnum(object.documentFormat, documentFormats, "documentFormat"),
    contentLanguage: requireEnum(object.contentLanguage, supportedLanguages, "contentLanguage"),
    chunkId: requireNonEmptyString(object.chunkId, "chunkId"),
    sourceAdapterId: requireNonEmptyString(object.sourceAdapterId, "sourceAdapterId"),
    sourceTrustClass: requireEnum(
      object.sourceTrustClass,
      sourceTrustClasses,
      "sourceTrustClass",
    ),
    excerpt: requireString(object.excerpt, "excerpt"),
    title: requireNullableString(object.title, "title"),
    pageStart: requireNullableInteger(object.pageStart, "pageStart", 1),
    pageEnd: requireNullableInteger(object.pageEnd, "pageEnd", 1),
    recordStart: requireNullableInteger(object.recordStart, "recordStart", 1),
    recordEnd: requireNullableInteger(object.recordEnd, "recordEnd", 1),
    columns,
    canonicalUrl: requireNullableString(object.canonicalUrl, "canonicalUrl"),
    sourceSnapshotId: requireNullableString(object.sourceSnapshotId, "sourceSnapshotId"),
    revalidatedAt: requireNullableDateTime(object.revalidatedAt, "revalidatedAt"),
    sourceFreshness: requireEnum(
      object.sourceFreshness,
      sourceFreshnessStates,
      "sourceFreshness",
    ),
  };
}

function decodeEvidenceCoverage(value: unknown): EvidenceCoverageV1 {
  const object = requireObject(value, "evidenceCoverage");
  const degradedObject = requireObject(object.degradedSources, "degradedSources");
  const degradedSources: Record<string, string> = {};

  for (const [sourceId, state] of Object.entries(degradedObject)) {
    degradedSources[sourceId] = requireNonEmptyString(state, `degradedSources.${sourceId}`);
  }

  return {
    activeDatabaseCount: requireInteger(object.activeDatabaseCount, "activeDatabaseCount", 0),
    activeDocumentCount: requireInteger(object.activeDocumentCount, "activeDocumentCount", 0),
    eligibleDatabaseCount: requireInteger(
      object.eligibleDatabaseCount,
      "eligibleDatabaseCount",
      0,
    ),
    eligibleDocumentCount: requireInteger(
      object.eligibleDocumentCount,
      "eligibleDocumentCount",
      0,
    ),
    degradedSources,
  };
}

function decodeLanguageModelDescriptor(value: unknown): LanguageModelDescriptorV1 {
  const object = requireObject(value, "languageModelDescriptor");
  return {
    providerId: requireNonEmptyString(object.providerId, "providerId"),
    modelId: requireNonEmptyString(object.modelId, "modelId"),
    modelRevision: requireNonEmptyString(object.modelRevision, "modelRevision"),
  };
}

function requireObject(value: unknown, field: string): Record<string, unknown> {
  if (typeof value !== "object" || value === null || Array.isArray(value)) {
    throw new ContractValidationError(`${field} must be an object.`);
  }

  return value as Record<string, unknown>;
}

function requireArray(value: unknown, field: string): readonly unknown[] {
  if (!Array.isArray(value)) {
    throw new ContractValidationError(`${field} must be an array.`);
  }

  return value;
}

function requireString(value: unknown, field: string): string {
  if (typeof value !== "string") {
    throw new ContractValidationError(`${field} must be a string.`);
  }

  return value;
}

function requireNonEmptyString(value: unknown, field: string): string {
  const stringValue = requireString(value, field);
  if (stringValue.length === 0) {
    throw new ContractValidationError(`${field} must not be empty.`);
  }

  return stringValue;
}

function requireNullableString(value: unknown, field: string): string | null {
  if (value === null || value === undefined) {
    return null;
  }

  return requireString(value, field);
}

function requireInteger(value: unknown, field: string, minimum: number): number {
  if (!Number.isInteger(value) || (value as number) < minimum) {
    throw new ContractValidationError(`${field} must be an integer of at least ${minimum}.`);
  }

  return value as number;
}

function requireNullableInteger(
  value: unknown,
  field: string,
  minimum: number,
): number | null {
  if (value === null || value === undefined) {
    return null;
  }

  return requireInteger(value, field, minimum);
}

function requireNullableDateTime(value: unknown, field: string): string | null {
  const stringValue = requireNullableString(value, field);
  if (stringValue !== null && Number.isNaN(Date.parse(stringValue))) {
    throw new ContractValidationError(`${field} must be an ISO 8601 date-time.`);
  }

  return stringValue;
}

function requireCorrelationId(value: unknown): string {
  const correlationId = requireNonEmptyString(value, "correlationId");
  if (correlationId.length > 128 || !/^[A-Za-z0-9_-]+$/.test(correlationId)) {
    throw new ContractValidationError("correlationId is not sanitised.");
  }

  return correlationId;
}

function requireEnum<const T extends readonly string[]>(
  value: unknown,
  acceptedValues: T,
  field: string,
): T[number] {
  if (typeof value !== "string" || !acceptedValues.includes(value)) {
    throw new ContractValidationError(`${field} contains an unsupported value.`);
  }

  return value;
}
