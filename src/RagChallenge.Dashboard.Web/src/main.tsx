// Purpose: Mounts the accessible Dashboard composition without introducing server, provider, or administration authority into the browser.
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

import { App } from "./App";
import "./styles.css";

const rootElement = document.getElementById("root");

if (rootElement === null) {
  throw new Error("The dashboard root element is required.");
}

createRoot(rootElement).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
