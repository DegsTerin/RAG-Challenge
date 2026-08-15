// Purpose: Removes the product credential from the CLI process before loading any orchestrator dependency.
const productCredentialIdentifier = ["OPENAI", "API", "KEY"].join("_");
Reflect.deleteProperty(process.env, productCredentialIdentifier);

await import("./cli-main.js");
