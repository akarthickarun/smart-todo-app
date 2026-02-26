import { Link } from 'react-router-dom';

export default function Header() {
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
          <Link
            to="/login"
            className="text-sm text-slate-600 hover:text-slate-900 transition-colors"
          >
            Login
          </Link>
        </nav>
      </div>
    </header>
  );
}
