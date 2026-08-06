// Purpose: Verifies that the dashboard scaffold is reproducible and isolated from server and provider dependencies.
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const packageJson = JSON.parse(
  await readFile(new URL("../package.json", import.meta.url), "utf8"),
);

test("enforces bounded compatible dashboard toolchain updates", () => {
  assert.equal("packageManager" in packageJson, false);
  assert.deepEqual(packageJson.engines, {
    node: ">=24.18.0 <25",
    npm: ">=11.16.0 <12",
  });
  assert.deepEqual(packageJson.devEngines, {
    runtime: {
      name: "node",
      version: ">=24.18.0 <25",
      onFail: "error",
    },
    packageManager: {
      name: "npm",
      version: ">=11.16.0 <12",
      onFail: "error",
    },
  });
});

test("declares only the approved React build boundary", () => {
  assert.deepEqual(Object.keys(packageJson.dependencies).sort(), [
    "react",
    "react-dom",
  ]);

  const serialisedPackage = JSON.stringify(packageJson).toLowerCase();

  for (const prohibitedTerm of [
    "langchain",
    "openai",
    "oracle",
    "sqlite",
    "db-notifier",
  ]) {
    assert.equal(serialisedPackage.includes(prohibitedTerm), false);
  }
});
