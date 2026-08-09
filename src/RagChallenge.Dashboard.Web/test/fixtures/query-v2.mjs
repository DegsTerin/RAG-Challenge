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
          },
        ]
      : [],
  })),
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
