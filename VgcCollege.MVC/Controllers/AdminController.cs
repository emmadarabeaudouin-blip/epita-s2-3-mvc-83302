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
        private readonly Microsoft.Extensions.Logging.ILogger<AdminController> _logger;

        public AdminController(AppDbContext db, Microsoft.Extensions.Logging.ILogger<AdminController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // DASHBOARD
        public IActionResult Index()
        {
            ViewBag.BranchCount = _db.Branches.Count();
            ViewBag.CourseCount = _db.Courses.Count();
            ViewBag.StudentCount = _db.StudentProfiles.Count();
            ViewBag.FacultyCount = _db.FacultyProfiles.Count();
            ViewBag.EnrolmentCount = _db.CourseEnrolments.Count();

            return View();
        }

        // BRANCHES
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

        // COURSES
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

        // CREATE ASSIGNMENT
        public IActionResult CreateAssignment(int courseId)
        {
            var course = _db.Courses.Find(courseId);
            if (course == null) return NotFound();

            ViewBag.Course = course;
            var model = new Assignment
            {
                CourseId = courseId,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAssignment(Assignment model, int? courseId)
        {
            _logger?.LogInformation("CreateAssignment POST invoked for CourseId route={CourseId} model.CourseId={ModelCourseId} Title={Title}", courseId, model?.CourseId, model?.Title);
            if (!ModelState.IsValid)
            {
                _logger?.LogWarning("CreateAssignment modelstate invalid: {Errors}", string.Join(";", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            // ensure CourseId is set (handle binding cases)
            if ((model.CourseId == 0 || model.CourseId == default) && courseId.HasValue)
                model.CourseId = courseId.Value;

            // remove validation for the navigation property (Course) which is populated server-side
            ModelState.Remove(nameof(Assignment.Course));

            if (!ModelState.IsValid)
            {
                ViewBag.Course = _db.Courses.Find(model.CourseId == 0 ? courseId : model.CourseId);
                // show validation errors at top of form
                TempData["Error"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(model);
            }

            if (model.DueDate == default)
                model.DueDate = DateTime.UtcNow.AddDays(7);

            _db.Assignments.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Assignment created.";
            return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
        }

        // CREATE EXAM
        public IActionResult CreateExam(int courseId)
        {
            var course = _db.Courses.Find(courseId);
            if (course == null) return NotFound();

            ViewBag.Course = course;
            var model = new Exam
            {
                CourseId = courseId,
                Date = DateTime.UtcNow.AddDays(14)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateExam(Exam model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Course = _db.Courses.Find(model.CourseId);
                return View(model);
            }

            _db.Exams.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Exam created.";
            return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
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

        public IActionResult CourseDetails(int id)
        {
            var course = _db.Courses
                .Include(c => c.Branch)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.StudentProfile)
                .Include(c => c.Enrolments)
                    .ThenInclude(e => e.AttendanceRecords)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Results)
                .Include(c => c.Exams)
                    .ThenInclude(ex => ex.Results)
                .Include(c => c.FacultyAssignments)
                    .ThenInclude(fa => fa.FacultyProfile)
                .FirstOrDefault(c => c.Id == id);

            if (course == null) return NotFound();

            return View(course);
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

        // STUDENTS
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

        // FACULTY
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

        // ENROLMENTS
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

        // ATTENDANCE CRUD (Admin)
        public IActionResult CreateAttendance(int enrolmentId)
        {
            var enrolment = _db.CourseEnrolments.Find(enrolmentId);
            if (enrolment == null) return NotFound();

            var model = new VgcCollege.Domain.Entities.AttendanceRecord
            {
                CourseEnrolmentId = enrolmentId,
                Date = DateTime.UtcNow
            };

            ViewBag.Enrolment = enrolment;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAttendance(VgcCollege.Domain.Entities.AttendanceRecord model)
        {
            if (!ModelState.IsValid)
            {
                var enrol = _db.CourseEnrolments.Find(model.CourseEnrolmentId);
                ViewBag.Enrolment = enrol;
                return View(model);
            }

            _db.AttendanceRecords.Add(model);
            _db.SaveChanges();

            return RedirectToAction(nameof(EnrolmentDetails), new { id = model.CourseEnrolmentId });
        }

        public IActionResult EditAttendance(int id)
        {
            var att = _db.AttendanceRecords
                .Include(a => a.CourseEnrolment)
                    .ThenInclude(e => e.StudentProfile)
                .FirstOrDefault(a => a.Id == id);
            if (att == null) return NotFound();

            ViewBag.Enrolment = att.CourseEnrolment;
            return View(att);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAttendance(VgcCollege.Domain.Entities.AttendanceRecord model)
        {
            if (!ModelState.IsValid)
            {
                var enrol = _db.CourseEnrolments.Find(model.CourseEnrolmentId);
                ViewBag.Enrolment = enrol;
                return View(model);
            }

            _db.AttendanceRecords.Update(model);
            _db.SaveChanges();

            return RedirectToAction(nameof(EnrolmentDetails), new { id = model.CourseEnrolmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttendance(int id)
        {
            var att = _db.AttendanceRecords.Find(id);
            if (att == null) return NotFound();

            var enrolmentId = att.CourseEnrolmentId;
            _db.AttendanceRecords.Remove(att);
            _db.SaveChanges();

            return RedirectToAction(nameof(EnrolmentDetails), new { id = enrolmentId });
        }

        // FACULTY ASSIGNMENTS

        public IActionResult FacultyAssignments()
        {
            var assignments = _db.FacultyCourseAssignments
                .Include(a => a.FacultyProfile)
                .Include(a => a.Course)
                    .ThenInclude(c => c.Branch)
                .ToList();

            return View(assignments);
        }

        // ASSIGNMENT RESULTS (Admin)
        public IActionResult AssignmentResults()
        {
            var results = _db.AssignmentResults
                .Include(ar => ar.Assignment)
                .Include(ar => ar.StudentProfile)
                .ToList();

            return View(results);
        }

        public IActionResult CreateAssignmentResult()
        {
            ViewBag.Assignments = new SelectList(_db.Assignments, "Id", "Title");
            ViewBag.Students = new SelectList(_db.StudentProfiles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAssignmentResult(VgcCollege.Domain.Entities.AssignmentResult model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Assignments = new SelectList(_db.Assignments, "Id", "Title", model.AssignmentId);
                ViewBag.Students = new SelectList(_db.StudentProfiles, "Id", "Name", model.StudentProfileId);
                return View(model);
            }

            _db.AssignmentResults.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Assignment result posted.";
            return RedirectToAction(nameof(AssignmentResults));
        }

        // EXAM RESULTS (Admin)
        public IActionResult ExamResults()
        {
            var results = _db.ExamResults
                .Include(er => er.Exam)
                .Include(er => er.StudentProfile)
                .ToList();

            return View(results);
        }

        public IActionResult CreateExamResult()
        {
            ViewBag.Exams = new SelectList(_db.Exams, "Id", "Title");
            ViewBag.Students = new SelectList(_db.StudentProfiles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateExamResult(VgcCollege.Domain.Entities.ExamResult model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Exams = new SelectList(_db.Exams, "Id", "Title", model.ExamId);
                ViewBag.Students = new SelectList(_db.StudentProfiles, "Id", "Name", model.StudentProfileId);
                return View(model);
            }

            _db.ExamResults.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Exam result posted.";
            return RedirectToAction(nameof(ExamResults));
        }

        // New AssignFaculty actions to support the custom view at Views/Admin/AssignFaculty.cshtml
        public IActionResult AssignFaculty()
        {
            ViewBag.Faculty = new SelectList(_db.FacultyProfiles, "Id", "Name");
            ViewBag.Courses = new SelectList(_db.Courses, "Id", "Name");
            return View("AssignFaculty");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignFaculty(int facultyProfileId, int courseId)
        {
            // Validate that user selected both faculty and course
            if (facultyProfileId == 0)
                ModelState.AddModelError("facultyProfileId", "Please select a faculty member.");
            if (courseId == 0)
                ModelState.AddModelError("courseId", "Please select a course.");

            if (ModelState.IsValid)
            {
                var exists = _db.FacultyCourseAssignments.Any(a =>
                    a.FacultyProfileId == facultyProfileId &&
                    a.CourseId == courseId);

                if (exists)
                {
                    ModelState.AddModelError("", "This faculty member is already assigned to that course.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Faculty = new SelectList(_db.FacultyProfiles, "Id", "Name", facultyProfileId);
                ViewBag.Courses = new SelectList(_db.Courses, "Id", "Name", courseId);
                return View("AssignFaculty");
            }

            var assignment = new VgcCollege.Domain.Entities.FacultyCourseAssignment
            {
                FacultyProfileId = facultyProfileId,
                CourseId = courseId
            };

            _db.FacultyCourseAssignments.Add(assignment);
            _db.SaveChanges();

            TempData["Success"] = "Faculty assigned successfully.";
            return RedirectToAction(nameof(FacultyAssignments));
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
