import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { RefreshCw, TrendingUp, TrendingDown, DollarSign, Percent } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Input } from '@/shared/components/ui/input';
import { Label } from '@/shared/components/ui/label';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { useMarginReport, useRevenueBreakdown } from '../hooks';

function StatCard({
  label,
  value,
  subValue,
  icon: Icon,
  iconClass,
}: {
  label: string;
  value: string;
  subValue?: string;
  icon: React.ComponentType<{ className?: string }>;
  iconClass?: string;
}) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle>
        <Icon className={`h-4 w-4 ${iconClass ?? 'text-muted-foreground'}`} />
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-bold tabular-nums">{value}</p>
        {subValue && <p className="text-xs text-muted-foreground mt-1">{subValue}</p>}
      </CardContent>
    </Card>
  );
}

function MarginSection() {
  const { data: margin, isLoading, refetch } = useMarginReport();

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i}>
              <CardHeader className="pb-2"><Skeleton className="h-4 w-24" /></CardHeader>
              <CardContent><Skeleton className="h-8 w-28" /></CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Margin Report</h2>
        <Button variant="outline" size="sm" onClick={() => refetch()}>
          <RefreshCw className="me-2 h-4 w-4" />
          Refresh
        </Button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label="Total Revenue"
          value={`${(margin?.totalRevenue ?? 0).toLocaleString()} AED`}
          icon={TrendingUp}
          iconClass="text-blue-500"
        />
        <StatCard
          label="Total Cost"
          value={`${(margin?.totalCost ?? 0).toLocaleString()} AED`}
          icon={TrendingDown}
          iconClass="text-red-500"
        />
        <StatCard
          label="Gross Margin"
          value={`${(margin?.grossMargin ?? 0).toLocaleString()} AED`}
          icon={DollarSign}
          iconClass="text-green-500"
        />
        <StatCard
          label="Margin %"
          value={`${(margin?.marginPercentage ?? 0).toFixed(1)}%`}
          icon={Percent}
          iconClass="text-purple-500"
        />
      </div>

      {margin?.lines && margin.lines.length > 0 && (
        <Card>
          <CardHeader><CardTitle>Margin by Contract</CardTitle></CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Contract</TableHead>
                  <TableHead>Worker</TableHead>
                  <TableHead>Client</TableHead>
                  <TableHead className="text-right">Revenue</TableHead>
                  <TableHead className="text-right">Cost</TableHead>
                  <TableHead className="text-right">Margin</TableHead>
                  <TableHead className="text-right">Margin %</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {margin.lines.map((line, idx) => (
                  <TableRow key={idx}>
                    <TableCell className="text-sm">{line.contract?.contractCode ?? line.contractId ?? '—'}</TableCell>
                    <TableCell className="text-sm">{line.worker?.fullNameEn ?? line.workerId ?? '—'}</TableCell>
                    <TableCell className="text-sm">{line.client?.nameEn ?? line.clientId ?? '—'}</TableCell>
                    <TableCell className="text-right tabular-nums">
                      {line.revenue.toLocaleString()} AED
                    </TableCell>
                    <TableCell className="text-right tabular-nums text-red-600 dark:text-red-400">
                      {line.cost.toLocaleString()} AED
                    </TableCell>
                    <TableCell className={`text-right tabular-nums font-medium ${line.margin >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}>
                      {line.margin.toLocaleString()} AED
                    </TableCell>
                    <TableCell className={`text-right tabular-nums ${line.marginPercentage >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}>
                      {line.marginPercentage.toFixed(1)}%
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function RevenueBreakdownSection() {
  const [fromDate, setFromDate] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() - 3);
    return d.toISOString().slice(0, 10);
  });
  const [toDate, setToDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [queryFrom, setQueryFrom] = useState(fromDate);
  const [queryTo, setQueryTo] = useState(toDate);

  const { data: breakdown, isLoading, refetch } = useRevenueBreakdown(queryFrom, queryTo);

  const handleApply = () => {
    setQueryFrom(fromDate);
    setQueryTo(toDate);
  };

  const byPeriodEntries = breakdown?.byPeriod ? Object.entries(breakdown.byPeriod).sort(([a], [b]) => a.localeCompare(b)) : [];
  const byMethodEntries = breakdown?.byPaymentMethod ? Object.entries(breakdown.byPaymentMethod) : [];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Revenue Breakdown</h2>
      </div>

      {/* Date Range Filter */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-4">
            <div className="space-y-2">
              <Label>From</Label>
              <Input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>To</Label>
              <Input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
            </div>
            <Button onClick={handleApply}>Apply</Button>
            <Button variant="outline" size="icon" onClick={() => refetch()}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        </CardContent>
      </Card>

      {isLoading ? (
        <div className="grid gap-6 md:grid-cols-2">
          <Card>
            <CardHeader><Skeleton className="h-6 w-32" /></CardHeader>
            <CardContent className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-8 w-full" />
              ))}
            </CardContent>
          </Card>
          <Card>
            <CardHeader><Skeleton className="h-6 w-32" /></CardHeader>
            <CardContent className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-8 w-full" />
              ))}
            </CardContent>
          </Card>
        </div>
      ) : breakdown ? (
        <>
          <div className="mb-2">
            <p className="text-sm text-muted-foreground">
              Total Revenue:{' '}
              <span className="font-bold text-foreground tabular-nums">
                {(breakdown.totalRevenue ?? 0).toLocaleString()} AED
              </span>
            </p>
          </div>

          <div className="grid gap-6 md:grid-cols-2">
            {/* By Period */}
            {byPeriodEntries.length > 0 && (
              <Card>
                <CardHeader><CardTitle>Revenue by Period</CardTitle></CardHeader>
                <CardContent className="p-0">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Period</TableHead>
                        <TableHead className="text-right">Revenue (AED)</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {byPeriodEntries.map(([period, amount]) => (
                        <TableRow key={period}>
                          <TableCell className="font-mono">{period}</TableCell>
                          <TableCell className="text-right tabular-nums font-medium">
                            {(amount as number).toLocaleString()}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </CardContent>
              </Card>
            )}

            {/* By Payment Method */}
            {byMethodEntries.length > 0 && (
              <Card>
                <CardHeader><CardTitle>Revenue by Payment Method</CardTitle></CardHeader>
                <CardContent className="p-0">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Method</TableHead>
                        <TableHead className="text-right">Amount (AED)</TableHead>
                        <TableHead className="text-right">Share</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {byMethodEntries.map(([method, amount]) => {
                        const total = breakdown.totalRevenue || 1;
                        const pct = (((amount as number) / total) * 100).toFixed(1);
                        return (
                          <TableRow key={method}>
                            <TableCell>
                              {method === 'BankTransfer' ? 'Bank Transfer' : method === 'EDirham' ? 'E-Dirham' : method}
                            </TableCell>
                            <TableCell className="text-right tabular-nums font-medium">
                              {(amount as number).toLocaleString()}
                            </TableCell>
                            <TableCell className="text-right tabular-nums text-muted-foreground">
                              {pct}%
                            </TableCell>
                          </TableRow>
                        );
                      })}
                    </TableBody>
                  </Table>
                </CardContent>
              </Card>
            )}
          </div>
        </>
      ) : (
        <Card>
          <CardContent className="py-12 text-center text-muted-foreground">
            Select a date range and click Apply to view revenue breakdown.
          </CardContent>
        </Card>
      )}
    </div>
  );
}

export function FinancialReportsPage() {
  useTranslation('finance');

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Financial Reports</h1>
        <p className="text-muted-foreground">Revenue, cost, and margin analytics</p>
      </div>

      <MarginSection />
      <RevenueBreakdownSection />
    </div>
  );
}
