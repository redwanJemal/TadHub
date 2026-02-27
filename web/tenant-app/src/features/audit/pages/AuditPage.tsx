import { useState, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { ClipboardList, ChevronDown, ChevronRight } from "lucide-react";
import { DataTableAdvanced } from "@/shared/components/data-table";
import type { Column } from "@/shared/components/data-table/DataTableAdvanced";
import { useAuditEvents } from "../hooks";
import type { AuditEventDto } from "../types";

function JsonPreview({ json }: { json: string | null }) {
  const [expanded, setExpanded] = useState(false);
  if (!json) return <span className="text-muted-foreground text-xs">-</span>;

  let parsed: unknown;
  try {
    parsed = JSON.parse(json);
  } catch {
    return <span className="text-xs text-muted-foreground">{json}</span>;
  }

  const formatted = JSON.stringify(parsed, null, 2);
  const preview = JSON.stringify(parsed).slice(0, 60);

  return (
    <div>
      <button
        onClick={() => setExpanded(!expanded)}
        className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground"
      >
        {expanded ? <ChevronDown className="h-3 w-3" /> : <ChevronRight className="h-3 w-3" />}
        <span className="font-mono">{expanded ? "collapse" : preview + (preview.length >= 60 ? "..." : "")}</span>
      </button>
      {expanded && (
        <pre className="mt-1 max-h-48 overflow-auto rounded bg-muted p-2 text-xs font-mono">
          {formatted}
        </pre>
      )}
    </div>
  );
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleString();
}

function formatUserId(userId: string | null) {
  if (!userId) return <span className="text-muted-foreground">-</span>;
  return <span className="font-mono text-xs">{userId.slice(0, 8)}...</span>;
}

export function AuditPage() {
  const { t } = useTranslation("audit");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [search, setSearch] = useState("");

  const params = useMemo(() => ({
    page,
    pageSize,
    sort: "-createdAt",
    ...(search ? { filter: { name: search } } : {}),
  }), [page, pageSize, search]);

  const { data, isLoading, refetch } = useAuditEvents(params);

  const columns: Column<AuditEventDto>[] = [
    { key: "eventName", header: t("events.eventName"), cell: (row) => <span className="font-medium">{row.eventName}</span> },
    { key: "userId", header: t("events.userId"), cell: (row) => formatUserId(row.userId) },
    { key: "createdAt", header: t("events.time"), cell: (row) => <span className="text-sm">{formatTime(row.createdAt)}</span> },
    { key: "payload", header: t("events.payload"), cell: (row) => <JsonPreview json={row.payload} /> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t("title")}</h1>
        <p className="text-muted-foreground">{t("description")}</p>
      </div>

      <DataTableAdvanced<AuditEventDto>
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        searchPlaceholder={t("filters.eventName")}
        searchValue={search}
        onSearchChange={setSearch}
        page={page}
        pageSize={pageSize}
        totalPages={data?.totalPages ?? 0}
        total={data?.totalCount ?? 0}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        onRefresh={() => refetch()}
        emptyIcon={ClipboardList}
        emptyTitle={t("empty.events.title")}
        emptyDescription={t("empty.events.description")}
      />
    </div>
  );
}
