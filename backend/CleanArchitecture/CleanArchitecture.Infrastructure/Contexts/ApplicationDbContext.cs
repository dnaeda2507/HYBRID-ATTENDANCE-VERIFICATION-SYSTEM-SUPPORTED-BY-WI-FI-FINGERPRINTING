using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Entities.Attendances;
using CleanArchitecture.Core.Entities.Courses;
using CleanArchitecture.Core.Entities.Departmants;
using CleanArchitecture.Core.Entities.Faculties;
using CleanArchitecture.Core.Entities.Lectures;
using CleanArchitecture.Core.Entities.Sessions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure.Seeds.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public DbSet<Departmant> Departmants { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseStaff> CourseStaffs { get; set; }
        public DbSet<CourseStudent> CourseStudents { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = _dateTime.NowUtc;
                        entry.Entity.CreatedBy = _authenticatedUser.UserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModified = _dateTime.NowUtc;
                        entry.Entity.LastModifiedBy = _authenticatedUser.UserId;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "User");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Role");
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");

            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            //All Decimals will have 18,6 Range
            foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,6)");
            }
            base.OnModelCreating(builder);

            ConfigureModels(builder);
            ConfigureSeeds(builder);
        }

        private void ConfigureModels(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(e => e.Departmant)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmantId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany<Attendance>()
                    .WithOne(e => e.Student)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Session>(entity =>
            {
                entity.HasOne(e => e.Course)
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(s => s.Attendances)
                    .WithOne(e => e.Session)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Departmant>(entity =>
            {
                entity.HasOne(e => e.Faculty)
                    .WithMany()
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Course>(entity =>
            {
                entity.HasOne(e => e.Departmant)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmantId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Lecture)
                    .WithMany()
                    .HasForeignKey(e => e.LectureId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Teacher)
                    .WithMany()
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.CourseStaffs)
                    .WithOne()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.CourseStudents)
                    .WithOne()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CourseStaff>(entity =>
            {
                entity.HasKey(e => new { e.CourseId, e.StaffId });

                entity.HasOne(e => e.Staff)
                    .WithMany()
                    .HasForeignKey(e => e.StaffId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CourseStudent>(entity =>
            {
                entity.HasKey(e => new { e.CourseId, e.StudentId });

                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => new { e.SessionId, e.StudentId });

                entity.HasOne(e => e.Session)
                    .WithMany(s => s.Attendances)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Lecture>(entity =>
            {
                entity.HasIndex(e => e.Code)
                    .IsUnique();
            });
        }

        private void ConfigureSeeds(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new FacultyConfigurations());
            builder.ApplyConfiguration(new DepartmantConfigurations());
        }
    }
}
