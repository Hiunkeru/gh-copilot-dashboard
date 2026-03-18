'use client';

import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { FeatureUsagePieChart } from '@/components/charts/FeatureUsagePieChart';
import { KpiCard } from '@/components/cards/KpiCard';
import { useMsalInstance } from '@/hooks/useMsalInstance';

export default function FeaturesPage() {
  const instance = useMsalInstance();

  const { data, isLoading } = useQuery({
    queryKey: ['features'],
    queryFn: () => api.getFeatures(instance),
  });

  if (isLoading || !data) {
    return <div className="animate-pulse h-96 bg-gray-200 dark:bg-gray-800 rounded-xl" />;
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Feature Usage</h2>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard title="Completions" value={`${data.completionsPercent}%`} subtitle={`${data.completionsUsers} users`} />
        <KpiCard title="Chat" value={`${data.chatPercent}%`} subtitle={`${data.chatUsers} users`} />
        <KpiCard title="Agent Mode" value={`${data.agentPercent}%`} subtitle={`${data.agentUsers} users`} />
        <KpiCard title="CLI" value={`${data.cliPercent}%`} subtitle={`${data.cliUsers} users`} />
      </div>

      <FeatureUsagePieChart data={data} />
    </div>
  );
}
