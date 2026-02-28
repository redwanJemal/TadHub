import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { UserPlus, FileText, ShieldCheck, Users } from 'lucide-react';

export function QuickActions() {
  const { t } = useTranslation('dashboard');
  const navigate = useNavigate();

  const actions = [
    { label: t('quickActions.addCandidate'), icon: UserPlus, href: '/candidates/new' },
    { label: t('quickActions.newContract'), icon: FileText, href: '/contracts/new' },
    { label: t('quickActions.compliance'), icon: ShieldCheck, href: '/compliance' },
    { label: t('quickActions.workers'), icon: Users, href: '/workers' },
  ];

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
