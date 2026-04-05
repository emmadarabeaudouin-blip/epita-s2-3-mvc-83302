using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using Microsoft.EntityFrameworkCore.Design;

namespace VgcCollege.MVC.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<AppDbContext>();
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();

            await db.Database.MigrateAsync();

            // Roles
            foreach (var role in new[] { "Admin", "Faculty", "Student" })
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole(role));

            // Admin
            await CreateUser(userMgr, "admin@vgc.ie", "Admin123", "Admin", "Admin");

            // Faculty
            var fac = await CreateUser(userMgr, "faculty@vgc.ie", "Faculty123", "Faculty", "Dr. Smith");

            // 2 Students
            var s1 = await CreateUser(userMgr, "student1@vgc.ie", "Student123", "Student", "Alice Murphy");
            var s2 = await CreateUser(userMgr, "student2@vgc.ie", "Student123", "Student", "Bob Kelly");

            // Branches
            if (!db.Branches.Any())
            {
                db.Branches.AddRange(
                    new Branch { Name = "Dublin", Address = "1 College St, Dublin" },
                    new Branch { Name = "Cork", Address = "5 Academy Rd, Cork" },
                    new Branch { Name = "Galway", Address = "12 University Ave, Galway" }
                );
                await db.SaveChangesAsync();

                var branch = db.Branches.First();
                var course = new Course
                {
                    Name = "BSc Computer Science",
                    BranchId = branch.Id,
                    StartDate = DateTime.Today.AddMonths(-2),
                    EndDate = DateTime.Today.AddMonths(10)
                };
                db.Courses.Add(course);
                await db.SaveChangesAsync();

                // Profiles + Enrolments
                var sp1 = new StudentProfile { IdentityUserId = s1!.Id, Name = "Alice Murphy", Email = s1.Email! };
                var sp2 = new StudentProfile { IdentityUserId = s2!.Id, Name = "Bob Kelly", Email = s2.Email! };
                db.StudentProfiles.AddRange(sp1, sp2);
                db.FacultyProfiles.Add(new FacultyProfile { IdentityUserId = fac!.Id, Name = "Dr. Smith", Email = fac.Email! });
                await db.SaveChangesAsync();

                db.CourseEnrolments.AddRange(
                    new CourseEnrolment { StudentProfileId = sp1.Id, CourseId = course.Id, EnrolDate = DateTime.Today, Status = "Active" },
                    new CourseEnrolment { StudentProfileId = sp2.Id, CourseId = course.Id, EnrolDate = DateTime.Today, Status = "Active" }
                );
                await db.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser?> CreateUser(
            UserManager<ApplicationUser> mgr, string email, string pwd, string role, string name)
        {
            if (await mgr.FindByEmailAsync(email) != null) return null;
            var user = new ApplicationUser { UserName = email, Email = email, FullName = name, EmailConfirmed = true };
            await mgr.CreateAsync(user, pwd);
            await mgr.AddToRoleAsync(user, role);
            return user;
        }
    }

    // C#
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            options.UseSqlite("Data Source=vgccollege.db");
            return new AppDbContext(options.Options);
        }
    }
}
