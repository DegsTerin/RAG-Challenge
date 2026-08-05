// Purpose: Executes the bounded same-origin API v1 query with injected fetch support and no retry, credentials, redirects, or external authority.
import {
  ContractValidationError,
  createQueryRequest,
  decodeProblemDetails,
  decodeQueryResponse,
  queryEndpointV1,
  type ProblemDetailsV1,
  type QueryResponseV1,
  type SupportedLanguage,
  utf8ByteCount,
} from "./contracts/api-v1.ts";

const maximumResponseBytes = 262_144;

export type QueryClientResult =
  | { kind: "completed"; response: QueryResponseV1 }
  | { kind: "problem"; problem: ProblemDetailsV1 };

export type FetchFunction = (input: string, init: RequestInit) => Promise<Response>;

export async function askQuestion(
  question: string,
  questionLanguage: SupportedLanguage,
  signal: AbortSignal,
  fetchFunction: FetchFunction = fetch,
): Promise<QueryClientResult> {
  const { body } = createQueryRequest(question, questionLanguage);
  const response = await fetchFunction(queryEndpointV1, {
    method: "POST",
    headers: {
      Accept: "application/json, application/problem+json",
      "Content-Type": "application/json",
    },
    body,
    cache: "no-store",
    credentials: "omit",
    mode: "same-origin",
    redirect: "error",
    signal,
  });
  const mediaType = response.headers.get("content-type")?.split(";", 1)[0]?.trim().toLowerCase();

  if (response.ok && mediaType !== "application/json") {
    throw new ContractValidationError("Completed response has an unsupported media type.");
  }

  if (!response.ok && mediaType !== "application/problem+json") {
    throw new ContractValidationError("Failure response has an unsupported media type.");
  }

  const responseText = await response.text();
  if (utf8ByteCount(responseText) > maximumResponseBytes) {
    throw new ContractValidationError("Response exceeds the Dashboard safety bound.");
  }

  let payload: unknown;
  try {
    payload = JSON.parse(responseText);
  } catch {
    throw new ContractValidationError("Response is not valid JSON.");
  }

  return response.ok
    ? { kind: "completed", response: decodeQueryResponse(payload) }
    : { kind: "problem", problem: decodeProblemDetails(payload) };
}
