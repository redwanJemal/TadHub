import { NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { cn } from "@/shared/lib/cn";
import { useSidebar } from "./DashboardLayout";
import { useAppAuth } from "@/features/auth/AuthProvider";
import { keycloakUrls } from "@/lib/auth-config";
import {
  LayoutDashboard,
  Building2,
  Users,
  Shield,
  FileText,
  X,
  LogOut,
  KeyRound,
} from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/shared/components/ui/avatar";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";

const navItems = [
  { path: "/", icon: LayoutDashboard, labelKey: "nav.home", exact: true },
  { path: "/tenants", icon: Building2, labelKey: "nav.tenants" },
  { path: "/platform-team", icon: Shield, labelKey: "nav.platformTeam" },
  { path: "/users", icon: Users, labelKey: "nav.allUsers" },
  { path: "/audit-logs", icon: FileText, labelKey: "nav.auditLogs" },
];

interface SidebarProps {
  onClose?: () => void;
}

export function Sidebar({ onClose }: SidebarProps) {
  const { t, i18n } = useTranslation();
  const { collapsed } = useSidebar();
  const { employee, logout } = useAppAuth();
  const isRtl = i18n.dir() === "rtl";

  const userInitials = employee
    ? `${employee.firstName?.[0] || ""}${employee.lastName?.[0] || ""}`.toUpperCase() || "A"
    : "A";

  return (
    <TooltipProvider delayDuration={0}>
      <div className="flex h-full flex-col border-e bg-card">
        {/* Header */}
        <div className={cn(
          "flex h-16 items-center border-b shrink-0",
          collapsed ? "justify-center px-3" : "justify-between px-4"
        )}>
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-primary-foreground font-bold text-lg shrink-0">
              T
            </div>
            {!collapsed && (
              <span className="font-semibold text-lg whitespace-nowrap">TadHub Admin</span>
            )}
          </div>
          {!collapsed && (
            <Button variant="ghost" size="icon" className="lg:hidden shrink-0" onClick={onClose}>
              <X className="h-5 w-5" />
            </Button>
          )}
        </div>

        {/* Navigation */}
        <nav className={cn(
          "flex-1 overflow-y-auto py-4",
          collapsed ? "px-3" : "px-3"
        )}>
          <div className="space-y-1">
            {navItems.map((item) => {
              const Icon = item.icon;
              const linkContent = (
                <NavLink
                  key={item.path}
                  to={item.path}
                  end={item.exact}
                  onClick={onClose}
                  className={({ isActive }) =>
                    cn(
                      "flex items-center rounded-lg text-sm font-medium transition-colors",
                      collapsed 
                        ? "justify-center h-10 w-10 mx-auto" 
                        : "gap-3 px-3 py-2.5",
                      isActive
                        ? "bg-primary text-primary-foreground"
                        : "text-muted-foreground hover:bg-muted hover:text-foreground"
                    )
                  }
                >
                  <Icon className="h-5 w-5 shrink-0" />
                  {!collapsed && <span>{t(item.labelKey)}</span>}
                </NavLink>
              );

              if (collapsed) {
                return (
                  <Tooltip key={item.path}>
                    <TooltipTrigger asChild>
                      {linkContent}
                    </TooltipTrigger>
                    <TooltipContent side={isRtl ? "left" : "right"} sideOffset={8}>
                      {t(item.labelKey)}
                    </TooltipContent>
                  </Tooltip>
                );
              }

              return <div key={item.path}>{linkContent}</div>;
            })}
          </div>
        </nav>

        {/* User section */}
        <div className={cn("border-t shrink-0", collapsed ? "p-3" : "p-3")}>
          {collapsed ? (
            <div className="flex flex-col items-center gap-2">
              <Tooltip>
                <TooltipTrigger asChild>
                  <button className="focus:outline-none">
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={employee?.avatar} alt={employee?.firstName} />
                      <AvatarFallback className="bg-primary/10 text-primary text-sm font-medium">
                        {userInitials}
                      </AvatarFallback>
                    </Avatar>
                  </button>
                </TooltipTrigger>
                <TooltipContent side={isRtl ? "left" : "right"} sideOffset={8}>
                  <p className="font-medium">{employee?.firstName} {employee?.lastName}</p>
                  <p className="text-xs opacity-70">{employee?.email}</p>
                </TooltipContent>
              </Tooltip>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-10 w-10"
                    onClick={() => window.open(keycloakUrls.account + '/#/security/signingin', '_blank')}
                  >
                    <KeyRound className="h-5 w-5" />
                  </Button>
                </TooltipTrigger>
                <TooltipContent side={isRtl ? "left" : "right"} sideOffset={8}>
                  {t("auth:changePassword")}
                </TooltipContent>
              </Tooltip>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button variant="ghost" size="icon" className="h-10 w-10" onClick={logout}>
                    <LogOut className="h-5 w-5" />
                  </Button>
                </TooltipTrigger>
                <TooltipContent side={isRtl ? "left" : "right"} sideOffset={8}>
                  {t("auth:logout")}
                </TooltipContent>
              </Tooltip>
            </div>
          ) : (
            <div className="flex flex-col gap-2">
              <div className="flex items-center gap-3 rounded-lg bg-muted/50 p-3">
                <Avatar className="h-10 w-10 shrink-0">
                  <AvatarImage src={employee?.avatar} alt={employee?.firstName} />
                  <AvatarFallback className="bg-primary/10 text-primary text-sm font-medium">
                    {userInitials}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">
                    {employee?.firstName} {employee?.lastName}
                  </p>
                  <p className="text-xs text-muted-foreground truncate">{employee?.email}</p>
                </div>
                <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0" onClick={logout}>
                  <LogOut className="h-4 w-4" />
                </Button>
              </div>
              <Button
                variant="ghost"
                size="sm"
                className="w-full justify-start gap-2 text-muted-foreground"
                onClick={() => window.open(keycloakUrls.account + '/#/security/signingin', '_blank')}
              >
                <KeyRound className="h-4 w-4" />
                {t("auth:changePassword")}
              </Button>
            </div>
          )}
        </div>
      </div>
    </TooltipProvider>
  );
}
