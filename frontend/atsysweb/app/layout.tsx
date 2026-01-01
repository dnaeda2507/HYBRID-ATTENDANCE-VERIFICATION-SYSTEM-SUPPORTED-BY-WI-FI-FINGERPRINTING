import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import Sidebar from "@/components/sidebar_component/sidebar_component";
import Header from "@/components/header_component";
import Footer from "@/components/footer_component";
import AuthProvider from "@/components/AuthProvider";
import Providers from "../redux/Provider";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Attendance System",
  description: "AKDU CSE Attendance System",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <Providers>
          <AuthProvider>
            <div className="flex min-h-screen bg-gray-50">
              {/* Sidebar - only on desktop */}
              <div className="hidden md:block md:w-64 md:fixed md:h-full">
                <Sidebar />
              </div>

              {/* Main content */}
              <div className="flex-1 md:ml-64">
                {/* Header - only on mobile */}
                <div className="block md:hidden">
                  <Header />
                </div>

                {/* Main content area */}
                <main className="p-4 mt-16 md:mt-0 pb-16">{children}</main>

                <Footer />
              </div>
            </div>
          </AuthProvider>
        </Providers>
      </body>
    </html>
  );
}
