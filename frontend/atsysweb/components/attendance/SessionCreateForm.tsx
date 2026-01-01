"use client";
import {
  atsysApi,
  AttendedUserDto,
  CourseListingDto,
  CreateSessionResult,
  useGetApiVbyVersionCourseGetAllQuery,
  usePostApiSessionsCreateMutation,
  usePostApiSessionsEndMutation,
} from "@/redux/generatedTypes";
import {
  Combobox,
  ComboboxButton,
  ComboboxInput,
  ComboboxOption,
  ComboboxOptions,
  Transition,
} from "@headlessui/react";
import { CheckIcon, ChevronUpDownIcon } from "@heroicons/react/16/solid";
import React, { Fragment, useEffect, useState } from "react";
import SessionModal from "./SessionModal";
import { toast, Toaster } from "react-hot-toast";
import SessionReportModal from "./SessionReportModal";

const SessionCreateForm = () => {
  const [selectedCourseId, setSelectedCourseId] = useState<number | null>(null);
  const [query, setQuery] = useState("");
  const [selected, setSelected] = useState<CourseListingDto | null>();
  const [qrResponse, setQrResponse] = useState<CreateSessionResult>();
  const [reportData, setReportData] = useState<AttendedUserDto[]>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isReportModalOpen, setIsReportModalOpen] = useState(false);
  const pageNumber = 1;
  const pageSize = 10;

  const { data, isFetching } = useGetApiVbyVersionCourseGetAllQuery({
    pageNumber,
    pageSize,
    version: process.env.NEXT_PUBLIC_API_VERSION as string,
  });
  const [endSession] = usePostApiSessionsEndMutation();
  const [createSession, { isLoading }] = usePostApiSessionsCreateMutation();
  const [trigger, { isLoading: isReportLoading }] =
    atsysApi.endpoints.getApiSessionsAttendanceReport.useLazyQuery();

  const onCourseSelect = (courseId: number) => {
    setSelectedCourseId(courseId);
  };

  useEffect(() => {
    const found =
      data?.data?.data?.find((c) => c.id === selectedCourseId) || null;
    setSelected(found);
  }, [selectedCourseId, data]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedCourseId) {
      const formData = new FormData(e.currentTarget as HTMLFormElement);

      const withSeconds = (t: string | null) =>
        t ? (t.length === 5 ? `${t}:00` : t) : null;

      const startTime = withSeconds(formData.get("start-time") as string);
      const endTime = withSeconds(formData.get("end-time") as string);

      const promise = createSession({
        createSessionCommand: {
          courseId: selectedCourseId,
          startTime: startTime,
          endTime: endTime,
        },
      }).unwrap();

      toast.promise(
        async () => {
          const res = await promise;
          return res;
        },
        {
          loading: "Creating session...",
          success: (res) => {
            if (res?.success) {
              setQrResponse(res?.data);
              setIsModalOpen(true);
              return res?.message ?? "Session created successfully";
            } else {
              throw new Error(
                `${res?.message ?? ""} \n ${res.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to create session: ${error.message}`;
          },
        }
      );
    }
  };

  const handleEndSession = async (): Promise<boolean> => {
    if (!qrResponse?.sessionId) {
      toast.error("No session to end.");
      return false;
    }
    const wrapped = endSession({ sessionId: qrResponse.sessionId })
      .unwrap()
      .then((res) => {
        if (!res.success) {
          const errs = res.errors?.length ? ` (${res.errors.join(", ")})` : "";
          throw new Error(res.message ?? `Failed to end session${errs}`);
        }
        return res;
      });

    try {
      await toast.promise(wrapped, {
        loading: "Ending session...",
        success: (res) => {
          if (res.success) {
            return res.message ?? "Session ended successfully";
          } else {
            throw new Error(res.message ?? "Failed to end session");
          }
        },
        error: (err: Error) => `Failed to end session: ${err.message}`,
      });

      return true;
    } catch {
      return false;
    }
  };
  const filtered =
    query === ""
      ? data?.data?.data
      : data?.data?.data?.filter((c) =>
          [c.lectureName, c.teacherName, c.departmentName]
            .filter(Boolean)
            .some((str) => str!.toLowerCase().includes(query.toLowerCase()))
        );

  const showReport = async () => {
    if (qrResponse?.sessionId) {
      const res = await trigger({
        sessionId: qrResponse.sessionId,
      }).unwrap();
      if (res.success) {
        setReportData(res?.data ?? []);
        setIsReportModalOpen(true);
      } else {
        toast.error(
          `Failed to fetch report: ${res.message ?? "Unknown error"}`
        );
      }
    }
  };

  const onClose = () => {
    setTimeout(() => setQrResponse(undefined), 500);
    setIsModalOpen(false);
    setIsReportModalOpen(false);
  };

  return (
    <div className="p-6 bg-white rounded-lg shadow h-full">
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      <h2 className="text-xl font-bold mb-4">Create Session</h2>
      <SessionReportModal
        isOpen={isReportModalOpen}
        isLoading={isReportLoading}
        reportData={reportData ?? []}
        onClose={onClose}
      />
      <SessionModal
        isOpen={isModalOpen}
        isLoading={isLoading}
        qrResponse={qrResponse}
        onEndSession={handleEndSession}
        showReport={showReport}
        onClose={onClose}
      />
      {isFetching ? (
        <div className="flex items-center justify-center h-full">
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
      ) : (
        <form className="space-y-4 w-1/3 p-6" onSubmit={handleSubmit}>
          <div className="mb-4">
            <label
              htmlFor="course"
              className="block text-sm font-medium text-gray-700"
            >
              Course
            </label>
            <Combobox
              value={selected ?? null}
              onChange={(course) => {
                setSelected(course);
                onCourseSelect(course?.id ?? 0);
              }}
            >
              <div className="relative mt-1">
                <ComboboxInput
                  className="w-full px-3 py-2 bg-gray-50 border-b focus:outline-none"
                  displayValue={(c: CourseListingDto) => c?.lectureName ?? ""}
                  onChange={(e) => setQuery(e.target.value)}
                  placeholder="Pick a course…"
                />

                <ComboboxButton className="absolute right-0 top-0 px-2 py-2">
                  <ChevronUpDownIcon className="h-5 w-5 text-gray-500" />
                </ComboboxButton>

                <Transition
                  as={Fragment}
                  leave="ease-in duration-100"
                  leaveFrom="opacity-100"
                  leaveTo="opacity-0"
                >
                  <ComboboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto bg-white shadow-lg rounded-md">
                    {filtered?.length === 0 ? (
                      <div className="p-2 text-gray-700">Nothing found.</div>
                    ) : (
                      filtered?.map((course) => (
                        <ComboboxOption
                          key={course.id}
                          value={course}
                          className={({ active }) =>
                            `cursor-pointer select-none relative p-2 ${
                              active
                                ? "bg-blue-600 text-white"
                                : "text-gray-900"
                            }`
                          }
                        >
                          {({ selected, active }) => (
                            <>
                              <span
                                className={`block truncate ${
                                  selected ? "font-semibold" : "font-normal"
                                }`}
                              >
                                {course.lectureName}
                              </span>
                              <span className="text-sm block">
                                {course.teacherName} — {course.departmentName}
                              </span>
                              {selected && (
                                <CheckIcon
                                  className={`${
                                    active ? "text-white" : "text-blue-600"
                                  } absolute inset-y-0 right-0 my-auto h-5 w-5`}
                                />
                              )}
                            </>
                          )}
                        </ComboboxOption>
                      ))
                    )}
                  </ComboboxOptions>
                </Transition>
              </div>
            </Combobox>
          </div>

          <div className="flex align-items-center justify-between w-full gap-6">
            <div className="mb-4 flex-1">
              <label
                htmlFor="start-time"
                className="block text-sm font-medium text-gray-700"
              >
                Start Time
              </label>
              <input
                type="time"
                id="start-time"
                name="start-time"
                className="mt-1 block w-full px-2 py-1 bg-gray-50 border-b focus:outline-none"
              />
            </div>

            <div className="mb-4 flex-1">
              <label
                htmlFor="end-time"
                className="block text-sm font-medium text-gray-700"
              >
                End Time
              </label>
              <input
                type="time"
                id="end-time"
                name="end-time"
                className="mt-1 block w-full px-2 py-1  bg-gray-50 border-b focus:outline-none"
              />
            </div>
          </div>
          <button
            type="submit"
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Create Session
          </button>
        </form>
      )}
    </div>
  );
};

export default SessionCreateForm;
