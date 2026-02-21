import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useCommonNationalities, useCountryRefs } from '../hooks/use-reference-data';
import { getFlagEmoji } from '../types';
import type { CountryRefDto } from '../types';

export interface NationalitySelectProps {
  /** Selected country ID (nationality is derived from country) */
  value?: string;
  /** Change handler */
  onChange?: (value: string, country?: CountryRefDto) => void;
  /** Whether the select is disabled */
  disabled?: boolean;
  /** Placeholder text */
  placeholder?: string;
  /** Show only common Tadbeer nationalities (Philippines, Indonesia, etc.) */
  commonOnly?: boolean;
  /** Show country flag emoji */
  showFlag?: boolean;
  /** Additional class names */
  className?: string;
  /** Error state */
  error?: boolean;
  /** ID for form association */
  id?: string;
  /** Name for form association */
  name?: string;
}

/**
 * Reusable Nationality Select component
 * Shows nationality based on country data
 * Can show only common Tadbeer nationalities or all countries
 * 
 * @example
 * // Common nationalities only (for worker registration)
 * <NationalitySelect
 *   value={formData.nationalityId}
 *   onChange={(id) => setFormData({ ...formData, nationalityId: id })}
 *   commonOnly
 * />
 * 
 * // All countries
 * <NationalitySelect
 *   value={formData.nationalityId}
 *   onChange={(id) => setFormData({ ...formData, nationalityId: id })}
 * />
 */
export function NationalitySelect({
  value,
  onChange,
  disabled,
  placeholder = 'Select nationality',
  commonOnly = false,
  showFlag = true,
  className,
  error,
  id,
  name,
}: NationalitySelectProps) {
  // Use common nationalities if commonOnly, otherwise all countries
  const commonQuery = useCommonNationalities();
  const allQuery = useCountryRefs();
  
  const { data: countries, isLoading, isError } = commonOnly ? commonQuery : allQuery;

  const handleChange = (newValue: string) => {
    const country = countries?.find((c) => c.id === newValue);
    onChange?.(newValue, country);
  };

  // Find the selected country for display
  const selectedCountry = countries?.find((c) => c.id === value);

  if (isLoading) {
    return <Skeleton className="h-10 w-full" />;
  }

  if (isError) {
    return (
      <Select disabled>
        <SelectTrigger className={className}>
          <SelectValue placeholder="Failed to load nationalities" />
        </SelectTrigger>
      </Select>
    );
  }

  return (
    <Select
      value={value}
      onValueChange={handleChange}
      disabled={disabled}
      name={name}
    >
      <SelectTrigger
        id={id}
        className={`${className} ${error ? 'border-red-500' : ''}`}
      >
        <SelectValue placeholder={placeholder}>
          {selectedCountry && (
            <span className="flex items-center gap-2">
              {showFlag && (
                <span className="text-lg">{getFlagEmoji(selectedCountry.code)}</span>
              )}
              <span>{selectedCountry.nameEn}</span>
            </span>
          )}
        </SelectValue>
      </SelectTrigger>
      <SelectContent>
        {countries?.map((country) => (
          <SelectItem key={country.id} value={country.id}>
            <span className="flex items-center gap-2">
              {showFlag && (
                <span className="text-lg">{getFlagEmoji(country.code)}</span>
              )}
              <span>{country.nameEn}</span>
            </span>
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

export default NationalitySelect;
