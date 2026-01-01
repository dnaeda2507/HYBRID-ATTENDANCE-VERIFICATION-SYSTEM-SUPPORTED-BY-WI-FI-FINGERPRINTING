"use client";

import Link from "next/link";
import Image from "next/image";
import { usePathname } from "next/navigation";
import { useAppSelector } from "@/redux/hooks";

interface MenuItem {
  title: string;
  path: string;
  roles?: string[];
  disabled?: boolean;
}

const Sidebar = () => {
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
      roles: ["Teacher", "Academic Staff"],
    },
    { title: "Settings", path: "/settings" },
  ];

  const filteredItems = menuItems.filter((item) => {
    if (!item.roles) return true;
    return item.roles.some((r) => userRoles.includes(r));
  });

  return (
    <div className="fixed left-0 top-0 h-screen w-64 bg-white border-r border-gray-200 shadow-lg">
      <div className="p-6">
        <div className="flex items-center gap-3 mb-6">
          <Image
            src="/image/akdu_logo.png"
            alt="ATSYS Logo"
            width={60}
            height={60}
            className="object-contain"
          />
          <h1 className="text-xl font-bold text-gray-800">
            Student Attendance System
          </h1>
        </div>

        <nav className="space-y-2">
          {filteredItems?.map((item) =>
            item.disabled ? (
              <div
                key={item.path}
                className={`flex items-center px-4 py-3 text-base rounded-lg transition-colors ${
                  pathname === item.path
                    ? "bg-blue-50 text-blue-700 font-medium"
                    : "text-gray-700 hover:bg-gray-50 hover:text-blue-600"
                }`}
              >
                {item.title}
              </div>
            ) : (
              <Link
                key={item.path}
                href={item.path}
                className={`flex items-center px-4 py-3 text-base rounded-lg transition-colors ${
                  pathname === item.path
                    ? "bg-blue-50 text-blue-700 font-medium"
                    : "text-gray-700 hover:bg-gray-50 hover:text-blue-600"
                }`}
              >
                {item.title}
              </Link>
            )
          )}
        </nav>
      </div>
    </div>
  );
};

export default Sidebar;
