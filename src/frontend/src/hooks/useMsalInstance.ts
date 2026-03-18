'use client';

import { IPublicClientApplication } from '@azure/msal-browser';

const DEV_MODE = process.env.NEXT_PUBLIC_DEV_MODE === 'true';

export function useMsalInstance(): IPublicClientApplication | null {
  if (DEV_MODE) return null;

  // Only import useMsal when not in dev mode
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const { useMsal } = require('@azure/msal-react');
  const { instance } = useMsal();
  return instance;
}
