'use client';

import { useQuery } from '@tanstack/react-query';
import { useMsal } from '@azure/msal-react';
import { api } from '@/lib/api';
import { KpiCard } from '@/components/cards/KpiCard';

export default function RoiPage() {
  const { instance } = useMsal();

  const { data, isLoading } = useQuery({
    queryKey: ['roi'],
    queryFn: () => api.getRoi(instance),
  });

  if (isLoading || !data) {
    return <div className="animate-pulse space-y-4"><div className="h-32 bg-gray-200 dark:bg-gray-800 rounded-xl" /></div>;
  }

  const monthlyCost = data.totalSeats * data.licenseCostPerMonth;
  const wastedCost = (data.totalSeats - data.activeUsers) * data.licenseCostPerMonth;

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Return on Investment</h2>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <KpiCard title="Total Lines Accepted" value={data.totalLinesAccepted.toLocaleString()} subtitle="Last 28 days" />
        <KpiCard title="Lines Accepted (7d)" value={data.totalLinesAcceptedLast7Days.toLocaleString()} subtitle="Last 7 days" />
        <KpiCard title="Avg Lines / User / Day" value={data.avgLinesPerActiveUserPerDay} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <KpiCard title="Cost per Active User" value={`$${data.costPerActiveUser}`} subtitle={`License: $${data.licenseCostPerMonth}/seat/month`} />
        <KpiCard title="Monthly License Cost" value={`$${monthlyCost.toLocaleString()}`} subtitle={`${data.totalSeats} seats`} />
        <KpiCard title="Wasted Spend" value={`$${wastedCost.toLocaleString()}`} subtitle={`${data.totalSeats - data.activeUsers} unused seats`} />
      </div>
    </div>
  );
}
