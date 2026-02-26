export default function LoginPage() {
  return (
    <div className="flex min-h-[calc(100vh-8rem)] items-center justify-center">
      <div className="w-full max-w-md space-y-6 rounded-lg border border-slate-200 bg-white p-8 shadow-sm">
        <div className="text-center">
          <h1 className="text-2xl font-semibold text-slate-900">Login</h1>
          <p className="mt-2 text-sm text-slate-500">
            Authentication will be implemented in Ticket #17
          </p>
        </div>
        <div className="space-y-4">
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-slate-700">
              Email
            </label>
            <input
              id="email"
              type="email"
              placeholder="Enter your email"
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-sm placeholder-slate-400 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              disabled
            />
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-slate-700">
              Password
            </label>
            <input
              id="password"
              type="password"
              placeholder="Enter your password"
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-sm placeholder-slate-400 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              disabled
            />
          </div>
          <button
            className="w-full rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors disabled:bg-slate-300 disabled:cursor-not-allowed"
            disabled
          >
            Login (Coming Soon)
          </button>
        </div>
      </div>
    </div>
  );
}
