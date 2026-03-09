import { useTranslation } from 'react-i18next';
import { Download } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';

interface ExportCsvButtonProps<T> {
  data: T[];
  filename: string;
  columns: { key: string; label: string }[];
}

export function ExportCsvButton<T extends object>({ data, filename, columns }: ExportCsvButtonProps<T>) {
  const { t } = useTranslation('reports');

  const handleExport = () => {
    if (data.length === 0) return;

    const header = columns.map((c) => `"${c.label}"`).join(',');
    const rows = data.map((row) =>
      columns
        .map((c) => {
          const val = (row as Record<string, unknown>)[c.key];
          if (val == null) return '""';
          const str = String(val).replace(/"/g, '""');
          return `"${str}"`;
        })
        .join(',')
    );

    const csv = [header, ...rows].join('\n');
    const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${filename}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  };

  return (
    <Button variant="outline" size="sm" onClick={handleExport} disabled={data.length === 0}>
      <Download className="h-4 w-4 me-2" />
      {t('reports.exportCsv')}
    </Button>
  );
}
