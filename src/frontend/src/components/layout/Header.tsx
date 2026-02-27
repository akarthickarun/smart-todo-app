import { authStore } from '@/features/auth/stores/authStore';
import { useNavigate, Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';

export default function Header() {
  const isAuthenticated = authStore((state) => state.isAuthenticated);
  const user = authStore((state) => state.user);
  const logout = authStore((state) => state.logout);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <Link to="/" className="text-xl font-semibold text-slate-900">
          Smart Todo App
        </Link>
        <nav className="flex items-center gap-4">
          <Link
            to="/"
            className="text-sm text-slate-600 hover:text-slate-900 transition-colors"
          >
            Todos
          </Link>
          {!isAuthenticated ? (
            <Link
              to="/login"
              className="text-sm text-slate-600 hover:text-slate-900 transition-colors"
            >
              Login
            </Link>
          ) : (
            <>
              <span className="text-sm text-slate-700 mr-2">{user?.name}</span>
              <Button variant="outline" size="sm" onClick={handleLogout}>
                Logout
              </Button>
            </>
          )}
        </nav>
      </div>
    </header>
  );
}
