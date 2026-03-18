import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';
import { ThemeProvider } from '@/providers/ThemeProvider';
import { MsalProvider } from '@/providers/MsalProvider';
import { QueryProvider } from '@/providers/QueryProvider';
import { Sidebar } from '@/components/layout/Sidebar';
import { Header } from '@/components/layout/Header';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'GH Dashboard',
  description: 'GH engineering tools and analytics dashboard',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.className} bg-gray-50 dark:bg-gray-950`}>
        <ThemeProvider>
          <MsalProvider>
            <QueryProvider>
              <div className="flex min-h-screen">
                <Sidebar />
                <div className="flex-1 flex flex-col">
                  <Header />
                  <main className="flex-1 p-6">{children}</main>
                </div>
              </div>
            </QueryProvider>
          </MsalProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
