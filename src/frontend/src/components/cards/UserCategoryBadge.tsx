import { CATEGORY_COLORS } from '@/lib/constants';

export function UserCategoryBadge({ category }: { category: string }) {
  const config = CATEGORY_COLORS[category] || CATEGORY_COLORS.NeverUsed;

  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.bg} ${config.text}`}>
      {config.label}
    </span>
  );
}
