import React, { useEffect, useState } from "react";
import { Fragment } from "react";
import { CheckIcon, ChevronUpDownIcon } from "@heroicons/react/16/solid";
import {
  Dialog,
  DialogBackdrop,
  DialogPanel,
  DialogTitle,
  Combobox,
  ComboboxButton,
  ComboboxInput,
  ComboboxOption,
  ComboboxOptions,
  Transition,
} from "@headlessui/react";
import "react-datepicker/dist/react-datepicker.css";
import {
  atsysApi,
  useGetApiVbyVersionUserGetTeachersQuery,
  useGetApiVbyVersionUserGetStudentsQuery,
  useGetApiVbyVersionUserGetAcademicStaffQuery,
  useGetApiVbyVersionDepartmantGetAllQuery,
  useGetApiVbyVersionLectureGetAllQuery,
  usePostApiVbyVersionCourseCreateMutation,
  usePutApiVbyVersionCourseUpdateMutation,
  CreateCourseCommand,
  CourseDetailDto,
  UpdateCourseCommand,
  LectureDto,
  UserListingDto,
  DepartmantLookupDto,
  UserDetailDto,
  DayOfWeek,
  TimeSpanRead,
  TimeSpan,
} from "@/redux/generatedTypes";
import { toast, Toaster } from "react-hot-toast";
import { formatStartTime } from "./CoursesList";

interface AddOrUpdateCourseModalProps {
  id: number | null;
  isOpen: boolean;
  onClose: () => void;
  refetch: () => void;
}

export const WEEK_DAYS: Record<number, string> = {
  0: "Sunday",
  1: "Monday",
  2: "Tuesday",
  3: "Wednesday",
  4: "Thursday",
  5: "Friday",
  6: "Saturday",
};

