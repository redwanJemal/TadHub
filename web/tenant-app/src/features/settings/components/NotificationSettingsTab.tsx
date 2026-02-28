import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import { Button } from '@/shared/components/ui/button';
import { Checkbox } from '@/shared/components/ui/checkbox';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Badge } from '@/shared/components/ui/badge';
import { Mail, MessageCircle, Send, Loader2 } from 'lucide-react';
import { useNotificationSettings, useUpdateNotificationSettings } from '../hooks';
import type { TenantNotificationSettings } from '../types';

const defaultSettings: TenantNotificationSettings = {
  email: {
    enabled: false,
    provider: 'smtp',
    smtpHost: '',
    smtpPort: 587,
    smtpUsername: '',
    smtpPassword: '',
    useSsl: true,
    sendGridApiKey: '',
    fromEmail: '',
    fromName: '',
  },
  whatsapp: { enabled: false },
  telegram: { enabled: false },
  eventPreferences: {},
};

export function NotificationSettingsTab() {
  const { t } = useTranslation('settings');
  const { data, isLoading } = useNotificationSettings();
  const updateSettings = useUpdateNotificationSettings();

  const [settings, setSettings] = useState<TenantNotificationSettings>(defaultSettings);
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  useEffect(() => {
    if (data) {
      setSettings({
        ...defaultSettings,
        ...data,
        email: { ...defaultSettings.email, ...data.email },
        whatsapp: { ...defaultSettings.whatsapp, ...data.whatsapp },
        telegram: { ...defaultSettings.telegram, ...data.telegram },
      });
    }
  }, [data]);

  const handleSave = async () => {
    setFeedback(null);
    try {
      await updateSettings.mutateAsync(settings);
      setFeedback({ type: 'success', message: t('notifications.saveSuccess') });
    } catch {
      setFeedback({ type: 'error', message: t('notifications.saveError') });
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium">{t('notifications.title')}</h3>
        <p className="text-sm text-muted-foreground">{t('notifications.description')}</p>
      </div>

      {/* Email Channel */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-blue-500/10">
                <Mail className="h-5 w-5 text-blue-500" />
              </div>
              <div>
                <CardTitle className="text-base">{t('notifications.email.title')}</CardTitle>
                <CardDescription>{t('notifications.email.description')}</CardDescription>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Checkbox
                id="email-enabled"
                checked={settings.email.enabled}
                onCheckedChange={(checked) =>
                  setSettings((s) => ({ ...s, email: { ...s.email, enabled: checked === true } }))
                }
              />
              <Label htmlFor="email-enabled" className="text-sm">
                {t('notifications.email.enabled')}
              </Label>
            </div>
          </div>
        </CardHeader>

        {settings.email.enabled && (
          <CardContent className="space-y-4">
            {/* Provider */}
            <div className="space-y-2">
              <Label>{t('notifications.email.provider')}</Label>
              <Select
                value={settings.email.provider}
                onValueChange={(value: 'smtp' | 'sendgrid') =>
                  setSettings((s) => ({ ...s, email: { ...s.email, provider: value } }))
                }
              >
                <SelectTrigger className="w-48">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="smtp">{t('notifications.email.smtp')}</SelectItem>
                  <SelectItem value="sendgrid">{t('notifications.email.sendgrid')}</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* SMTP Fields */}
            {settings.email.provider === 'smtp' && (
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="smtp-host">{t('notifications.email.smtpHost')}</Label>
                  <Input
                    id="smtp-host"
                    value={settings.email.smtpHost ?? ''}
                    onChange={(e) =>
                      setSettings((s) => ({ ...s, email: { ...s.email, smtpHost: e.target.value } }))
                    }
                    placeholder={t('notifications.email.smtpHostPlaceholder')}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="smtp-port">{t('notifications.email.smtpPort')}</Label>
                  <Input
                    id="smtp-port"
                    type="number"
                    value={settings.email.smtpPort}
                    onChange={(e) =>
                      setSettings((s) => ({ ...s, email: { ...s.email, smtpPort: parseInt(e.target.value) || 587 } }))
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="smtp-username">{t('notifications.email.smtpUsername')}</Label>
                  <Input
                    id="smtp-username"
                    value={settings.email.smtpUsername ?? ''}
                    onChange={(e) =>
                      setSettings((s) => ({ ...s, email: { ...s.email, smtpUsername: e.target.value } }))
                    }
                    placeholder={t('notifications.email.smtpUsernamePlaceholder')}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="smtp-password">{t('notifications.email.smtpPassword')}</Label>
                  <Input
                    id="smtp-password"
                    type="password"
                    value={settings.email.smtpPassword ?? ''}
                    onChange={(e) =>
                      setSettings((s) => ({ ...s, email: { ...s.email, smtpPassword: e.target.value } }))
                    }
                  />
                </div>
                <div className="flex items-center gap-2 sm:col-span-2">
                  <Checkbox
                    id="smtp-ssl"
                    checked={settings.email.useSsl}
                    onCheckedChange={(checked) =>
                      setSettings((s) => ({ ...s, email: { ...s.email, useSsl: checked === true } }))
                    }
                  />
                  <Label htmlFor="smtp-ssl">{t('notifications.email.useSsl')}</Label>
                </div>
              </div>
            )}

            {/* SendGrid Fields */}
            {settings.email.provider === 'sendgrid' && (
              <div className="space-y-2">
                <Label htmlFor="sendgrid-key">{t('notifications.email.sendGridApiKey')}</Label>
                <Input
                  id="sendgrid-key"
                  type="password"
                  value={settings.email.sendGridApiKey ?? ''}
                  onChange={(e) =>
                    setSettings((s) => ({ ...s, email: { ...s.email, sendGridApiKey: e.target.value } }))
                  }
                  placeholder={t('notifications.email.sendGridApiKeyPlaceholder')}
                />
              </div>
            )}

            {/* From fields (shared) */}
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="from-email">{t('notifications.email.fromEmail')}</Label>
                <Input
                  id="from-email"
                  type="email"
                  value={settings.email.fromEmail ?? ''}
                  onChange={(e) =>
                    setSettings((s) => ({ ...s, email: { ...s.email, fromEmail: e.target.value } }))
                  }
                  placeholder={t('notifications.email.fromEmailPlaceholder')}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="from-name">{t('notifications.email.fromName')}</Label>
                <Input
                  id="from-name"
                  value={settings.email.fromName ?? ''}
                  onChange={(e) =>
                    setSettings((s) => ({ ...s, email: { ...s.email, fromName: e.target.value } }))
                  }
                  placeholder={t('notifications.email.fromNamePlaceholder')}
                />
              </div>
            </div>
          </CardContent>
        )}
      </Card>

      {/* WhatsApp Channel */}
      <Card className="opacity-60">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-green-500/10">
                <MessageCircle className="h-5 w-5 text-green-500" />
              </div>
              <div>
                <CardTitle className="text-base">{t('notifications.whatsapp.title')}</CardTitle>
              </div>
            </div>
            <Badge variant="secondary">{t('notifications.whatsapp.comingSoon')}</Badge>
          </div>
        </CardHeader>
      </Card>

      {/* Telegram Channel */}
      <Card className="opacity-60">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-sky-500/10">
                <Send className="h-5 w-5 text-sky-500" />
              </div>
              <div>
                <CardTitle className="text-base">{t('notifications.telegram.title')}</CardTitle>
              </div>
            </div>
            <Badge variant="secondary">{t('notifications.telegram.comingSoon')}</Badge>
          </div>
        </CardHeader>
      </Card>

      {/* Save */}
      <div className="flex items-center gap-4">
        <Button onClick={handleSave} disabled={updateSettings.isPending}>
          {updateSettings.isPending && <Loader2 className="me-2 h-4 w-4 animate-spin" />}
          {updateSettings.isPending ? t('notifications.saving') : t('notifications.save')}
        </Button>
        {feedback && (
          <p className={feedback.type === 'success' ? 'text-sm text-green-600' : 'text-sm text-destructive'}>
            {feedback.message}
          </p>
        )}
      </div>
    </div>
  );
}
