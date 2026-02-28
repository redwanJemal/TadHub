import { useNavigate } from 'react-router-dom';
import { Info, AlertTriangle, CheckCircle, XCircle } from 'lucide-react';
import { cn } from '@/shared/lib/cn';
import type { NotificationDto } from '../types';

const typeConfig = {
  info: { icon: Info, color: 'text-blue-500' },
  warning: { icon: AlertTriangle, color: 'text-amber-500' },
  success: { icon: CheckCircle, color: 'text-green-500' },
  error: { icon: XCircle, color: 'text-red-500' },
} as const;

function timeAgo(dateString: string): string {
  const now = Date.now();
  const date = new Date(dateString).getTime();
  const seconds = Math.floor((now - date) / 1000);

  if (seconds < 60) return 'just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(dateString).toLocaleDateString();
}

interface NotificationItemProps {
  notification: NotificationDto;
  onMarkRead: (id: string) => void;
  onClose?: () => void;
}

export function NotificationItem({ notification, onMarkRead, onClose }: NotificationItemProps) {
  const navigate = useNavigate();
  const config = typeConfig[notification.type] ?? typeConfig.info;
  const Icon = config.icon;

  const handleClick = () => {
    if (!notification.isRead) {
      onMarkRead(notification.id);
    }
    if (notification.link) {
      onClose?.();
      navigate(notification.link);
    }
  };

  return (
    <button
      onClick={handleClick}
      className={cn(
        'flex items-start gap-3 w-full rounded-lg p-3 text-start transition-colors hover:bg-muted/50',
        !notification.isRead && 'bg-primary/5'
      )}
    >
      <Icon className={cn('h-5 w-5 shrink-0 mt-0.5', config.color)} />
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <p className={cn('text-sm font-medium truncate', !notification.isRead && 'font-semibold')}>
            {notification.title}
          </p>
          {!notification.isRead && (
            <span className="h-2 w-2 rounded-full bg-primary shrink-0" />
          )}
        </div>
        <p className="text-xs text-muted-foreground line-clamp-2 mt-0.5">
          {notification.body}
        </p>
        <p className="text-xs text-muted-foreground mt-1">
          {timeAgo(notification.createdAt)}
        </p>
      </div>
    </button>
  );
}
