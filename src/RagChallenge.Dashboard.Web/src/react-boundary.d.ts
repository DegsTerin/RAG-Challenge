// Purpose: Defines only the React surface used by the empty dashboard scaffold; full UI typings remain a later implementation concern.
declare module "react" {
  export const StrictMode: unknown;

  export function createElement(
    type: unknown,
    properties: Record<string, unknown> | null,
    ...children: unknown[]
  ): unknown;
}

declare module "react-dom/client" {
  interface Root {
    render(node: unknown): void;
  }

  export function createRoot(container: Element | DocumentFragment): Root;
}
