import { QueryClient, MutationCache } from "@tanstack/react-query";
import { toast } from "sonner";
import { ApiError } from "@/shared/api/client";

/**
 * Global mutation cache with error handling
 * Shows toast notifications for all mutation errors
 */
const mutationCache = new MutationCache({
  onError: (error, _variables, _context, mutation) => {
    // Skip if the mutation has its own onError handler that returns false
    // to indicate it handled the error
    if (mutation.options.meta?.skipGlobalErrorHandler) {
      return;
    }

    // Extract error message
    let errorMessage = "An unexpected error occurred";
    
    if (error instanceof ApiError) {
      errorMessage = error.message;
    } else if (error instanceof Error) {
      errorMessage = error.message;
    } else if (typeof error === "object" && error !== null) {
      const err = error as { message?: string; error?: string };
      errorMessage = err.message || err.error || errorMessage;
    }

    // Show error toast
    toast.error("Error", {
      description: errorMessage,
      duration: 5000,
    });
  },
  onSuccess: (_data, _variables, _context, mutation) => {
    // Show success toast if mutation has a success message in meta
    const successMessage = mutation.options.meta?.successMessage as string | undefined;
    if (successMessage) {
      toast.success("Success", {
        description: successMessage,
        duration: 3000,
      });
    }
  },
});

export const queryClient = new QueryClient({
  mutationCache,
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
    mutations: {
      retry: 0,
    },
  },
});
