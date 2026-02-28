import { getAccessToken, getTenantId } from '@/features/auth/AuthProvider';
import { API_BASE } from '@/shared/api/client';

type SseEventHandler = (data: unknown) => void;

/**
 * Custom SSE client using fetch() + ReadableStream.
 * Avoids putting tokens in URLs — sends Authorization header instead.
 * Auto-reconnects on error with exponential backoff.
 */
export class SseClient {
  private controller: AbortController | null = null;
  private reconnectAttempt = 0;
  private maxReconnectDelay = 30_000;
  private handlers = new Map<string, Set<SseEventHandler>>();
  private active = false;

  constructor(private endpoint: string = '/events/stream') {}

  on(eventType: string, handler: SseEventHandler) {
    if (!this.handlers.has(eventType)) {
      this.handlers.set(eventType, new Set());
    }
    this.handlers.get(eventType)!.add(handler);
    return () => this.off(eventType, handler);
  }

  off(eventType: string, handler: SseEventHandler) {
    this.handlers.get(eventType)?.delete(handler);
  }

  connect() {
    if (this.active) return;
    this.active = true;
    this.reconnectAttempt = 0;
    this.startConnection();
  }

  disconnect() {
    this.active = false;
    this.controller?.abort();
    this.controller = null;
  }

  private async startConnection() {
    if (!this.active) return;

    const token = getAccessToken();
    const tenantId = getTenantId();
    if (!token) {
      this.scheduleReconnect();
      return;
    }

    this.controller = new AbortController();
    const url = `${API_BASE}${this.endpoint}`;

    try {
      const response = await fetch(url, {
        headers: {
          'Authorization': `Bearer ${token}`,
          ...(tenantId ? { 'X-Tenant-ID': tenantId } : {}),
        },
        signal: this.controller.signal,
      });

      if (!response.ok || !response.body) {
        throw new Error(`SSE connection failed: ${response.status}`);
      }

      this.reconnectAttempt = 0;
      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (this.active) {
        const { done, value } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n');
        buffer = lines.pop() || '';

        let currentEvent = '';
        let currentData = '';

        for (const line of lines) {
          if (line.startsWith('event: ')) {
            currentEvent = line.slice(7).trim();
          } else if (line.startsWith('data: ')) {
            currentData += line.slice(6);
          } else if (line === '' && currentEvent) {
            this.emit(currentEvent, currentData);
            currentEvent = '';
            currentData = '';
          }
        }
      }
    } catch (err) {
      if ((err as Error).name === 'AbortError') return;
      console.warn('[SSE] Connection error:', err);
    }

    this.scheduleReconnect();
  }

  private emit(eventType: string, rawData: string) {
    let data: unknown;
    try {
      data = JSON.parse(rawData);
    } catch {
      data = rawData;
    }

    const handlers = this.handlers.get(eventType);
    if (handlers) {
      for (const handler of handlers) {
        try {
          handler(data);
        } catch (err) {
          console.error(`[SSE] Handler error for ${eventType}:`, err);
        }
      }
    }

    // Also emit to wildcard listeners
    const wildcardHandlers = this.handlers.get('*');
    if (wildcardHandlers) {
      for (const handler of wildcardHandlers) {
        try {
          handler({ type: eventType, data });
        } catch (err) {
          console.error('[SSE] Wildcard handler error:', err);
        }
      }
    }
  }

  private scheduleReconnect() {
    if (!this.active) return;
    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempt), this.maxReconnectDelay);
    this.reconnectAttempt++;
    console.log(`[SSE] Reconnecting in ${delay}ms (attempt ${this.reconnectAttempt})`);
    setTimeout(() => this.startConnection(), delay);
  }
}
