import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ChevronLeft, Printer } from 'lucide-react';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table';
import { Button } from '@/shared/components/ui/button';
import { useAccommodationDaily } from '../hooks';
import { ExportCsvButton } from '../components/ExportCsvButton';
import type { AccommodationDailyItem } from '../types';

export function AccommodationDailyPage() {
  const { t } = useTranslation('reports');
  const today = new Date().toISOString().split('T')[0];
  const [selectedDate, setSelectedDate] = useState(today);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);
  const [search, setSearch] = useState('');

  const params: Record<string, unknown> = { page, pageSize, search: search || undefined };
  const { data, isLoading } = useAccommodationDaily(selectedDate, params);

  const columns: Column<AccommodationDailyItem>[] = [
    { key: 'stayCode', header: t('reports.accommodationDaily.stayCode') },
    { key: 'workerNameEn', header: t('reports.accommodationDaily.worker'), cell: (row) => row.workerNameEn ?? '-' },
    { key: 'room', header: t('reports.accommodationDaily.room'), cell: (row) => row.room ?? '-' },
    { key: 'locationName', header: t('reports.accommodationDaily.location'), cell: (row) => row.locationName ?? '-' },
    { key: 'checkInDate', header: t('reports.accommodationDaily.checkInDate') },
    { key: 'checkOutDate', header: t('reports.accommodationDaily.checkOutDate'), cell: (row) => row.checkOutDate ?? '-' },
    { key: 'status', header: t('reports.accommodationDaily.status') },
  ];

  const csvColumns = [
    { key: 'stayCode', label: 'Stay Code' },
    { key: 'workerNameEn', label: 'Worker' },
    { key: 'room', label: 'Room' },
    { key: 'locationName', label: 'Location' },
    { key: 'checkInDate', label: 'Check-in Date' },
    { key: 'checkOutDate', label: 'Check-out Date' },
    { key: 'status', label: 'Status' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link to="/reports" className="text-muted-foreground hover:text-foreground">
            <ChevronLeft className="h-5 w-5" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">{t('reports.accommodationDaily.title')}</h1>
            <p className="text-muted-foreground text-sm">{t('reports.accommodationDaily.description')}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={() => window.print()}>
            <Printer className="h-4 w-4 me-2" />
            {t('reports.print')}
          </Button>
          <ExportCsvButton data={data?.items ?? []} filename={`accommodation-daily-${selectedDate}`} columns={csvColumns} />
        </div>
      </div>

      <div className="flex items-end gap-3">
        <div className="space-y-1">
          <label className="text-sm font-medium text-muted-foreground">{t('reports.accommodationDaily.date')}</label>
          <input
            type="date"
            value={selectedDate}
            onChange={(e) => { setSelectedDate(e.target.value); setPage(1); }}
            className="flex h-9 rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
          />
        </div>
      </div>

      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('reports.accommodationDaily.worker')}
        searchValue={search}
        onSearchChange={setSearch}
        page={page}
        totalPages={data?.totalPages ?? 0}
        total={data?.totalCount ?? 0}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
      />
    </div>
  );
}
