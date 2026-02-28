export interface EmailChannelSettings {
  enabled: boolean;
  provider: 'smtp' | 'sendgrid';
  smtpHost?: string;
  smtpPort: number;
  smtpUsername?: string;
  smtpPassword?: string;
  useSsl: boolean;
  sendGridApiKey?: string;
  fromEmail?: string;
  fromName?: string;
}

export interface WhatsAppChannelSettings {
  enabled: boolean;
  apiToken?: string;
}

export interface TelegramChannelSettings {
  enabled: boolean;
  apiToken?: string;
}

export interface EventNotificationPreference {
  channels: string[];
  muted: boolean;
}

export interface TenantNotificationSettings {
  email: EmailChannelSettings;
  whatsapp: WhatsAppChannelSettings;
  telegram: TelegramChannelSettings;
  eventPreferences: Record<string, EventNotificationPreference>;
}
