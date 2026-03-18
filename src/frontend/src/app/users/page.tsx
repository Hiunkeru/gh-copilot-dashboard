'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useMsal } from '@azure/msal-react';
import { api } from '@/lib/api';
import { UserActivityTable } from '@/components/tables/UserActivityTable';

export default function UsersPage() {
  const { instance } = useMsal();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [sortBy, setSortBy] = useState('activeDays');
  const [sortDesc, setSortDesc] = useState(true);

  const { data, isLoading } = useQuery({
    queryKey: ['users', page, search, category, sortBy, sortDesc],
    queryFn: () => api.getUsers(instance, {
      page: String(page),
      pageSize: '20',
      ...(search && { search }),
      ...(category && { category }),
      sortBy,
      sortDesc: String(sortDesc),
    }),
  });

  const handleSort = (field: string) => {
    if (sortBy === field) {
      setSortDesc(!sortDesc);
    } else {
      setSortBy(field);
      setSortDesc(true);
    }
    setPage(1);
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white">User Activity</h2>

      <div className="flex flex-wrap gap-4">
        <input
          type="text"
          placeholder="Search users..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:ring-2 focus:ring-blue-500"
        />
        <select
          value={category}
          onChange={(e) => { setCategory(e.target.value); setPage(1); }}
          className="px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm"
        >
          <option value="">All Categories</option>
          <option value="PowerUser">Power User</option>
          <option value="Occasional">Occasional</option>
          <option value="Inactive">Inactive</option>
          <option value="NeverUsed">Never Used</option>
        </select>
      </div>

      {isLoading ? (
        <div className="animate-pulse h-96 bg-gray-200 dark:bg-gray-800 rounded-xl" />
      ) : data ? (
        <UserActivityTable
          users={data.users}
          totalCount={data.totalCount}
          page={data.page}
          pageSize={data.pageSize}
          onPageChange={setPage}
          onSort={handleSort}
          sortBy={sortBy}
          sortDesc={sortDesc}
        />
      ) : null}
    </div>
  );
}
