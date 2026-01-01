/* eslint-disable @typescript-eslint/no-empty-object-type */
import { emptySplitApi as api } from "./emptyApi";
const injectedRtkApi = api.injectEndpoints({
  endpoints: (build) => ({
    postApiAccountAuthenticateWeb: build.mutation<
      PostApiAccountAuthenticateWebApiResponse,
      PostApiAccountAuthenticateWebApiArg
    >({
      query: (queryArg) => ({
        url: `/api/Account/authenticate/web`,
        method: "POST",
        body: queryArg.authenticationRequest,
      }),
    }),
    postApiAccountAuthenticateMobile: build.mutation<
      PostApiAccountAuthenticateMobileApiResponse,
      PostApiAccountAuthenticateMobileApiArg
    >({
      query: (queryArg) => ({
        url: `/api/Account/authenticate/mobile`,
        method: "POST",
        body: queryArg.authenticationRequest,
      }),
    }),
    postApiAccountChangePassword: build.mutation<
      PostApiAccountChangePasswordApiResponse,
      PostApiAccountChangePasswordApiArg
    >({
      query: (queryArg) => ({
        url: `/api/Account/change-password`,
        method: "POST",
        body: queryArg.changePasswordRequest,
      }),
    }),
    postApiVbyVersionCourseCreate: build.mutation<
      PostApiVbyVersionCourseCreateApiResponse,
      PostApiVbyVersionCourseCreateApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Course/create`,
        method: "POST",
        body: queryArg.createCourseCommand,
      }),
    }),
    putApiVbyVersionCourseUpdate: build.mutation<
      PutApiVbyVersionCourseUpdateApiResponse,
      PutApiVbyVersionCourseUpdateApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Course/update`,
        method: "PUT",
        body: queryArg.updateCourseCommand,
      }),
    }),
    deleteApiVbyVersionCourseDeleteAndId: build.mutation<
      DeleteApiVbyVersionCourseDeleteAndIdApiResponse,
      DeleteApiVbyVersionCourseDeleteAndIdApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Course/delete/${queryArg.id}`,
        method: "DELETE",
      }),
    }),
    getApiVbyVersionCourseGetAll: build.query<
      GetApiVbyVersionCourseGetAllApiResponse,
      GetApiVbyVersionCourseGetAllApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Course/get-all`,
        params: {
          PageNumber: queryArg.pageNumber,
          PageSize: queryArg.pageSize,
        },
      }),
    }),
    getApiVbyVersionCourseGetByIdAndId: build.query<
      GetApiVbyVersionCourseGetByIdAndIdApiResponse,
      GetApiVbyVersionCourseGetByIdAndIdApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Course/get-by-id/${queryArg.id}`,
      }),
    }),
    getApiVbyVersionCourseGetByUserAndUserId: build.query<
      GetApiVbyVersionCourseGetByUserAndUserIdApiResponse,
      GetApiVbyVersionCourseGetByUserAndUserIdApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Course/get-by-user/${queryArg.userId}`,
      }),
    }),
    getApiVbyVersionCourseGetForCurrentUser: build.query<
      GetApiVbyVersionCourseGetForCurrentUserApiResponse,
      GetApiVbyVersionCourseGetForCurrentUserApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Course/get-for-current-user`,
      }),
    }),
    getApiVbyVersionDepartmantGetAll: build.query<
      GetApiVbyVersionDepartmantGetAllApiResponse,
      GetApiVbyVersionDepartmantGetAllApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Departmant/get-all`,
      }),
    }),
    postApiVbyVersionLectureCreate: build.mutation<
      PostApiVbyVersionLectureCreateApiResponse,
      PostApiVbyVersionLectureCreateApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Lecture/create`,
        method: "POST",
        body: queryArg.createLectureCommand,
      }),
    }),
    getApiVbyVersionLectureGetAll: build.query<
      GetApiVbyVersionLectureGetAllApiResponse,
      GetApiVbyVersionLectureGetAllApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Lecture/get-all`,
        params: {
          PageNumber: queryArg.pageNumber,
          PageSize: queryArg.pageSize,
        },
      }),
    }),
    getApiVbyVersionLectureGetByIdAndId: build.query<
      GetApiVbyVersionLectureGetByIdAndIdApiResponse,
      GetApiVbyVersionLectureGetByIdAndIdApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Lecture/get-by-id/${queryArg.id}`,
      }),
    }),
    putApiVbyVersionLectureUpdate: build.mutation<
      PutApiVbyVersionLectureUpdateApiResponse,
      PutApiVbyVersionLectureUpdateApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Lecture/update`,
        method: "PUT",
        body: queryArg.updateLectureCommand,
      }),
    }),
    deleteApiVbyVersionLectureDeleteAndId: build.mutation<
      DeleteApiVbyVersionLectureDeleteAndIdApiResponse,
      DeleteApiVbyVersionLectureDeleteAndIdApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/Lecture/delete/${queryArg.id}`,
        method: "DELETE",
      }),
    }),
    getInfo: build.query<GetInfoApiResponse, GetInfoApiArg>({
      query: () => ({ url: `/info` }),
    }),
    postApiSessionsCreate: build.mutation<
      PostApiSessionsCreateApiResponse,
      PostApiSessionsCreateApiArg
    >({
      query: (queryArg) => ({
        url: `/api/sessions/create`,
        method: "POST",
        body: queryArg.createSessionCommand,
      }),
    }),
    postApiSessionsEnd: build.mutation<
      PostApiSessionsEndApiResponse,
      PostApiSessionsEndApiArg
    >({
      query: (queryArg) => ({
        url: `/api/sessions/end`,
        method: "POST",
        params: {
          sessionId: queryArg.sessionId,
        },
      }),
    }),
    postApiSessionsAttend: build.mutation<
      PostApiSessionsAttendApiResponse,
      PostApiSessionsAttendApiArg
    >({
      query: (queryArg) => ({
        url: `/api/sessions/attend`,
        method: "POST",
        body: queryArg.markAttendanceCommand,
      }),
    }),
    getApiSessionsGetCurrentuserAttendances: build.query<
      GetApiSessionsGetCurrentuserAttendancesApiResponse,
      GetApiSessionsGetCurrentuserAttendancesApiArg
    >({
      query: (queryArg) => ({
        url: `/api/sessions/get-currentuser-attendances`,
        params: {
          PageNumber: queryArg.pageNumber,
          PageSize: queryArg.pageSize,
        },
      }),
    }),
    getApiSessionsAttendanceReport: build.query<
      GetApiSessionsAttendanceReportApiResponse,
      GetApiSessionsAttendanceReportApiArg
    >({
      query: (queryArg) => ({
        url: `/api/sessions/attendance-report`,
        params: {
          sessionId: queryArg.sessionId,
        },
      }),
    }),
    postApiVbyVersionUserCreate: build.mutation<
      PostApiVbyVersionUserCreateApiResponse,
      PostApiVbyVersionUserCreateApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/create`,
        method: "POST",
        body: queryArg.createUserCommand,
      }),
    }),
    putApiVbyVersionUserUpdate: build.mutation<
      PutApiVbyVersionUserUpdateApiResponse,
      PutApiVbyVersionUserUpdateApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/update`,
        method: "PUT",
        body: queryArg.updateUserCommand,
      }),
    }),
    putApiVbyVersionUserUpdateProfile: build.mutation<
      PutApiVbyVersionUserUpdateProfileApiResponse,
      PutApiVbyVersionUserUpdateProfileApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/update-profile`,
        method: "PUT",
        body: queryArg.updateProfileCommand,
      }),
    }),
    getApiVbyVersionUserGetAll: build.query<
      GetApiVbyVersionUserGetAllApiResponse,
      GetApiVbyVersionUserGetAllApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/get-all`,
        params: {
          PageNumber: queryArg.pageNumber,
          PageSize: queryArg.pageSize,
        },
      }),
    }),
    getApiVbyVersionUserGetByIdAndId: build.query<
      GetApiVbyVersionUserGetByIdAndIdApiResponse,
      GetApiVbyVersionUserGetByIdAndIdApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/get-by-id/${queryArg.id}`,
      }),
    }),
    getApiVbyVersionUserGetCurrentUser: build.query<
      GetApiVbyVersionUserGetCurrentUserApiResponse,
      GetApiVbyVersionUserGetCurrentUserApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/get-current-user`,
      }),
    }),
    deleteApiVbyVersionUserDeleteAndId: build.mutation<
      DeleteApiVbyVersionUserDeleteAndIdApiResponse,
      DeleteApiVbyVersionUserDeleteAndIdApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/delete/${queryArg.id}`,
        method: "DELETE",
      }),
    }),
    getApiVbyVersionUserGetStudents: build.query<
      GetApiVbyVersionUserGetStudentsApiResponse,
      GetApiVbyVersionUserGetStudentsApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/get-students`,
      }),
    }),
    getApiVbyVersionUserGetTeachers: build.query<
      GetApiVbyVersionUserGetTeachersApiResponse,
      GetApiVbyVersionUserGetTeachersApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/get-teachers`,
      }),
    }),
    getApiVbyVersionUserGetItStaff: build.query<
      GetApiVbyVersionUserGetItStaffApiResponse,
      GetApiVbyVersionUserGetItStaffApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/get-it-staff`,
      }),
    }),
    getApiVbyVersionUserGetAcademicStaff: build.query<
      GetApiVbyVersionUserGetAcademicStaffApiResponse,
      GetApiVbyVersionUserGetAcademicStaffApiArg
    >({
      query: (queryArg) => ({
        url: `/api/v${queryArg.version}/User/get-academic-staff`,
      }),
    }),
  }),
  overrideExisting: false,
});
export { injectedRtkApi as atsysApi };
export type PostApiAccountAuthenticateWebApiResponse =
  /** status 200 Success */ AuthenticationResponseApiResponseRead;
export type PostApiAccountAuthenticateWebApiArg = {
  authenticationRequest: AuthenticationRequest;
};
export type PostApiAccountAuthenticateMobileApiResponse =
  /** status 200 Success */ AuthenticationResponse;
export type PostApiAccountAuthenticateMobileApiArg = {
  authenticationRequest: AuthenticationRequest;
};
export type PostApiAccountChangePasswordApiResponse =
  /** status 200 Success */ ApiResponseRead;
export type PostApiAccountChangePasswordApiArg = {
  changePasswordRequest: ChangePasswordRequest;
};
export type PostApiVbyVersionCourseCreateApiResponse =
  /** status 200 Success */ Int32ApiResponseRead;
export type PostApiVbyVersionCourseCreateApiArg = {
  version: string;
  createCourseCommand: CreateCourseCommand;
};
export type PutApiVbyVersionCourseUpdateApiResponse =
  /** status 200 Success */ Int32ApiResponseRead;
export type PutApiVbyVersionCourseUpdateApiArg = {
  version: string;
  updateCourseCommand: UpdateCourseCommand;
};
export type DeleteApiVbyVersionCourseDeleteAndIdApiResponse =
  /** status 200 Success */ Int32ApiResponseRead;
export type DeleteApiVbyVersionCourseDeleteAndIdApiArg = {
  id: number;
  version: string;
};
export type GetApiVbyVersionCourseGetAllApiResponse =
  /** status 200 Success */ CourseListingDtoPagedResponseApiResponseRead;
export type GetApiVbyVersionCourseGetAllApiArg = {
  pageNumber?: number;
  pageSize?: number;
  version: string;
};
export type GetApiVbyVersionCourseGetByIdAndIdApiResponse =
  /** status 200 Success */ CourseDetailDtoApiResponseRead;
export type GetApiVbyVersionCourseGetByIdAndIdApiArg = {
  id: number;
  version: string;
};
export type GetApiVbyVersionCourseGetByUserAndUserIdApiResponse =
  /** status 200 Success */ CourseDetailDtoApiResponseRead;
export type GetApiVbyVersionCourseGetByUserAndUserIdApiArg = {
  userId: string;
  version: string;
};
export type GetApiVbyVersionCourseGetForCurrentUserApiResponse =
  /** status 200 Success */ CourseDetailDtoApiResponseRead;
export type GetApiVbyVersionCourseGetForCurrentUserApiArg = {
  version: string;
};
export type GetApiVbyVersionDepartmantGetAllApiResponse =
  /** status 200 Success */ DepartmantLookupDtoListApiResponseRead;
export type GetApiVbyVersionDepartmantGetAllApiArg = {
  version: string;
};
export type PostApiVbyVersionLectureCreateApiResponse =
  /** status 200 Success */ Int32ApiResponseRead;
export type PostApiVbyVersionLectureCreateApiArg = {
  version: string;
  createLectureCommand: CreateLectureCommand;
};
export type GetApiVbyVersionLectureGetAllApiResponse =
  /** status 200 Success */ LectureListingDtoPagedResponseApiResponseRead;
export type GetApiVbyVersionLectureGetAllApiArg = {
  pageNumber?: number;
  pageSize?: number;
  version: string;
};
export type GetApiVbyVersionLectureGetByIdAndIdApiResponse =
  /** status 200 Success */ LectureDtoApiResponseRead;
export type GetApiVbyVersionLectureGetByIdAndIdApiArg = {
  id: number;
  version: string;
};
export type PutApiVbyVersionLectureUpdateApiResponse =
  /** status 200 Success */ Int32ApiResponseRead;
export type PutApiVbyVersionLectureUpdateApiArg = {
  version: string;
  updateLectureCommand: UpdateLectureCommand;
};
export type DeleteApiVbyVersionLectureDeleteAndIdApiResponse =
  /** status 200 Success */ Int32ApiResponseRead;
export type DeleteApiVbyVersionLectureDeleteAndIdApiArg = {
  id: number;
  version: string;
};
export type GetInfoApiResponse = /** status 200 Success */ string;
export type GetInfoApiArg = void;
export type PostApiSessionsCreateApiResponse =
  /** status 200 Success */ CreateSessionResultApiResponseRead;
export type PostApiSessionsCreateApiArg = {
  createSessionCommand: CreateSessionCommand;
};
export type PostApiSessionsEndApiResponse =
  /** status 200 Success */ BooleanApiResponseRead;
export type PostApiSessionsEndApiArg = {
  sessionId?: number;
};
export type PostApiSessionsAttendApiResponse =
  /** status 200 Success */ BooleanApiResponseRead;
export type PostApiSessionsAttendApiArg = {
  markAttendanceCommand: MarkAttendanceCommand;
};
export type GetApiSessionsGetCurrentuserAttendancesApiResponse =
  /** status 200 Success */ AttendanceListingDtoPagedResponseApiResponseRead;
export type GetApiSessionsGetCurrentuserAttendancesApiArg = {
  pageNumber?: number;
  pageSize?: number;
};
export type GetApiSessionsAttendanceReportApiResponse =
  /** status 200 Success */ AttendedUserDtoListApiResponseRead;
export type GetApiSessionsAttendanceReportApiArg = {
  sessionId?: number;
};
export type PostApiVbyVersionUserCreateApiResponse =
  /** status 200 Success */ ApiResponseRead;
export type PostApiVbyVersionUserCreateApiArg = {
  version: string;
  createUserCommand: CreateUserCommand;
};
export type PutApiVbyVersionUserUpdateApiResponse =
  /** status 200 Success */ ApiResponseRead;
export type PutApiVbyVersionUserUpdateApiArg = {
  version: string;
  updateUserCommand: UpdateUserCommand;
};
export type PutApiVbyVersionUserUpdateProfileApiResponse =
  /** status 200 Success */ ApiResponseRead;
export type PutApiVbyVersionUserUpdateProfileApiArg = {
  version: string;
  updateProfileCommand: UpdateProfileCommand;
};
export type GetApiVbyVersionUserGetAllApiResponse =
  /** status 200 Success */ UserListingDtoPagedResponseApiResponseRead;
export type GetApiVbyVersionUserGetAllApiArg = {
  pageNumber?: number;
  pageSize?: number;
  version: string;
};
export type GetApiVbyVersionUserGetByIdAndIdApiResponse =
  /** status 200 Success */ UserDetailDtoApiResponseRead;
export type GetApiVbyVersionUserGetByIdAndIdApiArg = {
  id: string;
  version: string;
};
export type GetApiVbyVersionUserGetCurrentUserApiResponse =
  /** status 200 Success */ UserDetailDtoApiResponseRead;
export type GetApiVbyVersionUserGetCurrentUserApiArg = {
  version: string;
};
export type DeleteApiVbyVersionUserDeleteAndIdApiResponse =
  /** status 200 Success */ ApiResponseRead;
export type DeleteApiVbyVersionUserDeleteAndIdApiArg = {
  id: string;
  version: string;
};
export type GetApiVbyVersionUserGetStudentsApiResponse =
  /** status 200 Success */ UserListingDtoListApiResponseRead;
export type GetApiVbyVersionUserGetStudentsApiArg = {
  version: string;
};
export type GetApiVbyVersionUserGetTeachersApiResponse =
  /** status 200 Success */ UserListingDtoListApiResponseRead;
export type GetApiVbyVersionUserGetTeachersApiArg = {
  version: string;
};
export type GetApiVbyVersionUserGetItStaffApiResponse =
  /** status 200 Success */ UserListingDtoListApiResponseRead;
export type GetApiVbyVersionUserGetItStaffApiArg = {
  version: string;
};
export type GetApiVbyVersionUserGetAcademicStaffApiResponse =
  /** status 200 Success */ UserListingDtoListApiResponseRead;
export type GetApiVbyVersionUserGetAcademicStaffApiArg = {
  version: string;
};
export type AuthenticationResponse = {
  id?: string | null;
  userName?: string | null;
  email?: string | null;
  roles?: string[] | null;
  isVerified?: boolean;
  jwToken?: string | null;
};
export type AuthenticationResponseApiResponse = {
  data?: AuthenticationResponse;
};
export type AuthenticationResponseApiResponseRead = {
  success?: boolean;
  data?: AuthenticationResponse;
  message?: string | null;
  errors?: string[] | null;
};
export type AuthenticationRequest = {
  email?: string | null;
  password?: string | null;
};
export type ApiResponse = {};
export type ApiResponseRead = {
  success?: boolean;
  message?: string | null;
  errors?: string[] | null;
};
export type ChangePasswordRequest = {
  userId?: string | null;
  oldPassword?: string | null;
  newPassword?: string | null;
  confirmPassword?: string | null;
};
export type Int32ApiResponse = {};
export type Int32ApiResponseRead = {
  success?: boolean;
  data?: number;
  message?: string | null;
  errors?: string[] | null;
};
export type DayOfWeek = 0 | 1 | 2 | 3 | 4 | 5 | 6;
export type TimeSpan = {
  ticks?: number;
};
export type TimeSpanRead = {
  ticks?: number;
  days?: number;
  hours?: number;
  milliseconds?: number;
  microseconds?: number;
  nanoseconds?: number;
  minutes?: number;
  seconds?: number;
  totalDays?: number;
  totalHours?: number;
  totalMilliseconds?: number;
  totalMicroseconds?: number;
  totalNanoseconds?: number;
  totalMinutes?: number;
  totalSeconds?: number;
};
export type CreateCourseCommand = {
  id?: number;
  lectureId?: number;
  teacherId?: string | null;
  departmantId?: number;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpan;
  duration?: TimeSpan;
  location?: string | null;
  courseStaffIds?: string[] | null;
  courseStudentIds?: string[] | null;
};
export type CreateCourseCommandRead = {
  id?: number;
  lectureId?: number;
  teacherId?: string | null;
  departmantId?: number;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpanRead;
  duration?: TimeSpanRead;
  location?: string | null;
  courseStaffIds?: string[] | null;
  courseStudentIds?: string[] | null;
};
export type UpdateCourseCommand = {
  id?: number;
  lectureId?: number;
  teacherId?: string | null;
  departmantId?: number;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpan;
  duration?: TimeSpan;
  location?: string | null;
  courseStaffIds?: string[] | null;
  courseStudentIds?: string[] | null;
};
export type UpdateCourseCommandRead = {
  id?: number;
  lectureId?: number;
  teacherId?: string | null;
  departmantId?: number;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpanRead;
  duration?: TimeSpanRead;
  location?: string | null;
  courseStaffIds?: string[] | null;
  courseStudentIds?: string[] | null;
};
export type CourseListingDto = {
  id?: number;
  lectureName?: string | null;
  teacherName?: string | null;
  departmentName?: string | null;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpan;
  location?: string | null;
};
export type CourseListingDtoRead = {
  id?: number;
  lectureName?: string | null;
  teacherName?: string | null;
  departmentName?: string | null;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpanRead;
  location?: string | null;
};
export type CourseListingDtoPagedResponse = {
  pageNumber?: number;
  pageSize?: number;
  totalRecords?: number;
  data?: CourseListingDto[] | null;
};
export type CourseListingDtoPagedResponseRead = {
  pageNumber?: number;
  pageSize?: number;
  totalRecords?: number;
  data?: CourseListingDtoRead[] | null;
};
export type CourseListingDtoPagedResponseApiResponse = {
  data?: CourseListingDtoPagedResponse;
};
export type CourseListingDtoPagedResponseApiResponseRead = {
  success?: boolean;
  data?: CourseListingDtoPagedResponseRead;
  message?: string | null;
  errors?: string[] | null;
};
export type LectureDto = {
  id?: number;
  code?: string | null;
  name?: string | null;
  description?: string | null;
};
export type Roles = 0 | 1 | 2 | 3;
export type UserDetailDto = {
  id?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  informationMail?: string | null;
  roles?: Roles[] | null;
};
export type DepartmantLookupDto = {
  id?: number;
  name?: string | null;
  facultyId?: string | null;
};
export type CourseDetailDto = {
  id?: number;
  lecture?: LectureDto;
  teacher?: UserDetailDto;
  department?: DepartmantLookupDto;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpan;
  duration?: TimeSpan;
  location?: string | null;
  courseStaffs?: UserDetailDto[] | null;
  courseStudents?: UserDetailDto[] | null;
};
export type CourseDetailDtoRead = {
  id?: number;
  lecture?: LectureDto;
  teacher?: UserDetailDto;
  department?: DepartmantLookupDto;
  dayOfWeek?: DayOfWeek;
  startTime?: TimeSpanRead;
  duration?: TimeSpanRead;
  location?: string | null;
  courseStaffs?: UserDetailDto[] | null;
  courseStudents?: UserDetailDto[] | null;
};
export type CourseDetailDtoApiResponse = {
  data?: CourseDetailDto;
};
export type CourseDetailDtoApiResponseRead = {
  success?: boolean;
  data?: CourseDetailDtoRead;
  message?: string | null;
  errors?: string[] | null;
};
export type DepartmantLookupDtoListApiResponse = {};
export type DepartmantLookupDtoListApiResponseRead = {
  success?: boolean;
  data?: DepartmantLookupDto[] | null;
  message?: string | null;
  errors?: string[] | null;
};
export type CreateLectureCommand = {
  code?: string | null;
  name?: string | null;
  description?: string | null;
};
export type LectureListingDto = {
  id?: number;
  code?: string | null;
  name?: string | null;
  description?: string | null;
};
export type LectureListingDtoPagedResponse = {
  pageNumber?: number;
  pageSize?: number;
  totalRecords?: number;
  data?: LectureListingDto[] | null;
};
export type LectureListingDtoPagedResponseApiResponse = {
  data?: LectureListingDtoPagedResponse;
};
export type LectureListingDtoPagedResponseApiResponseRead = {
  success?: boolean;
  data?: LectureListingDtoPagedResponse;
  message?: string | null;
  errors?: string[] | null;
};
export type LectureDtoApiResponse = {
  data?: LectureDto;
};
export type LectureDtoApiResponseRead = {
  success?: boolean;
  data?: LectureDto;
  message?: string | null;
  errors?: string[] | null;
};
export type UpdateLectureCommand = {
  id?: number;
  code?: string | null;
  name?: string | null;
  description?: string | null;
};
export type CreateSessionResult = {
  sessionId?: number;
  token?: string | null;
  qrCodeDataUri?: string | null;
  endTime?: string;
};
export type CreateSessionResultApiResponse = {
  data?: CreateSessionResult;
};
export type CreateSessionResultApiResponseRead = {
  success?: boolean;
  data?: CreateSessionResult;
  message?: string | null;
  errors?: string[] | null;
};
export type CreateSessionCommand = {
  courseId?: number;
  startTime?: string | null;
  endTime?: string | null;
};
export type BooleanApiResponse = {};
export type BooleanApiResponseRead = {
  success?: boolean;
  data?: boolean;
  message?: string | null;
  errors?: string[] | null;
};
export type MarkAttendanceCommand = {
  sessionId?: number;
  token?: string | null;
};
export type AttendanceListingDto = {
  courseName?: string | null;
  markedAtUtc?: string;
};
export type AttendanceListingDtoPagedResponse = {
  pageNumber?: number;
  pageSize?: number;
  totalRecords?: number;
  data?: AttendanceListingDto[] | null;
};
export type AttendanceListingDtoPagedResponseApiResponse = {
  data?: AttendanceListingDtoPagedResponse;
};
export type AttendanceListingDtoPagedResponseApiResponseRead = {
  success?: boolean;
  data?: AttendanceListingDtoPagedResponse;
  message?: string | null;
  errors?: string[] | null;
};
export type AttendedUserDtoListApiResponse = {};
export type AttendedUserDto = {
  id?: number;
  fullName?: string | null;
  markedAtUtc?: string | null;
};
export type AttendedUserDtoListApiResponseRead = {
  success?: boolean;
  data?: AttendedUserDto[] | null;
  message?: string | null;
  errors?: string[] | null;
};
export type CreateUserCommand = {
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  informationMail?: string | null;
  roles?: Roles[] | null;
  password?: string | null;
};
export type UpdateUserCommand = {
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  informationMail?: string | null;
  roles?: Roles[] | null;
};
export type UpdateProfileCommand = {
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  phoneNumber?: string | null;
  informationMail?: string | null;
};
export type UserListingDto = {
  id?: string | null;
  fullName?: string | null;
  email?: string | null;
};
export type UserListingDtoPagedResponse = {
  pageNumber?: number;
  pageSize?: number;
  totalRecords?: number;
  data?: UserListingDto[] | null;
};
export type UserListingDtoPagedResponseApiResponse = {
  data?: UserListingDtoPagedResponse;
};
export type UserListingDtoPagedResponseApiResponseRead = {
  success?: boolean;
  data?: UserListingDtoPagedResponse;
  message?: string | null;
  errors?: string[] | null;
};
export type UserDetailDtoApiResponse = {
  data?: UserDetailDto;
};
export type UserDetailDtoApiResponseRead = {
  success?: boolean;
  data?: UserDetailDto;
  message?: string | null;
  errors?: string[] | null;
};
export type UserListingDtoListApiResponse = {};
export type UserListingDtoListApiResponseRead = {
  success?: boolean;
  data?: UserListingDto[] | null;
  message?: string | null;
  errors?: string[] | null;
};
export const {
  usePostApiAccountAuthenticateWebMutation,
  usePostApiAccountAuthenticateMobileMutation,
  usePostApiAccountChangePasswordMutation,
  usePostApiVbyVersionCourseCreateMutation,
  usePutApiVbyVersionCourseUpdateMutation,
  useDeleteApiVbyVersionCourseDeleteAndIdMutation,
  useGetApiVbyVersionCourseGetAllQuery,
  useGetApiVbyVersionCourseGetByIdAndIdQuery,
  useGetApiVbyVersionCourseGetByUserAndUserIdQuery,
  useGetApiVbyVersionCourseGetForCurrentUserQuery,
  useGetApiVbyVersionDepartmantGetAllQuery,
  usePostApiVbyVersionLectureCreateMutation,
  useGetApiVbyVersionLectureGetAllQuery,
  useGetApiVbyVersionLectureGetByIdAndIdQuery,
  usePutApiVbyVersionLectureUpdateMutation,
  useDeleteApiVbyVersionLectureDeleteAndIdMutation,
  useGetInfoQuery,
  usePostApiSessionsCreateMutation,
  usePostApiSessionsEndMutation,
  usePostApiSessionsAttendMutation,
  useGetApiSessionsGetCurrentuserAttendancesQuery,
  useGetApiSessionsAttendanceReportQuery,
  usePostApiVbyVersionUserCreateMutation,
  usePutApiVbyVersionUserUpdateMutation,
  usePutApiVbyVersionUserUpdateProfileMutation,
  useGetApiVbyVersionUserGetAllQuery,
  useGetApiVbyVersionUserGetByIdAndIdQuery,
  useGetApiVbyVersionUserGetCurrentUserQuery,
  useDeleteApiVbyVersionUserDeleteAndIdMutation,
  useGetApiVbyVersionUserGetStudentsQuery,
  useGetApiVbyVersionUserGetTeachersQuery,
  useGetApiVbyVersionUserGetItStaffQuery,
  useGetApiVbyVersionUserGetAcademicStaffQuery,
} = injectedRtkApi;
