import { WebStorageStateStore } from 'oidc-client-ts';
import type { UserManagerSettings } from 'oidc-client-ts';

// TadHub Backoffice Keycloak Configuration
const KEYCLOAK_URL = import.meta.env.VITE_AUTH_URL || 'https://auth.endlessmaker.com';
const KEYCLOAK_REALM = import.meta.env.VITE_AUTH_REALM || 'tadhub';
const KEYCLOAK_CLIENT_ID = import.meta.env.VITE_AUTH_CLIENT_ID || 'backoffice-app';

export const oidcConfig: UserManagerSettings = {
  authority: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}`,
  client_id: KEYCLOAK_CLIENT_ID,
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: `${window.location.origin}/login`,
  response_type: 'code',
  scope: 'openid profile email',
  
  automaticSilentRenew: true,
  silentRequestTimeoutInSeconds: 30,
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  loadUserInfo: true,
  accessTokenExpiringNotificationTimeInSeconds: 60,
};

export const keycloakUrls = {
  base: KEYCLOAK_URL,
  realm: KEYCLOAK_REALM,
  account: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/account`,
  logout: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/logout`,
  register: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/registrations?client_id=${KEYCLOAK_CLIENT_ID}&response_type=code&scope=openid&redirect_uri=${encodeURIComponent(window.location.origin + '/callback')}`,
  resetPassword: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/login-actions/reset-credentials?client_id=${KEYCLOAK_CLIENT_ID}`,
};

export const isDevelopment = import.meta.env.DEV;
export const apiBaseUrl = import.meta.env.VITE_API_URL || 'https://api.endlessmaker.com/api/v1';
