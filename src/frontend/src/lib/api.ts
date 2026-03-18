import { IPublicClientApplication } from '@azure/msal-browser';
import { loginRequest } from './msalConfig';
import type { AdoptionOverview, UserActivityPage, FeatureUsage, DistributionItem, TrendPoint, Roi, UserDayDetail, AdoptionReport } from './types';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5135';
const DEV_MODE = process.env.NEXT_PUBLIC_DEV_MODE === 'true';

async function getToken(msalInstance: IPublicClientApplication | null): Promise<string | null> {
  if (DEV_MODE || !msalInstance) return null;

  const accounts = msalInstance.getAllAccounts();
  if (accounts.length === 0) throw new Error('No accounts found');

  try {
    const response = await msalInstance.acquireTokenSilent({
      ...loginRequest,
      account: accounts[0],
    });
    return response.accessToken;
  } catch {
    const response = await msalInstance.acquireTokenPopup(loginRequest);
    return response.accessToken;
  }
}

async function fetchApi<T>(path: string, msalInstance: IPublicClientApplication | null): Promise<T> {
  const token = await getToken(msalInstance);
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_URL}${path}`, { headers });
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
}

export const api = {
  getOverview: (msal: IPublicClientApplication | null) => fetchApi<AdoptionOverview>('/api/dashboard/overview', msal),
  getTrends: (msal: IPublicClientApplication | null, days = 28) => fetchApi<TrendPoint[]>(`/api/dashboard/trends?days=${days}`, msal),
  getFeatures: (msal: IPublicClientApplication | null, days = 28) => fetchApi<FeatureUsage>(`/api/dashboard/features?days=${days}`, msal),
  getLanguages: (msal: IPublicClientApplication | null, days = 28) => fetchApi<DistributionItem[]>(`/api/dashboard/languages?days=${days}`, msal),
  getEditors: (msal: IPublicClientApplication | null, days = 28) => fetchApi<DistributionItem[]>(`/api/dashboard/editors?days=${days}`, msal),
  getRoi: (msal: IPublicClientApplication | null) => fetchApi<Roi>('/api/dashboard/roi', msal),
  getUsers: (msal: IPublicClientApplication | null, params: Record<string, string>) => {
    const qs = new URLSearchParams(params).toString();
    return fetchApi<UserActivityPage>(`/api/users?${qs}`, msal);
  },
  getUserHistory: (msal: IPublicClientApplication | null, login: string) => fetchApi<UserDayDetail[]>(`/api/users/${login}/history`, msal),
  triggerSync: async (msal: IPublicClientApplication | null) => {
    const token = await getToken(msal);
    const headers: Record<string, string> = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const res = await fetch(`${API_URL}/api/sync/trigger`, { method: 'POST', headers });
    if (!res.ok) throw new Error(`Sync failed: ${res.status}`);
    return res.json();
  },
  generateReport: async (msal: IPublicClientApplication | null): Promise<AdoptionReport> => {
    const token = await getToken(msal);
    const headers: Record<string, string> = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const res = await fetch(`${API_URL}/api/reports/generate`, { method: 'POST', headers });
    if (!res.ok) throw new Error(`Report generation failed: ${res.status}`);
    return res.json();
  },
};
