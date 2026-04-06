using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VgcCollege.MVC.Data;

namespace VgcCollege.MVC.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _db;

        public StudentController(AppDbContext db)
        {
            _db = db;
        }

        // STUDENT DASHBOARD
        public IActionResult Index(int id = 1)
        {
            ViewBag.StudentId = id;

            var student = _db.StudentProfiles
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // MY PROFILE
        public IActionResult MyProfile(int id = 1)
        {
            var student = _db.StudentProfiles
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // MY COURSES / ENROLMENTS
        public IActionResult MyCourses(int id = 1)
        {
            var student = _db.StudentProfiles
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }
        // MY ASSIGNMENT RESULTS
        public IActionResult MyAssignments(int id = 1)
        {
            var student = _db.StudentProfiles
                .Include(s => s.AssignmentResults)
                    .ThenInclude(ar => ar.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // MY EXAM RESULTS
        public IActionResult MyResults(int id = 1)
        {
            var student = _db.StudentProfiles
                .Include(s => s.ExamResults)
                    .ThenInclude(er => er.Exam)
                    .ThenInclude(e => e.Course)
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            return View(student);
        }
    }
}