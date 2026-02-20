import { useNavigate } from 'react-router-dom';
import { ShieldX, ArrowLeft, Home } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface ForbiddenPageProps {
  message?: string;
  requiredPermission?: string;
}

export function ForbiddenPage({ message, requiredPermission }: ForbiddenPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
      <div className="mb-6">
        <div className="mx-auto w-20 h-20 rounded-full bg-destructive/10 flex items-center justify-center">
          <ShieldX className="h-10 w-10 text-destructive" />
        </div>
      </div>
      
      <h1 className="text-3xl font-bold mb-2">{t('forbidden.title', 'Access Denied')}</h1>
      
      <p className="text-muted-foreground mb-2 max-w-md">
        {message || t('forbidden.message', 'You do not have permission to access this page.')}
      </p>
      
      {requiredPermission && (
        <p className="text-sm text-muted-foreground mb-6">
          {t('forbidden.requiredPermission', 'Required permission')}: <code className="bg-muted px-2 py-0.5 rounded">{requiredPermission}</code>
        </p>
      )}

      <div className="flex gap-3 mt-4">
        <button
          onClick={() => navigate(-1)}
          className="inline-flex items-center gap-2 px-4 py-2 rounded-lg border bg-background hover:bg-muted transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('forbidden.goBack', 'Go Back')}
        </button>
        
        <button
          onClick={() => navigate('/')}
          className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-primary text-primary-foreground hover:bg-primary/90 transition-colors"
        >
          <Home className="h-4 w-4" />
          {t('forbidden.goHome', 'Go to Dashboard')}
        </button>
      </div>
    </div>
  );
}
