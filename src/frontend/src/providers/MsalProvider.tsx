'use client';

import { MsalProvider as MsalReactProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';
import { msalConfig } from '@/lib/msalConfig';
import { ReactNode, useMemo } from 'react';

const DEV_MODE = process.env.NEXT_PUBLIC_DEV_MODE === 'true';

export function MsalProvider({ children }: { children: ReactNode }) {
  const msalInstance = useMemo(() => {
    if (DEV_MODE) return null;
    return new PublicClientApplication(msalConfig);
  }, []);

  if (DEV_MODE || !msalInstance) {
    return <>{children}</>;
  }

  return (
    <MsalReactProvider instance={msalInstance}>
      {children}
    </MsalReactProvider>
  );
}
