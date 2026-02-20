import { WebStorageStateStore } from 'oidc-client-ts';
import type { UserManagerSettings } from 'oidc-client-ts';

// TadHub Backoffice Keycloak Configuration (Staff/Admin realm)
const KEYCLOAK_URL = import.meta.env.VITE_KEYCLOAK_URL || 'https://auth.tadhub.ae';
const KEYCLOAK_REALM = import.meta.env.VITE_KEYCLOAK_REALM || 'tadhub-staff';
const KEYCLOAK_CLIENT_ID = import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'tadhub-backoffice-app';

export const oidcConfig: UserManagerSettings = {
  authority: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}`,
  client_id: KEYCLOAK_CLIENT_ID,
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile email',
  
  // Token refresh settings
  automaticSilentRenew: true,
  silentRequestTimeoutInSeconds: 30,
  
  // Store tokens in localStorage
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  
  // Load additional user info
  loadUserInfo: true,
  
  // Refresh tokens 60 seconds before expiry
  accessTokenExpiringNotificationTimeInSeconds: 60,
};

/**
 * Keycloak URLs for various operations
 */
export const keycloakUrls = {
  base: KEYCLOAK_URL,
  realm: KEYCLOAK_REALM,
  account: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/account`,
  logout: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/logout`,
};

/**
 * Check if running in development mode
 */
export const isDevelopment = import.meta.env.DEV;

/**
 * API base URL
 */
export const apiBaseUrl = import.meta.env.VITE_API_URL || 'https://api.tadhub.ae/api/v1';
