'use client';

import { useQuery } from '@tanstack/react-query';
import { useMsal } from '@azure/msal-react';
import { api } from '@/lib/api';
import { KpiCard } from '@/components/cards/KpiCard';
import { DauWauChart } from '@/components/charts/DauWauChart';
import { AcceptanceRateChart } from '@/components/charts/AcceptanceRateChart';

export default function HomePage() {
  const { instance } = useMsal();

  const { data: overview, isLoading: overviewLoading } = useQuery({
    queryKey: ['overview'],
    queryFn: () => api.getOverview(instance),
  });

  const { data: trends, isLoading: trendsLoading } = useQuery({
    queryKey: ['trends'],
    queryFn: () => api.getTrends(instance),
  });

  if (overviewLoading || trendsLoading) {
    return <div className="animate-pulse space-y-4"><div className="h-32 bg-gray-200 dark:bg-gray-800 rounded-xl" /><div className="h-80 bg-gray-200 dark:bg-gray-800 rounded-xl" /></div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Adoption Overview</h2>
        {overview?.dataAsOf && (
          <span className="text-sm text-gray-500 dark:text-gray-400">Data as of {overview.dataAsOf}</span>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard title="Adoption Rate" value={`${overview?.adoptionRate?.toFixed(1) ?? 0}%`} subtitle={`${overview?.activeUsers ?? 0} of ${overview?.totalSeats ?? 0} seats`} />
        <KpiCard title="Daily Active Users" value={overview?.dailyActiveUsers ?? 0} />
        <KpiCard title="Weekly Active Users" value={overview?.weeklyActiveUsers ?? 0} />
        <KpiCard title="Wasted Seats" value={overview?.wastedSeats ?? 0} subtitle="Inactive in last 28 days" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <KpiCard title="Acceptance Rate" value={`${overview?.acceptanceRate ?? 0}%`} />
        <KpiCard title="Total Suggestions" value={(overview?.totalSuggestions ?? 0).toLocaleString()} subtitle="Last 28 days" />
        <KpiCard title="Lines Accepted" value={(overview?.totalLinesAccepted ?? 0).toLocaleString()} subtitle="Last 28 days" />
      </div>

      {trends && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <DauWauChart data={trends} />
          <AcceptanceRateChart data={trends} />
        </div>
      )}
    </div>
  );
}
