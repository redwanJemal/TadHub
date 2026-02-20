import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useTranslation } from 'react-i18next';
import { Building2, Check, Plus, Mail, ArrowRight, Loader2 } from 'lucide-react';
import { apiClient } from '@/shared/api/client';
import { useTenantStore, Tenant } from '@/features/auth/hooks/useTenant';
import { cn } from '@/shared/lib/cn';

interface UserOnboardingStatus {
  status: 'onboarding' | 'select_tenant' | 'active';
  pendingInvitations: Array<{ 
    id: string; 
    tenantName: string; 
    tenantId: string;
    role: string;
    invitedByName: string;
  }>;
  canCreateTenant: boolean;
  tenants: Tenant[];
  profile: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
  };
}

export function OnboardingPage() {
  const { t } = useTranslation();
  const auth = useAuth();
  const navigate = useNavigate();
  const { setCurrentTenant, setAvailableTenants } = useTenantStore();
  
  const [status, setStatus] = useState<UserOnboardingStatus | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedTenant, setSelectedTenant] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Show create tenant form
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [newTenantName, setNewTenantName] = useState('');

  useEffect(() => {
    async function fetchStatus() {
      try {
        const data = await apiClient.get<UserOnboardingStatus>('/me');
        setStatus(data);
        
        // If already active, redirect
        if (data.status === 'active') {
          navigate('/', { replace: true });
        }
        
        // Pre-select if only one tenant
        if (data.tenants.length === 1 && data.pendingInvitations.length === 0) {
          setSelectedTenant(data.tenants[0].id);
        }
      } catch (err) {
        console.error('Failed to fetch status:', err);
        setError('Failed to load your account status');
      } finally {
        setIsLoading(false);
      }
    }

    if (auth.isAuthenticated) {
      fetchStatus();
    }
  }, [auth.isAuthenticated, navigate]);

  const handleSelectTenant = async () => {
    if (!selectedTenant || !status) return;

    setIsSubmitting(true);
    setError(null);

    try {
      // Set the tenant and fetch updated status
      const tenant = status.tenants.find(t => t.id === selectedTenant);
      if (tenant) {
        setCurrentTenant(tenant);
        setAvailableTenants(status.tenants);
      }
      
      // Update user's default tenant
      await apiClient.patch('/users/me', { defaultTenantId: selectedTenant });
      
      navigate('/', { replace: true });
    } catch (err) {
      console.error('Failed to select tenant:', err);
      setError('Failed to select workspace. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAcceptInvitation = async (invitationId: string) => {
    setIsSubmitting(true);
    setError(null);

    try {
      await apiClient.post(`/invitations/${invitationId}/accept`);
      // Refresh the status
      const data = await apiClient.get<UserOnboardingStatus>('/me');
      setStatus(data);
    } catch (err) {
      console.error('Failed to accept invitation:', err);
      setError('Failed to accept invitation. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCreateTenant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTenantName.trim()) return;

    setIsSubmitting(true);
    setError(null);

    try {
      const tenant = await apiClient.post<Tenant>('/tenants', {
        name: newTenantName.trim(),
      });
      
      setCurrentTenant(tenant);
      setAvailableTenants([...(status?.tenants || []), tenant]);
      
      navigate('/', { replace: true });
    } catch (err) {
      console.error('Failed to create tenant:', err);
      setError('Failed to create workspace. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  const user = status?.profile;
  const hasTenants = (status?.tenants.length || 0) > 0;
  const hasInvitations = (status?.pendingInvitations.length || 0) > 0;

  return (
    <div className="min-h-screen bg-background flex flex-col">
      {/* Header */}
      <header className="border-b bg-card">
        <div className="container mx-auto px-4 py-4 flex justify-between items-center">
          <div className="flex items-center gap-2">
            <Building2 className="h-6 w-6 text-primary" />
            <span className="font-bold text-xl">TadHub</span>
          </div>
          <div className="text-sm text-muted-foreground">
            {user?.firstName} {user?.lastName}
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="flex-1 container mx-auto px-4 py-8 max-w-2xl">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold mb-2">
            {t('onboarding.welcome', 'Welcome to TadHub')}
          </h1>
          <p className="text-muted-foreground">
            {hasTenants || hasInvitations
              ? t('onboarding.selectWorkspace', 'Select a workspace to continue')
              : t('onboarding.createWorkspace', 'Create your first workspace to get started')}
          </p>
        </div>

        {error && (
          <div className="mb-6 p-4 rounded-lg bg-destructive/10 text-destructive text-sm">
            {error}
          </div>
        )}

        {/* Pending Invitations */}
        {hasInvitations && (
          <div className="mb-8">
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <Mail className="h-5 w-5" />
              {t('onboarding.pendingInvitations', 'Pending Invitations')}
            </h2>
            <div className="space-y-3">
              {status?.pendingInvitations.map((inv) => (
                <div
                  key={inv.id}
                  className="flex items-center justify-between p-4 rounded-lg border bg-card"
                >
                  <div>
                    <p className="font-medium">{inv.tenantName}</p>
                    <p className="text-sm text-muted-foreground">
                      {t('onboarding.invitedBy', 'Invited by {{name}} as {{role}}', {
                        name: inv.invitedByName,
                        role: inv.role,
                      })}
                    </p>
                  </div>
                  <button
                    onClick={() => handleAcceptInvitation(inv.id)}
                    disabled={isSubmitting}
                    className="px-4 py-2 rounded-lg bg-primary text-primary-foreground text-sm font-medium hover:bg-primary/90 disabled:opacity-50"
                  >
                    {t('onboarding.accept', 'Accept')}
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Existing Tenants */}
        {hasTenants && (
          <div className="mb-8">
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              {t('onboarding.yourWorkspaces', 'Your Workspaces')}
            </h2>
            <div className="space-y-3">
              {status?.tenants.map((tenant) => (
                <button
                  key={tenant.id}
                  onClick={() => setSelectedTenant(tenant.id)}
                  className={cn(
                    'w-full flex items-center gap-4 p-4 rounded-lg border transition-colors text-start',
                    selectedTenant === tenant.id
                      ? 'border-primary bg-primary/5'
                      : 'bg-card hover:bg-muted'
                  )}
                >
                  {tenant.logo ? (
                    <img
                      src={tenant.logo}
                      alt={tenant.name}
                      className="h-10 w-10 rounded-lg object-cover"
                    />
                  ) : (
                    <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10 text-lg font-semibold text-primary">
                      {tenant.name[0]?.toUpperCase()}
                    </div>
                  )}
                  <div className="flex-1">
                    <p className="font-medium">{tenant.name}</p>
                    <p className="text-sm text-muted-foreground">{tenant.slug}</p>
                  </div>
                  {selectedTenant === tenant.id && (
                    <Check className="h-5 w-5 text-primary" />
                  )}
                </button>
              ))}
            </div>

            {selectedTenant && (
              <button
                onClick={handleSelectTenant}
                disabled={isSubmitting}
                className="mt-4 w-full flex items-center justify-center gap-2 px-4 py-3 rounded-lg bg-primary text-primary-foreground font-medium hover:bg-primary/90 disabled:opacity-50"
              >
                {isSubmitting ? (
                  <Loader2 className="h-5 w-5 animate-spin" />
                ) : (
                  <>
                    {t('onboarding.continue', 'Continue')}
                    <ArrowRight className="h-5 w-5" />
                  </>
                )}
              </button>
            )}
          </div>
        )}

        {/* Create New Tenant */}
        {status?.canCreateTenant && (
          <div>
            {hasTenants && (
              <div className="relative my-8">
                <div className="absolute inset-0 flex items-center">
                  <div className="w-full border-t" />
                </div>
                <div className="relative flex justify-center text-sm">
                  <span className="bg-background px-4 text-muted-foreground">
                    {t('onboarding.or', 'or')}
                  </span>
                </div>
              </div>
            )}

            {!showCreateForm ? (
              <button
                onClick={() => setShowCreateForm(true)}
                className={cn(
                  'w-full flex items-center justify-center gap-2 p-4 rounded-lg border-2 border-dashed transition-colors',
                  'hover:border-primary hover:bg-primary/5 text-muted-foreground hover:text-foreground'
                )}
              >
                <Plus className="h-5 w-5" />
                {t('onboarding.createNew', 'Create a New Workspace')}
              </button>
            ) : (
              <form onSubmit={handleCreateTenant} className="p-4 rounded-lg border bg-card">
                <h3 className="font-semibold mb-4">
                  {t('onboarding.newWorkspace', 'New Workspace')}
                </h3>
                <div className="mb-4">
                  <label className="block text-sm font-medium mb-1">
                    {t('onboarding.workspaceName', 'Workspace Name')}
                  </label>
                  <input
                    type="text"
                    value={newTenantName}
                    onChange={(e) => setNewTenantName(e.target.value)}
                    placeholder={t('onboarding.workspaceNamePlaceholder', 'e.g., My Company')}
                    className="w-full px-3 py-2 rounded-lg border bg-background focus:outline-none focus:ring-2 focus:ring-primary"
                    autoFocus
                  />
                </div>
                <div className="flex gap-3">
                  <button
                    type="button"
                    onClick={() => {
                      setShowCreateForm(false);
                      setNewTenantName('');
                    }}
                    className="flex-1 px-4 py-2 rounded-lg border hover:bg-muted"
                  >
                    {t('onboarding.cancel', 'Cancel')}
                  </button>
                  <button
                    type="submit"
                    disabled={isSubmitting || !newTenantName.trim()}
                    className="flex-1 flex items-center justify-center gap-2 px-4 py-2 rounded-lg bg-primary text-primary-foreground font-medium hover:bg-primary/90 disabled:opacity-50"
                  >
                    {isSubmitting ? (
                      <Loader2 className="h-5 w-5 animate-spin" />
                    ) : (
                      t('onboarding.create', 'Create')
                    )}
                  </button>
                </div>
              </form>
            )}
          </div>
        )}
      </main>
    </div>
  );
}
