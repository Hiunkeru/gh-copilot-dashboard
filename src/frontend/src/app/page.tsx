'use client';

import Link from 'next/link';

const sections = [
  {
    href: '/metrics',
    title: 'Copilot Metrics',
    description: 'Usage metrics, adoption rates, and ROI analysis for GitHub Copilot',
    icon: '📊',
  },
];

export default function HomePage() {
  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white">APS Dashboard</h2>
        <p className="mt-2 text-gray-500 dark:text-gray-400">
          Central hub for engineering tools and analytics
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {sections.map((section) => (
          <Link
            key={section.href}
            href={section.href}
            className="group bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6 hover:border-blue-500 dark:hover:border-blue-500 transition-colors"
          >
            <div className="text-3xl mb-4">{section.icon}</div>
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
              {section.title}
            </h3>
            <p className="mt-2 text-sm text-gray-500 dark:text-gray-400">
              {section.description}
            </p>
          </Link>
        ))}
      </div>
    </div>
  );
}
