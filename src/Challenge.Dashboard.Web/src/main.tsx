// Purpose: Verifies the React composition boundary while deliberately rendering no product interface during project setup.
import { createElement, StrictMode } from "react";
import { createRoot } from "react-dom/client";

const rootElement = document.getElementById("root");

if (rootElement === null) {
  throw new Error("The dashboard root element is required.");
}

createRoot(rootElement).render(createElement(StrictMode, null));
