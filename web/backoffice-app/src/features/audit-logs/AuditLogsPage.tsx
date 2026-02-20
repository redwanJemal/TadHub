import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { DataTableAdvanced, type Column, type Filter } from "@/shared/components/data-table";
import { Badge } from "@/shared/components/ui/badge";
import { FileText, Loader2 } from "lucide-react";
import { apiClient } from "@/shared/api";
import { format } from "date-fns";

interface AuditLog {
  id: string;
  action: string;
  entity: string;
  entityId: string | null;
  userId: string | null;
  tenantId: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  changes: Record<string, unknown> | null;
  metadata: Record<string, unknown> | null;
  createdAt: string;
}

export function AuditLogsPage() {
  const { t } = useTranslation("auditLogs");

  // State
  const [data, setData] = useState<AuditLog[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [actionFilter, setActionFilter] = useState<string | undefined>();
  const [entityFilter, setEntityFilter] = useState<string | undefined>();
  const [page, setPage] = useState(1);
  const [perPage, setPerPage] = useState(20);
  const [total, setTotal] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  // Fetch audit logs
  const fetchLogs = useCallback(async () => {
    setIsLoading(true);
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        perPage: perPage.toString(),
      });

      if (actionFilter) params.append("action", actionFilter);
      if (entityFilter) params.append("entity", entityFilter);

      // PaginatedResult convention: { items, total, page, perPage, totalPages }
      const response = await apiClient.getWithMeta<{ items: AuditLog[]; total: number; page: number; perPage: number; totalPages: number }>(
        `/admin/audit-logs?${params.toString()}`
      );
      
      setData(response.data.items ?? []);
      setTotal(response.data.total ?? 0);
      setTotalPages(response.data.totalPages ?? 0);
    } catch (error) {
      console.error("Failed to fetch audit logs:", error);
      setData([]);
    } finally {
      setIsLoading(false);
    }
  }, [page, perPage, actionFilter, entityFilter]);

  useEffect(() => {
    fetchLogs();
  }, [fetchLogs]);

  const getActionVariant = (action: string) => {
    switch (action) {
      case "create":
        return "success";
      case "update":
        return "warning";
      case "delete":
        return "destructive";
      case "login":
        return "default";
      case "logout":
        return "secondary";
      default:
        return "outline";
    }
  };

  const columns: Column<AuditLog>[] = [
    {
      key: "action",
      header: t("columns.action"),
      cell: (row) => (
        <Badge variant={getActionVariant(row.action) as "default"}>
          {t(`actions.${row.action}`, { defaultValue: row.action })}
        </Badge>
      ),
    },
    {
      key: "entity",
      header: t("columns.entity"),
      cell: (row) => (
        <div className="flex items-center gap-2">
          <FileText className="h-4 w-4 text-muted-foreground" />
          <span className="font-medium">{row.entity}</span>
          {row.entityId && (
            <span className="text-xs text-muted-foreground">
              #{row.entityId.slice(0, 8)}
            </span>
          )}
        </div>
      ),
    },
    {
      key: "userId",
      header: t("columns.user"),
      cell: (row) => (
        <span className="text-sm">
          {row.userId ? row.userId.slice(0, 8) + "..." : "-"}
        </span>
      ),
    },
    {
      key: "tenantId",
      header: t("columns.tenant"),
      cell: (row) => (
        <span className="text-sm">
          {row.tenantId ? row.tenantId.slice(0, 8) + "..." : "Platform"}
        </span>
      ),
    },
    {
      key: "ipAddress",
      header: t("columns.ipAddress"),
      cell: (row) => (
        <code className="rounded bg-muted px-1.5 py-0.5 text-xs">
          {row.ipAddress || "-"}
        </code>
      ),
    },
    {
      key: "createdAt",
      header: t("columns.timestamp"),
      cell: (row) => (
        <span className="text-sm text-muted-foreground">
          {format(new Date(row.createdAt), "yyyy-MM-dd HH:mm:ss")}
        </span>
      ),
    },
  ];

  // Filters
  const filters: Filter[] = [
    {
      key: "action",
      label: t("filters.action"),
      options: [
        { label: t("actions.create"), value: "create" },
        { label: t("actions.update"), value: "update" },
        { label: t("actions.delete"), value: "delete" },
        { label: t("actions.login"), value: "login" },
        { label: t("actions.logout"), value: "logout" },
      ],
      value: actionFilter,
    },
    {
      key: "entity",
      label: t("filters.entity"),
      options: [
        { label: "User", value: "user" },
        { label: "Tenant", value: "tenant" },
        { label: "Role", value: "role" },
        { label: "Settings", value: "settings" },
        { label: "Session", value: "session" },
      ],
      value: entityFilter,
    },
  ];

  const handleFilterChange = (key: string, value: string | undefined) => {
    if (key === "action") {
      setActionFilter(value);
    } else if (key === "entity") {
      setEntityFilter(value);
    }
    setPage(1);
  };

  // Filter data locally by search (for entity/action text)
  const filteredData = (data ?? []).filter(
    (log) =>
      !search ||
      log.action.toLowerCase().includes(search.toLowerCase()) ||
      log.entity.toLowerCase().includes(search.toLowerCase()) ||
      (log.entityId && log.entityId.toLowerCase().includes(search.toLowerCase()))
  );

  if (isLoading && data.length === 0) {
    return (
      <div className="flex h-96 items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold md:text-3xl">{t("title")}</h1>
        <p className="text-muted-foreground">{t("subtitle")}</p>
      </div>

      <DataTableAdvanced
        columns={columns}
        data={filteredData}
        searchValue={search}
        onSearchChange={setSearch}
        searchPlaceholder={t("searchPlaceholder")}
        filters={filters}
        onFilterChange={handleFilterChange}
        page={page}
        perPage={perPage}
        total={total}
        totalPages={totalPages}
        onPageChange={setPage}
        onPerPageChange={(value: number) => {
          setPerPage(value);
          setPage(1);
        }}
        isLoading={isLoading}
      />
    </div>
  );
}
