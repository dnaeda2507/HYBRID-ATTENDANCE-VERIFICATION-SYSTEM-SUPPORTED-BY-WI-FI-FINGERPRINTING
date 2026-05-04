'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import Image from 'next/image';
import { useAppSelector } from "@/redux/hooks";

interface MenuItem {
  title: string;
  path: string;
  roles?: string[];
  disabled?: boolean;
}

export default function Header() {
  const [isOpen, setIsOpen] = useState(false);
  const pathname = usePathname();
  const userRoles = useAppSelector((state) => state.auth.roles) ?? [];

  const menuItems: MenuItem[] = [
    { title: "Home", path: "/" },
    { title: "Users", path: "/users", roles: ["ItStaff"] },
    {
      title: "Lectures",
      path: "/lectures",
      roles: ["ItStaff", "Teacher", "AcademicStaff"],
    },
    {
      title: "Courses",
      path: "/courses",
      roles: ["ItStaff", "Teacher", "AcademicStaff"],
    },
    {
      title: "Attendance",
      path: "/attendance",
      roles: ["ItStaff", "Teacher", "AcademicStaff"],
    },
    {
      title: "Past Sessions",
      path: "/attendance/past-sessions",
      roles: ["ItStaff", "Teacher", "AcademicStaff"],
    },
    { title: "Settings", path: "/settings" },
  ];

  const filteredItems = menuItems.filter((item) => {
    if (!item.roles) return true;
    return item.roles.some((r) => userRoles.includes(r));
  });

  // Close menu when route changes
  useEffect(() => {
    setIsOpen(false);
  }, [pathname]);

  return (
    <header className="fixed top-0 left-0 right-0 bg-white shadow-md z-50 md:hidden">
      <div className="px-4 py-3 flex items-center justify-between">
        <Link href="/" className="flex items-center gap-3">
          <Image
            src="/image/akdu_logo.png"
            alt="AKDU Logo"
            width={32}
            height={32}
            className="w-8 h-8 object-contain"
          />
          <span className="text-xl font-semibold">Student Atsys</span>
        </Link>
        
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="p-2 hover:bg-gray-100 rounded-lg"
          aria-label="Toggle menu"
        >
          <svg
            className="w-6 h-6"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            {isOpen ? (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            ) : (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 6h16M4 12h16M4 18h16"
              />
            )}
          </svg>
        </button>
      </div>

      {/* Mobile menu */}
      {isOpen && (
        <nav className="border-t border-gray-200">
          {filteredItems.map((item) => (
            <Link
              key={item.path}
              href={item.path}
              className={`block px-4 py-3 ${
                pathname === item.path 
                  ? 'bg-blue-50 text-blue-700 font-medium'
                  : 'text-gray-700 hover:bg-gray-50'
              }`}
              onClick={() => setIsOpen(false)}
            >
              {item.title}
            </Link>
          ))}
        </nav>
      )}
    </header>
  );
}