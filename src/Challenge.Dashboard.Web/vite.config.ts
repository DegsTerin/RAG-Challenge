// Purpose: Builds the isolated React/TypeScript dashboard boundary without server, provider, or deployment coupling.
import { defineConfig } from "vite";

export default defineConfig({
  build: {
    outDir: "dist",
    emptyOutDir: true,
  },
});
