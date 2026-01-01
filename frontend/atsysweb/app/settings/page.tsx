import SettingsPanel from "@/components/settings/SettingsPanel";
import React from "react";

export default function SettingsPage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Settings</h1>
      <SettingsPanel />
    </div>
  );
}
