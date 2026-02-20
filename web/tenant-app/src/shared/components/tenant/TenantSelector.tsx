import { useState } from 'react';
import { Check, ChevronsUpDown, Building2, Plus } from 'lucide-react';
import { useTenant, Tenant } from '@/features/auth/hooks/useTenant';
import { useTranslation } from 'react-i18next';
import { cn } from '@/shared/lib/cn';

interface TenantSelectorProps {
  className?: string;
  showCreateOption?: boolean;
  onCreateTenant?: () => void;
}

export function TenantSelector({ 
  className,
  showCreateOption = false,
  onCreateTenant,
}: TenantSelectorProps) {
  const { t } = useTranslation();
  const { tenant, availableTenants, hasMultipleTenants, switchTenant } = useTenant();
  const [isOpen, setIsOpen] = useState(false);

  // Don't show if user only has one tenant
  if (!hasMultipleTenants && !showCreateOption) {
    return null;
  }

  const handleSelect = (selectedTenant: Tenant) => {
    if (selectedTenant.id !== tenant?.id) {
      switchTenant(selectedTenant);
    }
    setIsOpen(false);
  };

  return (
    <div className={cn('relative', className)}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 rounded-lg border bg-background px-3 py-2 text-sm hover:bg-muted transition-colors min-w-[180px]"
      >
        <Building2 className="h-4 w-4 text-muted-foreground" />
        <span className="flex-1 text-start truncate">
          {tenant?.name || t('tenant.select', 'Select Workspace')}
        </span>
        <ChevronsUpDown className="h-4 w-4 text-muted-foreground" />
      </button>

      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setIsOpen(false)}
          />
          <div className="absolute start-0 top-full z-50 mt-1 w-full min-w-[220px] rounded-lg border bg-card shadow-lg">
            <div className="p-1">
              <p className="px-2 py-1.5 text-xs font-medium text-muted-foreground">
                {t('tenant.workspaces', 'Workspaces')}
              </p>
              
              {availableTenants.map((t) => (
                <button
                  key={t.id}
                  onClick={() => handleSelect(t)}
                  className={cn(
                    'flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm hover:bg-muted transition-colors',
                    t.id === tenant?.id && 'bg-muted'
                  )}
                >
                  {t.logo ? (
                    <img
                      src={t.logo}
                      alt={t.name}
                      className="h-5 w-5 rounded object-cover"
                    />
                  ) : (
                    <div className="flex h-5 w-5 items-center justify-center rounded bg-primary/10 text-xs font-medium text-primary">
                      {t.name[0]?.toUpperCase()}
                    </div>
                  )}
                  <span className="flex-1 truncate text-start">{t.name}</span>
                  {t.id === tenant?.id && (
                    <Check className="h-4 w-4 text-primary" />
                  )}
                </button>
              ))}

              {showCreateOption && (
                <>
                  <div className="my-1 border-t" />
                  <button
                    onClick={() => {
                      setIsOpen(false);
                      onCreateTenant?.();
                    }}
                    className="flex w-full items-center gap-2 rounded-md px-2 py-2 text-sm text-primary hover:bg-muted transition-colors"
                  >
                    <Plus className="h-4 w-4" />
                    {t('tenant.create', 'Create Workspace')}
                  </button>
                </>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
