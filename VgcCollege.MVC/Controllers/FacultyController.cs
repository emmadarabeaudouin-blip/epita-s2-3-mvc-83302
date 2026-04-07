using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.MVC.Data;
using VgcCollege.Domain.Entities;

namespace VgcCollege.MVC.Controllers
{
    public class FacultyController : Controller
    {
        private readonly AppDbContext _context;

        public FacultyController(AppDbContext context)
        {
            _context = context;
        }

        // Faculty dashboard
        public IActionResult Index(int id)
        {
            var faculty = _context.FacultyProfiles.FirstOrDefault(f => f.Id == id);

            if (faculty == null)
                return NotFound();

            return View(faculty);
        }

        // View faculty assigned courses
        public IActionResult MyCourses(int id)
        {
            var courses = _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                .Where(fca => fca.FacultyProfileId == id)
                .Select(fca => fca.Course)
                .ToList();

            ViewBag.FacultyId = id;
            return View(courses);
        }

        // =========================
        // ADD ASSIGNMENT
        // =========================

        [HttpGet]
        public IActionResult AddAssignment(int facultyId)
        {
            var assignedCourses = _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                .Where(fca => fca.FacultyProfileId == facultyId)
                .Select(fca => fca.Course)
                .ToList();

            ViewBag.FacultyId = facultyId;
            ViewBag.Courses = assignedCourses;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAssignment(Assignment assignment, int facultyId)
        {
            var isAssigned = _context.FacultyCourseAssignments
                .Any(fca => fca.FacultyProfileId == facultyId && fca.CourseId == assignment.CourseId);

            if (!isAssigned)
            {
                ModelState.AddModelError("", "You can only add assignments for your assigned courses.");
            }

            if (ModelState.IsValid)
            {
                _context.Assignments.Add(assignment);
                _context.SaveChanges();
                return RedirectToAction("Assignments", new { facultyId = facultyId });
            }

            var assignedCourses = _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                .Where(fca => fca.FacultyProfileId == facultyId)
                .Select(fca => fca.Course)
                .ToList();

            ViewBag.FacultyId = facultyId;
            ViewBag.Courses = assignedCourses;

            return View(assignment);
        }

        // View assignments created for assigned courses
        public IActionResult Assignments(int facultyId)
        {
            var assignments = _context.Assignments
                .Include(a => a.Course)
                .Where(a => _context.FacultyCourseAssignments
                    .Any(fca => fca.FacultyProfileId == facultyId && fca.CourseId == a.CourseId))
                .ToList();

            ViewBag.FacultyId = facultyId;
            return View(assignments);
        }

        // =========================
        // ADD EXAM
        // =========================

        [HttpGet]
        public IActionResult AddExam(int facultyId)
        {
            var assignedCourses = _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                .Where(fca => fca.FacultyProfileId == facultyId)
                .Select(fca => fca.Course)
                .ToList();

            ViewBag.FacultyId = facultyId;
            ViewBag.Courses = assignedCourses;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddExam(Exam exam, int facultyId)
        {
            var isAssigned = _context.FacultyCourseAssignments
                .Any(fca => fca.FacultyProfileId == facultyId && fca.CourseId == exam.CourseId);

            if (!isAssigned)
            {
                ModelState.AddModelError("", "You can only add exams for your assigned courses.");
            }

            if (ModelState.IsValid)
            {
                _context.Exams.Add(exam);
                _context.SaveChanges();
                return RedirectToAction("Exams", new { facultyId = facultyId });
            }

            var assignedCourses = _context.FacultyCourseAssignments
                .Include(fca => fca.Course)
                .Where(fca => fca.FacultyProfileId == facultyId)
                .Select(fca => fca.Course)
                .ToList();

            ViewBag.FacultyId = facultyId;
            ViewBag.Courses = assignedCourses;

            return View(exam);
        }

        // View exams for assigned courses
        public IActionResult Exams(int facultyId)
        {
            var exams = _context.Exams
                .Include(e => e.Course)
                .Where(e => _context.FacultyCourseAssignments
                    .Any(fca => fca.FacultyProfileId == facultyId && fca.CourseId == e.CourseId))
                .ToList();

            ViewBag.FacultyId = facultyId;
            return View(exams);
        }

        public IActionResult MyStudents(int id)
        {
            var students = _context.CourseEnrolments
                  .Include(e => e.StudentProfile)
                  .Include(e => e.Course)
                  .Where(e => _context.FacultyCourseAssignments
                      .Any(fca => fca.FacultyProfileId == id && fca.CourseId == e.CourseId))
                  .ToList();

            ViewBag.FacultyId = id;
            return View(students);
        }
        
    }
}