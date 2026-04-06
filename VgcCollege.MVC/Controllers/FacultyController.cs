using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.MVC.Data;

namespace VgcCollege.MVC.Controllers
{
    public class FacultyController : Controller
    {
        private readonly AppDbContext _db;

        public FacultyController(AppDbContext db)
        {
            _db = db;
        }

        // FACULTY DASHBOARD
        public IActionResult Index(int id = 1)
        {
            var faculty = _db.FacultyProfiles
                .FirstOrDefault(f => f.Id == id);

            if (faculty == null)
                return NotFound();

            return View(faculty);
        }

        // MY PROFILE
        public IActionResult MyProfile(int id = 1)
        {
            var faculty = _db.FacultyProfiles
                .FirstOrDefault(f => f.Id == id);

            if (faculty == null)
                return NotFound();

            return View(faculty);
        }

        // MY COURSES
        public IActionResult MyCourses(int id = 1)
        {
            var faculty = _db.FacultyProfiles
                .Include(f => f.CourseAssignments)
                    .ThenInclude(a => a.Course)
                    .ThenInclude(c => c.Branch)
                .FirstOrDefault(f => f.Id == id);

            if (faculty == null)
                return NotFound();

            return View(faculty);
        }

        // MY STUDENTS
        public IActionResult MyStudents(int id = 1)
        {
            var faculty = _db.FacultyProfiles
                .Include(f => f.CourseAssignments)
                    .ThenInclude(a => a.Course)
                        .ThenInclude(c => c.Enrolments)
                            .ThenInclude(e => e.StudentProfile)
                .FirstOrDefault(f => f.Id == id);

            if (faculty == null)
                return NotFound();

            return View(faculty);
        }
    }
}