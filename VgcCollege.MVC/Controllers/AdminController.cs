using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.MVC.Data;

namespace VgcCollege.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) => _db = db;

        // ─── DASHBOARD ────────────────────────────────────────────────
        public IActionResult Index()
        {
            ViewBag.BranchCount = _db.Branches.Count();
            ViewBag.CourseCount = _db.Courses.Count();
            ViewBag.StudentCount = _db.StudentProfiles.Count();
            ViewBag.FacultyCount = _db.FacultyProfiles.Count();
            return View();
        }

        // ─── BRANCHES ─────────────────────────────────────────────────
        public IActionResult Branches()
        {
            var branches = _db.Branches
                .Include(b => b.Courses)
                .ToList();
            return View(branches);
        }

        public IActionResult BranchDetails(int id)
        {
            var branch = _db.Branches
                .Include(b => b.Courses)
                    .ThenInclude(c => c.Enrolments)
                        .ThenInclude(e => e.StudentProfile)
                .Include(b => b.Courses)
                    .ThenInclude(c => c.FacultyAssignments)
                        .ThenInclude(fa => fa.FacultyProfile)
                .FirstOrDefault(b => b.Id == id);

            if (branch == null) return NotFound();
            return View(branch);
        }

        public IActionResult CreateBranch() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreateBranch(Branch model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Branches.Add(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Branches));
        }

        public IActionResult EditBranch(int id)
        {
            var branch = _db.Branches.Find(id);
            if (branch == null) return NotFound();
            return View(branch);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditBranch(Branch model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Branches.Update(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Branches));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DeleteBranch(int id)
        {
            var branch = _db.Branches.Find(id);
            if (branch != null) { _db.Branches.Remove(branch); _db.SaveChanges(); }
            return RedirectToAction(nameof(Branches));
        }

        // ─── COURSES ──────────────────────────────────────────────────
        public IActionResult Courses()
        {
            var courses = _db.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                .Include(c => c.FacultyAssignments)
                    .ThenInclude(fa => fa.FacultyProfile)
                .ToList();
            return View(courses);
        }

        public IActionResult CourseDetails(int id)
        {
            var course = _db.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.StudentProfile)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.AttendanceRecords)
                .Include(c => c.FacultyAssignments)
                    .ThenInclude(fa => fa.FacultyProfile)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Results)
                .Include(c => c.Exams)
                .FirstOrDefault(c => c.Id == id);

            if (course == null) return NotFound();
            return View(course);
        }

        public IActionResult CreateCourse()
        {
            ViewBag.Branches = _db.Branches.ToList();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreateCourse(Course model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Branches = _db.Branches.ToList();
                return View(model);
            }
            _db.Courses.Add(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Courses));
        }

        public IActionResult EditCourse(int id)
        {
            var course = _db.Courses.Find(id);
            if (course == null) return NotFound();
            ViewBag.Branches = _db.Branches.ToList();
            return View(course);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EditCourse(Course model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Branches = _db.Branches.ToList();
                return View(model);
            }
            _db.Courses.Update(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Courses));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DeleteCourse(int id)
        {
            var course = _db.Courses.Find(id);
            if (course != null) { _db.Courses.Remove(course); _db.SaveChanges(); }
            return RedirectToAction(nameof(Courses));
        }

        // ─── ENROLMENTS ───────────────────────────────────────────────
        public IActionResult Enrolments()
        {
            var enrolments = _db.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .ToList();
            return View(enrolments);
        }

        public IActionResult EnrolmentDetails(int id)
        {
            var enrolment = _db.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Include(e => e.AttendanceRecords)
                .FirstOrDefault(e => e.Id == id);

            if (enrolment == null) return NotFound();
            return View(enrolment);
        }

        public IActionResult CreateEnrolment()
        {
            ViewBag.Students = _db.StudentProfiles.ToList();
            ViewBag.Courses = _db.Courses.Include(c => c.Branch).ToList();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreateEnrolment(CourseEnrolment model)
        {
            var exists = _db.CourseEnrolments.Any(e =>
                e.StudentProfileId == model.StudentProfileId &&
                e.CourseId == model.CourseId);

            if (exists)
            {
                ModelState.AddModelError("", "This student is already enrolled in this course.");
                ViewBag.Students = _db.StudentProfiles.ToList();
                ViewBag.Courses = _db.Courses.Include(c => c.Branch).ToList();
                return View(model);
            }

            model.EnrolDate = DateTime.Today;
            model.Status = "Active";
            _db.CourseEnrolments.Add(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Enrolments));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DeleteEnrolment(int id)
        {
            var enrolment = _db.CourseEnrolments.Find(id);
            if (enrolment != null) { _db.CourseEnrolments.Remove(enrolment); _db.SaveChanges(); }
            return RedirectToAction(nameof(Enrolments));
        }

        // ─── STUDENTS ─────────────────────────────────────────────────
        public IActionResult Students()
        {
            var students = _db.StudentProfiles
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                .ToList();
            return View(students);
        }

        public IActionResult StudentDetails(int id)
        {
            var student = _db.StudentProfiles
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Branch)
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.AttendanceRecords)
                .Include(s => s.AssignmentResults)
                    .ThenInclude(ar => ar.Assignment)
                .Include(s => s.ExamResults)
                    .ThenInclude(er => er.Exam)
                .FirstOrDefault(s => s.Id == id);

            if (student == null) return NotFound();
            return View(student);
        }
    }
}
