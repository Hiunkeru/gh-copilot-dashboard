import { UserDetailContent } from '@/components/pages/UserDetailContent';

export function generateStaticParams() {
  // Return a placeholder param to satisfy output: 'export' requirement
  // Azure SWA fallback routing will handle actual dynamic paths
  return [{ login: '_placeholder' }];
}

export default function UserDetailPage() {
  return <UserDetailContent />;
}