const AddOrUpdateCourseModal: React.FC<AddOrUpdateCourseModalProps> = ({
  id,
  isOpen,
  onClose,
  refetch,
}) => {
  const [trigger, { isLoading: isFetching }] =
    atsysApi.endpoints.getApiVbyVersionCourseGetByIdAndId.useLazyQuery();

  const [createCourse] = usePostApiVbyVersionCourseCreateMutation();
  const [updateCourse] = usePutApiVbyVersionCourseUpdateMutation();

  const [course, setCourse] = useState<CourseDetailDto | null>(null);

  const apiVersion = process.env.NEXT_PUBLIC_API_VERSION as string;

  const { data: lectures } = useGetApiVbyVersionLectureGetAllQuery({
    pageNumber: 1,
    pageSize: 100,
    version: apiVersion,
  });

  const { data: teachers } = useGetApiVbyVersionUserGetTeachersQuery({
    version: apiVersion,
  });

  const { data: students } = useGetApiVbyVersionUserGetStudentsQuery({
    version: apiVersion,
  });

  const { data: academicStaff } = useGetApiVbyVersionUserGetAcademicStaffQuery({
    version: apiVersion,
  });

  const { data: departments } = useGetApiVbyVersionDepartmantGetAllQuery({
    version: apiVersion,
  });

  const [lectureQuery, setLectureQuery] = useState("");
  const [teacherQuery, setTeacherQuery] = useState("");
  const [departmentQuery, setDepartmentQuery] = useState("");
  const [staffQuery, setStaffQuery] = useState("");
  const [studentQuery, setStudentQuery] = useState("");
  const [dayQuery, setDayQuery] = useState("");

  const filteredLectures =
    lectureQuery === ""
      ? lectures?.data?.data || []
      : lectures?.data?.data?.filter(
          (lecture) =>
            lecture.name?.toLowerCase().includes(lectureQuery.toLowerCase()) ??
            false
        ) || [];

  const filteredTeachers =
    teacherQuery === ""
      ? teachers?.data || []
      : teachers?.data?.filter(
          (teacher) =>
            teacher.fullName
              ?.toLowerCase()
              .includes(teacherQuery.toLowerCase()) ?? false
        ) || [];

  const filteredDepartments =
    departmentQuery === ""
      ? departments?.data || []
      : departments?.data?.filter(
          (department) =>
            department.name
              ?.toLowerCase()
              .includes(departmentQuery.toLowerCase()) ?? false
        ) || [];

  const filteredStaff =
    staffQuery === ""
      ? academicStaff?.data || []
      : academicStaff?.data?.filter(
          (staff) =>
            staff.fullName?.toLowerCase().includes(staffQuery.toLowerCase()) ??
            false
        ) || [];

  const filteredStudents =
    studentQuery === ""
      ? students?.data || []
      : students?.data?.filter(
          (student) =>
            student.fullName
              ?.toLowerCase()
              .includes(studentQuery.toLowerCase()) ?? false
        ) || [];

  const filteredDays =
    dayQuery === ""
      ? Object.entries(WEEK_DAYS)
      : (Object.entries(WEEK_DAYS) as [string, string][]).filter(([, label]) =>
          label.toLowerCase().includes(dayQuery.toLowerCase())
        );

  function toTimeSpanString(ts?: TimeSpanRead): string {
    if (!ts) return "00:00:00";

    const h = ts.hours ?? 0;
    const m = ts.minutes ?? 0;
    const s = ts.seconds ?? 0;
    const ms = ts.milliseconds ?? 0;

    const hh = String(h).padStart(2, "0");
    const mm = String(m).padStart(2, "0");
    const ss = String(s).padStart(2, "0");
    const mss = String(ms).padStart(3, "0");

    return `${hh}:${mm}:${ss}.${mss}`;
  }

  useEffect(() => {
    if (isOpen && id) {
      trigger({
        id: id,
        version: process.env.NEXT_PUBLIC_API_VERSION as string,
      })
        .unwrap()
        .then((res) => {
          if (res.success) {
            setCourse(res?.data || null);
          } else {
            toast.error(
              `Failed to fetch course details: ${res?.message} \n ${res?.errors?.join(", ")}`
            );
            setCourse(null);
          }
        })
        .catch((error) => {
          toast.error(`Error when fetching course details: ${error.message}`);
        });
    } else if (isOpen) {
      setCourse({
        id: undefined,
        lecture: undefined,
        teacher: undefined,
        department: undefined,
        duration: { ticks: 0 },
        startTime: { ticks: 0 },
        dayOfWeek: 0,
        location: "",
        courseStaffs: [],
        courseStudents: [],
      });
    }
  }, [isOpen, id, trigger]);

  const handleAddOrUpdateCourse = async () => {
    if (!course) return;

    try {
      if (id) {
        const updatedCourse: UpdateCourseCommand = {
          id: id,
          lectureId: course.lecture?.id,
          teacherId: course.teacher?.id,
          departmantId: course.department?.id || 1,
          startTime: toTimeSpanString(course.startTime) as unknown as TimeSpan,
          duration: toTimeSpanString(course.duration) as unknown as TimeSpan,
          dayOfWeek: course.dayOfWeek,
          location: course.location,
          courseStaffIds:
            course.courseStaffs?.map((staff) => staff.id || "") || [],
          courseStudentIds:
            course.courseStudents?.map((student) => student.id || "") || [],
        };

        const promise = updateCourse({
          version: process.env.NEXT_PUBLIC_API_VERSION as string,
          updateCourseCommand: updatedCourse,
        }).unwrap();

        toast.promise(promise, {
          loading: "Updating course...",
          success: (response) => {
            if (response.success) {
              refetch();
              onClose();
              return response?.message || "Course updated successfully";
            } else {
              throw new Error(
                `${response?.message ?? ""} \n ${response.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to update course: ${error.message}`;
          },
        });
      } else {
        const newCourse: CreateCourseCommand = {
          lectureId: course.lecture?.id,
          teacherId: course.teacher?.id,
          departmantId: course.department?.id || 1,
          startTime: toTimeSpanString(course.startTime) as unknown as TimeSpan,
          duration: toTimeSpanString(course.duration) as unknown as TimeSpan,
          dayOfWeek: course.dayOfWeek,
          location: course.location,
          courseStaffIds:
            course.courseStaffs?.map((staff) => staff.id || "") || [],
          courseStudentIds:
            course.courseStudents?.map((student) => student.id || "") || [],
        };

        const promise = createCourse({
          version: process.env.NEXT_PUBLIC_API_VERSION as string,
          createCourseCommand: newCourse,
        }).unwrap();

        toast.promise(promise, {
          loading: "Creating course...",
          success: (response) => {
            if (response.success) {
              refetch();
              onClose();
              return response?.message || "Course created successfully";
            } else {
              throw new Error(
                `${response?.message ?? ""} \n ${response.errors?.join(", ")}`
              );
            }
          },
          error: (error) => {
            return `Failed to create course: ${error.message}`;
          },
        });
      }
    } catch {
      toast.error(`Failed to ${id ? "update" : "create"} course`);
    }
  };

  return (
    <>
      <Toaster
        position="top-right"
        toastOptions={{
          className: "toast",
          duration: 3000,
        }}
      />
      <Dialog open={isOpen} onClose={onClose} className="relative z-10">
        {isFetching ? (
          <svg
            className={`animate-spin w-8 h-8 text-blue-500`}
            xmlns="[http://www.w3.org/2000/svg](http://www.w3.org/2000/svg)"
            fill="none"
            viewBox="0 0 24 24"
            role="status"
            aria-label="Loading"
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
              className="fixed inset-0 bg-gray-500/75 transition-opacity"
            />
            <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
              <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
                <DialogPanel
                  transition
                  className="relative transform overflow-hidden rounded-lg bg-white text-left shadow-xl transition-all"
                >
                  <form
                    className="w-full max-w-4xl px-8 pt-8 pb-6 sm:p-10 sm:pb-8"
                    onSubmit={(e) => {
                      e.preventDefault();
                      handleAddOrUpdateCourse();
                    }}
                  >
                    <DialogTitle
                      as="h3"
                      className="text-base font-semibold text-gray-900 mb-4"
                    >
                      {id ? "Update Course" : "Create Course"}
                    </DialogTitle>

                    <div className="flex flex-wrap -mx-3 mb-6">
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Lecture
                        </label>
                        <div className="relative w-full">
                          <Combobox
                            value={
                              course?.lecture?.id
                                ? lectures?.data?.data?.find(
                                    (l) => l.id === course.lecture?.id
                                  ) || null
                                : null
                            }
                            onChange={(selectedLecture) => {
                              if (selectedLecture) {
                                setCourse((prev) => ({
                                  ...prev!,
                                  lecture: {
                                    ...prev?.lecture,
                                    id: selectedLecture.id,
                                    name: selectedLecture.name,
                                  },
                                }));
                              }
                            }}
                          >
                            <div className="relative">
                              <ComboboxInput
                                className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                                displayValue={(lecture: LectureDto) =>
                                  lecture?.name ?? ""
                                }
                                onChange={(e) =>
                                  setLectureQuery(e.target.value)
                                }
                                placeholder="Select a lecture"
                              />
                              <ComboboxButton className="absolute inset-y-0 right-0 flex items-center pr-2">
                                <ChevronUpDownIcon
                                  className="h-5 w-5 text-gray-400"
                                  aria-hidden="true"
                                />
                              </ComboboxButton>
                            </div>
                            <Transition
                              as={Fragment}
                              leave="transition ease-in duration-100"
                              leaveFrom="opacity-100"
                              leaveTo="opacity-0"
                            >
                              <ComboboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                {filteredLectures.length === 0 &&
                                lectureQuery !== "" ? (
                                  <div className="relative cursor-default select-none py-2 px-4 text-gray-700">
                                    Nothing found.
                                  </div>
                                ) : (
                                  filteredLectures.map((lecture) => (
                                    <ComboboxOption
                                      key={lecture.id}
                                      value={lecture}
                                      className={({ active }) =>
                                        `relative cursor-default select-none py-2 pl-10 pr-4 ${
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
                                              selected
                                                ? "font-medium"
                                                : "font-normal"
                                            }`}
                                          >
                                            {lecture.name}
                                          </span>
                                          {selected ? (
                                            <span
                                              className={`absolute inset-y-0 left-0 flex items-center pl-3 ${
                                                active
                                                  ? "text-white"
                                                  : "text-blue-600"
                                              }`}
                                            >
                                              <CheckIcon
                                                className="h-5 w-5"
                                                aria-hidden="true"
                                              />
                                            </span>
                                          ) : null}
                                        </>
                                      )}
                                    </ComboboxOption>
                                  ))
                                )}
                              </ComboboxOptions>
                            </Transition>
                          </Combobox>
                        </div>
                      </div>
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Teacher
                        </label>
                        <div className="relative w-full">
                          <Combobox
                            value={
                              course?.teacher?.id
                                ? teachers?.data?.find(
                                    (t) => t.id === course.teacher?.id
                                  ) || null
                                : null
                            }
                            onChange={(selectedTeacher) => {
                              if (selectedTeacher) {
                                setCourse((prev) => ({
                                  ...prev!,
                                  teacher: {
                                    id: selectedTeacher.id,
                                    fullName: selectedTeacher.fullName,
                                    email: selectedTeacher.email,
                                  },
                                  teacherId: selectedTeacher.id,
                                }));
                              }
                            }}
                          >
                            <div className="relative">
                              <ComboboxInput
                                className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                                displayValue={(teacher: UserListingDto) =>
                                  teacher?.fullName ?? ""
                                }
                                onChange={(e) =>
                                  setTeacherQuery(e.target.value)
                                }
                                placeholder="Select a teacher"
                              />
                              <ComboboxButton className="absolute inset-y-0 right-0 flex items-center pr-2">
                                <ChevronUpDownIcon
                                  className="h-5 w-5 text-gray-400"
                                  aria-hidden="true"
                                />
                              </ComboboxButton>
                            </div>
                            <Transition
                              as={Fragment}
                              leave="transition ease-in duration-100"
                              leaveFrom="opacity-100"
                              leaveTo="opacity-0"
                            >
                              <ComboboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                {filteredTeachers.length === 0 &&
                                teacherQuery !== "" ? (
                                  <div className="relative cursor-default select-none py-2 px-4 text-gray-700">
                                    Nothing found.
                                  </div>
                                ) : (
                                  filteredTeachers.map((teacher) => (
                                    <ComboboxOption
                                      key={teacher.id}
                                      value={teacher}
                                      className={({ active }) =>
                                        `relative cursor-default select-none py-2 pl-10 pr-4 ${
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
                                              selected
                                                ? "font-medium"
                                                : "font-normal"
                                            }`}
                                          >
                                            {teacher.fullName}
                                          </span>
                                          {selected ? (
                                            <span
                                              className={`absolute inset-y-0 left-0 flex items-center pl-3 ${
                                                active
                                                  ? "text-white"
                                                  : "text-blue-600"
                                              }`}
                                            >
                                              <CheckIcon
                                                className="h-5 w-5"
                                                aria-hidden="true"
                                              />
                                            </span>
                                          ) : null}
                                        </>
                                      )}
                                    </ComboboxOption>
                                  ))
                                )}
                              </ComboboxOptions>
                            </Transition>
                          </Combobox>
                        </div>
                      </div>
                    </div>

                    <div className="flex flex-wrap -mx-3 mb-6">
                      <div className="w-full px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Department
                        </label>
                        <div className="relative w-full">
                          <Combobox
                            value={
                              course?.department?.id
                                ? departments?.data?.find(
                                    (d) => d.id === course.department?.id
                                  ) || null
                                : null
                            }
                            onChange={(selectedDepartment) => {
                              if (selectedDepartment) {
                                setCourse((prev) => ({
                                  ...prev!,
                                  department: {
                                    id: selectedDepartment.id,
                                    name: selectedDepartment.name,
                                  },
                                  departmentId: selectedDepartment.id,
                                }));
                              }
                            }}
                          >
                            <div className="relative">
                              <ComboboxInput
                                className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                                displayValue={(
                                  department: DepartmantLookupDto
                                ) => department?.name ?? ""}
                                onChange={(e) =>
                                  setDepartmentQuery(e.target.value)
                                }
                                placeholder="Select a department"
                              />
                              <ComboboxButton className="absolute inset-y-0 right-0 flex items-center pr-2">
                                <ChevronUpDownIcon
                                  className="h-5 w-5 text-gray-400"
                                  aria-hidden="true"
                                />
                              </ComboboxButton>
                            </div>
                            <Transition
                              as={Fragment}
                              leave="transition ease-in duration-100"
                              leaveFrom="opacity-100"
                              leaveTo="opacity-0"
                            >
                              <ComboboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                {filteredDepartments.length === 0 &&
                                departmentQuery !== "" ? (
                                  <div className="relative cursor-default select-none py-2 px-4 text-gray-700">
                                    Nothing found.
                                  </div>
                                ) : (
                                  filteredDepartments.map((department) => (
                                    <ComboboxOption
                                      key={department.id}
                                      value={department}
                                      className={({ active }) =>
                                        `relative cursor-default select-none py-2 pl-10 pr-4 ${
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
                                              selected
                                                ? "font-medium"
                                                : "font-normal"
                                            }`}
                                          >
                                            {department.name}
                                          </span>
                                          {selected ? (
                                            <span
                                              className={`absolute inset-y-0 left-0 flex items-center pl-3 ${
                                                active
                                                  ? "text-white"
                                                  : "text-blue-600"
                                              }`}
                                            >
                                              <CheckIcon
                                                className="h-5 w-5"
                                                aria-hidden="true"
                                              />
                                            </span>
                                          ) : null}
                                        </>
                                      )}
                                    </ComboboxOption>
                                  ))
                                )}
                              </ComboboxOptions>
                            </Transition>
                          </Combobox>
                        </div>
                      </div>
                    </div>

                    <div className="flex flex-wrap -mx-3 mb-6">
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Research Assistants
                        </label>
                        <div className="relative w-full">
                          <Combobox
                            multiple
                            value={course?.courseStaffs || []}
                            onChange={(
                              selectedStaffList: {
                                id: string;
                                fullName?: string;
                                email?: string;
                              }[]
                            ) => {
                              setCourse((prev) => ({
                                ...prev!,
                                courseStaffs: selectedStaffList.map(
                                  (selectedStaff) => ({
                                    id: selectedStaff.id,
                                    firstName:
                                      selectedStaff.fullName?.split(" ")[0] ||
                                      "",
                                    lastName:
                                      selectedStaff.fullName?.split(" ")[1] ||
                                      "",
                                    email: selectedStaff.email,
                                    fullName: selectedStaff.fullName,
                                  })
                                ),
                              }));
                            }}
                          >
                            <div className="relative">
                              <ComboboxInput
                                className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                                displayValue={(staffs: UserDetailDto[]) =>
                                  staffs
                                    .map(
                                      (s) => `${s.firstName + " " + s.lastName}`
                                    )
                                    .join(", ")
                                }
                                onChange={(e) => setStaffQuery(e.target.value)}
                                placeholder="Select research assistants"
                              />
                              <ComboboxButton className="absolute inset-y-0 right-0 flex items-center pr-2">
                                <ChevronUpDownIcon
                                  className="h-5 w-5 text-gray-400"
                                  aria-hidden="true"
                                />
                              </ComboboxButton>
                              <Transition
                                as={Fragment}
                                leave="transition ease-in duration-100"
                                leaveFrom="opacity-100"
                                leaveTo="opacity-0"
                              >
                                <ComboboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                  {filteredStaff.length === 0 &&
                                  staffQuery !== "" ? (
                                    <div className="relative cursor-default select-none py-2 px-4 text-gray-700">
                                      Nothing found.
                                    </div>
                                  ) : (
                                    filteredStaff.map((staff) => (
                                      <ComboboxOption
                                        key={staff.id}
                                        value={staff}
                                      >
                                        {({ selected, active }) => (
                                          <div
                                            className={`flex items-center p-2 ${
                                              active
                                                ? "bg-blue-600 text-white"
                                                : ""
                                            }`}
                                          >
                                            <span
                                              className={
                                                selected ? "font-semibold" : ""
                                              }
                                            >
                                              {staff.fullName}
                                            </span>
                                            {selected && (
                                              <CheckIcon className="ml-auto h-5 w-5" />
                                            )}
                                          </div>
                                        )}
                                      </ComboboxOption>
                                    ))
                                  )}
                                </ComboboxOptions>
                              </Transition>
                            </div>
                          </Combobox>
                        </div>
                      </div>
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Students
                        </label>
                        <div className="relative w-full">
                          <Combobox
                            multiple
                            value={course?.courseStudents || []}
                            onChange={(
                              selectedStudentList: {
                                id: string;
                                fullName?: string;
                                email?: string;
                              }[]
                            ) => {
                              setCourse((prev) => ({
                                ...prev!,
                                courseStudents: selectedStudentList.map(
                                  (selectedStudent) => ({
                                    id: selectedStudent.id,
                                    firstName:
                                      selectedStudent.fullName?.split(" ")[0] ||
                                      "",
                                    lastName:
                                      selectedStudent.fullName?.split(" ")[1] ||
                                      "",
                                    email: selectedStudent.email,
                                    fullName: selectedStudent.fullName,
                                  })
                                ),
                              }));
                            }}
                          >
                            <div className="relative">
                              <ComboboxInput
                                className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                                displayValue={(students: UserDetailDto[]) =>
                                  students
                                    .map(
                                      (s) => `${s.firstName + " " + s.lastName}`
                                    )
                                    .join(", ")
                                }
                                onChange={(e) =>
                                  setStudentQuery(e.target.value)
                                }
                                placeholder="Select students"
                              />
                              <ComboboxButton className="absolute inset-y-0 right-0 flex items-center pr-2">
                                <ChevronUpDownIcon
                                  className="h-5 w-5 text-gray-400"
                                  aria-hidden="true"
                                />
                              </ComboboxButton>
                            </div>
                            <Transition
                              as={Fragment}
                              leave="transition ease-in duration-100"
                              leaveFrom="opacity-100"
                              leaveTo="opacity-0"
                            >
                              <ComboboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                {filteredStudents.length === 0 &&
                                studentQuery !== "" ? (
                                  <div className="relative cursor-default select-none py-2 px-4 text-gray-700">
                                    Nothing found.
                                  </div>
                                ) : (
                                  filteredStudents.map((student) => (
                                    <ComboboxOption
                                      key={student.id}
                                      value={student}
                                    >
                                      {({ selected, active }) => (
                                        <div
                                          className={`flex items-center p-2 ${
                                            active
                                              ? "bg-blue-600 text-white"
                                              : ""
                                          }`}
                                        >
                                          <span
                                            className={
                                              selected ? "font-semibold" : ""
                                            }
                                          >
                                            {student.fullName}
                                          </span>
                                          {selected && (
                                            <CheckIcon className="ml-auto h-5 w-5" />
                                          )}
                                        </div>
                                      )}
                                    </ComboboxOption>
                                  ))
                                )}
                              </ComboboxOptions>
                            </Transition>
                          </Combobox>
                        </div>
                      </div>
                    </div>

                    <div className="flex flex-wrap -mx-3 mb-6">
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Location
                        </label>
                        <input
                          type="text"
                          className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                          value={course?.location || ""}
                          onChange={(e) =>
                            setCourse((prev) => ({
                              ...prev!,
                              location: e.target.value,
                            }))
                          }
                          required
                        />
                      </div>
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Day of Week
                        </label>
                        <div className="relative w-full">
                          <Combobox
                            value={course?.dayOfWeek ?? 0}
                            onChange={(val: number | null) => {
                              if (val !== null) {
                                setCourse((prev) => ({
                                  ...prev!,
                                  dayOfWeek: val as DayOfWeek,
                                }));
                              }
                            }}
                          >
                            <div className="relative">
                              <ComboboxInput
                                className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                                displayValue={(day: number) =>
                                  WEEK_DAYS[day] ?? ""
                                }
                                onChange={(e) => setDayQuery(e.target.value)}
                                placeholder="Select a day"
                              />
                              <ComboboxButton className="absolute inset-y-0 right-0 flex items-center pr-2">
                                <ChevronUpDownIcon
                                  className="h-5 w-5 text-gray-400"
                                  aria-hidden="true"
                                />
                              </ComboboxButton>
                            </div>

                            <Transition
                              as={Fragment}
                              leave="transition ease-in duration-100"
                              leaveFrom="opacity-100"
                              leaveTo="opacity-0"
                            >
                              <ComboboxOptions className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1 text-base shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none sm:text-sm">
                                {filteredDays.length === 0 &&
                                dayQuery !== "" ? (
                                  <div className="relative cursor-default select-none py-2 px-4 text-gray-700">
                                    Nothing found.
                                  </div>
                                ) : (
                                  filteredDays.map(([value, label]) => (
                                    <ComboboxOption
                                      key={value}
                                      value={parseInt(value, 10)}
                                    >
                                      {({ selected, active }) => (
                                        <div
                                          className={`flex items-center p-2 ${
                                            active
                                              ? "bg-blue-600 text-white"
                                              : ""
                                          }`}
                                        >
                                          <span
                                            className={
                                              selected ? "font-semibold" : ""
                                            }
                                          >
                                            {label}
                                          </span>
                                          {selected && (
                                            <CheckIcon className="ml-auto h-5 w-5" />
                                          )}
                                        </div>
                                      )}
                                    </ComboboxOption>
                                  ))
                                )}
                              </ComboboxOptions>
                            </Transition>
                          </Combobox>
                        </div>
                      </div>
                    </div>

                    <div className="flex flex-wrap -mx-3 mb-6">
                      {/* Start Time */}
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Start Time
                        </label>
                        <input
                          type="time"
                          className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                          value={formatStartTime(course?.startTime)}
                          onChange={(e) => {
                            const [h, m] = e.target.value.split(":");
                            setCourse((prev) => ({
                              ...prev!,
                              startTime: {
                                ...prev!.startTime,
                                hours: parseInt(h, 10),
                                minutes: parseInt(m, 10),
                              },
                            }));
                          }}
                        />
                      </div>
                      {/* Duration */}
                      <div className="w-full md:w-1/2 px-3">
                        <label className="block uppercase tracking-wide text-gray-700 text-xs font-bold mb-2">
                          Duration
                        </label>
                        <input
                          type="time"
                          step={60}
                          className="appearance-none block w-full bg-gray-50 border border-gray-200 rounded py-3 px-4 leading-tight focus:outline-none focus:bg-white focus:border-gray-500"
                          value={formatStartTime(course?.duration)}
                          onChange={(e) => {
                            const [h, m] = e.target.value.split(":");
                            setCourse((prev) => ({
                              ...prev!,
                              duration: {
                                ...prev!.duration,
                                hours: parseInt(h, 10),
                                minutes: parseInt(m, 10),
                              },
                            }));
                          }}
                        />
                      </div>
                    </div>

                    <div className="px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6">
                      <button
                        type="submit"
                        className="inline-flex w-full justify-center rounded-md bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 sm:ml-3 sm:w-auto sm:text-sm"
                      >
                        {id ? "Update Course" : "Create Course"}
                      </button>
                      <button
                        type="button"
                        className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-4 py-2 text-gray-700 hover:bg-gray-50 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                        onClick={onClose}
                      >
                        Cancel
                      </button>
                    </div>
                  </form>
                </DialogPanel>
              </div>
            </div>
          </>
        )}
      </Dialog>
    </>
  );
};

export default AddOrUpdateCourseModal;
