'use client';

import { useSearchParams } from 'next/navigation';
import { Suspense } from 'react';
import { UserDetailContent } from '@/components/pages/UserDetailContent';

function UserDetailInner() {
  const searchParams = useSearchParams();
  const login = searchParams.get('login');

  if (!login) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500 dark:text-gray-400">No user specified.</p>
        <a href="/metrics/users" className="text-blue-600 dark:text-blue-400 hover:underline mt-2 inline-block">
          Back to Users
        </a>
      </div>
    );
  }

  return <UserDetailContent login={login} />;
}

export default function UserDetailPage() {
  return (
    <Suspense fallback={<div className="animate-pulse h-96 bg-gray-200 dark:bg-gray-800 rounded-xl" />}>
      <UserDetailInner />
    </Suspense>
  );
}
