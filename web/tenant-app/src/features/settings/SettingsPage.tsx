import { useTranslation } from 'react-i18next';
import { useState } from 'react';

export function SettingsPage() {
  const { t } = useTranslation('settings');
  const [activeTab, setActiveTab] = useState('general');

  const tabs = [
    { id: 'general', label: t('tabs.general') },
    { id: 'security', label: t('tabs.security') },
    { id: 'api', label: t('tabs.api') },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">{t('title')}</h1>
      </div>

      {/* Tabs */}
      <div className="border-b border-border">
        <nav className="flex gap-6">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab.id
                  ? 'border-primary text-primary'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab content */}
      <div className="rounded-xl bg-card border border-border p-6">
        {activeTab === 'general' && (
          <div className="space-y-6">
            <h2 className="text-lg font-semibold text-foreground">
              {t('general.title')}
            </h2>
            <div className="grid gap-6 max-w-xl">
              <div>
                <label className="block text-sm font-medium text-foreground mb-2">
                  {t('general.workspaceName')}
                </label>
                <input
                  type="text"
                  defaultValue="My Workspace"
                  className="w-full rounded-lg border border-input bg-background px-4 py-2 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-2">
                  {t('general.timezone')}
                </label>
                <select className="w-full rounded-lg border border-input bg-background px-4 py-2 text-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20">
                  <option>UTC</option>
                  <option>America/New_York</option>
                  <option>Europe/London</option>
                  <option>Asia/Riyadh</option>
                </select>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'security' && (
          <div className="space-y-6">
            <h2 className="text-lg font-semibold text-foreground">
              {t('security.title')}
            </h2>
            <p className="text-muted-foreground">
              Security settings coming soon...
            </p>
          </div>
        )}

        {activeTab === 'api' && (
          <div className="space-y-6">
            <h2 className="text-lg font-semibold text-foreground">
              {t('api.title')}
            </h2>
            <p className="text-muted-foreground">
              {t('api.description')}
            </p>
            <button className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 transition-colors">
              {t('api.createKey')}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
