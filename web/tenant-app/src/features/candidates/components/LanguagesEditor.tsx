import { useTranslation } from 'react-i18next';
import { Plus, X } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { PRESET_LANGUAGES, LANGUAGE_PROFICIENCY_LEVELS } from '../constants';
import type { CandidateLanguageRequest } from '../types';

interface LanguagesEditorProps {
  languages: CandidateLanguageRequest[];
  onChange: (languages: CandidateLanguageRequest[]) => void;
}

export function LanguagesEditor({ languages, onChange }: LanguagesEditorProps) {
  const { t } = useTranslation('candidates');

  const addLanguage = () => {
    onChange([...languages, { language: '', proficiencyLevel: 'Basic' }]);
  };

  const removeLanguage = (index: number) => {
    onChange(languages.filter((_, i) => i !== index));
  };

  const updateLanguage = (index: number, field: keyof CandidateLanguageRequest, value: string) => {
    const updated = languages.map((l, i) =>
      i === index ? { ...l, [field]: value } : l
    );
    onChange(updated);
  };

  const usedLanguages = new Set(languages.map((l) => l.language));

  return (
    <div className="space-y-3">
      {languages.map((lang, index) => (
        <div key={index} className="flex items-center gap-2">
          <div className="flex-1">
            <Select
              value={PRESET_LANGUAGES.includes(lang.language) ? lang.language : '__custom'}
              onValueChange={(v) => {
                if (v === '__custom') {
                  updateLanguage(index, 'language', '');
                } else {
                  updateLanguage(index, 'language', v);
                }
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder={t('languages.selectLanguage')} />
              </SelectTrigger>
              <SelectContent>
                {PRESET_LANGUAGES.filter((l) => !usedLanguages.has(l) || l === lang.language).map((l) => (
                  <SelectItem key={l} value={l}>
                    {t(`languages.presets.${l}`, l)}
                  </SelectItem>
                ))}
                <SelectItem value="__custom">{t('languages.custom')}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          {!PRESET_LANGUAGES.includes(lang.language) && (
            <Input
              className="flex-1"
              value={lang.language}
              onChange={(e) => updateLanguage(index, 'language', e.target.value)}
              placeholder={t('languages.customPlaceholder')}
            />
          )}
          <div className="w-40">
            <Select
              value={lang.proficiencyLevel}
              onValueChange={(v) => updateLanguage(index, 'proficiencyLevel', v)}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {LANGUAGE_PROFICIENCY_LEVELS.map((level) => (
                  <SelectItem key={level} value={level}>
                    {t(`languages.proficiency.${level}`, level)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="h-9 w-9 shrink-0"
            onClick={() => removeLanguage(index)}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      ))}
      <Button type="button" variant="outline" size="sm" onClick={addLanguage}>
        <Plus className="me-1 h-4 w-4" />
        {t('languages.add')}
      </Button>
    </div>
  );
}
