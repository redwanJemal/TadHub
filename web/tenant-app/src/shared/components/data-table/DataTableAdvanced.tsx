import { useCallback, useMemo } from "react";
import { useTranslation } from "react-i18next";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Checkbox } from "@/shared/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { EmptyState } from "@/shared/components/ui/empty-state";
import { ChevronLeft, ChevronRight, Search, RefreshCw, X, type LucideIcon } from "lucide-react";

export interface Column<T> {
  key: keyof T | string;
  header: string;
  cell?: (row: T) => React.ReactNode;
  className?: string;
  sortable?: boolean;
}

export interface FilterOption {
  label: string;
  value: string;
}

export interface Filter {
  key: string;
  label: string;
  options: FilterOption[];
  value?: string;
}

export interface DataTableAdvancedProps<T> {
  columns: Column<T>[];
  data: T[];
  isLoading?: boolean;
  // Search
  searchPlaceholder?: string;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
  // Filters
  filters?: Filter[];
  onFilterChange?: (key: string, value: string | undefined) => void;
  // Pagination
  page?: number;
  totalPages?: number;
  total?: number;
  perPage?: number;
  onPageChange?: (page: number) => void;
  onPerPageChange?: (perPage: number) => void;
  // Selection
  selectable?: boolean;
  selectedIds?: string[];
  onSelectionChange?: (ids: string[]) => void;
  // Actions
  onRefresh?: () => void;
  actions?: React.ReactNode;
  bulkActions?: React.ReactNode;
  // Empty State
  emptyIcon?: LucideIcon;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyAction?: React.ReactNode;
  emptyVariant?: 'default' | 'roles' | 'team' | 'files' | 'settings';
}

