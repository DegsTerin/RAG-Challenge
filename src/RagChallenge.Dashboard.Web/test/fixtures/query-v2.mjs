// Purpose: Extends the synthetic v1 query fixture only with the separately frozen v2 language and visual-evidence fields.
import {
  answeredResponse as answeredResponseV1,
  insufficientEvidenceResponse as insufficientEvidenceResponseV1,
  rateLimitedProblem,
} from "./query-v1.mjs";

const indexGenerationId =
  "idxgen-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

export const answeredResponse = {
  ...answeredResponseV1,
  indexGenerationId,
  citations: answeredResponseV1.citations.map((citation, index) => ({
    ...citation,
    indexGenerationId,
    sourceDeclaredLanguage: index === 0 ? "EN-gb" : null,
    derivativeObligationPresentation: null,
    pageImages: index === 0
      ? [
          {
            pageNumber: 142,
            renderManifestId:
              "rendermanifest-bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
            imageContentObjectId:
              "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
            mediaType: "image/png",
            widthPixels: 1240,
            heightPixels: 1754,
            contentSha256:
              "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc",
            obligationSetId: null,
          },
        ]
      : [],
  })),
};

const obligationSetId =
  "obligationset-dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd";

export const noticeBearingAnsweredResponse = {
  ...answeredResponse,
  citations: answeredResponse.citations.map((citation, index) => index === 0
    ? {
        ...citation,
        derivativeObligationPresentation: {
          obligationSetId,
          contentLanguage: "en-GB",
          authoritativePublisherOrAuthor: "Synthetic Documentation Group",
          documentTitle: "Synthetic Database Documentation",
          documentVersionLabel: "1.0",
          sourceReference: "synthetic-source-v1",
          attributionText: "Synthetic source attribution.",
          copyrightNotice: "Synthetic copyright notice.",
          permissionNotice: "Synthetic permission notice.",
          orderedDisclaimers: [
            "Synthetic first disclaimer.",
            "Synthetic second disclaimer.",
          ],
          trademarkTreatment: "NotApplicable",
          trademarkOrNonEndorsementText: "No endorsement is claimed.",
          changeMarkingText: "Rendered derivative of version 1.0, page 142.",
        },
        pageImages: citation.pageImages.map(page => ({
          ...page,
          obligationSetId,
        })),
      }
    : citation),
};

export const insufficientEvidenceResponse = {
  ...insufficientEvidenceResponseV1,
  indexGenerationId,
};

export const answeredResponseEnGb = {
  ...answeredResponse,
  answerLanguage: "en-GB",
  answer: "PostgreSQL uses multiversion concurrency control to retain row versions.",
};

export { rateLimitedProblem };
