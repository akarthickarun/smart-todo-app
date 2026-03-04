import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { authStore } from '@/features/auth/stores/authStore';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const loginTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    return () => {
      if (loginTimeoutRef.current !== null) {
        clearTimeout(loginTimeoutRef.current);
      }
    };
  }, []);

  const handleLogin = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    // Mock login: accept any email/password, set hardcoded token/user
    loginTimeoutRef.current = setTimeout(() => {
      authStore.getState().login('mock-token', {
        id: 'user-1',
        email,
        name: 'Test User',
      });
      setLoading(false);
      navigate('/');
    }, 700);
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted">
      <form
        onSubmit={handleLogin}
        className="bg-white p-8 rounded-lg shadow-md w-full max-w-sm space-y-6"
      >
        <h1 className="text-2xl font-bold mb-2">Login</h1>
        <div>
          <label htmlFor="email" className="block mb-1 text-sm font-medium">Email</label>
          <Input
            id="email"
            type="email"
            value={email}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setEmail(e.target.value)}
            required
            autoFocus
            placeholder="you@email.com"
          />
        </div>
        <div>
          <label htmlFor="password" className="block mb-1 text-sm font-medium">Password</label>
          <Input
            id="password"
            type="password"
            value={password}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
            required
            placeholder="••••••••"
          />
        </div>
        <Button type="submit" className="w-full" disabled={loading}>
          {loading ? 'Logging in...' : 'Login'}
        </Button>
      </form>
    </div>
  );
}
