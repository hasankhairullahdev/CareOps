import type { Metadata } from "next";
import "../globals.css";
import Providers from "@/components/Providers";
import Sidebar from "@/components/Sidebar";

export const metadata: Metadata = { title: "CareOps" };

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="flex h-screen overflow-hidden bg-gray-50">
        <Providers>
          <Sidebar />
          <main className="flex-1 overflow-auto p-8">{children}</main>
        </Providers>
      </body>
    </html>
  );
}
