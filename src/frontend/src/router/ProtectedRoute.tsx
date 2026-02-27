import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';
import { authStore } from '@/features/auth/stores/authStore';

interface ProtectedRouteProps {
  children: ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const isAuthenticated = authStore((state) => state.isAuthenticated);
  
  // Development bypass: allows access without auth when flag is set
  const bypassAuth = import.meta.env.VITE_BYPASS_AUTH === 'true';

  if (!isAuthenticated && !bypassAuth) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
