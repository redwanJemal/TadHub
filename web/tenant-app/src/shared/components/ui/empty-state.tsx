import { cn } from '@/shared/lib/cn';
import { useTranslation } from 'react-i18next';
import { 
  Inbox, 
  Users, 
  Shield, 
  Settings,
  FolderOpen,
  type LucideIcon 
} from 'lucide-react';

interface EmptyStateProps {
  icon?: LucideIcon;
  title?: string;
  description?: string;
  action?: React.ReactNode;
  variant?: 'default' | 'roles' | 'team' | 'files' | 'settings';
  className?: string;
}

const variantConfig: Record<string, { icon: LucideIcon; titleKey: string; descKey: string }> = {
  default: {
    icon: Inbox,
    titleKey: 'common:empty.title',
    descKey: 'common:empty.description',
  },
  roles: {
    icon: Shield,
    titleKey: 'common:empty.roles.title',
    descKey: 'common:empty.roles.description',
  },
  team: {
    icon: Users,
    titleKey: 'common:empty.team.title',
    descKey: 'common:empty.team.description',
  },
  files: {
    icon: FolderOpen,
    titleKey: 'common:empty.title',
    descKey: 'common:empty.description',
  },
  settings: {
    icon: Settings,
    titleKey: 'common:empty.title',
    descKey: 'common:empty.description',
  },
};

export function EmptyState({
  icon,
  title,
  description,
  action,
  variant = 'default',
  className,
}: EmptyStateProps) {
  const { t } = useTranslation();
  const config = variantConfig[variant];
  const Icon = icon || config.icon;

  return (
    <div
      className={cn(
        'flex flex-col items-center justify-center py-12 px-4 text-center',
        className
      )}
    >
      <div className="flex h-20 w-20 items-center justify-center rounded-full bg-muted mb-4">
        <Icon className="h-10 w-10 text-muted-foreground" />
      </div>
      <h3 className="text-lg font-semibold text-foreground mb-2">
        {title || t(config.titleKey)}
      </h3>
      <p className="text-sm text-muted-foreground max-w-sm mb-6">
        {description || t(config.descKey)}
      </p>
      {action && <div>{action}</div>}
    </div>
  );
}
