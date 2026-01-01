import { AttendedUserDto } from "@/redux/generatedTypes";
import {
  Button,
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
} from "@headlessui/react";
import React from "react";

interface SessionReportModalProps {
  isOpen: boolean;
  isLoading: boolean;
  reportData: AttendedUserDto[];
  onClose: () => void;
}

const SessionReportModal = ({
  isOpen,
  isLoading,
  reportData,
  onClose,
}: SessionReportModalProps) => {
  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-50">
      <DialogBackdrop
        transition
        className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
      />
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="w-full max-w-md rounded bg-white p-6">
          <DialogTitle className="text-lg font-semibold">
            Session Report
          </DialogTitle>
          {isLoading ? (
            <p>Loading...</p>
          ) : (
            <div>
              {reportData?.length === 0 ? (
                <p className="text-center text-gray-500">
                  No students attended this session.
                </p>
              ) : (
                reportData.map((user) => (
                  <div key={user.id} className="flex justify-between py-2">
                    <span>{user.fullName}</span>
                    <span>{user.markedAtUtc}</span>
                  </div>
                ))
              )}
            </div>
          )}
          <div className="px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
            <Button
              onClick={onClose}
              className="inline-flex items-center gap-2 rounded-md 
                             bg-gray-200 px-3 py-1.5 text-sm font-semibold text-gray-700
                             hover:bg-gray-300 focus:outline-none"
            >
              Close
            </Button>
          </div>
        </DialogPanel>
      </div>
    </Dialog>
  );
};

export default SessionReportModal;
