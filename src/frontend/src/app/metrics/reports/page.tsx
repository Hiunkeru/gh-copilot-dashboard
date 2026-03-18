'use client';

import { useState, useEffect, useCallback } from 'react';
import { useMsalInstance } from '@/hooks/useMsalInstance';
import { api } from '@/lib/api';
import type { AdoptionReport, ReportListItem } from '@/lib/types';

const sectionOrder = [
  { key: 'executiveSummary', icon: '📋' },
  { key: 'adoptionAnalysis', icon: '📊' },
  { key: 'topPerformers', icon: '🏆' },
  { key: 'atRiskUsers', icon: '⚠️' },
  { key: 'featureAdoption', icon: '⚡' },
  { key: 'trends', icon: '📈' },
  { key: 'recommendations', icon: '✅' },
  { key: 'roiAnalysis', icon: '💰' },
] as const;

export default function ReportsPage() {
  const instance = useMsalInstance();
  const [reports, setReports] = useState<ReportListItem[]>([]);
  const [selectedReport, setSelectedReport] = useState<AdoptionReport | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadingList, setLoadingList] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadReports = useCallback(async () => {
    try {
      const data = await api.getReports(instance);
      setReports(data);
    } catch {
      // Ignore - empty list
    } finally {
      setLoadingList(false);
    }
  }, [instance]);

  useEffect(() => { loadReports(); }, [loadReports]);

  const handleGenerate = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.generateReport(instance);
      setSelectedReport(data);
      await loadReports(); // Refresh list
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error generating report. Check AI Foundry configuration.');
    } finally {
      setLoading(false);
    }
  };

  const handleViewReport = async (id: number) => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.getReport(instance, id);
      setSelectedReport(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error loading report');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">AI Reports</h2>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            AI-powered adoption analysis via Azure AI Foundry
          </p>
        </div>
        <button
          onClick={handleGenerate}
          disabled={loading}
          className="px-6 py-3 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
        >
          {loading ? (
            <>
              <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
              Generating...
            </>
          ) : (
            'Generate New Report'
          )}
        </button>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl p-4">
          <p className="text-red-800 dark:text-red-200 text-sm">{error}</p>
        </div>
      )}

      {/* Report list */}
      {!selectedReport && (
        <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
          <div className="p-4 border-b border-gray-200 dark:border-gray-700">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white">Report History</h3>
          </div>
          {loadingList ? (
            <div className="p-8 text-center text-gray-400">Loading...</div>
          ) : reports.length === 0 ? (
            <div className="p-12 text-center">
              <div className="text-4xl mb-4">🤖</div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white">No reports yet</h3>
              <p className="text-gray-500 dark:text-gray-400 mt-2">
                Click &quot;Generate New Report&quot; to create your first AI adoption analysis.
              </p>
            </div>
          ) : (
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Date</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Period</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Seats</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Active</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Adoption</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Acceptance</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                {reports.map((r) => (
                  <tr key={r.id} className="hover:bg-gray-50 dark:hover:bg-gray-800">
                    <td className="px-4 py-3 text-sm text-gray-900 dark:text-white">{r.generatedAt}</td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{r.periodStart} — {r.periodEnd}</td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{r.totalSeats}</td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{r.activeUsers}</td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{r.adoptionRate.toFixed(1)}%</td>
                    <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{r.acceptanceRate.toFixed(1)}%</td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => handleViewReport(r.id)}
                        className="text-sm text-blue-600 dark:text-blue-400 hover:underline"
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Selected report detail */}
      {selectedReport && (
        <div className="space-y-6">
          <button
            onClick={() => setSelectedReport(null)}
            className="text-sm text-blue-600 dark:text-blue-400 hover:underline"
          >
            &larr; Back to report list
          </button>

          <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-xl p-4 flex items-center justify-between">
            <div>
              <p className="text-sm text-blue-800 dark:text-blue-200">
                Report generated: {selectedReport.generatedAt}
              </p>
              <p className="text-sm text-blue-600 dark:text-blue-300">
                Period: {selectedReport.periodStart} to {selectedReport.periodEnd}
              </p>
            </div>
          </div>

          {sectionOrder.map(({ key, icon }) => {
            const section = selectedReport[key];
            if (!section?.content) return null;
            return (
              <div key={key} className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                  <span>{icon}</span>
                  {section.title}
                </h3>
                <div
                  className="prose dark:prose-invert prose-sm max-w-none text-gray-700 dark:text-gray-300"
                  dangerouslySetInnerHTML={{ __html: markdownToHtml(section.content) }}
                />
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

function markdownToHtml(md: string): string {
  return md
    .replace(/\*\*\[([^\]]+)\]\*\*/g, '<strong class="text-blue-600 dark:text-blue-400">[$1]</strong>')
    .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
    .replace(/^### (.+)$/gm, '<h4 class="font-semibold mt-4 mb-2">$1</h4>')
    .replace(/^- (.+)$/gm, '<li class="ml-4">$1</li>')
    .replace(/^(\d+)\. (.+)$/gm, '<li class="ml-4 list-decimal">$2</li>')
    .replace(/\n{2,}/g, '</p><p class="mt-2">')
    .replace(/\n/g, '<br/>');
}
