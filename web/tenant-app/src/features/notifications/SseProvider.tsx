import type { ReactNode } from 'react';
import { useSseConnection } from './hooks/useSseConnection';

/**
 * Provider that maintains the SSE connection lifecycle.
 * Place inside AuthProvider in the app root.
 */
export function SseProvider({ children }: { children: ReactNode }) {
  useSseConnection();
  return <>{children}</>;
}
