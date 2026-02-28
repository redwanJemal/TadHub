import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { UserPlus, FileText, ShieldCheck, Users } from 'lucide-react';
import { usePermissions } from '@/features/auth/hooks/usePermissions';

export function QuickActions() {
  const { t } = useTranslation('dashboard');
  const navigate = useNavigate();
  const { hasPermission, isLoaded } = usePermissions();

  const allActions = [
    { label: t('quickActions.addCandidate'), icon: UserPlus, href: '/candidates/new', permission: 'candidates.create' },
    { label: t('quickActions.newContract'), icon: FileText, href: '/contracts/new', permission: 'contracts.create' },
    { label: t('quickActions.compliance'), icon: ShieldCheck, href: '/compliance', permission: 'documents.view' },
    { label: t('quickActions.workers'), icon: Users, href: '/workers', permission: 'workers.view' },
  ];

  const actions = isLoaded
    ? allActions.filter((a) => hasPermission(a.permission))
    : allActions;

  if (actions.length === 0) return null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t('quickActions.title')}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-2">
          {actions.map((action) => (
            <Button
              key={action.href}
              variant="outline"
              className="h-auto flex-col gap-1 py-3"
              onClick={() => navigate(action.href)}
            >
              <action.icon className="h-4 w-4" />
              <span className="text-xs">{action.label}</span>
            </Button>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
