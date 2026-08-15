// Purpose: Enforces repository text hygiene and source-module purpose headers without adding a second linting stack.
import { readFile, readdir } from "node:fs/promises";
import { extname, join, relative } from "node:path";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("../", import.meta.url));
const checkedExtensions = new Set([".ts", ".mjs", ".json", ".md"]);
const excludedDirectories = new Set(["dist", "node_modules"]);
const failures = [];

async function walk(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  entries.sort((left, right) => left.name.localeCompare(right.name, "en"));

  for (const entry of entries) {
    if (excludedDirectories.has(entry.name)) {
      continue;
    }

    const path = join(directory, entry.name);
    if (entry.isDirectory()) {
      await walk(path);
      continue;
    }

    if (!checkedExtensions.has(extname(entry.name))) {
      continue;
    }

    const bytes = await readFile(path);
    const text = bytes.toString("utf8");
    const displayPath = relative(root, path);

    if (bytes.includes(13)) {
      failures.push(`${displayPath}: contains carriage-return bytes`);
    }
    if (!text.endsWith("\n")) {
      failures.push(`${displayPath}: missing final newline`);
    }
    if (text.split("\n").some((line) => /[ \t]+$/.test(line))) {
      failures.push(`${displayPath}: contains trailing whitespace`);
    }
    if ((entry.name.endsWith(".ts") || entry.name.endsWith(".mjs")) &&
        !text.startsWith("// Purpose:")) {
      failures.push(`${displayPath}: missing module purpose header`);
    }
  }
}

await walk(root);

if (failures.length > 0) {
  for (const failure of failures) {
    console.error(failure);
  }
  process.exitCode = 1;
} else {
  console.log("Orchestrator text and module-header checks passed.");
}
