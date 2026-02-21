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
  /** Selected value (country ID or name, based on valueType) */
  value?: string;
  /** Change handler - returns value based on valueType */
  onChange?: (value: string, country?: CountryRefDto) => void;
  /** Whether the select is disabled */
  disabled?: boolean;
  /** Placeholder text */
  placeholder?: string;
  /** Show only common Tadbeer nationalities (Philippines, Indonesia, etc.) */
  commonOnly?: boolean;
  /** Show country flag emoji */
  showFlag?: boolean;
  /** What value to use - 'id' for country UUID, 'name' for country nameEn */
  valueType?: 'id' | 'name';
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
  valueType = 'id',
  className,
  error,
  id,
  name,
}: NationalitySelectProps) {
  // Use common nationalities if commonOnly, otherwise all countries
  const commonQuery = useCommonNationalities();
  const allQuery = useCountryRefs();
  
  const { data: countries, isLoading, isError } = commonOnly ? commonQuery : allQuery;

  // Get the value to use for each country based on valueType
  const getCountryValue = (country: CountryRefDto) => 
    valueType === 'name' ? country.nameEn : country.id;

  const handleChange = (newValue: string) => {
    const country = countries?.find((c) => getCountryValue(c) === newValue);
    onChange?.(newValue, country);
  };

  // Find the selected country for display
  const selectedCountry = countries?.find((c) => getCountryValue(c) === value);

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
          <SelectItem key={country.id} value={getCountryValue(country)}>
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
