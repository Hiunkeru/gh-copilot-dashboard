'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

const navItems = [
  { href: '/', label: 'Adoption', icon: '📊' },
  { href: '/users', label: 'Users', icon: '👥' },
  { href: '/features', label: 'Features', icon: '⚡' },
  { href: '/languages', label: 'Languages', icon: '💻' },
  { href: '/trends', label: 'Trends', icon: '📈' },
  { href: '/roi', label: 'ROI', icon: '💰' },
];

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="w-64 bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-700 min-h-screen p-4 hidden md:block">
      <div className="mb-8">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">Copilot Dashboard</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">Usage Metrics</p>
      </div>
      <nav className="space-y-1">
        {navItems.map((item) => {
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-blue-50 dark:bg-blue-900/50 text-blue-700 dark:text-blue-300'
                  : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800'
              }`}
            >
              <span>{item.icon}</span>
              {item.label}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
