/**
 * Reference Data Types
 * Global lookup data for Countries and Job Categories
 */

// =============================================================================
// Job Categories (MoHRE)
// =============================================================================

/**
 * Full Job Category DTO
 */
export interface JobCategoryDto {
  id: string;
  moHRECode: string;
  nameEn: string;
  nameAr: string;
  isActive: boolean;
  displayOrder: number;
}

/**
 * Lightweight Job Category reference for dropdowns
 */
export interface JobCategoryRefDto {
  id: string;
  moHRECode: string;
  nameEn: string;
  nameAr: string;
}

// =============================================================================
// Countries (ISO 3166-1)
// =============================================================================

/**
 * Full Country DTO
 */
export interface CountryDto {
  id: string;
  code: string;           // ISO alpha-2 (e.g., "AE")
  alpha3Code: string;     // ISO alpha-3 (e.g., "ARE")
  nameEn: string;
  nameAr: string;
  nationalityEn: string;  // e.g., "Emirati"
  nationalityAr: string;  // e.g., "إماراتي"
  dialingCode: string;    // e.g., "+971"
  isActive: boolean;
  displayOrder: number;
}

/**
 * Lightweight Country reference for dropdowns
 */
export interface CountryRefDto {
  id: string;
  code: string;
  nameEn: string;
  nameAr: string;
}

// =============================================================================
// Paginated Response (matches backend)
// =============================================================================

export interface PagedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// =============================================================================
// Filter Parameters
// =============================================================================

export interface JobCategoryFilterParams {
  page?: number;
  pageSize?: number;
  sort?: string;
  moHRECode?: string;
  isActive?: boolean;
}

export interface CountryFilterParams {
  page?: number;
  pageSize?: number;
  sort?: string;
  code?: string;
  isActive?: boolean;
  isCommonNationality?: boolean;
}

// =============================================================================
// Country Flag Emoji Helper
// =============================================================================

/**
 * Convert ISO alpha-2 code to flag emoji
 * @example getFlagEmoji('AE') => '🇦🇪'
 */
export function getFlagEmoji(countryCode: string): string {
  const codePoints = countryCode
    .toUpperCase()
    .split('')
    .map((char) => 127397 + char.charCodeAt(0));
  return String.fromCodePoint(...codePoints);
}
