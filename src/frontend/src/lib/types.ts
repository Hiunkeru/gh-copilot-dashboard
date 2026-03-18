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
  locAdded: number;
  locSuggestedToAdd: number;
  interactionCount: number;
  usedChat: boolean;
  usedAgent: boolean;
  usedCli: boolean;
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
  locAdded: number;
  locSuggestedToAdd: number;
  interactionCount: number;
}

export interface UserDayDetail {
  date: string;
  isActive: boolean;
  interactionCount: number;
  codeGenerationCount: number;
  codeAcceptanceCount: number;
  locSuggestedToAdd: number;
  locAdded: number;
  locSuggestedToDelete: number;
  locDeleted: number;
  usedChat: boolean;
  usedAgent: boolean;
  usedCli: boolean;
  chatAgentModeCount: number;
  chatAskModeCount: number;
  chatEditModeCount: number;
  acceptanceRate: number;
  primaryEditor: string | null;
  primaryLanguage: string | null;
  features: FeatureBreakdown[];
  languages: LanguageBreakdown[];
}

export interface FeatureBreakdown {
  feature: string;
  codeGenerationCount: number;
  codeAcceptanceCount: number;
  locAdded: number;
}

export interface LanguageBreakdown {
  language: string;
  feature?: string;
  codeGenerationCount: number;
  codeAcceptanceCount: number;
  locSuggestedToAdd: number;
  locAdded: number;
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

export interface ReportSection {
  title: string;
  content: string;
}

export interface AdoptionReport {
  id: number;
  generatedAt: string;
  periodStart: string;
  periodEnd: string;
  executiveSummary: ReportSection;
  adoptionAnalysis: ReportSection;
  topPerformers: ReportSection;
  atRiskUsers: ReportSection;
  featureAdoption: ReportSection;
  trends: ReportSection;
  recommendations: ReportSection;
  roiAnalysis: ReportSection;
  fullReportMarkdown: string;
}

export interface ReportListItem {
  id: number;
  generatedAt: string;
  periodStart: string;
  periodEnd: string;
  totalSeats: number;
  activeUsers: number;
  adoptionRate: number;
  acceptanceRate: number;
  generatedBy: string;
}
