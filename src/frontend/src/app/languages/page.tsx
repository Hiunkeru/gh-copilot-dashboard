'use client';

import { useQuery } from '@tanstack/react-query';
import { useMsal } from '@azure/msal-react';
import { api } from '@/lib/api';
import { LanguageBarChart } from '@/components/charts/LanguageBarChart';
import { EditorDonutChart } from '@/components/charts/EditorDonutChart';

export default function LanguagesPage() {
  const { instance } = useMsal();

  const { data: languages, isLoading: langLoading } = useQuery({
    queryKey: ['languages'],
    queryFn: () => api.getLanguages(instance),
  });

  const { data: editors, isLoading: editorLoading } = useQuery({
    queryKey: ['editors'],
    queryFn: () => api.getEditors(instance),
  });

  if (langLoading || editorLoading) {
    return <div className="animate-pulse space-y-4"><div className="h-96 bg-gray-200 dark:bg-gray-800 rounded-xl" /></div>;
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Languages & Editors</h2>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {languages && <LanguageBarChart data={languages} title="Top Languages by Acceptances" />}
        {editors && <EditorDonutChart data={editors} />}
      </div>
    </div>
  );
}
