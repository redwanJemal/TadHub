import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import {
  Package, Users, RotateCcw, AlertTriangle,
  Plane, Building2, GitBranch,
  DollarSign, RefreshCcw, Calculator,
} from 'lucide-react';
import { Card, CardHeader, CardTitle, CardDescription } from '@/shared/components/ui/card';
import type { LucideIcon } from 'lucide-react';

interface ReportCard {
  titleKey: string;
  descriptionKey: string;
  path: string;
  icon: LucideIcon;
  category: string;
}

const REPORT_CARDS: ReportCard[] = [
  { titleKey: 'reports.inventory.title', descriptionKey: 'reports.inventory.description', path: '/reports/inventory', icon: Package, category: 'workforce' },
  { titleKey: 'reports.deployed.title', descriptionKey: 'reports.deployed.description', path: '/reports/deployed', icon: Users, category: 'workforce' },
  { titleKey: 'reports.returnees.title', descriptionKey: 'reports.returnees.description', path: '/reports/returnees', icon: RotateCcw, category: 'workforce' },
  { titleKey: 'reports.runaways.title', descriptionKey: 'reports.runaways.description', path: '/reports/runaways', icon: AlertTriangle, category: 'workforce' },
  { titleKey: 'reports.arrivals.title', descriptionKey: 'reports.arrivals.description', path: '/reports/arrivals', icon: Plane, category: 'operational' },
  { titleKey: 'reports.accommodationDaily.title', descriptionKey: 'reports.accommodationDaily.description', path: '/reports/accommodation-daily', icon: Building2, category: 'operational' },
  { titleKey: 'reports.deploymentPipeline.title', descriptionKey: 'reports.deploymentPipeline.description', path: '/reports/deployment-pipeline', icon: GitBranch, category: 'operational' },
  { titleKey: 'reports.supplierCommissions.title', descriptionKey: 'reports.supplierCommissions.description', path: '/reports/supplier-commissions', icon: DollarSign, category: 'finance' },
  { titleKey: 'reports.refunds.title', descriptionKey: 'reports.refunds.description', path: '/reports/refunds', icon: RefreshCcw, category: 'finance' },
  { titleKey: 'reports.costPerMaid.title', descriptionKey: 'reports.costPerMaid.description', path: '/reports/cost-per-maid', icon: Calculator, category: 'finance' },
];

export function ReportsHubPage() {
  const { t } = useTranslation('reports');
  const navigate = useNavigate();

  const categories = [
    { key: 'workforce', label: t('reports.category.workforce') },
    { key: 'operational', label: t('reports.category.operational') },
    { key: 'finance', label: t('reports.category.finance') },
  ];

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('reports.title')}</h1>
        <p className="text-muted-foreground">{t('reports.description')}</p>
      </div>

      {categories.map((cat) => (
        <div key={cat.key} className="space-y-4">
          <h2 className="text-lg font-semibold">{cat.label}</h2>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {REPORT_CARDS.filter((r) => r.category === cat.key).map((report) => {
              const Icon = report.icon;
              return (
                <Card
                  key={report.path}
                  className="cursor-pointer transition-colors hover:bg-muted/50"
                  onClick={() => navigate(report.path)}
                >
                  <CardHeader className="flex flex-row items-center gap-4">
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                      <Icon className="h-5 w-5" />
                    </div>
                    <div className="min-w-0">
                      <CardTitle className="text-base">{t(report.titleKey)}</CardTitle>
                      <CardDescription className="text-xs">{t(report.descriptionKey)}</CardDescription>
                    </div>
                  </CardHeader>
                </Card>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}
