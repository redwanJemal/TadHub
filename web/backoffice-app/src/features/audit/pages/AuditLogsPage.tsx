import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { FileText, Construction, Clock, User, Database } from "lucide-react";

export function AuditLogsPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Audit Logs</h1>
        <p className="text-muted-foreground">
          Activity and compliance logs
        </p>
      </div>

      {/* Coming Soon Card */}
      <Card className="border-dashed">
        <CardHeader className="text-center pb-2">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
            <Construction className="h-8 w-8 text-primary" />
          </div>
          <CardTitle className="text-xl">Coming Soon</CardTitle>
          <CardDescription className="text-base">
            Audit log features are under development
          </CardDescription>
        </CardHeader>
        <CardContent className="text-center text-muted-foreground">
          <p className="mb-4">Planned features include:</p>
          <ul className="space-y-2 text-sm">
            <li className="flex items-center justify-center gap-2">
              <FileText className="h-4 w-4" />
              View all system events and entity changes
            </li>
            <li className="flex items-center justify-center gap-2">
              <Clock className="h-4 w-4" />
              Filter by date range
            </li>
            <li className="flex items-center justify-center gap-2">
              <User className="h-4 w-4" />
              Filter by user
            </li>
            <li className="flex items-center justify-center gap-2">
              <Database className="h-4 w-4" />
              View old/new value diffs
            </li>
            <li>• Export audit logs</li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
