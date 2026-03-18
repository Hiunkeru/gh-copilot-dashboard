import { IPublicClientApplication } from '@azure/msal-browser';
import { loginRequest } from './msalConfig';
import type { AdoptionOverview, UserActivityPage, FeatureUsage, DistributionItem, TrendPoint, Roi } from './types';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

async function getToken(msalInstance: IPublicClientApplication): Promise<string> {
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

async function fetchApi<T>(path: string, msalInstance: IPublicClientApplication): Promise<T> {
  const token = await getToken(msalInstance);
  const res = await fetch(`${API_URL}${path}`, {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });
  if (!res.ok) throw new Error(`API error: ${res.status}`);
  return res.json();
}

export const api = {
  getOverview: (msal: IPublicClientApplication) => fetchApi<AdoptionOverview>('/api/dashboard/overview', msal),
  getTrends: (msal: IPublicClientApplication, days = 28) => fetchApi<TrendPoint[]>(`/api/dashboard/trends?days=${days}`, msal),
  getFeatures: (msal: IPublicClientApplication, days = 28) => fetchApi<FeatureUsage>(`/api/dashboard/features?days=${days}`, msal),
  getLanguages: (msal: IPublicClientApplication, days = 28) => fetchApi<DistributionItem[]>(`/api/dashboard/languages?days=${days}`, msal),
  getEditors: (msal: IPublicClientApplication, days = 28) => fetchApi<DistributionItem[]>(`/api/dashboard/editors?days=${days}`, msal),
  getRoi: (msal: IPublicClientApplication) => fetchApi<Roi>('/api/dashboard/roi', msal),
  getUsers: (msal: IPublicClientApplication, params: Record<string, string>) => {
    const qs = new URLSearchParams(params).toString();
    return fetchApi<UserActivityPage>(`/api/users?${qs}`, msal);
  },
  getUserHistory: (msal: IPublicClientApplication, login: string) => fetchApi<TrendPoint[]>(`/api/users/${login}/history`, msal),
  triggerSync: async (msal: IPublicClientApplication) => {
    const token = await getToken(msal);
    const res = await fetch(`${API_URL}/api/sync/trigger`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) throw new Error(`Sync failed: ${res.status}`);
    return res.json();
  },
};