export function DataTableAdvanced<T extends { id: string }>({
  columns,
  data,
  isLoading,
  searchPlaceholder,
  searchValue,
  onSearchChange,
  filters,
  onFilterChange,
  page = 1,
  totalPages = 1,
  total,
  perPage = 20,
  onPageChange,
  onPerPageChange,
  selectable,
  selectedIds = [],
  onSelectionChange,
  onRefresh,
  actions,
  bulkActions,
  emptyIcon,
  emptyTitle,
  emptyDescription,
  emptyAction,
  emptyVariant = 'default',
}: DataTableAdvancedProps<T>) {
  const { t } = useTranslation();

  const allSelected = useMemo(() => {
    return data.length > 0 && data.every((row) => selectedIds.includes(row.id));
  }, [data, selectedIds]);

  const someSelected = useMemo(() => {
    return data.some((row) => selectedIds.includes(row.id)) && !allSelected;
  }, [data, selectedIds, allSelected]);

  const handleSelectAll = useCallback(() => {
    if (!onSelectionChange) return;
    if (allSelected) {
      onSelectionChange([]);
    } else {
      onSelectionChange(data.map((row) => row.id));
    }
  }, [allSelected, data, onSelectionChange]);

  const handleSelectRow = useCallback(
    (id: string) => {
      if (!onSelectionChange) return;
      if (selectedIds.includes(id)) {
        onSelectionChange(selectedIds.filter((sid) => sid !== id));
      } else {
        onSelectionChange([...selectedIds, id]);
      }
    },
    [selectedIds, onSelectionChange]
  );

  const getCellValue = (row: T, column: Column<T>): React.ReactNode => {
    if (column.cell) {
      return column.cell(row);
    }
    const value = row[column.key as keyof T];
    return value as React.ReactNode;
  };

  const hasActiveFilters = filters?.some((f) => f.value && f.value !== "all");

  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <div className="flex flex-col gap-4">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-1 items-center gap-2">
            {onSearchChange && (
              <div className="relative flex-1 sm:max-w-sm">
                <Search className="absolute start-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder={searchPlaceholder ?? t("actions.search")}
                  value={searchValue}
                  onChange={(e) => onSearchChange(e.target.value)}
                  className="ps-9"
                />
              </div>
            )}
            {onRefresh && (
              <Button variant="outline" size="icon" onClick={onRefresh}>
                <RefreshCw className="h-4 w-4" />
              </Button>
            )}
          </div>
          {actions && <div className="flex items-center gap-2">{actions}</div>}
        </div>

        {/* Filters Row */}
        {filters && filters.length > 0 && (
          <div className="flex flex-wrap items-center gap-2">
            {filters.map((filter) => (
              <Select
                key={filter.key}
                value={filter.value ?? "all"}
                onValueChange={(value) =>
                  onFilterChange?.(filter.key, value === "all" ? undefined : value)
                }
              >
                <SelectTrigger className="w-[150px]">
                  <SelectValue placeholder={filter.label} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">{t("common:all")} {filter.label}</SelectItem>
                  {filter.options.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ))}
            {hasActiveFilters && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => filters.forEach((f) => onFilterChange?.(f.key, undefined))}
              >
                <X className="h-4 w-4 me-1" />
                {t("actions.clearFilters")}
              </Button>
            )}
          </div>
        )}

        {/* Bulk Actions */}
        {selectable && selectedIds.length > 0 && bulkActions && (
          <div className="flex items-center gap-2 rounded-lg bg-muted p-2">
            <span className="text-sm text-muted-foreground">
              {selectedIds.length} {t("common.selected")}
            </span>
            {bulkActions}
          </div>
        )}
      </div>

      {/* Table */}
      <div className="rounded-lg border bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              {selectable && (
                <TableHead className="w-[50px]">
                  <Checkbox
                    checked={allSelected}
                    indeterminate={someSelected}
                    onCheckedChange={handleSelectAll}
                    aria-label="Select all"
                  />
                </TableHead>
              )}
              {columns.map((column) => (
                <TableHead key={String(column.key)} className={column.className}>
                  {column.header}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  {selectable && (
                    <TableCell>
                      <Skeleton className="h-4 w-4" />
                    </TableCell>
                  )}
                  {columns.map((column) => (
                    <TableCell key={String(column.key)}>
                      <Skeleton className="h-5 w-full" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : data.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={columns.length + (selectable ? 1 : 0)}
                  className="p-0"
                >
                  <EmptyState
                    icon={emptyIcon}
                    title={emptyTitle}
                    description={emptyDescription}
                    action={emptyAction}
                    variant={emptyVariant}
                    className="py-16"
                  />
                </TableCell>
              </TableRow>
            ) : (
              data.map((row) => (
                <TableRow
                  key={row.id}
                  data-state={selectedIds.includes(row.id) ? "selected" : undefined}
                >
                  {selectable && (
                    <TableCell>
                      <Checkbox
                        checked={selectedIds.includes(row.id)}
                        onCheckedChange={() => handleSelectRow(row.id)}
                        aria-label="Select row"
                      />
                    </TableCell>
                  )}
                  {columns.map((column) => (
                    <TableCell key={String(column.key)} className={column.className}>
                      {getCellValue(row, column)}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          {total !== undefined && (
            <span>{t("common:table.totalResults")}: {total}</span>
          )}
          {onPerPageChange && (
            <Select
              value={String(perPage)}
              onValueChange={(v) => onPerPageChange(Number(v))}
            >
              <SelectTrigger className="w-[80px] h-8">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="10">10</SelectItem>
                <SelectItem value="20">20</SelectItem>
                <SelectItem value="50">50</SelectItem>
                <SelectItem value="100">100</SelectItem>
              </SelectContent>
            </Select>
          )}
        </div>
        {totalPages > 1 && (
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">
              {t("common:table.page")} {page} {t("common:table.of")} {totalPages}
            </span>
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => onPageChange?.(page - 1)}
              disabled={page <= 1}
            >
              <ChevronLeft className="h-4 w-4 rtl:rotate-180" />
            </Button>
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => onPageChange?.(page + 1)}
              disabled={page >= totalPages}
            >
              <ChevronRight className="h-4 w-4 rtl:rotate-180" />
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
