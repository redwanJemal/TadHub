import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  ArrowLeft,
  Mail,
  Phone,
  Globe,
  MapPin,
  UserSearch,
  HardHat,
  Plane,
  DollarSign,
} from 'lucide-react';
import { Badge } from '@/shared/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
import { DataTableAdvanced, type Column } from '@/shared/components/data-table/DataTableAdvanced';
import { useCountryRefs, getFlagEmoji } from '@/features/reference-data';
import { useSupplier } from '../hooks';
import { useCandidates } from '@/features/candidates/hooks';
import { useWorkers } from '@/features/workers/hooks';
import { useArrivals } from '@/features/arrivals/hooks';
import { useSupplierPayments } from '@/features/finance/hooks';
import type { CandidateListDto } from '@/features/candidates/types';
import type { SupplierPaymentListDto } from '@/features/finance/types';

const statusVariant: Record<string, 'default' | 'secondary' | 'destructive'> = {
  Active: 'default',
  Suspended: 'secondary',
  Terminated: 'destructive',
};

export function SupplierDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation('suppliers');
  const { data: tenantSupplier, isLoading } = useSupplier(id!);
  const { data: countries } = useCountryRefs();

  const supplier = tenantSupplier?.supplier;
  const supplierId = tenantSupplier?.supplierId;

  const getCountryName = (code?: string) => {
    if (!code) return '—';
    const country = countries?.find((c) => c.code === code);
    return country?.nameEn ?? code;
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 w-full" />
      </div>
    );
  }

  if (!tenantSupplier || !supplier) {
    return (
      <div className="space-y-4">
        <Link to="/suppliers" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" />
          {t('backToList', 'Back to Suppliers')}
        </Link>
        <p className="text-muted-foreground">{t('notFound', 'Supplier not found')}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <Link to="/suppliers" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-4">
          <ArrowLeft className="h-4 w-4" />
          {t('backToList', 'Back to Suppliers')}
        </Link>
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold tracking-tight">{supplier.nameEn}</h1>
          <Badge variant={statusVariant[tenantSupplier.status] ?? 'secondary'}>
            {t(`status.${tenantSupplier.status}`)}
          </Badge>
        </div>
        {supplier.nameAr && (
          <p className="text-muted-foreground" dir="rtl">{supplier.nameAr}</p>
        )}
      </div>

      <Tabs defaultValue="overview">
        <TabsList>
          <TabsTrigger value="overview">{t('tabs.overview', 'Overview')}</TabsTrigger>
          <TabsTrigger value="candidates">{t('tabs.candidates', 'Candidates')}</TabsTrigger>
          <TabsTrigger value="workers">{t('tabs.workers', 'Workers')}</TabsTrigger>
          <TabsTrigger value="arrivals">{t('tabs.arrivals', 'Arrivals')}</TabsTrigger>
          <TabsTrigger value="commissions">{t('tabs.commissions', 'Commissions')}</TabsTrigger>
        </TabsList>

        <TabsContent value="overview">
          <OverviewTab
            supplier={supplier}
            tenantSupplier={tenantSupplier}
            getCountryName={getCountryName}
            t={t}
          />
        </TabsContent>

        <TabsContent value="candidates">
          <CandidatesTab tenantSupplierId={id!} />
        </TabsContent>

        <TabsContent value="workers">
          <WorkersTab tenantSupplierId={id!} />
        </TabsContent>

        <TabsContent value="arrivals">
          <ArrivalsTab supplierId={supplierId!} />
        </TabsContent>

        <TabsContent value="commissions">
          <CommissionsTab supplierId={supplierId!} />
        </TabsContent>
      </Tabs>
    </div>
  );
}

