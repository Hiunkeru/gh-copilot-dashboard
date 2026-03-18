'use client';

import { useParams } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useMsalInstance } from '@/hooks/useMsalInstance';
import { KpiCard } from '@/components/cards/KpiCard';
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

export function UserDetailContent() {
  const params = useParams();
  const login = params.login as string;
  const instance = useMsalInstance();

  const { data: history, isLoading } = useQuery({
    queryKey: ['userHistory', login],
    queryFn: () => api.getUserHistory(instance, login),
  });

  if (isLoading || !history) {
    return <div className="animate-pulse space-y-4"><div className="h-32 bg-gray-200 dark:bg-gray-800 rounded-xl" /><div className="h-80 bg-gray-200 dark:bg-gray-800 rounded-xl" /></div>;
  }

  const activeDays = history.filter(d => d.isActive).length;
  const totalGenerated = history.reduce((sum, d) => sum + d.codeGenerationCount, 0);
  const totalAccepted = history.reduce((sum, d) => sum + d.codeAcceptanceCount, 0);
  const totalLocAdded = history.reduce((sum, d) => sum + d.locAdded, 0);
  const totalLocSuggested = history.reduce((sum, d) => sum + d.locSuggestedToAdd, 0);
  const totalInteractions = history.reduce((sum, d) => sum + d.interactionCount, 0);
  const avgAcceptanceRate = totalGenerated > 0 ? Math.round(totalAccepted / totalGenerated * 100 * 10) / 10 : 0;
  const chatDays = history.filter(d => d.usedChat).length;
  const agentDays = history.filter(d => d.usedAgent).length;
  const cliDays = history.filter(d => d.usedCli).length;

  // Aggregate languages across all days
  const langMap = new Map<string, { gen: number; acc: number; loc: number }>();
  history.forEach(d => d.languages.forEach(l => {
    const existing = langMap.get(l.language) || { gen: 0, acc: 0, loc: 0 };
    langMap.set(l.language, { gen: existing.gen + l.codeGenerationCount, acc: existing.acc + l.codeAcceptanceCount, loc: existing.loc + l.locAdded });
  }));
  const langData = Array.from(langMap.entries()).map(([name, v]) => ({ name, ...v })).sort((a, b) => b.gen - a.gen).slice(0, 10);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <a href="/metrics/users" className="text-blue-600 dark:text-blue-400 hover:underline text-sm">&larr; Back to Users</a>
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white">{login}</h2>
      </div>

      {/* KPI Summary */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <KpiCard title="Active Days" value={`${activeDays}/28`} />
        <KpiCard title="Code Generated" value={totalGenerated.toLocaleString()} />
        <KpiCard title="Code Accepted" value={totalAccepted.toLocaleString()} />
        <KpiCard title="Acceptance Rate" value={`${avgAcceptanceRate}%`} />
        <KpiCard title="LOC Added" value={totalLocAdded.toLocaleString()} subtitle={`${totalLocSuggested.toLocaleString()} suggested`} />
        <KpiCard title="Interactions" value={totalInteractions.toLocaleString()} />
      </div>

      {/* Feature usage summary */}
      <div className="grid grid-cols-3 gap-4">
        <KpiCard title="Chat" value={`${chatDays} days`} subtitle={chatDays > 0 ? 'Used' : 'Not used'} />
        <KpiCard title="Agent" value={`${agentDays} days`} subtitle={agentDays > 0 ? 'Used' : 'Not used'} />
        <KpiCard title="CLI" value={`${cliDays} days`} subtitle={cliDays > 0 ? 'Used' : 'Not used'} />
      </div>

      {/* Daily activity chart */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Daily Code Activity</h3>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={history}>
            <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.3} />
            <XAxis dataKey="date" tick={{ fontSize: 10 }} stroke="#9ca3af" />
            <YAxis stroke="#9ca3af" />
            <Tooltip contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }} />
            <Legend />
            <Bar dataKey="codeGenerationCount" name="Generated" fill="#93c5fd" radius={[2, 2, 0, 0]} />
            <Bar dataKey="codeAcceptanceCount" name="Accepted" fill="#3b82f6" radius={[2, 2, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* LOC chart */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Lines of Code</h3>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={history}>
            <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.3} />
            <XAxis dataKey="date" tick={{ fontSize: 10 }} stroke="#9ca3af" />
            <YAxis stroke="#9ca3af" />
            <Tooltip contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }} />
            <Legend />
            <Bar dataKey="locSuggestedToAdd" name="Suggested" fill="#d8b4fe" radius={[2, 2, 0, 0]} />
            <Bar dataKey="locAdded" name="Added" fill="#8b5cf6" radius={[2, 2, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* Acceptance rate trend */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Acceptance Rate Trend</h3>
        <ResponsiveContainer width="100%" height={250}>
          <LineChart data={history.filter(d => d.isActive)}>
            <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.3} />
            <XAxis dataKey="date" tick={{ fontSize: 10 }} stroke="#9ca3af" />
            <YAxis domain={[0, 100]} stroke="#9ca3af" />
            <Tooltip contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }} />
            <Line type="monotone" dataKey="acceptanceRate" name="Acceptance %" stroke="#10b981" strokeWidth={2} dot={{ r: 3 }} />
          </LineChart>
        </ResponsiveContainer>
      </div>

      {/* Language breakdown */}
      {langData.length > 0 && (
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Top Languages (28 days)</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={langData} layout="vertical" margin={{ left: 80 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" opacity={0.3} />
              <XAxis type="number" stroke="#9ca3af" />
              <YAxis type="category" dataKey="name" tick={{ fontSize: 12 }} stroke="#9ca3af" width={80} />
              <Tooltip contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }} />
              <Legend />
              <Bar dataKey="gen" name="Generated" fill="#93c5fd" radius={[0, 4, 4, 0]} />
              <Bar dataKey="acc" name="Accepted" fill="#3b82f6" radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Daily detail table */}
      <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white p-6 pb-0">Daily Breakdown</h3>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700 mt-4">
            <thead className="bg-gray-50 dark:bg-gray-800">
              <tr>
                {['Date', 'Active', 'Generated', 'Accepted', 'Accept %', 'LOC +', 'LOC suggested', 'Interactions', 'Chat', 'Agent', 'CLI', 'Editor', 'Language'].map(h => (
                  <th key={h} className="px-3 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
              {history.map((d) => (
                <tr key={d.date} className={`${d.isActive ? '' : 'opacity-40'} hover:bg-gray-50 dark:hover:bg-gray-800`}>
                  <td className="px-3 py-2 text-sm text-gray-900 dark:text-white whitespace-nowrap">{d.date}</td>
                  <td className="px-3 py-2 text-sm">{d.isActive ? <span className="text-green-600">Yes</span> : <span className="text-gray-400">No</span>}</td>
                  <td className="px-3 py-2 text-sm text-gray-700 dark:text-gray-300">{d.codeGenerationCount}</td>
                  <td className="px-3 py-2 text-sm text-gray-700 dark:text-gray-300">{d.codeAcceptanceCount}</td>
                  <td className="px-3 py-2 text-sm text-gray-700 dark:text-gray-300">{d.acceptanceRate}%</td>
                  <td className="px-3 py-2 text-sm text-gray-700 dark:text-gray-300">{d.locAdded}</td>
                  <td className="px-3 py-2 text-sm text-gray-700 dark:text-gray-300">{d.locSuggestedToAdd}</td>
                  <td className="px-3 py-2 text-sm text-gray-700 dark:text-gray-300">{d.interactionCount}</td>
                  <td className="px-3 py-2 text-sm">{d.usedChat ? '\u2713' : ''}</td>
                  <td className="px-3 py-2 text-sm">{d.usedAgent ? '\u2713' : ''}</td>
                  <td className="px-3 py-2 text-sm">{d.usedCli ? '\u2713' : ''}</td>
                  <td className="px-3 py-2 text-sm text-gray-500 dark:text-gray-400">{d.primaryEditor}</td>
                  <td className="px-3 py-2 text-sm text-gray-500 dark:text-gray-400">{d.primaryLanguage}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
