using System.Linq;
using AutoMapper;
using CleanArchitecture.Application.DTOs.Attendances;
using CleanArchitecture.Application.DTOs.Courses;
using CleanArchitecture.Application.DTOs.Departmant;
using CleanArchitecture.Application.DTOs.Lectures;
using CleanArchitecture.Application.DTOs.Users;
using CleanArchitecture.Application.Features.Courses.Commands.CreateCourse;
using CleanArchitecture.Application.Features.Courses.Commands.UpdateCourse;
using CleanArchitecture.Application.Features.Lectures.Commands.CreateLecture;
using CleanArchitecture.Application.Features.Lectures.Commands.UpdateLecture;
using CleanArchitecture.Application.Features.Users.Commands.UpdateUser;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Entities.Attendances;
using CleanArchitecture.Core.Entities.Courses;
using CleanArchitecture.Core.Entities.Departmants;
using CleanArchitecture.Core.Entities.Lectures;

namespace CleanArchitecture.Core.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<UpdateUserCommand, ApplicationUser>();
            CreateMap<UpdateProfileCommand, ApplicationUser>();
            CreateMap<ApplicationUser, UserListingDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            CreateMap<ApplicationUser, UserDetailDTO>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            CreateMap<CreateLectureCommand, Lecture>();
            CreateMap<UpdateLectureCommand, Lecture>();
            CreateMap<Lecture, LectureListingDTO>();
            CreateMap<Lecture, LectureDTO>();

            CreateMap<Departmant, DepartmantLookupDTO>();

            CreateMap<CreateCourseCommand, Course>()
                .ForMember(dest => dest.CourseStaffs, opt => opt.Ignore())
                .ForMember(dest => dest.CourseStudents, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.CourseStaffIds != null && src.CourseStaffIds.Count > 0)
                    {
                        foreach (var courseStaff in src.CourseStaffIds)
                        {
                            dest.AddCourseStaff(courseStaff);
                        }
                    }

                    if (src.CourseStudentIds != null && src.CourseStudentIds.Count > 0)
                    {
                        foreach (var courseStudent in src.CourseStudentIds)
                        {
                            dest.AddCourseStudent(courseStudent);
                        }
                    }
                });

            CreateMap<UpdateCourseCommand, Course>()
                .ForMember(dest => dest.CourseStaffs, opt => opt.Ignore())
                .ForMember(dest => dest.CourseStudents, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.CourseStaffIds != null && src.CourseStaffIds.Count > 0)
                    {
                        foreach (var courseStaff in src.CourseStaffIds)
                        {
                            dest.AddCourseStaff(courseStaff);
                        }
                    }

                    if (src.CourseStudentIds != null && src.CourseStudentIds.Count > 0)
                    {
                        foreach (var courseStudent in src.CourseStudentIds)
                        {
                            dest.AddCourseStudent(courseStudent);
                        }
                    }
                });

            CreateMap<Course, CourseListingDTO>()
                .ForMember(dest => dest.LectureName, opt => opt.MapFrom(src => src.Lecture.Name))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Departmant.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => $"{src.Teacher.FirstName} {src.Teacher.LastName}"));
            CreateMap<Course, CourseDetailDTO>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Departmant))
                .ForMember(dest => dest.CourseStaffs, opt => opt.MapFrom(src => src.CourseStaffs.Select(cs => cs.Staff)))
                .ForMember(dest => dest.CourseStudents, opt => opt.MapFrom(src => src.CourseStudents.Select(cs => cs.Student)));

            CreateMap<Attendance, AttendanceListingDTO>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Session.Course.Lecture.Name));

            CreateMap<Attendance, AttendedUserDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Student.FirstName} {src.Student.LastName}"))
                .ForMember(dest => dest.MarkedAtUtc, opt => opt.MapFrom(src => src.MarkedAtUtc.ToString("yyyy-MM-dd HH:mm:ss")));
        }
    }
}