function OverviewTab({
  supplier,
  tenantSupplier,
  getCountryName,
  t,
}: {
  supplier: NonNullable<ReturnType<typeof useSupplier>['data']>['supplier'];
  tenantSupplier: NonNullable<ReturnType<typeof useSupplier>['data']>;
  getCountryName: (code?: string) => string;
  t: ReturnType<typeof useTranslation>['t'];
}) {
  if (!supplier) return null;

  return (
    <div className="grid gap-6 md:grid-cols-2 mt-4">
      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t('detail.contactInfo', 'Contact Information')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3 text-sm">
          {supplier.email && (
            <div className="flex items-center gap-2">
              <Mail className="h-4 w-4 text-muted-foreground" />
              <span>{supplier.email}</span>
            </div>
          )}
          {supplier.phone && (
            <div className="flex items-center gap-2">
              <Phone className="h-4 w-4 text-muted-foreground" />
              <span>{supplier.phone}</span>
            </div>
          )}
          {supplier.website && (
            <div className="flex items-center gap-2">
              <Globe className="h-4 w-4 text-muted-foreground" />
              <span>{supplier.website}</span>
            </div>
          )}
          <div className="flex items-center gap-2">
            <MapPin className="h-4 w-4 text-muted-foreground" />
            <span>
              {supplier.country && (
                <span className="me-1">{getFlagEmoji(supplier.country)}</span>
              )}
              {getCountryName(supplier.country)}
              {supplier.city && ` / ${supplier.city}`}
            </span>
          </div>
          {supplier.licenseNumber && (
            <div>
              <span className="text-muted-foreground">{t('detail.license', 'License')}: </span>
              <span className="font-mono">{supplier.licenseNumber}</span>
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-base">{t('detail.agreement', 'Agreement Details')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3 text-sm">
          {tenantSupplier.contractReference && (
            <div>
              <span className="text-muted-foreground">{t('detail.contractRef', 'Contract Ref')}: </span>
              <span>{tenantSupplier.contractReference}</span>
            </div>
          )}
          {tenantSupplier.agreementStartDate && (
            <div>
              <span className="text-muted-foreground">{t('detail.startDate', 'Start Date')}: </span>
              <span>{new Date(tenantSupplier.agreementStartDate).toLocaleDateString()}</span>
            </div>
          )}
          {tenantSupplier.agreementEndDate && (
            <div>
              <span className="text-muted-foreground">{t('detail.endDate', 'End Date')}: </span>
              <span>{new Date(tenantSupplier.agreementEndDate).toLocaleDateString()}</span>
            </div>
          )}
          {tenantSupplier.notes && (
            <div>
              <span className="text-muted-foreground">{t('detail.notes', 'Notes')}: </span>
              <span>{tenantSupplier.notes}</span>
            </div>
          )}
          <div>
            <span className="text-muted-foreground">{t('detail.linkedAt', 'Linked')}: </span>
            <span>{new Date(tenantSupplier.createdAt).toLocaleDateString()}</span>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

function CandidatesTab({ tenantSupplierId }: { tenantSupplierId: string }) {
  const { t } = useTranslation('suppliers');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data, isLoading } = useCandidates({
    page,
    pageSize,
    search: search || undefined,
    'filter[tenantSupplierId]': tenantSupplierId,
  });

  const columns: Column<CandidateListDto>[] = [
    {
      key: 'fullNameEn',
      header: t('candidateCols.name', 'Name'),
      cell: (row) => (
        <Link to={`/candidates/${row.id}`} className="font-medium hover:underline">
          {row.fullNameEn}
        </Link>
      ),
    },
    {
      key: 'nationality',
      header: t('candidateCols.nationality', 'Nationality'),
    },
    {
      key: 'status',
      header: t('candidateCols.status', 'Status'),
      cell: (row) => <Badge variant="secondary">{row.status}</Badge>,
    },
    {
      key: 'createdAt',
      header: t('candidateCols.registered', 'Registered'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ];

  return (
    <div className="mt-4">
      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('candidateSearch', 'Search candidates...')}
        searchValue={search}
        onSearchChange={(val) => { setSearch(val); setPage(1); }}
        page={page}
        totalPages={data?.totalPages}
        total={data?.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        emptyIcon={UserSearch}
        emptyTitle={t('candidatesEmpty', 'No candidates')}
        emptyDescription={t('candidatesEmptyDesc', 'No candidates sourced by this supplier yet.')}
      />
    </div>
  );
}

function WorkersTab({ tenantSupplierId }: { tenantSupplierId: string }) {
  const { t } = useTranslation('suppliers');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data, isLoading } = useWorkers({
    page,
    pageSize,
    search: search || undefined,
    'filter[tenantSupplierId]': tenantSupplierId,
  });

  const columns: Column<{ id: string; fullNameEn: string; workerCode: string; status: string; nationality?: string; createdAt: string }>[] = [
    {
      key: 'workerCode',
      header: t('workerCols.code', 'Code'),
      cell: (row) => <span className="font-mono text-sm">{row.workerCode}</span>,
    },
    {
      key: 'fullNameEn',
      header: t('workerCols.name', 'Name'),
      cell: (row) => (
        <Link to={`/workers/${row.id}`} className="font-medium hover:underline">
          {row.fullNameEn}
        </Link>
      ),
    },
    {
      key: 'nationality',
      header: t('workerCols.nationality', 'Nationality'),
    },
    {
      key: 'status',
      header: t('workerCols.status', 'Status'),
      cell: (row) => <Badge variant="secondary">{row.status}</Badge>,
    },
    {
      key: 'createdAt',
      header: t('workerCols.created', 'Created'),
      cell: (row) => new Date(row.createdAt).toLocaleDateString(),
    },
  ];

  return (
    <div className="mt-4">
      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('workerSearch', 'Search workers...')}
        searchValue={search}
        onSearchChange={(val) => { setSearch(val); setPage(1); }}
        page={page}
        totalPages={data?.totalPages}
        total={data?.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        emptyIcon={HardHat}
        emptyTitle={t('workersEmpty', 'No workers')}
        emptyDescription={t('workersEmptyDesc', 'No workers from this supplier yet.')}
      />
    </div>
  );
}

function ArrivalsTab({ supplierId }: { supplierId: string }) {
  const { t } = useTranslation('suppliers');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data, isLoading } = useArrivals({
    page,
    pageSize,
    search: search || undefined,
    'filter[supplierId]': supplierId,
  });

  const columns: Column<{ id: string; workerNameEn?: string; flightNumber?: string; scheduledArrivalDate?: string; status?: string; airportCode?: string; createdAt: string }>[] = [
    {
      key: 'workerNameEn',
      header: t('arrivalCols.worker', 'Worker'),
    },
    {
      key: 'flightNumber',
      header: t('arrivalCols.flight', 'Flight'),
      cell: (row) => <span className="font-mono text-sm">{row.flightNumber || '—'}</span>,
    },
    {
      key: 'scheduledArrivalDate',
      header: t('arrivalCols.date', 'Arrival Date'),
      cell: (row) => row.scheduledArrivalDate ? new Date(row.scheduledArrivalDate).toLocaleDateString() : '—',
    },
    {
      key: 'airportCode',
      header: t('arrivalCols.airport', 'Airport'),
    },
    {
      key: 'status',
      header: t('arrivalCols.status', 'Status'),
      cell: (row) => row.status ? <Badge variant="secondary">{row.status}</Badge> : <span>—</span>,
    },
  ];

  return (
    <div className="mt-4">
      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('arrivalSearch', 'Search arrivals...')}
        searchValue={search}
        onSearchChange={(val) => { setSearch(val); setPage(1); }}
        page={page}
        totalPages={data?.totalPages}
        total={data?.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        emptyIcon={Plane}
        emptyTitle={t('arrivalsEmpty', 'No arrivals')}
        emptyDescription={t('arrivalsEmptyDesc', 'No arrivals for this supplier yet.')}
      />
    </div>
  );
}

function CommissionsTab({ supplierId }: { supplierId: string }) {
  const { t } = useTranslation('suppliers');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data, isLoading } = useSupplierPayments({
    page,
    pageSize,
    search: search || undefined,
    'filter[supplierId]': supplierId,
  });

  const columns: Column<SupplierPaymentListDto>[] = [
    {
      key: 'paymentNumber',
      header: t('commissionCols.reference', 'Reference'),
      cell: (row) => <span className="font-mono text-sm">{row.paymentNumber}</span>,
    },
    {
      key: 'paymentType',
      header: t('commissionCols.type', 'Type'),
    },
    {
      key: 'amount',
      header: t('commissionCols.amount', 'Amount'),
      cell: (row) => (
        <span className="font-mono">
          {row.currency} {row.amount.toLocaleString()}
        </span>
      ),
    },
    {
      key: 'status',
      header: t('commissionCols.status', 'Status'),
      cell: (row) => <Badge variant="secondary">{row.status}</Badge>,
    },
    {
      key: 'paymentDate',
      header: t('commissionCols.date', 'Payment Date'),
      cell: (row) => row.paymentDate ? new Date(row.paymentDate).toLocaleDateString() : '—',
    },
  ];

  return (
    <div className="mt-4">
      <DataTableAdvanced
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t('commissionSearch', 'Search commissions...')}
        searchValue={search}
        onSearchChange={(val) => { setSearch(val); setPage(1); }}
        page={page}
        totalPages={data?.totalPages}
        total={data?.totalCount}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        emptyIcon={DollarSign}
        emptyTitle={t('commissionsEmpty', 'No commissions')}
        emptyDescription={t('commissionsEmptyDesc', 'No commission payments to this supplier yet.')}
      />
    </div>
  );
}
