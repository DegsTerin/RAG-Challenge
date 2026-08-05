// Purpose: Declares the minimal React surface used by the Dashboard while keeping third-party type packages outside the authorised dependency boundary.
declare module "react" {
  export type ReactNode = unknown;
  export type SetStateAction<T> = T | ((previous: T) => T);
  export type Dispatch<T> = (value: T) => void;

  export interface RefObject<T> {
    current: T | null;
  }

  export function StrictMode(properties: { children?: ReactNode }): JSX.Element;

  export function createElement(
    type: unknown,
    properties: Record<string, unknown> | null,
    ...children: unknown[]
  ): unknown;

  export function useEffect(effect: () => void | (() => void), dependencies?: readonly unknown[]): void;
  export function useReducer<State, Action>(
    reducer: (state: State, action: Action) => State,
    initialState: State,
  ): [State, Dispatch<Action>];
  export function useRef<T>(initialValue: T | null): RefObject<T>;
  export function useState<T>(
    initialState: T | (() => T),
  ): [T, Dispatch<SetStateAction<T>>];
}

declare module "react/jsx-runtime" {
  export function jsx(type: unknown, properties: Record<string, unknown>, key?: string): JSX.Element;
  export function jsxs(type: unknown, properties: Record<string, unknown>, key?: string): JSX.Element;
  export const Fragment: unknown;
}

declare module "react-dom/client" {
  interface Root {
    render(node: unknown): void;
  }

  export function createRoot(container: Element | DocumentFragment): Root;
}

declare namespace JSX {
  interface Element {}

  interface IntrinsicElements {
    [elementName: string]: Record<string, unknown>;
  }
}
