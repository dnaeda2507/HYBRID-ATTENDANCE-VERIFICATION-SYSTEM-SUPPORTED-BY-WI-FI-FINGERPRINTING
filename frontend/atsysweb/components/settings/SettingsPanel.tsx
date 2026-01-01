"use client";
import React from "react";
import { Tab, TabGroup, TabList, TabPanel, TabPanels } from "@headlessui/react";
import UserInformations from "./UserInformations";
import ChangePassword from "./ChangePassword";
import { logout } from "@/app/utils/auth";

const SettingsPanel = () => {
  return (
    <TabGroup vertical>
      <div className="flex">
        <TabList className="space-y-2 w-48 bg-gray-100 p-2 ">
          {["User Informations", "Change Password"].map((label) => (
            <Tab
              key={label}
              className={({ selected }) =>
                selected
                  ? "block text-left w-full px-4 py-2 focus:outline-none focus:ring-0 bg-white text-blue-500 font-medium"
                  : "block text-left w-full px-4 py-2 focus:outline-none focus:ring-0 text-gray-700 hover:bg-gray-200"
              }
            >
              {label}
            </Tab>
          ))}
          <button
            type="button"
            onClick={() => logout()}
            className="block text-left w-full px-4 py-2 focus:outline-none focus:ring-0 cursor-pointer"
          >
            <span className="text-red-500 hover:text-red-700">Logout</span>
          </button>
        </TabList>

        <TabPanels className="flex-1 p-4 bg-white shadow ml-4">
          <TabPanel>
            <UserInformations />
          </TabPanel>
          <TabPanel>
            <ChangePassword />
          </TabPanel>
        </TabPanels>
      </div>
    </TabGroup>
  );
};

export default SettingsPanel;
