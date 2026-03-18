export interface AdoptionOverview {
  totalSeats: number;
  activeUsers: number;
  engagedUsers: number;
  adoptionRate: number;
  dailyActiveUsers: number;
  weeklyActiveUsers: number;
  wastedSeats: number;
  totalSuggestions: number;
  totalAcceptances: number;
  acceptanceRate: number;
  totalLinesAccepted: number;
  dataAsOf: string | null;
}

export interface UserActivity {
  userLogin: string;
  displayName: string | null;
  organization: string | null;
  team: string | null;
  lastActivity: string | null;
  activeDays: number;
  totalSuggestions: number;
  totalAcceptances: number;
  acceptanceRate: number;
  usesChat: boolean;
  usesAgent: boolean;
  usesCli: boolean;
  category: 'PowerUser' | 'Occasional' | 'Inactive' | 'NeverUsed';
}

export interface UserActivityPage {
  users: UserActivity[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface FeatureUsage {
  completionsUsers: number;
  chatUsers: number;
  agentUsers: number;
  cliUsers: number;
  totalUsers: number;
  completionsPercent: number;
  chatPercent: number;
  agentPercent: number;
  cliPercent: number;
}

export interface DistributionItem {
  name: string;
  suggestions: number;
  acceptances: number;
  linesAccepted: number;
  userCount: number;
}

export interface TrendPoint {
  date: string;
  activeUsers: number;
  engagedUsers: number;
  suggestions: number;
  acceptances: number;
  acceptanceRate: number;
}

export interface Roi {
  totalLinesAccepted: number;
  totalLinesAcceptedLast7Days: number;
  avgLinesPerActiveUserPerDay: number;
  costPerActiveUser: number;
  licenseCostPerMonth: number;
  activeUsers: number;
  totalSeats: number;
}
