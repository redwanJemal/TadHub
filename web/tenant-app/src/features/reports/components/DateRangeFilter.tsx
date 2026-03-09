import { useTranslation } from 'react-i18next';
import { Button } from '@/shared/components/ui/button';

interface DateRangeFilterProps {
  from: string;
  to: string;
  onFromChange: (val: string) => void;
  onToChange: (val: string) => void;
  onApply: () => void;
}

export function DateRangeFilter({ from, to, onFromChange, onToChange, onApply }: DateRangeFilterProps) {
  const { t } = useTranslation('reports');

  return (
    <div className="flex items-end gap-3">
      <div className="space-y-1">
        <label className="text-sm font-medium text-muted-foreground">{t('reports.from')}</label>
        <input
          type="date"
          value={from}
          onChange={(e) => onFromChange(e.target.value)}
          className="flex h-9 rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
        />
      </div>
      <div className="space-y-1">
        <label className="text-sm font-medium text-muted-foreground">{t('reports.to')}</label>
        <input
          type="date"
          value={to}
          onChange={(e) => onToChange(e.target.value)}
          className="flex h-9 rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
        />
      </div>
      <Button size="sm" onClick={onApply}>
        {t('reports.apply')}
      </Button>
    </div>
  );
}
