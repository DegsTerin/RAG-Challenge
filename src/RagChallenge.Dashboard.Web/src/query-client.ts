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

  const responseText = await readBoundedResponseText(response);

  let payload: unknown;
  try {
    payload = JSON.parse(responseText);
  } catch {
    throw new ContractValidationError("Response is not valid JSON.");
  }

  return response.ok
    ? { kind: "completed", response: decodeQueryResponse(payload, questionLanguage) }
    : { kind: "problem", problem: decodeProblemDetails(payload) };
}

async function readBoundedResponseText(response: Response): Promise<string> {
  const declaredLength = response.headers.get("content-length")?.trim();

  if (declaredLength !== undefined && /^\d+$/.test(declaredLength)) {
    const declaredBytes = BigInt(declaredLength);

    if (declaredBytes > BigInt(maximumResponseBytes)) {
      await cancelStreamQuietly(response.body);
      throw new ContractValidationError("Response exceeds the Dashboard safety bound.");
    }
  }

  if (response.body === null) {
    return "";
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  const chunks: string[] = [];
  let receivedBytes = 0;

  try {
    while (true) {
      const { done, value } = await reader.read();

      if (done) {
        break;
      }

      receivedBytes += value.byteLength;
      if (receivedBytes > maximumResponseBytes) {
        await cancelReaderQuietly(reader);
        throw new ContractValidationError("Response exceeds the Dashboard safety bound.");
      }

      chunks.push(decoder.decode(value, { stream: true }));
    }

    chunks.push(decoder.decode());
    return chunks.join("");
  } finally {
    reader.releaseLock();
  }
}

async function cancelReaderQuietly(
  reader: ReadableStreamDefaultReader<Uint8Array>,
): Promise<void> {
  try {
    await reader.cancel();
  } catch {
    // The safety-bound failure remains authoritative if transport cancellation also fails.
  }
}

async function cancelStreamQuietly(stream: ReadableStream<Uint8Array> | null): Promise<void> {
  if (stream === null) {
    return;
  }

  try {
    await stream.cancel();
  } catch {
    // The declared-length failure remains authoritative if transport cancellation also fails.
  }
}
