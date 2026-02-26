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
import { PRESET_SKILLS, SKILL_PROFICIENCY_LEVELS } from '../constants';
import type { CandidateSkillRequest } from '../types';

interface SkillsEditorProps {
  skills: CandidateSkillRequest[];
  onChange: (skills: CandidateSkillRequest[]) => void;
}

export function SkillsEditor({ skills, onChange }: SkillsEditorProps) {
  const { t } = useTranslation('candidates');

  const addSkill = () => {
    onChange([...skills, { skillName: '', proficiencyLevel: 'Basic' }]);
  };

  const removeSkill = (index: number) => {
    onChange(skills.filter((_, i) => i !== index));
  };

  const updateSkill = (index: number, field: keyof CandidateSkillRequest, value: string) => {
    const updated = skills.map((s, i) =>
      i === index ? { ...s, [field]: value } : s
    );
    onChange(updated);
  };

  // Filter out already-used preset skills
  const usedSkills = new Set(skills.map((s) => s.skillName));

  return (
    <div className="space-y-3">
      {skills.map((skill, index) => (
        <div key={index} className="flex items-center gap-2">
          <div className="flex-1">
            <Select
              value={PRESET_SKILLS.includes(skill.skillName) ? skill.skillName : '__custom'}
              onValueChange={(v) => {
                if (v === '__custom') {
                  updateSkill(index, 'skillName', '');
                } else {
                  updateSkill(index, 'skillName', v);
                }
              }}
            >
              <SelectTrigger>
                <SelectValue placeholder={t('skills.selectSkill')} />
              </SelectTrigger>
              <SelectContent>
                {PRESET_SKILLS.filter((s) => !usedSkills.has(s) || s === skill.skillName).map((s) => (
                  <SelectItem key={s} value={s}>
                    {t(`skills.presets.${s}`, s)}
                  </SelectItem>
                ))}
                <SelectItem value="__custom">{t('skills.custom')}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          {!PRESET_SKILLS.includes(skill.skillName) && skill.skillName !== '' && (
            <Input
              className="flex-1"
              value={skill.skillName}
              onChange={(e) => updateSkill(index, 'skillName', e.target.value)}
              placeholder={t('skills.customPlaceholder')}
            />
          )}
          {/* Show custom input if __custom was selected and skillName is empty */}
          {!PRESET_SKILLS.includes(skill.skillName) && skill.skillName === '' && (
            <Input
              className="flex-1"
              value=""
              onChange={(e) => updateSkill(index, 'skillName', e.target.value)}
              placeholder={t('skills.customPlaceholder')}
            />
          )}
          <div className="w-40">
            <Select
              value={skill.proficiencyLevel}
              onValueChange={(v) => updateSkill(index, 'proficiencyLevel', v)}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {SKILL_PROFICIENCY_LEVELS.map((level) => (
                  <SelectItem key={level} value={level}>
                    {t(`skills.proficiency.${level}`, level)}
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
            onClick={() => removeSkill(index)}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      ))}
      <Button type="button" variant="outline" size="sm" onClick={addSkill}>
        <Plus className="me-1 h-4 w-4" />
        {t('skills.add')}
      </Button>
    </div>
  );
}
