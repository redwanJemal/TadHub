import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Send, Info, AlertTriangle, CheckCircle, XCircle, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useSendNotification } from '../hooks';
import { useTenants } from '@/features/tenants/hooks';
import { useTenantMembers } from '@/features/tenants/hooks';
import { NotificationTypeSelect } from '../components/NotificationTypeSelect';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Textarea } from '@/shared/components/ui/textarea';
import { Badge } from '@/shared/components/ui/badge';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';

const typeIconMap: Record<string, typeof Info> = {
  info: Info,
  warning: AlertTriangle,
  success: CheckCircle,
  error: XCircle,
};

const typeColorMap: Record<string, string> = {
  info: 'border-blue-200 bg-blue-50',
  warning: 'border-yellow-200 bg-yellow-50',
  success: 'border-green-200 bg-green-50',
  error: 'border-red-200 bg-red-50',
};

const typeIconColorMap: Record<string, string> = {
  info: 'text-blue-600',
  warning: 'text-yellow-600',
  success: 'text-green-600',
  error: 'text-red-600',
};

export function SendNotificationPage() {
  const { t } = useTranslation('notifications');
  const { t: tc } = useTranslation('common');
  const navigate = useNavigate();
  const sendNotification = useSendNotification();

  const [tenantId, setTenantId] = useState('');
  const [recipientMode, setRecipientMode] = useState<'all' | 'specific'>('all');
  const [selectedUserIds, setSelectedUserIds] = useState<string[]>([]);
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [type, setType] = useState('info');
  const [link, setLink] = useState('');

  const { data: tenants } = useTenants({ pageSize: 100 });
  const { data: members } = useTenantMembers(tenantId, { pageSize: 100 });

  const canSend = tenantId && title.trim() && body.trim() && (recipientMode === 'all' || selectedUserIds.length > 0);

  const handleSend = async () => {
    if (!canSend) return;

    await sendNotification.mutateAsync({
      tenantId,
      title: title.trim(),
      body: body.trim(),
      type,
      link: link.trim() || undefined,
      userIds: recipientMode === 'specific' ? selectedUserIds : undefined,
    });

    navigate('/notifications');
  };

  const toggleUser = (userId: string) => {
    setSelectedUserIds((prev) =>
      prev.includes(userId) ? prev.filter((id) => id !== userId) : [...prev, userId],
    );
  };

  const PreviewIcon = typeIconMap[type] ?? Info;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/notifications')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('send.title')}</h1>
          <p className="text-muted-foreground">{t('send.description')}</p>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Form */}
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>{t('send.title')}</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Tenant Selector */}
              <div className="space-y-2">
                <label className="text-sm font-medium">{t('fields.tenant')}</label>
                <Select value={tenantId} onValueChange={(v) => { setTenantId(v); setSelectedUserIds([]); }}>
                  <SelectTrigger>
                    <SelectValue placeholder={t('send.selectTenant')} />
                  </SelectTrigger>
                  <SelectContent>
                    {tenants?.items?.map((tenant) => (
                      <SelectItem key={tenant.id} value={tenant.id}>
                        {tenant.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Recipients */}
              <div className="space-y-2">
                <label className="text-sm font-medium">{t('send.recipientMode')}</label>
                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant={recipientMode === 'all' ? 'default' : 'outline'}
                    size="sm"
                    onClick={() => { setRecipientMode('all'); setSelectedUserIds([]); }}
                  >
                    {t('send.allMembers')}
                  </Button>
                  <Button
                    type="button"
                    variant={recipientMode === 'specific' ? 'default' : 'outline'}
                    size="sm"
                    onClick={() => setRecipientMode('specific')}
                  >
                    {t('send.specificUsers')}
                  </Button>
                </div>

                {recipientMode === 'specific' && tenantId && (
                  <div className="mt-3 space-y-2">
                    {selectedUserIds.length > 0 && (
                      <div className="flex flex-wrap gap-1.5 mb-2">
                        {selectedUserIds.map((uid) => {
                          const member = members?.items?.find((m) => m.userId === uid);
                          return (
                            <Badge key={uid} variant="secondary" className="gap-1">
                              {member?.firstName ?? uid.slice(0, 8)}
                              <button type="button" onClick={() => toggleUser(uid)}>
                                <X className="h-3 w-3" />
                              </button>
                            </Badge>
                          );
                        })}
                      </div>
                    )}
                    <div className="border rounded-md max-h-48 overflow-y-auto">
                      {members?.items?.map((member) => (
                        <button
                          key={member.userId}
                          type="button"
                          onClick={() => toggleUser(member.userId)}
                          className={`w-full flex items-center gap-3 px-3 py-2 text-sm hover:bg-muted transition-colors ${
                            selectedUserIds.includes(member.userId) ? 'bg-primary/5' : ''
                          }`}
                        >
                          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-xs font-medium text-primary shrink-0">
                            {(member.firstName?.[0] ?? '') + (member.lastName?.[0] ?? '')}
                          </div>
                          <div className="text-start">
                            <p className="font-medium">{member.firstName} {member.lastName}</p>
                            <p className="text-xs text-muted-foreground">{member.email}</p>
                          </div>
                          {selectedUserIds.includes(member.userId) && (
                            <CheckCircle className="h-4 w-4 text-primary ms-auto shrink-0" />
                          )}
                        </button>
                      ))}
                      {(!members?.items || members.items.length === 0) && (
                        <p className="p-3 text-sm text-muted-foreground text-center">
                          {t('send.selectMembers')}
                        </p>
                      )}
                    </div>
                  </div>
                )}
              </div>

              {/* Type */}
              <div className="space-y-2">
                <label className="text-sm font-medium">{t('send.notificationType')}</label>
                <NotificationTypeSelect value={type} onChange={setType} />
              </div>

              {/* Title */}
              <div className="space-y-2">
                <label className="text-sm font-medium">{t('send.notificationTitle')}</label>
                <Input
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder={t('send.titlePlaceholder')}
                />
              </div>

              {/* Body */}
              <div className="space-y-2">
                <label className="text-sm font-medium">{t('send.notificationBody')}</label>
                <Textarea
                  value={body}
                  onChange={(e) => setBody(e.target.value)}
                  placeholder={t('send.bodyPlaceholder')}
                  rows={4}
                />
              </div>

              {/* Link */}
              <div className="space-y-2">
                <label className="text-sm font-medium">{t('send.linkOptional')}</label>
                <Input
                  value={link}
                  onChange={(e) => setLink(e.target.value)}
                  placeholder={t('send.linkPlaceholder')}
                />
              </div>

              {/* Send Button */}
              <div className="flex gap-3 pt-2">
                <Button onClick={handleSend} disabled={!canSend || sendNotification.isPending}>
                  <Send className="h-4 w-4 me-2" />
                  {sendNotification.isPending ? t('send.sending') : t('send.sendButton')}
                </Button>
                <Button variant="outline" onClick={() => navigate('/notifications')}>
                  {tc('actions.cancel')}
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Preview */}
        <div>
          <Card className="sticky top-6">
            <CardHeader>
              <CardTitle className="text-base">{t('send.preview')}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className={`rounded-lg border p-4 space-y-2 ${typeColorMap[type] ?? ''}`}>
                <div className="flex items-start gap-3">
                  <PreviewIcon className={`h-5 w-5 mt-0.5 shrink-0 ${typeIconColorMap[type] ?? ''}`} />
                  <div className="space-y-1 min-w-0">
                    <p className="font-semibold text-sm">
                      {title || t('send.titlePlaceholder')}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {body || t('send.bodyPlaceholder')}
                    </p>
                    {link && (
                      <p className="text-xs text-primary truncate">{link}</p>
                    )}
                  </div>
                </div>
                <div className="flex items-center justify-between pt-2 border-t border-current/10">
                  <span className="text-xs text-muted-foreground">
                    {new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </span>
                  <Badge variant="outline" className="text-xs">
                    {t(`types.${type}`)}
                  </Badge>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
