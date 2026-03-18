'use client';

import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { DistributionItem } from '@/lib/types';

export function LanguageBarChart({ data, title }: { data: DistributionItem[]; title: string }) {
  return (
    <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">{title}</h3>
      <ResponsiveContainer width="100%" height={400}>
        <BarChart data={data} layout="vertical" margin={{ left: 80 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.3} />
          <XAxis type="number" stroke="#9ca3af" />
          <YAxis type="category" dataKey="name" tick={{ fontSize: 12 }} stroke="#9ca3af" width={80} />
          <Tooltip contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }} />
          <Bar dataKey="acceptances" name="Acceptances" fill="#3b82f6" radius={[0, 4, 4, 0]} />
          <Bar dataKey="suggestions" name="Suggestions" fill="#93c5fd" radius={[0, 4, 4, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
