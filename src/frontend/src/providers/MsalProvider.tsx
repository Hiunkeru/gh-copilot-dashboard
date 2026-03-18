'use client';

import { MsalProvider as MsalReactProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';
import { msalConfig } from '@/lib/msalConfig';
import { ReactNode, useMemo } from 'react';

export function MsalProvider({ children }: { children: ReactNode }) {
  const msalInstance = useMemo(() => new PublicClientApplication(msalConfig), []);

  return (
    <MsalReactProvider instance={msalInstance}>
      {children}
    </MsalReactProvider>
  );
}
