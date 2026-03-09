import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Bell, BellOff, Save } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useNotificationPreferences, useUpdateNotificationPreferences } from '../hooks';
import type { UpdateUserNotificationPreferenceRequest } from '../types';

const EVENT_TYPES = [
  { key: 'candidate.status_changed', category: 'maid' },
  { key: 'placement.created', category: 'booking' },
  { key: 'placement.status_changed', category: 'booking' },
  { key: 'arrival.scheduled', category: 'arrival' },
  { key: 'arrival.confirmed', category: 'arrival' },
  { key: 'arrival.no_show', category: 'arrival' },
  { key: 'arrival.pickup_confirmed', category: 'arrival' },
  { key: 'visa.status_changed', category: 'visa' },
  { key: 'visa.issued', category: 'visa' },
  { key: 'trial.created', category: 'trial' },
  { key: 'trial.completion_due', category: 'trial' },
  { key: 'trial.completed', category: 'trial' },
  { key: 'returnee.created', category: 'returnee' },
  { key: 'returnee.approved', category: 'returnee' },
  { key: 'runaway.reported', category: 'runaway' },
  { key: 'runaway.confirmed', category: 'runaway' },
  { key: 'payment.received', category: 'financial' },
  { key: 'refund.processed', category: 'financial' },
  { key: 'supplier.commission_due', category: 'financial' },
  { key: 'payment.overdue', category: 'financial' },
  { key: 'document.expiring', category: 'document' },
  { key: 'document.expired', category: 'document' },
  { key: 'contract.status_changed', category: 'contract' },
] as const;

const CATEGORIES = [
  'maid', 'booking', 'arrival', 'visa', 'trial',
  'returnee', 'runaway', 'financial', 'document', 'contract',
] as const;

interface PrefState {
  [eventType: string]: { muted: boolean; channels: string };
}

export function NotificationPreferencesPage() {
  const { t } = useTranslation('notifications');
  const { data: preferences, isLoading } = useNotificationPreferences();
  const updatePreferences = useUpdateNotificationPreferences();
  const [prefs, setPrefs] = useState<PrefState>({});
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    if (preferences) {
      const state: PrefState = {};
      for (const p of preferences) {
        state[p.eventType] = { muted: p.muted, channels: p.channels };
      }
      setPrefs(state);
      setHasChanges(false);
    }
  }, [preferences]);

  const toggleMuted = (eventType: string) => {
    setPrefs((prev) => ({
      ...prev,
      [eventType]: {
        muted: !(prev[eventType]?.muted ?? false),
        channels: prev[eventType]?.channels ?? 'in_app',
      },
    }));
    setHasChanges(true);
  };

  const toggleChannel = (eventType: string, channel: string) => {
    setPrefs((prev) => {
      const current = prev[eventType]?.channels ?? 'in_app';
      const channels = current.split(',').filter(Boolean);
      const hasChannel = channels.includes(channel);
      const newChannels = hasChannel
        ? channels.filter((c) => c !== channel)
        : [...channels, channel];
      if (newChannels.length === 0) newChannels.push('in_app');
      return {
        ...prev,
        [eventType]: {
          muted: prev[eventType]?.muted ?? false,
          channels: newChannels.join(','),
        },
      };
    });
    setHasChanges(true);
  };

  const handleSave = () => {
    const items: UpdateUserNotificationPreferenceRequest[] = Object.entries(prefs).map(
      ([eventType, val]) => ({
        eventType,
        muted: val.muted,
        channels: val.channels,
      })
    );
    updatePreferences.mutate({ preferences: items });
    setHasChanges(false);
  };

  if (isLoading) return <PreferencesSkeleton />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('preferences.title')}</h1>
          <p className="text-sm text-muted-foreground mt-1">{t('preferences.description')}</p>
        </div>
        <Button onClick={handleSave} disabled={!hasChanges || updatePreferences.isPending}>
          <Save className="me-2 h-4 w-4" />
          {t('preferences.save')}
        </Button>
      </div>

      {CATEGORIES.map((category) => {
        const events = EVENT_TYPES.filter((e) => e.category === category);
        if (events.length === 0) return null;
        return (
          <Card key={category}>
            <CardHeader>
              <CardTitle className="text-base">{t(`preferences.categories.${category}`)}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {events.map((event) => {
                  const pref = prefs[event.key];
                  const isMuted = pref?.muted ?? false;
                  const channels = (pref?.channels ?? 'in_app').split(',');
                  return (
                    <div
                      key={event.key}
                      className="flex items-center justify-between rounded-lg border p-3"
                    >
                      <div className="flex items-center gap-3">
                        <button
                          onClick={() => toggleMuted(event.key)}
                          className="shrink-0"
                        >
                          {isMuted ? (
                            <BellOff className="h-4 w-4 text-muted-foreground" />
                          ) : (
                            <Bell className="h-4 w-4 text-primary" />
                          )}
                        </button>
                        <div>
                          <p className="text-sm font-medium">
                            {t(`preferences.events.${event.key.replace(/\./g, '_')}`)}
                          </p>
                        </div>
                      </div>
                      {!isMuted && (
                        <div className="flex items-center gap-2">
                          <label className="flex items-center gap-1.5 text-xs">
                            <input
                              type="checkbox"
                              checked={channels.includes('in_app')}
                              onChange={() => toggleChannel(event.key, 'in_app')}
                              className="rounded border-input"
                            />
                            {t('preferences.channels.in_app')}
                          </label>
                          <label className="flex items-center gap-1.5 text-xs">
                            <input
                              type="checkbox"
                              checked={channels.includes('email')}
                              onChange={() => toggleChannel(event.key, 'email')}
                              className="rounded border-input"
                            />
                            {t('preferences.channels.email')}
                          </label>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}

function PreferencesSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <Skeleton className="h-8 w-56" />
          <Skeleton className="h-4 w-80" />
        </div>
        <Skeleton className="h-9 w-24" />
      </div>
      {Array.from({ length: 4 }).map((_, i) => (
        <Card key={i}>
          <CardHeader>
            <Skeleton className="h-5 w-32" />
          </CardHeader>
          <CardContent className="space-y-3">
            {Array.from({ length: 3 }).map((_, j) => (
              <div key={j} className="flex items-center justify-between rounded-lg border p-3">
                <div className="flex items-center gap-3">
                  <Skeleton className="h-4 w-4" />
                  <Skeleton className="h-4 w-48" />
                </div>
                <div className="flex gap-2">
                  <Skeleton className="h-4 w-16" />
                  <Skeleton className="h-4 w-16" />
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
