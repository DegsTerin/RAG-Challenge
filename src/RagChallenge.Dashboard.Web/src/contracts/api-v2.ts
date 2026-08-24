// Purpose: Validates the frozen Dashboard-facing API v2 visual and notice-obligation contract and constructs only fixed same-origin evidence routes from server-returned selectors.
import {
  ContractValidationError,
  createQueryRequest as createQueryRequestV1,
  decodeEvidenceCoverage,
  decodeLanguageModelDescriptor,
  decodeProblemDetails as decodeProblemDetailsV1,
  documentFormats,
  isSafeHttpsUrl,
  isSourceFreshness,
  maximumQuestionBytes,
  productCorpusId,
  queryOutcomes,
  requireArray,
  requireCorrelationId,
  requireEnum,
  requireInteger,
  requireNonEmptyString,
  requireNullableDateTime,
  requireNullableInteger,
  requireNullableString,
  requireObject,
  requireString,
  sourceFreshnessStates,
  sourceTrustClasses,
  supportedLanguages,
  utf8ByteCount,
  validateQuestion,
  type DocumentFormat,
  type EvidenceCoverageV1,
  type LanguageModelDescriptorV1,
  type ProblemDetailsV1,
  type QueryOutcome,
  type QuestionValidationFailure,
  type QueryRequestV1,
  type SourceFreshness,
  type SourceTrustClass,
  type SupportedLanguage,
} from "./api-v1.ts";

export const queryEndpointV2 = "/api/v2/questions";
export const visualEvidenceRoutePrefix = "/api/v2/evidence/page-images";

export {
  ContractValidationError,
  isSafeHttpsUrl,
  isSourceFreshness,
  maximumQuestionBytes,
  productCorpusId,
  supportedLanguages,
  utf8ByteCount,
  validateQuestion,
};
export type {
  DocumentFormat,
  ProblemDetailsV1,
  QuestionValidationFailure,
  QueryOutcome,
  SourceFreshness,
  SourceTrustClass,
  SupportedLanguage,
};

export type QueryRequestV2 = QueryRequestV1;
export type EvidenceCoverageV2 = EvidenceCoverageV1;
export type LanguageModelDescriptorV2 = LanguageModelDescriptorV1;

export const derivativeObligationTreatments = [
  "Required",
  "Prohibited",
  "NotApplicable",
] as const;

export type DerivativeObligationTreatment =
  typeof derivativeObligationTreatments[number];

export function decodeProblemDetails(value: unknown): ProblemDetailsV1 {
  const object = requireObject(value, "problem details");
  requireOnlyProperties(
    object,
    ["type", "title", "status", "detail", "instance", "code", "correlationId",
      "retryAfterSeconds"],
    "problem details",
  );
  return decodeProblemDetailsV1(value);
}

export interface PageImageEvidenceV1 {
  pageNumber: number;
  renderManifestId: string;
  imageContentObjectId: string;
  mediaType: "image/png";
  widthPixels: number;
  heightPixels: number;
  contentSha256: string;
  obligationSetId: string | null;
}

export interface DerivativeObligationPresentationV1 {
  obligationSetId: string;
  contentLanguage: string;
  authoritativePublisherOrAuthor: string;
  documentTitle: string;
  documentVersionLabel: string;
  sourceReference: string;
  attributionText: string;
  copyrightNotice: string;
  permissionNotice: string;
  orderedDisclaimers: readonly string[];
  trademarkTreatment: DerivativeObligationTreatment;
  trademarkOrNonEndorsementText: string;
  changeMarkingText: string;
}

export interface CitationV2 {
  corpusId: string;
  indexGenerationId: string;
  databaseProductId: string;
  databaseProductRevision: number;
  documentId: string;
  documentVersion: number;
  documentFormat: DocumentFormat;
  contentLanguage: string;
  sourceDeclaredLanguage: string | null;
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
  pageImages: readonly PageImageEvidenceV1[];
  derivativeObligationPresentation: DerivativeObligationPresentationV1 | null;
}

