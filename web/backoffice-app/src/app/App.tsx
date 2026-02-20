import { BrowserRouter } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "sonner";
import { AppRoutes } from "./router";
import { queryClient } from "./providers";
import { AuthProvider } from "@/features/auth/AuthProvider";

// Use /admin as base path in production (check if deployed at /admin path)
const basename = window.location.pathname.startsWith('/admin') ? '/admin' : '/';

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter basename={basename}>
        <AuthProvider>
          <AppRoutes />
        </AuthProvider>
      </BrowserRouter>
      <Toaster position="top-right" richColors />
    </QueryClientProvider>
  );
}
