'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

interface NavSection {
  title: string;
  items: { href: string; label: string; icon: string }[];
}

const navSections: NavSection[] = [
  {
    title: 'General',
    items: [
      { href: '/', label: 'Home', icon: '🏠' },
    ],
  },
  {
    title: 'Copilot Metrics',
    items: [
      { href: '/metrics', label: 'Adoption', icon: '📊' },
      { href: '/metrics/users', label: 'Users', icon: '👥' },
      { href: '/metrics/features', label: 'Features', icon: '⚡' },
      { href: '/metrics/languages', label: 'Languages', icon: '💻' },
      { href: '/metrics/trends', label: 'Trends', icon: '📈' },
      { href: '/metrics/roi', label: 'ROI', icon: '💰' },
    ],
  },
];

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="w-64 bg-white dark:bg-gray-900 border-r border-gray-200 dark:border-gray-700 min-h-screen p-4 hidden md:block">
      <div className="mb-8">
        <h1 className="text-xl font-bold text-gray-900 dark:text-white">APS Dashboard</h1>
      </div>
      <nav className="space-y-6">
        {navSections.map((section) => (
          <div key={section.title}>
            <p className="px-3 mb-2 text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider">
              {section.title}
            </p>
            <div className="space-y-1">
              {section.items.map((item) => {
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
            </div>
          </div>
        ))}
      </nav>
    </aside>
  );
}
