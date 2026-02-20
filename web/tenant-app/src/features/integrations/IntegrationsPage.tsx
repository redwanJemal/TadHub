import { useTranslation } from 'react-i18next';
import { Webhook, Database, Cloud } from 'lucide-react';

export function IntegrationsPage() {
  const { t } = useTranslation();

  const integrations = [
    { name: 'Webhooks', icon: Webhook, description: 'Send data to external services', connected: false },
    { name: 'Database', icon: Database, description: 'Connect your database', connected: false },
    { name: 'Cloud Storage', icon: Cloud, description: 'Store files in the cloud', connected: false },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">{t('nav.integrations')}</h1>
        <p className="text-muted-foreground mt-1">Connect your workspace to external services</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {integrations.map((integration) => (
          <div key={integration.name} className="rounded-xl bg-card border border-border p-6">
            <integration.icon className="h-10 w-10 text-primary mb-4" />
            <h3 className="font-semibold text-foreground">{integration.name}</h3>
            <p className="text-sm text-muted-foreground mt-1">{integration.description}</p>
            <button className="mt-4 rounded-lg border border-border px-4 py-2 text-sm font-medium hover:bg-muted">
              Connect
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
