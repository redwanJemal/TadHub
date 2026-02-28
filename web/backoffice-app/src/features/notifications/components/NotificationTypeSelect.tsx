import { Info, AlertTriangle, CheckCircle, XCircle } from 'lucide-react';
import { cn } from '@/shared/lib/cn';
import { useTranslation } from 'react-i18next';

const types = [
  { value: 'info', icon: Info, colorClass: 'text-blue-600 border-blue-200 bg-blue-50' },
  { value: 'warning', icon: AlertTriangle, colorClass: 'text-yellow-600 border-yellow-200 bg-yellow-50' },
  { value: 'success', icon: CheckCircle, colorClass: 'text-green-600 border-green-200 bg-green-50' },
  { value: 'error', icon: XCircle, colorClass: 'text-red-600 border-red-200 bg-red-50' },
] as const;

interface NotificationTypeSelectProps {
  value: string;
  onChange: (value: string) => void;
}

export function NotificationTypeSelect({ value, onChange }: NotificationTypeSelectProps) {
  const { t } = useTranslation('notifications');

  return (
    <div className="flex gap-2">
      {types.map((type) => {
        const Icon = type.icon;
        const isSelected = value === type.value;
        return (
          <button
            key={type.value}
            type="button"
            onClick={() => onChange(type.value)}
            className={cn(
              'flex items-center gap-2 rounded-lg border px-4 py-2.5 text-sm font-medium transition-all',
              isSelected
                ? cn(type.colorClass, 'ring-2 ring-offset-1', `ring-current`)
                : 'border-border bg-background text-muted-foreground hover:bg-muted',
            )}
          >
            <Icon className="h-4 w-4" />
            {t(`types.${type.value}`)}
          </button>
        );
      })}
    </div>
  );
}
