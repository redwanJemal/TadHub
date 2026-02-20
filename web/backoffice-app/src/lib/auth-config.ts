import { WebStorageStateStore } from 'oidc-client-ts';
import type { UserManagerSettings } from 'oidc-client-ts';

const KEYCLOAK_URL = import.meta.env.VITE_KEYCLOAK_URL || 'https://auth.endlessmaker.com';
const KEYCLOAK_REALM = import.meta.env.VITE_KEYCLOAK_REALM || 'staff';
const KEYCLOAK_CLIENT_ID = import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'backoffice-app';

// Get base path for redirect URIs (handles /admin prefix in production)
const getRedirectUri = () => {
  const basePath = window.location.pathname.startsWith('/admin') ? '/admin' : '';
  return `${window.location.origin}${basePath}/callback`;
};

const getPostLogoutUri = () => {
  const basePath = window.location.pathname.startsWith('/admin') ? '/admin' : '';
  return `${window.location.origin}${basePath}`;
};

export const oidcConfig: UserManagerSettings = {
  authority: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}`,
  client_id: KEYCLOAK_CLIENT_ID,
  redirect_uri: getRedirectUri(),
  post_logout_redirect_uri: getPostLogoutUri(),
  response_type: 'code',
  scope: 'openid profile email',
  automaticSilentRenew: true,
  silentRequestTimeoutInSeconds: 10,
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  loadUserInfo: true,
};

export const keycloakUrls = {
  base: KEYCLOAK_URL,
  realm: KEYCLOAK_REALM,
  account: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/account`,
  logout: `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/logout`,
};
