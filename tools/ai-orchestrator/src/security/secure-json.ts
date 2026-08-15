// Purpose: Parses bounded JSON while rejecting duplicate object keys and excessive structural depth before schema validation.
import type { StopCode } from "../core/contracts.js";
import { OrchestratorStop } from "../core/errors.js";

export function parseSecureJson(text: string, label: string, stopCode: StopCode = "TEST_BASELINE_BROKEN"): unknown {
  let index = 0;
  const fail = (message: string): never => { throw new OrchestratorStop(stopCode, `${label}: ${message}`); };
  const whitespace = (): void => { while (/[\t\n\r ]/.test(text[index] ?? "")) index += 1; };

  const string = (): string => {
    const start = index;
    index += 1;
    let escaped = false;
    while (index < text.length) {
      const character = text[index];
      if (!escaped && character === '"') {
        index += 1;
        try { return JSON.parse(text.slice(start, index)) as string; }
        catch { return fail("contains an invalid string escape."); }
      }
      if (!escaped && character !== undefined && character.charCodeAt(0) < 0x20) fail("contains an unescaped control character.");
      if (!escaped && character === "\\") escaped = true;
      else escaped = false;
      index += 1;
    }
    return fail("contains an unterminated string.");
  };

  const value = (depth: number): unknown => {
    if (depth > 64) fail("exceeds the maximum structural depth.");
    whitespace();
    const character = text[index];
    if (character === '"') return string();
    if (character === "{") {
      index += 1;
      whitespace();
      const result: Record<string, unknown> = {};
      const keys = new Set<string>();
      if (text[index] === "}") { index += 1; return result; }
      let count = 0;
      while (true) {
        whitespace();
        if (text[index] !== '"') fail("contains a non-string object key.");
        const key = string();
        if (keys.has(key)) fail(`contains duplicate key '${key}'.`);
        keys.add(key);
        whitespace();
        if (text[index] !== ":") fail("contains an object key without a value separator.");
        index += 1;
        result[key] = value(depth + 1);
        count += 1;
        if (count > 10_000) fail("contains too many object members.");
        whitespace();
        if (text[index] === "}") { index += 1; return result; }
        if (text[index] !== ",") fail("contains an invalid object delimiter.");
        index += 1;
      }
    }
    if (character === "[") {
      index += 1;
      whitespace();
      const result: unknown[] = [];
      if (text[index] === "]") { index += 1; return result; }
      while (true) {
        result.push(value(depth + 1));
        if (result.length > 10_000) fail("contains too many array items.");
        whitespace();
        if (text[index] === "]") { index += 1; return result; }
        if (text[index] !== ",") fail("contains an invalid array delimiter.");
        index += 1;
      }
    }
    for (const [literal, parsed] of [["true", true], ["false", false], ["null", null]] as const) {
      if (text.startsWith(literal, index)) { index += literal.length; return parsed; }
    }
    const match = text.slice(index).match(/^-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?/);
    if (match !== null) {
      index += match[0].length;
      const parsed = Number(match[0]);
      if (!Number.isFinite(parsed)) fail("contains a non-finite number.");
      return parsed;
    }
    return fail("contains an invalid value.");
  };

  const parsed = value(0);
  whitespace();
  if (index !== text.length) fail("contains trailing content.");
  return parsed;
}
