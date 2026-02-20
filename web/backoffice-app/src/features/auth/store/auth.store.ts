import { create } from "zustand";
import { api, ApiError } from "@/shared/lib/api-client";

export interface Employee {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  avatar?: string;
}

interface AuthState {
  employee: Employee | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  loadFromStorage: () => void;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  employee: null,
  token: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,

  login: async (email, password) => {
    set({ isLoading: true, error: null });
    try {
      // Employee login endpoint for platform admins
      const response = await api.post<{
        accessToken: string;
        employee: Employee;
      }>("/auth/employee/login", { email, password });

      const { accessToken, employee } = response.data!;

      localStorage.setItem("forgebase_admin_token", accessToken);
      localStorage.setItem("forgebase_admin_employee", JSON.stringify(employee));

      set({
        token: accessToken,
        employee,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error) {
      const message = error instanceof ApiError ? error.message : "Login failed";
      set({ error: message, isLoading: false });
      throw error;
    }
  },

  logout: () => {
    localStorage.removeItem("forgebase_admin_token");
    localStorage.removeItem("forgebase_admin_employee");

    set({
      employee: null,
      token: null,
      isAuthenticated: false,
    });

    window.location.href = "/login";
  },

  loadFromStorage: () => {
    const token = localStorage.getItem("forgebase_admin_token");
    const employeeStr = localStorage.getItem("forgebase_admin_employee");

    if (token && employeeStr) {
      try {
        const employee = JSON.parse(employeeStr);

        set({
          token,
          employee,
          isAuthenticated: true,
        });
      } catch {
        // Invalid data, clear storage
        get().logout();
      }
    }
  },

  clearError: () => set({ error: null }),
}));
