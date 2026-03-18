'use client';

import { useTheme } from 'next-themes';
import { loginRequest } from '@/lib/msalConfig';
import { useEffect, useState } from 'react';
import { useMsalInstance } from '@/hooks/useMsalInstance';

const DEV_MODE = process.env.NEXT_PUBLIC_DEV_MODE === 'true';

export function Header() {
  const { theme, setTheme } = useTheme();
  const msalInstance = useMsalInstance();
  const [mounted, setMounted] = useState(false);

  useEffect(() => setMounted(true), []);

  const accounts = msalInstance?.getAllAccounts() ?? [];
  const isAuthenticated = DEV_MODE || accounts.length > 0;

  const handleLogin = () => msalInstance?.loginPopup(loginRequest);
  const handleLogout = () => msalInstance?.logoutPopup();

  return (
    <header className="h-14 border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 flex items-center justify-between px-6">
      <div className="md:hidden">
        <h1 className="text-lg font-bold text-gray-900 dark:text-white">Copilot Dashboard</h1>
      </div>
      <div className="flex-1" />
      <div className="flex items-center gap-4">
        {mounted && (
          <button
            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
            className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 text-gray-600 dark:text-gray-400"
            aria-label="Toggle theme"
          >
            {theme === 'dark' ? '☀️' : '🌙'}
          </button>
        )}
        {DEV_MODE ? (
          <span className="text-sm text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-900/30 px-2 py-1 rounded">
            Dev Mode
          </span>
        ) : isAuthenticated ? (
          <div className="flex items-center gap-3">
            <span className="text-sm text-gray-700 dark:text-gray-300">
              {accounts[0]?.name || accounts[0]?.username}
            </span>
            <button
              onClick={handleLogout}
              className="text-sm text-red-600 dark:text-red-400 hover:underline"
            >
              Logout
            </button>
          </div>
        ) : (
          <button
            onClick={handleLogin}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700"
          >
            Sign In
          </button>
        )}
      </div>
    </header>
  );
}
