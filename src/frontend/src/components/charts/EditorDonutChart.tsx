'use client';

import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip, PieLabelRenderProps } from 'recharts';
import { DistributionItem } from '@/lib/types';
import { CHART_COLORS } from '@/lib/constants';

const renderLabel = (props: PieLabelRenderProps) => `${props.name ?? ''} ${((Number(props.percent) || 0) * 100).toFixed(0)}%`;

export function EditorDonutChart({ data }: { data: DistributionItem[] }) {
  return (
    <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Editor Distribution</h3>
      <ResponsiveContainer width="100%" height={300}>
        <PieChart>
          <Pie data={data} cx="50%" cy="50%" innerRadius={60} outerRadius={100} dataKey="userCount" nameKey="name" label={renderLabel}>
            {data.map((_, index) => (
              <Cell key={index} fill={CHART_COLORS[index % CHART_COLORS.length]} />
            ))}
          </Pie>
          <Tooltip contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }} />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
