import { useEffect, useRef } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useAuth } from 'react-oidc-context';
import { SseClient } from '../lib/sse-client';

/**
 * Maintains SSE connection lifecycle.
 * On `notification.new` events, invalidates React Query notification caches.
 */
export function useSseConnection() {
  const auth = useAuth();
  const queryClient = useQueryClient();
  const clientRef = useRef<SseClient | null>(null);

  useEffect(() => {
    if (!auth.isAuthenticated) {
      clientRef.current?.disconnect();
      clientRef.current = null;
      return;
    }

    const client = new SseClient();
    clientRef.current = client;

    client.on('notification.new', () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['notifications-unread-count'] });
    });

    client.connect();

    return () => {
      client.disconnect();
      clientRef.current = null;
    };
  }, [auth.isAuthenticated, queryClient]);
}
