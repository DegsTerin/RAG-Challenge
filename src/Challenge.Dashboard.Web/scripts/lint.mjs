// Purpose: Applies dependency-free repository text checks to dashboard sources without requiring an external lint service.
import { readFile, readdir } from "node:fs/promises";
import { extname, join } from "node:path";

const root = new URL("../", import.meta.url);
const checkedExtensions = new Set([
  ".html",
  ".js",
  ".json",
  ".mjs",
  ".ts",
  ".tsx",
]);
const ignoredDirectories = new Set(["coverage", "dist", "node_modules"]);
const failures = [];

async function inspectDirectory(directoryUrl) {
  const entries = await readdir(directoryUrl, { withFileTypes: true });

  for (const entry of entries) {
    if (entry.isDirectory() && ignoredDirectories.has(entry.name)) {
      continue;
    }

    const entryUrl = new URL(`${entry.name}${entry.isDirectory() ? "/" : ""}`, directoryUrl);

    if (entry.isDirectory()) {
      await inspectDirectory(entryUrl);
      continue;
    }

    if (!checkedExtensions.has(extname(entry.name))) {
      continue;
    }

    const content = await readFile(entryUrl, "utf8");
    const relativePath = entryUrl.pathname.slice(root.pathname.length);

    if (content.includes("\r")) {
      failures.push(`${relativePath}: carriage return detected`);
    }

    if (!content.endsWith("\n")) {
      failures.push(`${relativePath}: final newline missing`);
    }

    content.split("\n").forEach((line, index) => {
      if (/[ \t]+$/.test(line)) {
        failures.push(`${relativePath}:${index + 1}: trailing whitespace`);
      }
    });
  }
}

await inspectDirectory(root);

if (failures.length > 0) {
  throw new Error(failures.join("\n"));
}
