import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { useJobCategoryRefs } from '../hooks/use-reference-data';
import type { JobCategoryRefDto } from '../types';

export interface JobCategorySelectProps {
  /** Selected job category ID */
  value?: string;
  /** Change handler */
  onChange?: (value: string, category?: JobCategoryRefDto) => void;
  /** Whether the select is disabled */
  disabled?: boolean;
  /** Placeholder text */
  placeholder?: string;
  /** Show MoHRE code prefix */
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
 * Reusable Job Category Select component
 * Fetches and caches job categories from the API
 * 
 * @example
 * <JobCategorySelect
 *   value={formData.jobCategoryId}
 *   onChange={(id) => setFormData({ ...formData, jobCategoryId: id })}
 *   showCode
 * />
 */
export function JobCategorySelect({
  value,
  onChange,
  disabled,
  placeholder = 'Select job category',
  showCode = false,
  className,
  error,
  id,
  name,
}: JobCategorySelectProps) {
  const { data: categories, isLoading, isError } = useJobCategoryRefs();

  const handleChange = (newValue: string) => {
    const category = categories?.find((c) => c.id === newValue);
    onChange?.(newValue, category);
  };

  // Find the selected category for display
  const selectedCategory = categories?.find((c) => c.id === value);

  if (isLoading) {
    return <Skeleton className="h-10 w-full" />;
  }

  if (isError) {
    return (
      <Select disabled>
        <SelectTrigger className={className}>
          <SelectValue placeholder="Failed to load categories" />
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
          {selectedCategory && (
            <span>
              {showCode && (
                <span className="font-mono text-muted-foreground me-2">
                  {selectedCategory.moHRECode}
                </span>
              )}
              {selectedCategory.nameEn}
            </span>
          )}
        </SelectValue>
      </SelectTrigger>
      <SelectContent>
        {categories?.map((category) => (
          <SelectItem key={category.id} value={category.id}>
            {showCode && (
              <span className="font-mono text-muted-foreground me-2">
                {category.moHRECode}
              </span>
            )}
            {category.nameEn}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

export default JobCategorySelect;
