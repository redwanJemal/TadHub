import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useCountryRefs } from '../hooks/use-reference-data';
import { getFlagEmoji } from '../types';
import type { CountryRefDto } from '../types';

export interface CountrySelectProps {
  /** Selected country ID */
  value?: string;
  /** Change handler */
  onChange?: (value: string, country?: CountryRefDto) => void;
  /** Whether the select is disabled */
  disabled?: boolean;
  /** Placeholder text */
  placeholder?: string;
  /** Show country flag emoji */
  showFlag?: boolean;
  /** Show country code prefix */
  showCode?: boolean;
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
 * Reusable Country Select component
 * Fetches and caches countries from the API
 * 
 * @example
 * <CountrySelect
 *   value={formData.countryId}
 *   onChange={(id) => setFormData({ ...formData, countryId: id })}
 *   showFlag
 * />
 */
export function CountrySelect({
  value,
  onChange,
  disabled,
  placeholder = 'Select country',
  showFlag = true,
  showCode = false,
  className,
  error,
  id,
  name,
}: CountrySelectProps) {
  const { data: countries, isLoading, isError } = useCountryRefs();

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
          <SelectValue placeholder="Failed to load countries" />
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
              {showCode && (
                <span className="font-mono text-muted-foreground">
                  {selectedCountry.code}
                </span>
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
              {showCode && (
                <span className="font-mono text-muted-foreground">
                  {country.code}
                </span>
              )}
              <span>{country.nameEn}</span>
            </span>
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

export default CountrySelect;
