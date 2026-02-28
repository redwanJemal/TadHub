import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
import { Settings, Bell } from 'lucide-react';
import { NotificationSettingsTab } from '../components/NotificationSettingsTab';

const VALID_TABS = ['general', 'notifications'] as const;
type SettingsTab = (typeof VALID_TABS)[number];

export function SettingsPage() {
  const { t } = useTranslation('settings');
  const { tab } = useParams<{ tab: string }>();
  const navigate = useNavigate();

  const activeTab: SettingsTab = VALID_TABS.includes(tab as SettingsTab)
    ? (tab as SettingsTab)
    : 'notifications';

  const handleTabChange = (value: string) => {
    navigate(`/settings/${value}`, { replace: true });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('title')}</h1>
        <p className="text-muted-foreground">{t('description')}</p>
      </div>

      <Tabs value={activeTab} onValueChange={handleTabChange}>
        <TabsList>
          <TabsTrigger value="general">
            <Settings className="me-2 h-4 w-4" />
            {t('tabs.general')}
          </TabsTrigger>
          <TabsTrigger value="notifications">
            <Bell className="me-2 h-4 w-4" />
            {t('tabs.notifications')}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="general">
          <div className="rounded-lg border bg-card p-8 text-center text-muted-foreground">
            {t('general.placeholder')}
          </div>
        </TabsContent>

        <TabsContent value="notifications">
          <NotificationSettingsTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}