export interface QueryResponseV2 {
  outcome: QueryOutcome;
  answerLanguage: SupportedLanguage;
  answer: string | null;
  citations: readonly CitationV2[];
  evidenceCoverage: EvidenceCoverageV2;
  indexGenerationId: string;
  retrievalPolicyVersion: string;
  promptVersion: string;
  languageModelDescriptor: LanguageModelDescriptorV2;
  correlationId: string;
}

export function createQueryRequest(
  question: string,
  questionLanguage: SupportedLanguage,
): { request: QueryRequestV2; body: string } {
  return createQueryRequestV1(question, questionLanguage);
}

export function createPageImageUrl(
  indexGenerationId: string,
  evidence: PageImageEvidenceV1,
): string {
  requirePattern(indexGenerationId, /^idxgen-[a-f0-9]{64}$/, "indexGenerationId");
  validatePageImage(evidence, "pageImage");
  return [
    visualEvidenceRoutePrefix,
    indexGenerationId,
    evidence.renderManifestId,
    String(evidence.pageNumber),
    evidence.imageContentObjectId,
  ].map((segment, index) => index === 0 ? segment : encodeURIComponent(segment)).join("/");
}

export function decodeQueryResponse(
  value: unknown,
  expectedAnswerLanguage: SupportedLanguage,
): QueryResponseV2 {
  const object = requireObject(value, "response");
  requireOnlyProperties(
    object,
    ["outcome", "answerLanguage", "answer", "citations", "evidenceCoverage",
      "indexGenerationId", "retrievalPolicyVersion", "promptVersion",
      "languageModelDescriptor", "correlationId"],
    "response",
  );
  const outcome = requireEnum(object.outcome, queryOutcomes, "outcome");
  const answerLanguage = requireEnum(object.answerLanguage, supportedLanguages, "answerLanguage");

  if (answerLanguage !== expectedAnswerLanguage) {
    throw new ContractValidationError(
      "Response answerLanguage does not match the requested questionLanguage.",
    );
  }

  const answer = requireNullableString(object.answer, "answer");
  const citations = requireArray(object.citations, "citations").map(decodeCitation);
  const indexGenerationId = requirePattern(
    object.indexGenerationId,
    /^idxgen-[a-f0-9]{64}$/,
    "indexGenerationId",
  );
  const coverageObject = requireObject(object.evidenceCoverage, "evidenceCoverage");
  requireOnlyProperties(
    coverageObject,
    ["activeDatabaseCount", "activeDocumentCount", "eligibleDatabaseCount",
      "eligibleDocumentCount", "degradedSources"],
    "evidenceCoverage",
  );
  const modelObject = requireObject(object.languageModelDescriptor, "languageModelDescriptor");
  requireOnlyProperties(
    modelObject,
    ["providerId", "modelId", "modelRevision"],
    "languageModelDescriptor",
  );
  const response: QueryResponseV2 = {
    outcome,
    answerLanguage,
    answer,
    citations,
    evidenceCoverage: decodeEvidenceCoverage(object.evidenceCoverage),
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

  const pageKeys = new Set<string>();
  let pageCount = 0;

  for (const citation of citations) {
    if (citation.corpusId !== productCorpusId || citation.indexGenerationId !== indexGenerationId) {
      throw new ContractValidationError("Citation identity does not match the response.");
    }

    if (citation.canonicalUrl !== null && !isSafeHttpsUrl(citation.canonicalUrl)) {
      throw new ContractValidationError("Citation URL does not use the approved HTTPS scheme.");
    }

    if (citation.sourceTrustClass === "LocalAuthorised" &&
      (citation.sourceFreshness !== "Local" || citation.canonicalUrl !== null)) {
      throw new ContractValidationError(
        "Local citation must use Local freshness and no canonical URL.",
      );
    }

    if (citation.sourceTrustClass === "OfficialExternal" &&
      (citation.sourceFreshness === "Local" || citation.canonicalUrl === null ||
        citation.sourceSnapshotId === null || citation.revalidatedAt === null)) {
      throw new ContractValidationError(
        "Official citation is missing required provenance metadata.",
      );
    }

    validateDerivativeObligationBinding(citation);

    for (const page of citation.pageImages) {
      if (citation.pageStart === null || citation.pageEnd === null ||
        page.pageNumber < citation.pageStart || page.pageNumber > citation.pageEnd) {
        throw new ContractValidationError("Page image is outside its citation range.");
      }

      const key = `${citation.documentId}\u0000${citation.documentVersion}\u0000${page.pageNumber}`;
      if (pageKeys.has(key)) {
        throw new ContractValidationError("Response contains a duplicate document page image.");
      }

      pageKeys.add(key);
      pageCount += 1;
    }
  }

  if (pageCount > 5) {
    throw new ContractValidationError("Response contains too many page images.");
  }

  return response;
}

function decodeCitation(value: unknown, index: number): CitationV2 {
  const object = requireObject(value, `citations[${index}]`);
  requireOnlyProperties(
    object,
    ["corpusId", "indexGenerationId", "databaseProductId", "databaseProductRevision",
      "documentId", "documentVersion", "documentFormat", "contentLanguage",
      "sourceDeclaredLanguage", "chunkId", "sourceAdapterId", "sourceTrustClass",
      "excerpt", "title", "pageStart", "pageEnd", "recordStart", "recordEnd",
      "columns", "canonicalUrl", "sourceSnapshotId", "revalidatedAt",
      "sourceFreshness", "pageImages", "derivativeObligationPresentation"],
    `citations[${index}]`,
  );
  const contentLanguage = requireCanonicalLanguage(object.contentLanguage, "contentLanguage");
  const sourceDeclaredLanguage = requireNullableLanguage(
    object.sourceDeclaredLanguage,
    "sourceDeclaredLanguage",
  );
  const columns = requireArray(object.columns, "columns").map((column, columnIndex) =>
    requireString(column, `columns[${columnIndex}]`),
  );

  if (columns.length > 64) {
    throw new ContractValidationError("Citation contains too many CSV columns.");
  }

  const pageImages = requireArray(object.pageImages, "pageImages")
    .map((page, pageIndex) => decodePageImage(page, `pageImages[${pageIndex}]`));
  const derivativeObligationPresentation = decodeNullableDerivativeObligationPresentation(
    object.derivativeObligationPresentation,
    `citations[${index}].derivativeObligationPresentation`,
  );

  if (pageImages.length > 5) {
    throw new ContractValidationError("Citation contains too many page images.");
  }

  return {
    corpusId: requireNonEmptyString(object.corpusId, "corpusId"),
    indexGenerationId: requirePattern(
      object.indexGenerationId,
      /^idxgen-[a-f0-9]{64}$/,
      "indexGenerationId",
    ),
    databaseProductId: requireNonEmptyString(object.databaseProductId, "databaseProductId"),
    databaseProductRevision: requireInteger(
      object.databaseProductRevision,
      "databaseProductRevision",
      1,
    ),
    documentId: requireNonEmptyString(object.documentId, "documentId"),
    documentVersion: requireInteger(object.documentVersion, "documentVersion", 1),
    documentFormat: requireEnum(object.documentFormat, documentFormats, "documentFormat"),
    contentLanguage,
    sourceDeclaredLanguage,
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
    pageImages,
    derivativeObligationPresentation,
  };
}

function decodePageImage(value: unknown, field: string): PageImageEvidenceV1 {
  const object = requireObject(value, field);
  requireOnlyProperties(
    object,
    ["pageNumber", "renderManifestId", "imageContentObjectId", "mediaType",
      "widthPixels", "heightPixels", "contentSha256", "obligationSetId"],
    field,
  );
  const page: PageImageEvidenceV1 = {
    pageNumber: requireInteger(object.pageNumber, `${field}.pageNumber`, 1),
    renderManifestId: requirePattern(
      object.renderManifestId,
      /^rendermanifest-[a-f0-9]{64}$/,
      `${field}.renderManifestId`,
    ),
    imageContentObjectId: requirePattern(
      object.imageContentObjectId,
      /^[a-f0-9]{64}$/,
      `${field}.imageContentObjectId`,
    ),
    mediaType: requireString(object.mediaType, `${field}.mediaType`) as "image/png",
    widthPixels: requireInteger(object.widthPixels, `${field}.widthPixels`, 1),
    heightPixels: requireInteger(object.heightPixels, `${field}.heightPixels`, 1),
    contentSha256: requirePattern(
      object.contentSha256,
      /^[a-f0-9]{64}$/,
      `${field}.contentSha256`,
    ),
    obligationSetId: requireNullablePattern(
      object.obligationSetId,
      /^obligationset-[a-f0-9]{64}$/,
      `${field}.obligationSetId`,
    ),
  };
  validatePageImage(page, field);
  return page;
}

function validatePageImage(page: PageImageEvidenceV1, field: string): void {
  if (page.mediaType !== "image/png" || page.widthPixels > 4096 ||
    page.heightPixels > 4096 || page.imageContentObjectId !== page.contentSha256) {
    throw new ContractValidationError(`${field} is outside the frozen PNG contract.`);
  }
}

function decodeNullableDerivativeObligationPresentation(
  value: unknown,
  field: string,
): DerivativeObligationPresentationV1 | null {
  if (value === null) {
    return null;
  }

  const object = requireObject(value, field);
  requireOnlyProperties(
    object,
    ["obligationSetId", "contentLanguage", "authoritativePublisherOrAuthor",
      "documentTitle", "documentVersionLabel", "sourceReference", "attributionText",
      "copyrightNotice", "permissionNotice", "orderedDisclaimers", "trademarkTreatment",
      "trademarkOrNonEndorsementText", "changeMarkingText"],
    field,
  );
  const orderedDisclaimers = requireArray(
    object.orderedDisclaimers,
    `${field}.orderedDisclaimers`,
  ).map((disclaimer, index) => requireBoundedText(
    disclaimer,
    `${field}.orderedDisclaimers[${index}]`,
    8192,
  ));

  if (orderedDisclaimers.length > 16) {
    throw new ContractValidationError(`${field} contains too many disclaimers.`);
  }

  return {
    obligationSetId: requirePattern(
      object.obligationSetId,
      /^obligationset-[a-f0-9]{64}$/,
      `${field}.obligationSetId`,
    ),
    contentLanguage: requireCanonicalLanguage(
      object.contentLanguage,
      `${field}.contentLanguage`,
    ),
    authoritativePublisherOrAuthor: requireBoundedText(
      object.authoritativePublisherOrAuthor,
      `${field}.authoritativePublisherOrAuthor`,
      512,
    ),
    documentTitle: requireBoundedText(object.documentTitle, `${field}.documentTitle`, 512),
    documentVersionLabel: requireBoundedText(
      object.documentVersionLabel,
      `${field}.documentVersionLabel`,
      128,
    ),
    sourceReference: requireBoundedText(
      object.sourceReference,
      `${field}.sourceReference`,
      2048,
    ),
    attributionText: requireBoundedText(
      object.attributionText,
      `${field}.attributionText`,
      4096,
    ),
    copyrightNotice: requireBoundedText(
      object.copyrightNotice,
      `${field}.copyrightNotice`,
      8192,
    ),
    permissionNotice: requireBoundedText(
      object.permissionNotice,
      `${field}.permissionNotice`,
      8192,
    ),
    orderedDisclaimers,
    trademarkTreatment: requireEnum(
      object.trademarkTreatment,
      derivativeObligationTreatments,
      `${field}.trademarkTreatment`,
    ),
    trademarkOrNonEndorsementText: requireBoundedText(
      object.trademarkOrNonEndorsementText,
      `${field}.trademarkOrNonEndorsementText`,
      4096,
    ),
    changeMarkingText: requireBoundedText(
      object.changeMarkingText,
      `${field}.changeMarkingText`,
      4096,
    ),
  };
}

function validateDerivativeObligationBinding(citation: CitationV2): void {
  const presentation = citation.derivativeObligationPresentation;

  if (citation.documentFormat === "Csv") {
    if (citation.pageImages.length !== 0 || presentation !== null) {
      throw new ContractValidationError(
        "CSV citations cannot contain page images or derivative obligations.",
      );
    }

    return;
  }

  if (citation.pageImages.length === 0) {
    if (presentation !== null) {
      throw new ContractValidationError(
        "A derivative obligation presentation requires notice-bearing page images.",
      );
    }

    return;
  }

  const obligationSetIds = citation.pageImages.map(page => page.obligationSetId);
  const noticeBearingIds = obligationSetIds.filter((id): id is string => id !== null);

  if (noticeBearingIds.length === 0) {
    if (presentation !== null) {
      throw new ContractValidationError(
        "Legacy page images cannot carry a derivative obligation presentation.",
      );
    }

    return;
  }

  if (presentation === null || noticeBearingIds.length !== obligationSetIds.length ||
    noticeBearingIds.some(id => id !== presentation.obligationSetId) ||
    presentation.contentLanguage !== citation.contentLanguage) {
    throw new ContractValidationError(
      "Notice-bearing page images require one matching derivative obligation presentation.",
    );
  }
}

function requireNullablePattern(
  value: unknown,
  pattern: RegExp,
  field: string,
): string | null {
  if (value === null) {
    return null;
  }

  return requirePattern(value, pattern, field);
}

function requireBoundedText(value: unknown, field: string, maximumLength: number): string {
  const observed = requireNonEmptyString(value, field);

  if (observed.length > maximumLength) {
    throw new ContractValidationError(`${field} exceeds its maximum length.`);
  }

  return observed;
}

function requireCanonicalLanguage(value: unknown, field: string): string {
  const observed = requireNonEmptyString(value, field);
  const canonical = canonicaliseLanguage(observed);

  if (canonical !== observed) {
    throw new ContractValidationError(`${field} must contain a canonical BCP 47 tag.`);
  }

  return observed;
}

function requireNullableLanguage(value: unknown, field: string): string | null {
  const observed = requireNullableString(value, field);
  if (observed !== null) {
    canonicaliseLanguage(observed);
  }

  return observed;
}

function canonicaliseLanguage(value: string): string {
  if (value.length > 128 || !/^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$/.test(value)) {
    throw new ContractValidationError("Language tag is outside the bounded BCP 47 grammar.");
  }

  const lowerValue = value.toLowerCase();
  const grandfathered = grandfatheredTags[lowerValue];
  if (grandfathered !== undefined) {
    return grandfathered;
  }

  const subtags = value.split("-");
  if (subtags[0]?.toLowerCase() === "x") {
    if (subtags.length === 1 || subtags.slice(1).some((tag) =>
      tag.length < 1 || tag.length > 8 || !isAlphaNumeric(tag))) {
      throw invalidLanguage();
    }

    return subtags.map((tag) => tag.toLowerCase()).join("-");
  }

  const primaryLanguage = subtags[0];
  if (primaryLanguage === undefined || primaryLanguage.length < 2 ||
    primaryLanguage.length > 8 || !isAlpha(primaryLanguage)) {
    throw invalidLanguage();
  }

  const canonical = [primaryLanguage.toLowerCase()];
  let index = 1;
  let extlangCount = 0;
  if (primaryLanguage.length === 2 || primaryLanguage.length === 3) {
    while (index < subtags.length && extlangCount < 3 &&
      subtags[index]?.length === 3 && isAlpha(subtags[index] ?? "")) {
      canonical.push((subtags[index] ?? "").toLowerCase());
      index += 1;
      extlangCount += 1;
    }
  }

  const script = subtags[index];
  if (script !== undefined && script.length === 4 && isAlpha(script)) {
    const lowerScript = script.toLowerCase();
    canonical.push(`${lowerScript[0]?.toUpperCase()}${lowerScript.slice(1)}`);
    index += 1;
  }

  const region = subtags[index];
  if (region !== undefined &&
    (region.length === 2 && isAlpha(region) || region.length === 3 && isNumeric(region))) {
    canonical.push(region.toUpperCase());
    index += 1;
  }

  const variants = new Set<string>();
  while (index < subtags.length && isVariant(subtags[index] ?? "")) {
    const variant = subtags[index] ?? "";
    const key = variant.toLowerCase();
    if (variants.has(key)) {
      throw invalidLanguage();
    }

    variants.add(key);
    canonical.push(key);
    index += 1;
  }

  const extensionSingletons = new Set<string>();
  while (index < subtags.length && subtags[index]?.length === 1 &&
    isAlphaNumeric(subtags[index] ?? "") && subtags[index]?.toLowerCase() !== "x") {
    const singleton = (subtags[index] ?? "").toLowerCase();
    if (extensionSingletons.has(singleton)) {
      throw invalidLanguage();
    }

    extensionSingletons.add(singleton);
    canonical.push(singleton);
    index += 1;
    const extensionStart = index;

    while (index < subtags.length && (subtags[index]?.length ?? 0) >= 2 &&
      (subtags[index]?.length ?? 0) <= 8 && isAlphaNumeric(subtags[index] ?? "")) {
      canonical.push((subtags[index] ?? "").toLowerCase());
      index += 1;
    }

    if (index === extensionStart) {
      throw invalidLanguage();
    }
  }

  if (index < subtags.length && subtags[index]?.toLowerCase() === "x") {
    canonical.push("x");
    index += 1;
    const privateUseStart = index;

    while (index < subtags.length && (subtags[index]?.length ?? 0) >= 1 &&
      (subtags[index]?.length ?? 0) <= 8 && isAlphaNumeric(subtags[index] ?? "")) {
      canonical.push((subtags[index] ?? "").toLowerCase());
      index += 1;
    }

    if (index === privateUseStart) {
      throw invalidLanguage();
    }
  }

  if (index !== subtags.length) {
    throw invalidLanguage();
  }

  return canonical.join("-");
}

function requirePattern(value: unknown, pattern: RegExp, field: string): string {
  const text = requireNonEmptyString(value, field);
  if (!pattern.test(text)) {
    throw new ContractValidationError(`${field} is not canonical.`);
  }

  return text;
}

function requireOnlyProperties(
  value: Record<string, unknown>,
  allowed: readonly string[],
  field: string,
): void {
  const allowedSet = new Set(allowed);
  if (Object.keys(value).some((property) => !allowedSet.has(property))) {
    throw new ContractValidationError(`${field} contains an unknown property.`);
  }
}

function isVariant(value: string): boolean {
  return value.length >= 5 && value.length <= 8 && isAlphaNumeric(value) ||
    value.length === 4 && /^[0-9]/.test(value) && isAlphaNumeric(value);
}

function isAlpha(value: string): boolean {
  return /^[A-Za-z]+$/.test(value);
}

function isNumeric(value: string): boolean {
  return /^[0-9]+$/.test(value);
}

function isAlphaNumeric(value: string): boolean {
  return /^[A-Za-z0-9]+$/.test(value);
}

function invalidLanguage(): ContractValidationError {
  return new ContractValidationError("Language tag is not a valid BCP 47 value.");
}

const grandfatheredTags: Readonly<Record<string, string>> = {
  "art-lojban": "art-lojban",
  "cel-gaulish": "cel-gaulish",
  "en-gb-oed": "en-GB-oed",
  "i-ami": "i-ami",
  "i-bnn": "i-bnn",
  "i-default": "i-default",
  "i-enochian": "i-enochian",
  "i-hak": "i-hak",
  "i-klingon": "i-klingon",
  "i-lux": "i-lux",
  "i-mingo": "i-mingo",
  "i-navajo": "i-navajo",
  "i-pwn": "i-pwn",
  "i-tao": "i-tao",
  "i-tay": "i-tay",
  "i-tsu": "i-tsu",
  "no-bok": "no-bok",
  "no-nyn": "no-nyn",
  "sgn-be-fr": "sgn-BE-FR",
  "sgn-be-nl": "sgn-BE-NL",
  "sgn-ch-de": "sgn-CH-DE",
  "zh-guoyu": "zh-guoyu",
  "zh-hakka": "zh-hakka",
  "zh-min": "zh-min",
  "zh-min-nan": "zh-min-nan",
  "zh-xiang": "zh-xiang",
};
