# Phase 8: Notification Extensions

Extend the boilerplate Notification module with WhatsApp Business API, SMS, bilingual templates, escalation rules, and tenant-branded messages.

**Estimated Time:** 2 weeks (parallel with P9)

---

## P8-T01: Extend Notification Module for Tadbeer Domain

**Dependencies:** P0-T03

**Files:**
- `src/Modules/Notification/Notification.Core/Entities/NotificationTemplate.cs`
- `src/Modules/Notification/Notification.Core/Entities/EscalationRule.cs`
- `src/Modules/Notification/Notification.Core/Services/WhatsAppNotificationChannel.cs`
- `src/Modules/Notification/Notification.Core/Services/SmsNotificationChannel.cs`
- `src/Modules/Notification/Notification.Core/Services/INotificationChannel.cs`
- `src/Modules/Notification/Notification.Core/Services/NotificationDispatcher.cs`
- `src/Modules/Notification/Notification.Core/Consumers/` (domain event consumers)

**Instructions:**
1. `NotificationTemplate` : TenantScopedEntity. EventType (string, e.g., 'contract.expiring'), Channel (enum: InApp, Email, SMS, WhatsApp, Push), Title (LocalizedString), Body (LocalizedString), IsActive, Variables (JSONB list of placeholder names). Templates per tenant (agency-branded).
2. `EscalationRule` : TenantScopedEntity. EventType, InitialChannel, EscalationChannel, EscalateAfterHours (int, e.g., 48), EscalateToRole (string, e.g., 'agency-admin').
3. `INotificationChannel`: SendAsync(Notification, Recipient). Implementations: InAppChannel (SSE), EmailChannel (Postmark/SES), SmsNotificationChannel (Twilio), WhatsAppNotificationChannel (WhatsApp Business API).
4. `NotificationDispatcher`: On domain event, look up templates for that event type. Resolve recipient's preferred channel. Send via INotificationChannel. If escalation rule exists, schedule Hangfire job to check for action; if no action, re-send via escalation channel.
5. Domain event consumers: Contract expiry reminders (30/14/7 days), visa expiry, medical certificate expiry, payment due, payment overdue, WPS deadline, guarantee period expiring. Each consumer calls NotificationDispatcher.
6. All client-facing notifications carry tenant branding: logo, name, contact info, colors (from Tenant entity).
7. API: GET `.../notification-templates` (admin manages templates), PUT `.../notification-templates/{id}`, GET `.../escalation-rules`.

**Tests:**
- [ ] Unit test: NotificationDispatcher selects correct channel from recipient preference
- [ ] Unit test: Escalation triggers after configured hours with no action
- [ ] Unit test: Bilingual template resolves correct language for recipient
- [ ] Integration test: Contract expiry 30 days out sends notification via configured channel

**Acceptance:** Multi-channel notifications with bilingual support, escalation, and tenant branding.

**Status:** ⏳ Pending
