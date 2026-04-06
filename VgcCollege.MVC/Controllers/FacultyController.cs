using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // DASHBOARD
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

        // MY ASSIGNMENTS
        public IActionResult Assignments(int id = 1)
        {
            var assignments = _db.Assignments
                .Include(a => a.Course)
                .Where(a => _db.FacultyCourseAssignments.Any(fa =>
                    fa.FacultyProfileId == id && fa.CourseId == a.CourseId))
                .ToList();

            ViewBag.FacultyId = id;
            return View(assignments);
        }

        // MY EXAMS
        public IActionResult Exams(int id = 1)
        {
            var exams = _db.Exams
                .Include(e => e.Course)
                .Where(e => _db.FacultyCourseAssignments.Any(fa =>
                    fa.FacultyProfileId == id && fa.CourseId == e.CourseId))
                .ToList();

            ViewBag.FacultyId = id;
            return View(exams);
        }

        // CREATE ASSIGNMENT RESULT (GRADE ASSIGNMENT)
        public IActionResult CreateAssignmentResult(int assignmentId, int facultyId = 1)
        {
            var assignment = _db.Assignments
                .Include(a => a.Course)
                .FirstOrDefault(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound();

            var isAllowed = _db.FacultyCourseAssignments.Any(fa =>
                fa.FacultyProfileId == facultyId && fa.CourseId == assignment.CourseId);

            if (!isAllowed)
                return Forbid();

            var students = _db.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Where(e => e.CourseId == assignment.CourseId)
                .Select(e => e.StudentProfile)
                .ToList();

            ViewBag.Assignment = assignment;
            ViewBag.FacultyId = facultyId;
            ViewBag.Students = new SelectList(students, "Id", "Name");

            var model = new AssignmentResult
            {
                AssignmentId = assignmentId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAssignmentResult(AssignmentResult model, int facultyId = 1)
        {
            var assignment = _db.Assignments
                .FirstOrDefault(a => a.Id == model.AssignmentId);

            if (assignment == null)
                return NotFound();

            var isAllowed = _db.FacultyCourseAssignments.Any(fa =>
                fa.FacultyProfileId == facultyId && fa.CourseId == assignment.CourseId);

            if (!isAllowed)
                return Forbid();

            var exists = _db.AssignmentResults.Any(ar =>
                ar.AssignmentId == model.AssignmentId &&
                ar.StudentProfileId == model.StudentProfileId);

            if (exists)
            {
                ModelState.AddModelError("", "This student already has a result for this assignment.");
            }

            if (!ModelState.IsValid)
            {
                var students = _db.CourseEnrolments
                    .Include(e => e.StudentProfile)
                    .Where(e => e.CourseId == assignment.CourseId)
                    .Select(e => e.StudentProfile)
                    .ToList();

                ViewBag.Assignment = assignment;
                ViewBag.FacultyId = facultyId;
                ViewBag.Students = new SelectList(students, "Id", "Name", model.StudentProfileId);

                return View(model);
            }

            _db.AssignmentResults.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Assignment result posted successfully.";
            return RedirectToAction(nameof(Assignments), new { id = facultyId });
        }

        // CREATE EXAM RESULT (GRADE EXAM)
        public IActionResult CreateExamResult(int examId, int facultyId = 1)
        {
            var exam = _db.Exams
                .Include(e => e.Course)
                .FirstOrDefault(e => e.Id == examId);

            if (exam == null)
                return NotFound();

            var isAllowed = _db.FacultyCourseAssignments.Any(fa =>
                fa.FacultyProfileId == facultyId && fa.CourseId == exam.CourseId);

            if (!isAllowed)
                return Forbid();

            var students = _db.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Where(e => e.CourseId == exam.CourseId)
                .Select(e => e.StudentProfile)
                .ToList();

            ViewBag.Exam = exam;
            ViewBag.FacultyId = facultyId;
            ViewBag.Students = new SelectList(students, "Id", "Name");

            var model = new ExamResult
            {
                ExamId = examId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateExamResult(ExamResult model, int facultyId = 1)
        {
            var exam = _db.Exams
                .FirstOrDefault(e => e.Id == model.ExamId);

            if (exam == null)
                return NotFound();

            var isAllowed = _db.FacultyCourseAssignments.Any(fa =>
                fa.FacultyProfileId == facultyId && fa.CourseId == exam.CourseId);

            if (!isAllowed)
                return Forbid();

            var exists = _db.ExamResults.Any(er =>
                er.ExamId == model.ExamId &&
                er.StudentProfileId == model.StudentProfileId);

            if (exists)
            {
                ModelState.AddModelError("", "This student already has a result for this exam.");
            }

            if (!ModelState.IsValid)
            {
                var students = _db.CourseEnrolments
                    .Include(e => e.StudentProfile)
                    .Where(e => e.CourseId == exam.CourseId)
                    .Select(e => e.StudentProfile)
                    .ToList();

                ViewBag.Exam = exam;
                ViewBag.FacultyId = facultyId;
                ViewBag.Students = new SelectList(students, "Id", "Name", model.StudentProfileId);

                return View(model);
            }

            _db.ExamResults.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Exam result posted successfully.";
            return RedirectToAction(nameof(Exams), new { id = facultyId });
        }

        // RELEASE EXAM RESULTS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReleaseExamResults(int examId, int facultyId = 1)
        {
            var exam = _db.Exams.FirstOrDefault(e => e.Id == examId);

            if (exam == null)
                return NotFound();

            var isAllowed = _db.FacultyCourseAssignments.Any(fa =>
                fa.FacultyProfileId == facultyId && fa.CourseId == exam.CourseId);

            if (!isAllowed)
                return Forbid();

            exam.ResultsReleased = true;
            _db.Exams.Update(exam);
            _db.SaveChanges();

            TempData["Success"] = "Exam results released.";
            return RedirectToAction(nameof(Exams), new { id = facultyId });
        }
    }
}