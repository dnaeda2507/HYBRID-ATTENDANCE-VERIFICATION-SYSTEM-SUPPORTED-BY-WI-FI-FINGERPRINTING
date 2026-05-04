"use client";
import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { toast, Toaster } from "react-hot-toast";
import SessionReportModal from "./SessionReportModal";
import { AttendedUserDto } from "@/redux/generatedTypes";

interface TeacherSessionDTO {
  id: number;
  courseId: number;
  courseName: string;
  date: string;
  startTime: string;
  endTime: string;
  status: string;
  attendedStudentCount: number;
}

export default function TeacherPastSessions() {
  const token = useSelector((state: { auth: { jwToken: string } }) => state.auth.jwToken);
  const [sessions, setSessions] = useState<TeacherSessionDTO[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [reportData, setReportData] = useState<AttendedUserDto[]>([]);
  const [isReportModalOpen, setIsReportModalOpen] = useState(false);
  const [isReportLoading, setIsReportLoading] = useState(false);

  useEffect(() => {
    if (!token) return;

    const fetchSessions = async () => {
      setIsLoading(true);
      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}/api/sessions/my-past-sessions`,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        const json = await res.json();
        if (json.success) {
          setSessions(json.data);
        } else {
          toast.error(json.message || "Failed to load sessions");
        }
      } catch {
        toast.error("An error occurred while fetching past sessions");
      } finally {
        setIsLoading(false);
      }
    };

    fetchSessions();
  }, [token]);

  const handleShowReport = async (sessionId: number) => {
    setIsReportLoading(true);
    setIsReportModalOpen(true);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/sessions/attendance-report?sessionId=${sessionId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      const json = await res.json();
      if (json.success) {
        setReportData(json.data);
      } else {
        toast.error(json.message || "Failed to load report");
      }
    } catch {
      toast.error("Failed to fetch report data");
    } finally {
      setIsReportLoading(false);
    }
  };

  return (
    <div className="p-6 bg-white rounded-lg shadow h-full">
      <Toaster position="top-right" />
      <h2 className="text-xl font-bold mb-4">My Past Sessions</h2>

      <SessionReportModal
        isOpen={isReportModalOpen}
        isLoading={isReportLoading}
        reportData={reportData}
        onClose={() => setIsReportModalOpen(false)}
      />

      {isLoading ? (
        <div className="flex items-center justify-center h-48">
          <svg
            className="animate-spin h-5 w-5 text-blue-500"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              fill="none"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 1 1 16 0A8 8 0 0 1 4 12z"
            />
          </svg>
        </div>
      ) : sessions.length === 0 ? (
        <p className="text-gray-500">You have no past sessions.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Course
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Time
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Attended
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Action
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {sessions.map((session) => (
                <tr key={session.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {session.courseName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {session.date}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {session.startTime} - {session.endTime}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    <span
                      className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                        session.status === "Open"
                          ? "bg-green-100 text-green-800"
                          : "bg-red-100 text-red-800"
                      }`}
                    >
                      {session.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {session.attendedStudentCount} Students
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button
                      onClick={() => handleShowReport(session.id)}
                      className="text-blue-600 hover:text-blue-900 font-semibold"
                    >
                      View Report
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
