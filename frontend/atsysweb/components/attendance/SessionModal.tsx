"use client";

import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
  Button,
} from "@headlessui/react";
import Image from "next/image";
import { CountdownTimer } from "nextjs-countdown-timer";
import React, { useEffect, useState } from "react";
import { CreateSessionResult } from "@/redux/generatedTypes";
import toast from "react-hot-toast";

interface SessionModalProps {
  isOpen: boolean;
  isLoading: boolean;
  qrResponse?: CreateSessionResult;
  onEndSession: () => Promise<boolean>;
  showReport: () => void;
  onClose: () => void;
}

const SessionModal: React.FC<SessionModalProps> = ({
  isOpen,
  isLoading,
  qrResponse,
  onEndSession,
  showReport,
  onClose,
}) => {
  const [isSessionOver, setIsSessionOver] = useState(false);
  const [remainingSeconds, setRemainingSeconds] = useState<number | null>(null);

  const secondsUntil = (end?: string) => {
    if (!end) return 0;

    const iso = Date.parse(end);
    let endDate: Date;

    if (!Number.isNaN(iso)) {
      endDate = new Date(iso);
    } else {
      const [h, m, s = "0"] = end.split(":");
      endDate = new Date();
      endDate.setHours(+h, +m, +s, 0);
    }

    return Math.max(0, Math.floor((endDate.getTime() - Date.now()) / 1000));
  };

  useEffect(() => {
    if (qrResponse?.endTime) {
      localStorage.removeItem("countdownTime");
      const secs = secondsUntil(qrResponse.endTime);
      setRemainingSeconds(secs);
      setIsSessionOver(secs === 0);
    } else {
      setRemainingSeconds(null);
      setIsSessionOver(false);
    }
  }, [qrResponse?.endTime]);

  const timerContent = (() => {
    if (!qrResponse?.endTime) return null;

    if (isSessionOver || remainingSeconds === 0) {
      return (
        <span className="text-lg font-semibold text-red-600">
          Session ended
        </span>
      );
    }

    if (remainingSeconds != null) {
      return (
        <CountdownTimer
          key={qrResponse.endTime}
          initialSeconds={remainingSeconds}
          onTimerEnd={() => {
            localStorage.removeItem("countdownTime");
            setTimeout(() => setIsSessionOver(true), 0);
          }}
        />
      );
    }
    return null;
  })();

  const handleEndSession = async () => {
    const wrapped = onEndSession();
    try {
      await wrapped;
      setIsSessionOver(true);
      setRemainingSeconds(0);
    } catch (error) {
      toast.error(
        `Failed to end session: ${error instanceof Error ? error.message : ""}`
      );
    }
  };

  return (
    <Dialog open={isOpen} onClose={onClose} className="relative z-10">
      {isLoading ? (
        <svg
          className="animate-spin w-8 h-8 text-blue-500"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          role="status"
        >
          <circle
            className="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            strokeWidth="4"
          />
          <path
            className="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
          />
        </svg>
      ) : (
        <>
          <DialogBackdrop
            transition
            className="fixed inset-0 bg-gray-500/75 transition-opacity data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in"
          />

          <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
            <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
              <DialogPanel
                transition
                className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all data-closed:translate-y-4 data-closed:opacity-0 data-enter:duration-300 data-enter:ease-out data-leave:duration-200 data-leave:ease-in sm:my-8 sm:w-full sm:max-w-lg data-closed:sm:translate-y-0 data-closed:sm:scale-95"
              >
                <form
                  className="w-full max-w-lg px-4 pt-5 pb-4 sm:p-6 sm:pb-4"
                  onSubmit={(e) => e.preventDefault()}
                >
                  <DialogTitle
                    as="h3"
                    className="text-base font-semibold text-gray-900 mb-4"
                  >
                    <div className="flex flex-col items-center">
                      <span className="text-sm font-bold text-gray-500">
                        Manual Code
                      </span>
                      <span className="text-lg text-gray-900">
                        {qrResponse?.sessionId + ":" + qrResponse?.token}
                      </span>
                    </div>
                  </DialogTitle>

                  <div className="flex flex-col items-center mb-4 text-center">
                    {timerContent}
                    <label
                      htmlFor="qr-code"
                      className="mt-4 block text-sm font-medium text-gray-700"
                    >
                      QR Code
                    </label>

                    {qrResponse?.qrCodeDataUri && (
                      <Image
                        src={qrResponse.qrCodeDataUri}
                        alt="QR Code"
                        width={200}
                        height={200}
                        className="w-full h-auto"
                      />
                    )}
                  </div>

                  <div className="px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6 gap-3">
                    {isSessionOver ? (
                      <>
                        <Button
                          onClick={showReport}
                          className="inline-flex items-center gap-2 rounded-md 
                   bg-blue-500 px-3 py-1.5 text-sm font-semibold text-white
                   hover:bg-blue-600 focus:outline-none"
                        >
                          Show Report
                        </Button>

                        <Button
                          onClick={onClose}
                          className="inline-flex items-center gap-2 rounded-md 
                   bg-gray-200 px-3 py-1.5 text-sm font-semibold text-gray-700
                   hover:bg-gray-300 focus:outline-none"
                        >
                          Close
                        </Button>
                      </>
                    ) : (
                      <Button
                        onClick={handleEndSession}
                        className="inline-flex items-center gap-2 rounded-md 
                 bg-red-500 px-3 py-1.5 text-sm font-semibold text-white
                 hover:bg-red-600 focus:outline-none"
                      >
                        End Session
                      </Button>
                    )}
                  </div>
                </form>
              </DialogPanel>
            </div>
          </div>
        </>
      )}
    </Dialog>
  );
};

export default SessionModal;
