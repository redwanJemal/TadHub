import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, Search, Filter, MoreHorizontal, Eye, Edit, Trash2 } from 'lucide-react';
import { useWorkers, useDeleteWorker } from '../hooks/use-workers';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
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
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Avatar, AvatarFallback, AvatarImage } from '@/shared/components/ui/avatar';
import type { WorkerStatus, WorkerFilterParams } from '../types';
import { STATUS_COLORS } from '../types';
import { useCommonNationalities } from '@/features/reference-data/hooks/use-reference-data';

const STATUSES: WorkerStatus[] = [
  'Draft',
  'InTraining',
  'ReadyForMarket',
  'Reserved',
  'Hired',
  'OnLeave',
  'Terminated',
  'MedicallyUnfit',
  'Absconded',
  'Deported',
];

export function WorkersList() {
  const { t } = useTranslation('workers');
  const navigate = useNavigate();

  // Filter state
  const [filters, setFilters] = useState<WorkerFilterParams>({
    page: 1,
    pageSize: 20,
    include: ['jobCategory'],
  });
  const [searchTerm, setSearchTerm] = useState('');
  const [deleteId, setDeleteId] = useState<string | null>(null);

  // Data fetching
  const { data, isLoading, error } = useWorkers({
    ...filters,
    search: searchTerm || undefined,
  });
  const { data: nationalities } = useCommonNationalities();

  const deleteWorker = useDeleteWorker();

  // Handlers
  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setFilters((prev) => ({ ...prev, page: 1 }));
  };

  const handleStatusFilter = (status: string) => {
    setFilters((prev) => ({
      ...prev,
      status: status === 'all' ? undefined : [status as WorkerStatus],
      page: 1,
    }));
  };

  const handleNationalityFilter = (nationality: string) => {
    setFilters((prev) => ({
      ...prev,
      nationality: nationality === 'all' ? undefined : [nationality],
      page: 1,
    }));
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
  };

  const handleDelete = async () => {
    if (deleteId) {
      await deleteWorker.mutateAsync(deleteId);
      setDeleteId(null);
    }
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  if (error) {
    return (
      <div className="p-8 text-center">
        <p className="text-red-500">{t('error.loadingFailed')}</p>
        <Button variant="outline" className="mt-4" onClick={() => window.location.reload()}>
          {t('common:tryAgain')}
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">{t('title')}</h1>
          <p className="text-muted-foreground">{t('subtitle')}</p>
        </div>
        <Button onClick={() => navigate('/workers/new')}>
          <Plus className="mr-2 h-4 w-4" />
          {t('actions.create')}
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-4">
        <div className="relative flex-1 min-w-[200px] max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder={t('filters.search')}
            value={searchTerm}
            onChange={(e) => handleSearch(e.target.value)}
            className="pl-9"
          />
        </div>

        <Select
          value={filters.status?.[0] || 'all'}
          onValueChange={handleStatusFilter}
        >
          <SelectTrigger className="w-[180px]">
            <Filter className="mr-2 h-4 w-4" />
            <SelectValue placeholder={t('filters.status')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('common:allStatuses')}</SelectItem>
            {STATUSES.map((status) => (
              <SelectItem key={status} value={status}>
                {t(`status.${status}`)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          value={filters.nationality?.[0] || 'all'}
          onValueChange={handleNationalityFilter}
        >
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder={t('filters.nationality')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t('common:all')}</SelectItem>
            {nationalities?.map((country) => (
              <SelectItem key={country.id} value={country.nameEn}>
                {country.nameEn}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('table.worker')}</TableHead>
              <TableHead>{t('table.cvSerial')}</TableHead>
              <TableHead>{t('table.nationality')}</TableHead>
              <TableHead>{t('table.jobCategory')}</TableHead>
              <TableHead>{t('table.status')}</TableHead>
              <TableHead>{t('table.salary')}</TableHead>
              <TableHead className="w-[70px]"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              // Loading skeleton
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Skeleton className="h-10 w-10 rounded-full" />
                      <div className="space-y-1">
                        <Skeleton className="h-4 w-32" />
                        <Skeleton className="h-3 w-24" />
                      </div>
                    </div>
                  </TableCell>
                  <TableCell><Skeleton className="h-4 w-20" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-24" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-28" /></TableCell>
                  <TableCell><Skeleton className="h-6 w-24" /></TableCell>
                  <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                  <TableCell><Skeleton className="h-8 w-8" /></TableCell>
                </TableRow>
              ))
            ) : data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="h-32 text-center">
                  <p className="text-muted-foreground">{t('table.noResults')}</p>
                </TableCell>
              </TableRow>
            ) : (
              data?.items.map((worker) => (
                <TableRow
                  key={worker.id}
                  className="cursor-pointer hover:bg-muted/50"
                  onClick={() => navigate(`/workers/${worker.id}`)}
                >
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar>
                        <AvatarImage src={worker.photoUrl} alt={worker.fullNameEn} />
                        <AvatarFallback>{getInitials(worker.fullNameEn)}</AvatarFallback>
                      </Avatar>
                      <div>
                        <p className="font-medium">{worker.fullNameEn}</p>
                        <p className="text-sm text-muted-foreground" dir="rtl">
                          {worker.fullNameAr}
                        </p>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell className="font-mono">{worker.cvSerial}</TableCell>
                  <TableCell>{worker.nationality}</TableCell>
                  <TableCell>{worker.jobCategory?.name || '-'}</TableCell>
                  <TableCell>
                    <Badge className={STATUS_COLORS[worker.currentStatus]}>
                      {t(`status.${worker.currentStatus}`)}
                    </Badge>
                  </TableCell>
                  <TableCell>AED {worker.monthlyBaseSalary.toLocaleString()}</TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
                        <Button variant="ghost" size="icon">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onClick={() => navigate(`/workers/${worker.id}`)}>
                          <Eye className="mr-2 h-4 w-4" />
                          {t('actions.view')}
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => navigate(`/workers/${worker.id}/edit`)}>
                          <Edit className="mr-2 h-4 w-4" />
                          {t('actions.edit')}
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          className="text-red-600"
                          onClick={(e) => {
                            e.stopPropagation();
                            setDeleteId(worker.id);
                          }}
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          {t('actions.delete')}
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {t('table.showing', {
              from: (data.page - 1) * data.pageSize + 1,
              to: Math.min(data.page * data.pageSize, data.totalCount),
              total: data.totalCount,
            })}
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={!data.hasPreviousPage}
              onClick={() => handlePageChange(data.page - 1)}
            >
              {t('common:back')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={!data.hasNextPage}
              onClick={() => handlePageChange(data.page + 1)}
            >
              {t('common:next')}
            </Button>
          </div>
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{t('deleteDialog.title')}</AlertDialogTitle>
            <AlertDialogDescription>{t('deleteDialog.description')}</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>{t('common:cancel')}</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-red-600 hover:bg-red-700"
            >
              {t('common:delete')}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
