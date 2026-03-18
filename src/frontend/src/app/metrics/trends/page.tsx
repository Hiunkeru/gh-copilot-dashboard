'use client';

import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useMsalInstance } from '@/hooks/useMsalInstance';
import { DauWauChart } from '@/components/charts/DauWauChart';
import { AcceptanceRateChart } from '@/components/charts/AcceptanceRateChart';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

export default function TrendsPage() {
  const instance = useMsalInstance();

  const { data: trends, isLoading } = useQuery({
    queryKey: ['trends', 90],
    queryFn: () => api.getTrends(instance, 90),
  });

  if (isLoading || !trends) {
    return <div className="animate-pulse space-y-4"><div className="h-96 bg-gray-200 dark:bg-gray-800 rounded-xl" /></div>;
  }

  // Group by week for week-over-week view
  const weeklyData: Record<string, { week: string; activeUsers: number; suggestions: number; acceptances: number; days: number }> = {};
  trends.forEach((point) => {
    const date = new Date(point.date);
    const weekStart = new Date(date);
    weekStart.setDate(date.getDate() - date.getDay());
    const weekKey = weekStart.toISOString().split('T')[0];
    if (!weeklyData[weekKey]) {
      weeklyData[weekKey] = { week: weekKey, activeUsers: 0, suggestions: 0, acceptances: 0, days: 0 };
    }
    weeklyData[weekKey].activeUsers = Math.max(weeklyData[weekKey].activeUsers, point.activeUsers);
    weeklyData[weekKey].suggestions += point.suggestions;
    weeklyData[weekKey].acceptances += point.acceptances;
    weeklyData[weekKey].days += 1;
  });
  const weekly = Object.values(weeklyData).sort((a, b) => a.week.localeCompare(b.week));

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Trends</h2>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <DauWauChart data={trends} />
        <AcceptanceRateChart data={trends} />
      </div>
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Weekly Suggestions vs Acceptances</h3>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={weekly}>
            <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.3} />
            <XAxis dataKey="week" tick={{ fontSize: 12 }} stroke="#9ca3af" />
            <YAxis stroke="#9ca3af" />
            <Tooltip contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }} />
            <Legend />
            <Bar dataKey="suggestions" name="Suggestions" fill="#93c5fd" radius={[4, 4, 0, 0]} />
            <Bar dataKey="acceptances" name="Acceptances" fill="#3b82f6" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
