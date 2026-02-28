import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Bell, CheckCheck } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useNotifications, useUnreadCount, useMarkAsRead, useMarkAllAsRead } from '../hooks';
import { NotificationItem } from './NotificationItem';

export function NotificationPanel() {
  const { t } = useTranslation('notifications');
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);

  const { data: unreadData } = useUnreadCount();
  const { data: notificationsData, isLoading } = useNotifications({
    pageSize: 10,
    sort: '-createdAt',
  });

  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();

  const unreadCount = unreadData?.count ?? 0;
  const notifications = notificationsData?.items ?? [];

  return (
    <div className="relative">
      <button
        className="relative rounded-lg p-2 hover:bg-muted"
        onClick={() => setOpen(!open)}
      >
        <Bell className="h-5 w-5" />
        {unreadCount > 0 && (
          <span className="absolute end-1 top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-medium text-primary-foreground">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setOpen(false)}
          />
          <div className="absolute end-0 top-full z-50 mt-2 w-96 rounded-lg border bg-card shadow-lg">
            {/* Header */}
            <div className="flex items-center justify-between border-b px-4 py-3">
              <h3 className="font-semibold text-foreground">
                {t('title')}
              </h3>
              {unreadCount > 0 && (
                <button
                  onClick={() => markAllAsRead.mutate()}
                  className="flex items-center gap-1 text-xs text-primary hover:text-primary/80"
                >
                  <CheckCheck className="h-3.5 w-3.5" />
                  {t('markAllRead')}
                </button>
              )}
            </div>

            {/* Notification list */}
            <div className="max-h-[400px] overflow-y-auto">
              {isLoading ? (
                <div className="flex items-center justify-center py-8">
                  <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-primary" />
                </div>
              ) : notifications.length === 0 ? (
                <div className="py-8 text-center">
                  <Bell className="h-8 w-8 text-muted-foreground/30 mx-auto mb-2" />
                  <p className="text-sm text-muted-foreground">{t('empty')}</p>
                </div>
              ) : (
                <div className="p-1">
                  {notifications.map((notification) => (
                    <NotificationItem
                      key={notification.id}
                      notification={notification}
                      onMarkRead={(id) => markAsRead.mutate(id)}
                      onClose={() => setOpen(false)}
                    />
                  ))}
                </div>
              )}
            </div>

            {/* Footer */}
            {notifications.length > 0 && (
              <div className="border-t px-4 py-2">
                <button
                  onClick={() => {
                    setOpen(false);
                    navigate('/notifications');
                  }}
                  className="w-full text-center text-xs text-primary hover:text-primary/80 py-1"
                >
                  {t('viewAll')}
                </button>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
