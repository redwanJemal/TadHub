import { useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Upload, X, FileText } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';

interface FileUploadProps {
  accept: string;
  maxSizeMB: number;
  currentUrl?: string;
  onUpload: (file: File) => Promise<void>;
  isPending?: boolean;
  type: 'photo' | 'video' | 'document';
  /** In deferred mode, calls onFileSelected instead of onUpload. Used on Create page. */
  mode?: 'immediate' | 'deferred';
  onFileSelected?: (file: File) => void;
}

export function FileUpload({
  accept,
  maxSizeMB,
  currentUrl,
  onUpload,
  isPending,
  type,
  mode = 'immediate',
  onFileSelected,
}: FileUploadProps) {
  const { t } = useTranslation('candidates');
  const inputRef = useRef<HTMLInputElement>(null);
  const [error, setError] = useState('');
  const [preview, setPreview] = useState<string | null>(null);
  const [selectedFileName, setSelectedFileName] = useState<string | null>(null);

  const handleChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setError('');

    if (file.size > maxSizeMB * 1024 * 1024) {
      setError(t('fileUpload.tooLarge', { max: maxSizeMB }));
      return;
    }

    // Show local preview
    if (type === 'photo') {
      setPreview(URL.createObjectURL(file));
    } else if (type === 'document') {
      setSelectedFileName(file.name);
    }

    if (mode === 'deferred' && onFileSelected) {
      onFileSelected(file);
    } else {
      try {
        await onUpload(file);
      } catch (err) {
        setError(err instanceof Error ? err.message : t('fileUpload.failed'));
      }
    }

    // Reset input so same file can be re-selected
    if (inputRef.current) inputRef.current.value = '';
  };

  const clearSelection = () => {
    setPreview(null);
    setSelectedFileName(null);
    if (mode === 'deferred' && onFileSelected) {
      // Signal parent that file was removed
      onFileSelected(undefined as unknown as File);
    }
  };

  const displayUrl = preview || currentUrl;

  return (
    <div className="space-y-2">
      {displayUrl && type === 'photo' && (
        <div className="relative inline-block">
          <img
            src={displayUrl}
            alt="Preview"
            className="h-32 w-32 rounded-lg object-cover border"
          />
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="absolute -top-2 -right-2 h-6 w-6 rounded-full bg-background shadow"
            onClick={clearSelection}
          >
            <X className="h-3 w-3" />
          </Button>
        </div>
      )}

      {displayUrl && type === 'video' && (
        <video
          src={displayUrl}
          controls
          className="h-48 max-w-full rounded-lg border"
        />
      )}

      {type === 'document' && (currentUrl || selectedFileName) && (
        <div className="flex items-center gap-2 rounded-lg border p-3">
          <FileText className="h-5 w-5 text-muted-foreground" />
          <span className="text-sm truncate flex-1">
            {selectedFileName || t('fileUpload.documentUploaded')}
          </span>
          {currentUrl && !selectedFileName && (
            <a
              href={currentUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-primary hover:underline"
            >
              {t('fileUpload.viewDocument')}
            </a>
          )}
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-6 w-6"
            onClick={clearSelection}
          >
            <X className="h-3 w-3" />
          </Button>
        </div>
      )}

      <div className="flex items-center gap-2">
        <input
          ref={inputRef}
          type="file"
          accept={accept}
          className="hidden"
          onChange={handleChange}
        />
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={isPending}
          onClick={() => inputRef.current?.click()}
        >
          <Upload className="me-1 h-4 w-4" />
          {isPending
            ? t('fileUpload.uploading')
            : currentUrl || selectedFileName
              ? t('fileUpload.replace')
              : t('fileUpload.upload')}
        </Button>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}
    </div>
  );
}
