'use client';

import { useState } from 'react';
import { UserActivity } from '@/lib/types';
import { UserCategoryBadge } from '@/components/cards/UserCategoryBadge';

interface Props {
  users: UserActivity[];
  totalCount: number;
  page: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onSort: (field: string) => void;
  sortBy: string;
  sortDesc: boolean;
}

export function UserActivityTable({ users, totalCount, page, pageSize, onPageChange, onSort, sortBy, sortDesc }: Props) {
  const totalPages = Math.ceil(totalCount / pageSize);

  const SortHeader = ({ field, children }: { field: string; children: React.ReactNode }) => (
    <th
      className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:text-gray-700 dark:hover:text-gray-200"
      onClick={() => onSort(field)}
    >
      <div className="flex items-center gap-1">
        {children}
        {sortBy === field && <span>{sortDesc ? '↓' : '↑'}</span>}
      </div>
    </th>
  );

  return (
    <div className="bg-white dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead className="bg-gray-50 dark:bg-gray-800">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">User</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Org</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Team</th>
              <SortHeader field="lastActivity">Last Activity</SortHeader>
              <SortHeader field="activeDays">Active Days</SortHeader>
              <SortHeader field="suggestions">Suggestions</SortHeader>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Acceptances</th>
              <SortHeader field="acceptanceRate">Accept Rate</SortHeader>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">LOC Added</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Interactions</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Features</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Category</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
            {users.map((user) => (
              <tr key={user.userLogin} className="hover:bg-gray-50 dark:hover:bg-gray-800">
                <td className="px-4 py-3">
                  <div>
                    <a href={`/metrics/users/detail?login=${user.userLogin}`} className="text-sm font-medium text-blue-600 dark:text-blue-400 hover:underline">{user.userLogin}</a>
                    {user.displayName && <p className="text-xs text-gray-500 dark:text-gray-400">{user.displayName}</p>}
                  </div>
                </td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.organization || '-'}</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.team || '-'}</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.lastActivity || 'Never'}</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.activeDays}</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.totalSuggestions.toLocaleString()}</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.totalAcceptances.toLocaleString()}</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.acceptanceRate}%</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.locAdded.toLocaleString()}</td>
                <td className="px-4 py-3 text-sm text-gray-700 dark:text-gray-300">{user.interactionCount.toLocaleString()}</td>
                <td className="px-4 py-3">
                  <div className="flex gap-1">
                    {user.usedChat && <span className="text-xs bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300 px-1.5 py-0.5 rounded">Chat</span>}
                    {user.usedAgent && <span className="text-xs bg-purple-100 dark:bg-purple-900 text-purple-700 dark:text-purple-300 px-1.5 py-0.5 rounded">Agent</span>}
                    {user.usedCli && <span className="text-xs bg-orange-100 dark:bg-orange-900 text-orange-700 dark:text-orange-300 px-1.5 py-0.5 rounded">CLI</span>}
                  </div>
                </td>
                <td className="px-4 py-3"><UserCategoryBadge category={user.category} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {totalPages > 1 && (
        <div className="px-4 py-3 border-t border-gray-200 dark:border-gray-700 flex items-center justify-between">
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Showing {(page - 1) * pageSize + 1}-{Math.min(page * pageSize, totalCount)} of {totalCount}
          </p>
          <div className="flex gap-2">
            <button
              onClick={() => onPageChange(page - 1)}
              disabled={page <= 1}
              className="px-3 py-1 text-sm rounded border border-gray-300 dark:border-gray-600 disabled:opacity-50 hover:bg-gray-100 dark:hover:bg-gray-800"
            >
              Previous
            </button>
            <button
              onClick={() => onPageChange(page + 1)}
              disabled={page >= totalPages}
              className="px-3 py-1 text-sm rounded border border-gray-300 dark:border-gray-600 disabled:opacity-50 hover:bg-gray-100 dark:hover:bg-gray-800"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
