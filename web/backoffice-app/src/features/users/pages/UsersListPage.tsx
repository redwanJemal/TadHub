import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Users, Construction } from "lucide-react";

export function UsersListPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Users</h1>
        <p className="text-muted-foreground">
          Manage platform user accounts
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
            User management features are under development
          </CardDescription>
        </CardHeader>
        <CardContent className="text-center text-muted-foreground">
          <p className="mb-4">Planned features include:</p>
          <ul className="space-y-2 text-sm">
            <li className="flex items-center justify-center gap-2">
              <Users className="h-4 w-4" />
              User list with search and filters
            </li>
            <li>• View user details and tenant memberships</li>
            <li>• Add users to tenants</li>
            <li>• Deactivate/reactivate users</li>
            <li>• Login history and audit</li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
