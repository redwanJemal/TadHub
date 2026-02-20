import { useTranslation } from 'react-i18next';
import { Plus, Key } from 'lucide-react';

export function ApiKeysPage() {
  const { t } = useTranslation('settings');

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">{t('api.title')}</h1>
          <p className="text-muted-foreground mt-1">{t('api.description')}</p>
        </div>
        <button className="flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90">
          <Plus className="h-4 w-4" />
          {t('api.createKey')}
        </button>
      </div>

      <div className="rounded-xl bg-card border border-border">
        <div className="p-6 text-center">
          <Key className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
          <p className="text-muted-foreground">No API keys yet. Create your first API key to get started.</p>
        </div>
      </div>
    </div>
  );
}
