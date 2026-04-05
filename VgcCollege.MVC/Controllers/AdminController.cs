using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.MVC.Data;

namespace VgcCollege.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────────────────────────────────
        public IActionResult Index()
        {
            ViewBag.BranchCount = _db.Branches.Count();
            ViewBag.CourseCount = _db.Courses.Count();
            ViewBag.StudentCount = _db.StudentProfiles.Count();
            ViewBag.FacultyCount = _db.FacultyProfiles.Count();
            ViewBag.EnrolmentCount = _db.CourseEnrolments.Count();

            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // BRANCHES
        // ─────────────────────────────────────────────────────────────
        public IActionResult Branches()
        {
            var branches = _db.Branches.ToList();
            return View(branches);
        }

        public IActionResult CreateBranch()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBranch(Branch model)
        {
            if (!ModelState.IsValid)
                return View(model);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBranch(Branch model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _db.Branches.Update(model);
            _db.SaveChanges();

            return RedirectToAction(nameof(Branches));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBranch(int id)
        {
            var branch = _db.Branches
                .Include(b => b.Courses)
                .FirstOrDefault(b => b.Id == id);

            if (branch == null)
                return NotFound();

            if (branch.Courses.Any())
            {
                TempData["Error"] = "Cannot delete a branch that still has courses.";
                return RedirectToAction(nameof(Branches));
            }

            _db.Branches.Remove(branch);
            _db.SaveChanges();

            return RedirectToAction(nameof(Branches));
        }

        // ─────────────────────────────────────────────────────────────
        // COURSES
        // ─────────────────────────────────────────────────────────────
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

        public IActionResult CreateCourse()
        {
            ViewBag.Branches = new SelectList(_db.Branches, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCourse(Course model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Branches = new SelectList(_db.Branches, "Id", "Name", model.BranchId);
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

            ViewBag.Branches = new SelectList(_db.Branches, "Id", "Name", course.BranchId);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(Course model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Branches = new SelectList(_db.Branches, "Id", "Name", model.BranchId);
                return View(model);
            }

            _db.Courses.Update(model);
            _db.SaveChanges();

            return RedirectToAction(nameof(Courses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCourse(int id)
        {
            var course = _db.Courses
                .Include(c => c.Enrolments)
                .Include(c => c.FacultyAssignments)
                .Include(c => c.Assignments)
                .Include(c => c.Exams)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
                return NotFound();

            if (course.Enrolments.Any() || course.FacultyAssignments.Any() || course.Assignments.Any() || course.Exams.Any())
            {
                TempData["Error"] = "Cannot delete a course that still has related records.";
                return RedirectToAction(nameof(Courses));
            }

            _db.Courses.Remove(course);
            _db.SaveChanges();

            return RedirectToAction(nameof(Courses));
        }

        // ─────────────────────────────────────────────────────────────
        // STUDENTS
        // ─────────────────────────────────────────────────────────────
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
                .Include(s => s.AssignmentResults)
                .Include(s => s.ExamResults)
                .FirstOrDefault(s => s.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        public IActionResult CreateStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStudent(StudentProfile model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = _db.StudentProfiles.Any(s =>
                s.Email == model.Email || s.StudentNumber == model.StudentNumber);

            if (exists)
            {
                ModelState.AddModelError("", "A student with this email or student number already exists.");
                return View(model);
            }

            _db.StudentProfiles.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Student created successfully.";
            return RedirectToAction(nameof(Students));
        }

        public IActionResult EditStudent(int id)
        {
            var student = _db.StudentProfiles.Find(id);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStudent(StudentProfile model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = _db.StudentProfiles.Any(s =>
                s.Id != model.Id &&
                (s.Email == model.Email || s.StudentNumber == model.StudentNumber));

            if (exists)
            {
                ModelState.AddModelError("", "Another student already uses this email or student number.");
                return View(model);
            }

            _db.StudentProfiles.Update(model);
            _db.SaveChanges();

            TempData["Success"] = "Student updated successfully.";
            return RedirectToAction(nameof(Students));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStudent(int id)
        {
            var student = _db.StudentProfiles
                .Include(s => s.Enrolments)
                .Include(s => s.AssignmentResults)
                .Include(s => s.ExamResults)
                .FirstOrDefault(s => s.Id == id);

            if (student == null)
                return NotFound();

            if (student.Enrolments.Any() || student.AssignmentResults.Any() || student.ExamResults.Any())
            {
                TempData["Error"] = "Cannot delete a student with enrolments or results.";
                return RedirectToAction(nameof(Students));
            }

            _db.StudentProfiles.Remove(student);
            _db.SaveChanges();

            TempData["Success"] = "Student deleted successfully.";
            return RedirectToAction(nameof(Students));
        }

        // ─────────────────────────────────────────────────────────────
        // FACULTY
        // ─────────────────────────────────────────────────────────────
        public IActionResult Faculty()
        {
            var faculty = _db.FacultyProfiles
                .Include(f => f.CourseAssignments)
                    .ThenInclude(a => a.Course)
                .ToList();

            return View(faculty);
        }

        public IActionResult FacultyDetails(int id)
        {
            var faculty = _db.FacultyProfiles
                .Include(f => f.CourseAssignments)
                    .ThenInclude(a => a.Course)
                        .ThenInclude(c => c.Branch)
                .FirstOrDefault(f => f.Id == id);

            if (faculty == null) return NotFound();

            return View(faculty);
        }

        public IActionResult CreateFaculty()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFaculty(FacultyProfile model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = _db.FacultyProfiles.Any(f => f.Email == model.Email);

            if (exists)
            {
                ModelState.AddModelError("", "A faculty member with this email already exists.");
                return View(model);
            }

            _db.FacultyProfiles.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Faculty member created successfully.";
            return RedirectToAction(nameof(Faculty));
        }

        public IActionResult EditFaculty(int id)
        {
            var faculty = _db.FacultyProfiles.Find(id);
            if (faculty == null) return NotFound();

            return View(faculty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFaculty(FacultyProfile model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var exists = _db.FacultyProfiles.Any(f =>
                f.Id != model.Id && f.Email == model.Email);

            if (exists)
            {
                ModelState.AddModelError("", "Another faculty member already uses this email.");
                return View(model);
            }

            _db.FacultyProfiles.Update(model);
            _db.SaveChanges();

            TempData["Success"] = "Faculty member updated successfully.";
            return RedirectToAction(nameof(Faculty));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFaculty(int id)
        {
            var faculty = _db.FacultyProfiles
                .Include(f => f.CourseAssignments)
                .FirstOrDefault(f => f.Id == id);

            if (faculty == null)
                return NotFound();

            if (faculty.CourseAssignments.Any())
            {
                TempData["Error"] = "Cannot delete faculty who is assigned to a course.";
                return RedirectToAction(nameof(Faculty));
            }

            _db.FacultyProfiles.Remove(faculty);
            _db.SaveChanges();

            TempData["Success"] = "Faculty member deleted successfully.";
            return RedirectToAction(nameof(Faculty));
        }

        // ─────────────────────────────────────────────────────────────
        // ENROLMENTS
        // ─────────────────────────────────────────────────────────────
        public IActionResult Enrolments()
        {
            var enrolments = _db.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .ToList();

            return View(enrolments);
        }

        public IActionResult CreateEnrolment()
        {
            ViewBag.Students = new SelectList(_db.StudentProfiles, "Id", "Name");
            ViewBag.Courses = new SelectList(_db.Courses, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEnrolment(CourseEnrolment model)
        {
            var exists = _db.CourseEnrolments.Any(e =>
                e.StudentProfileId == model.StudentProfileId &&
                e.CourseId == model.CourseId);

            if (exists)
            {
                ModelState.AddModelError("", "This student is already enrolled in this course.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Students = new SelectList(_db.StudentProfiles, "Id", "Name", model.StudentProfileId);
                ViewBag.Courses = new SelectList(_db.Courses, "Id", "Name", model.CourseId);
                return View(model);
            }

            if (model.EnrolDate == default)
                model.EnrolDate = DateTime.UtcNow;

            _db.CourseEnrolments.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Enrolment created successfully.";
            return RedirectToAction(nameof(Enrolments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEnrolment(int id)
        {
            var enrolment = _db.CourseEnrolments.Find(id);
            if (enrolment == null) return NotFound();

            _db.CourseEnrolments.Remove(enrolment);
            _db.SaveChanges();

            TempData["Success"] = "Enrolment deleted successfully.";
            return RedirectToAction(nameof(Enrolments));
        }

        // ─────────────────────────────────────────────────────────────
        // FACULTY ASSIGNMENTS
        // ─────────────────────────────────────────────────────────────
        public IActionResult FacultyAssignments()
        {
            var assignments = _db.FacultyCourseAssignments
                .Include(a => a.FacultyProfile)
                .Include(a => a.Course)
                    .ThenInclude(c => c.Branch)
                .ToList();

            return View(assignments);
        }

        public IActionResult CreateFacultyAssignment()
        {
            ViewBag.Faculty = new SelectList(_db.FacultyProfiles, "Id", "Name");
            ViewBag.Courses = new SelectList(_db.Courses, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFacultyAssignment(FacultyCourseAssignment model)
        {
            // Validate that user selected both faculty and course
            if (model.FacultyProfileId == 0)
                ModelState.AddModelError("FacultyProfileId", "Please select a faculty member.");
            if (model.CourseId == 0)
                ModelState.AddModelError("CourseId", "Please select a course.");

            if (ModelState.IsValid)
            {
                var exists = _db.FacultyCourseAssignments.Any(a =>
                    a.FacultyProfileId == model.FacultyProfileId &&
                    a.CourseId == model.CourseId);

                if (exists)
                {
                    ModelState.AddModelError("", "This faculty member is already assigned to that course.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Faculty = new SelectList(_db.FacultyProfiles, "Id", "Name", model.FacultyProfileId);
                ViewBag.Courses = new SelectList(_db.Courses, "Id", "Name", model.CourseId);
                return View(model);
            }

            _db.FacultyCourseAssignments.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Faculty assigned successfully.";
            return RedirectToAction(nameof(FacultyAssignments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFacultyAssignment(int id)
        {
            var assignment = _db.FacultyCourseAssignments.Find(id);
            if (assignment == null) return NotFound();

            _db.FacultyCourseAssignments.Remove(assignment);
            _db.SaveChanges();

            TempData["Success"] = "Faculty assignment removed successfully.";
            return RedirectToAction(nameof(FacultyAssignments));
        }

    }
}
