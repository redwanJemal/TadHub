import { useState, createContext, useContext } from "react";
import { Outlet } from "react-router-dom";
import { Sidebar } from "./Sidebar";
import { Header } from "./Header";
import { cn } from "@/shared/lib/cn";

interface SidebarContextType {
  collapsed: boolean;
  setCollapsed: (collapsed: boolean) => void;
}

const SidebarContext = createContext<SidebarContextType>({
  collapsed: false,
  setCollapsed: () => {},
});

export const useSidebar = () => useContext(SidebarContext);

export function DashboardLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(false);

  return (
    <SidebarContext.Provider value={{ collapsed, setCollapsed }}>
      <div className="flex h-screen overflow-hidden bg-muted/30">
        {/* Mobile sidebar overlay */}
        {sidebarOpen && (
          <div
            className="fixed inset-0 z-40 bg-black/50 lg:hidden"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        {/* Sidebar */}
        <aside
          className={cn(
            "fixed inset-y-0 start-0 z-50 bg-card transition-all duration-300 ease-in-out",
            "lg:relative lg:z-auto",
            collapsed ? "lg:w-[72px]" : "lg:w-64",
            sidebarOpen 
              ? "w-64 translate-x-0 rtl:-translate-x-0" 
              : "w-64 -translate-x-full rtl:translate-x-full lg:translate-x-0 rtl:lg:-translate-x-0"
          )}
        >
          <Sidebar onClose={() => setSidebarOpen(false)} />
        </aside>

        {/* Main content area */}
        <div className="flex flex-1 flex-col overflow-hidden">
          <Header onMenuClick={() => setSidebarOpen(true)} />
          <main className="flex-1 overflow-auto p-4 md:p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </SidebarContext.Provider>
  );
}
