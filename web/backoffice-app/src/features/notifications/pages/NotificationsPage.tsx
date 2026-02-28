import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Bell, Send, Info, AlertTriangle, CheckCircle, XCircle } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAdminNotifications } from '../hooks';
import { useTenants } from '@/features/tenants/hooks';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Skeleton } from '@/shared/components/ui/skeleton';

const typeIcons: Record<string, typeof Info> = {
  info: Info,
  warning: AlertTriangle,
  success: CheckCircle,
  error: XCircle,
};

const typeBadgeVariants: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  info: 'default',
  warning: 'secondary',
  success: 'default',
  error: 'destructive',
};

const typeBadgeColors: Record<string, string> = {
  info: 'bg-blue-500',
  warning: 'bg-yellow-500 text-white',
  success: 'bg-green-500',
  error: '',
};

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function NotificationsPage() {
  const { t } = useTranslation('notifications');
  const [page, setPage] = useState(1);
  const [typeFilter, setTypeFilter] = useState<string>('');
  const [tenantFilter, setTenantFilter] = useState<string>('');

  const { data: tenants } = useTenants({ pageSize: 100 });

  const filter: Record<string, string> = {};
  if (typeFilter) filter['type'] = typeFilter;
  if (tenantFilter) filter['tenantId'] = tenantFilter;

  const { data, isLoading } = useAdminNotifications({
    page,
    pageSize: 20,
    sort: '-createdAt',
    ...(Object.keys(filter).length > 0 ? { filter } : {}),
  });

  const notifications = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = data?.totalPages ?? 1;

  // Build a map of tenant names for display
  const tenantMap = new Map(
    tenants?.items?.map((t) => [t.id, t.name]) ?? [],
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
            <Bell className="h-6 w-6" />
            {t('history.title')}
          </h1>
          <p className="text-muted-foreground">{t('history.description')}</p>
        </div>
        <Link to="/notifications/send">
          <Button>
            <Send className="h-4 w-4 me-2" />
            {t('sendNotification')}
          </Button>
        </Link>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3">
        <Select value={typeFilter} onValueChange={(v) => { setTypeFilter(v === 'all' ? '' : v); setPage(1); }}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder={t('history.filterByType')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('history.allTypes')}</SelectItem>
            <SelectItem value="info">{t('types.info')}</SelectItem>
            <SelectItem value="warning">{t('types.warning')}</SelectItem>
            <SelectItem value="success">{t('types.success')}</SelectItem>
            <SelectItem value="error">{t('types.error')}</SelectItem>
          </SelectContent>
        </Select>

        <Select value={tenantFilter} onValueChange={(v) => { setTenantFilter(v === 'all' ? '' : v); setPage(1); }}>
          <SelectTrigger className="w-[220px]">
            <SelectValue placeholder={t('history.filterByTenant')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('history.allTenants')}</SelectItem>
            {tenants?.items?.map((tenant) => (
              <SelectItem key={tenant.id} value={tenant.id}>
                {tenant.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <div className="ms-auto text-sm text-muted-foreground">
          {totalCount > 0 && t('pagination.showing', {
            from: (page - 1) * 20 + 1,
            to: Math.min(page * 20, totalCount),
            total: totalCount,
          })}
        </div>
      </div>

      {/* Table */}
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('fields.type')}</TableHead>
              <TableHead>{t('fields.title')}</TableHead>
              <TableHead>{t('fields.tenant')}</TableHead>
              <TableHead>{t('fields.status')}</TableHead>
              <TableHead>{t('fields.createdAt')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  <TableCell><Skeleton className="h-5 w-16" /></TableCell>
                  <TableCell><Skeleton className="h-5 w-48" /></TableCell>
                  <TableCell><Skeleton className="h-5 w-24" /></TableCell>
                  <TableCell><Skeleton className="h-5 w-16" /></TableCell>
                  <TableCell><Skeleton className="h-5 w-32" /></TableCell>
                </TableRow>
              ))
            ) : notifications.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} className="h-32 text-center">
                  <div className="flex flex-col items-center gap-2">
                    <Bell className="h-8 w-8 text-muted-foreground" />
                    <p className="font-medium">{t('history.noNotifications')}</p>
                    <p className="text-sm text-muted-foreground">
                      {t('history.noNotificationsDescription')}
                    </p>
                    <Link to="/notifications/send">
                      <Button variant="outline" size="sm" className="mt-2">
                        <Send className="h-4 w-4 me-2" />
                        {t('sendNotification')}
                      </Button>
                    </Link>
                  </div>
                </TableCell>
              </TableRow>
            ) : (
              notifications.map((notification) => {
                const TypeIcon = typeIcons[notification.type] ?? Info;
                return (
                  <TableRow key={notification.id}>
                    <TableCell>
                      <Badge
                        variant={typeBadgeVariants[notification.type] ?? 'outline'}
                        className={typeBadgeColors[notification.type] ?? ''}
                      >
                        <TypeIcon className="h-3 w-3 me-1" />
                        {t(`types.${notification.type}`)}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{notification.title}</p>
                        <p className="text-sm text-muted-foreground line-clamp-1">
                          {notification.body}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {tenantMap.get(notification.tenantId) ?? notification.tenantId.slice(0, 8)}
                    </TableCell>
                    <TableCell>
                      {notification.isRead ? (
                        <Badge variant="outline">{t('history.read')}</Badge>
                      ) : (
                        <Badge variant="secondary">{t('history.unread')}</Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {formatDate(notification.createdAt)}
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
          >
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">
            {page} / {totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page >= totalPages}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
