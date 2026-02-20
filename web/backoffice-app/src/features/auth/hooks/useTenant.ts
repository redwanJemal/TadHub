import { create } from 'zustand';
import { persist } from 'zustand/middleware';

/**
 * Tenant information
 */
export interface Tenant {
  id: string;
  name: string;
  slug: string;
  logo?: string;
  type?: string;
}

/**
 * Tenant state management
 */
interface TenantState {
  currentTenant: Tenant | null;
  availableTenants: Tenant[];
  isLoading: boolean;
  
  setCurrentTenant: (tenant: Tenant | null) => void;
  setAvailableTenants: (tenants: Tenant[]) => void;
  setLoading: (loading: boolean) => void;
  clearTenant: () => void;
}

/**
 * Zustand store for tenant state with localStorage persistence
 */
export const useTenantStore = create<TenantState>()(
  persist(
    (set) => ({
      currentTenant: null,
      availableTenants: [],
      isLoading: false,
      
      setCurrentTenant: (tenant) => set({ currentTenant: tenant }),
      setAvailableTenants: (tenants) => set({ availableTenants: tenants }),
      setLoading: (loading) => set({ isLoading: loading }),
      clearTenant: () => set({ currentTenant: null, availableTenants: [] }),
    }),
    {
      name: 'tadhub-tenant',
      partialize: (state) => ({ 
        currentTenant: state.currentTenant,
      }),
    }
  )
);

/**
 * Hook to access current tenant context
 */
export function useTenant() {
  const { currentTenant, availableTenants, isLoading, setCurrentTenant, clearTenant } = useTenantStore();

  const switchTenant = (tenant: Tenant) => {
    setCurrentTenant(tenant);
    // Reload the page to refresh all data with new tenant context
    window.location.reload();
  };

  const hasTenant = !!currentTenant;
  const hasMultipleTenants = availableTenants.length > 1;

  return {
    tenant: currentTenant,
    tenantId: currentTenant?.id ?? null,
    tenantName: currentTenant?.name ?? null,
    availableTenants,
    isLoading,
    hasTenant,
    hasMultipleTenants,
    switchTenant,
    clearTenant,
  };
}
