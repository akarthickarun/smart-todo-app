import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  // TODO: Replace with actual auth store check in Ticket #17
  const isAuthenticated = false;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
