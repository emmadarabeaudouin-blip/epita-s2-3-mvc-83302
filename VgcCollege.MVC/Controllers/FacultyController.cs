using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VgcCollege.Domain.Entities;
using VgcCollege.Domain.Services;
using VgcCollege.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace VgcCollege.MVC.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly GradeService _grades;

        public FacultyController(AppDbContext db, UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
            _grades = new GradeService();
        }

        
        private async Task<FacultyProfile?> GetMyProfileAsync()
        {
            var user = await _userMgr.GetUserAsync(User);
            return await _db.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == user!.Id);
        }

        //DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var courses = await _db.FacultyCourseAssignments
                .Where(fa => fa.FacultyProfileId == profile.Id)
                .Include(fa => fa.Course)
                    .ThenInclude(c => c.Branch)
                .Include(fa => fa.Course)
                    .ThenInclude(c => c.Enrolments)
                .Select(fa => fa.Course)
                .ToListAsync();

            ViewBag.Profile = profile;
            ViewBag.CourseCount = courses.Count;
            ViewBag.StudentCount = courses.SelectMany(c => c.Enrolments).Count();
            return View(courses);
        }

        // MY STUDENTS
        public async Task<IActionResult> MyStudents()
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var students = await _db.CourseEnrolments
                .Where(e => e.Course.FacultyAssignments
                    .Any(fa => fa.FacultyProfileId == profile.Id))
                .Include(e => e.StudentProfile)
                .Include(e => e.Course)
                .ToListAsync();

            return View(students);
        }

        // STUDENT DETAIL
        public async Task<IActionResult> StudentDetail(int id)
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            // Security: verify this student is actually in one of my courses
            var hasAccess = await _db.CourseEnrolments.AnyAsync(e =>
                e.StudentProfileId == id &&
                e.Course.FacultyAssignments.Any(fa => fa.FacultyProfileId == profile.Id));

            if (!hasAccess) return Forbid();

            var student = await _db.StudentProfiles
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Branch)
                .Include(s => s.Enrolments)
                    .ThenInclude(e => e.AttendanceRecords)
                .Include(s => s.AssignmentResults)
                    .ThenInclude(ar => ar.Assignment)
                .Include(s => s.ExamResults)
                    .ThenInclude(er => er.Exam)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();
            return View(student);
        }

        //  GRADEBOOK 
        public async Task<IActionResult> Gradebook(int? courseId)
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var myCourses = await _db.FacultyCourseAssignments
                .Where(fa => fa.FacultyProfileId == profile.Id)
                .Include(fa => fa.Course)
                .Select(fa => fa.Course)
                .ToListAsync();

            ViewBag.Courses = myCourses;
            ViewBag.CourseId = courseId;

            if (courseId == null) return View(new List<AssignmentResult>());

            // Verify faculty owns this course
            var owns = myCourses.Any(c => c.Id == courseId);
            if (!owns) return Forbid();

            var results = await _db.AssignmentResults
                .Where(ar => ar.Assignment.CourseId == courseId)
                .Include(ar => ar.Assignment)
                .Include(ar => ar.StudentProfile)
                .ToListAsync();

            return View(results);
        }

        // ADD ASSIGNMENT RESULT 
        public async Task<IActionResult> AddResult(int courseId)
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var owns = await _db.FacultyCourseAssignments
                .AnyAsync(fa => fa.FacultyProfileId == profile.Id && fa.CourseId == courseId);
            if (!owns) return Forbid();

            ViewBag.Assignments = await _db.Assignments
                .Where(a => a.CourseId == courseId).ToListAsync();
            ViewBag.Students = await _db.CourseEnrolments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.StudentProfile)
                .Select(e => e.StudentProfile)
                .ToListAsync();
            ViewBag.CourseId = courseId;

            return View(new AssignmentResult());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResult(AssignmentResult model, int courseId)
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var assignment = await _db.Assignments.FindAsync(model.AssignmentId);
            if (assignment == null) return NotFound();

            model.SubmittedAt = DateTime.Now;
            _db.AssignmentResults.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Gradebook), new { courseId });
        }

        // EXAM RESULTS 
        public async Task<IActionResult> ExamResults(int? courseId)
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var myCourses = await _db.FacultyCourseAssignments
                .Where(fa => fa.FacultyProfileId == profile.Id)
                .Include(fa => fa.Course)
                .Select(fa => fa.Course)
                .ToListAsync();

            ViewBag.Courses = myCourses;
            ViewBag.CourseId = courseId;

            if (courseId == null) return View(new List<ExamResult>());

            var owns = myCourses.Any(c => c.Id == courseId);
            if (!owns) return Forbid();

            var results = await _db.ExamResults
                .Where(er => er.Exam.CourseId == courseId)
                .Include(er => er.Exam)
                .Include(er => er.StudentProfile)
                .ToListAsync();

            return View(results);
        }

        // RELEASE EXAM RESULTS
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReleaseResults(int examId, int courseId)
        {
            var exam = await _db.Exams.FindAsync(examId);
            if (exam != null)
            {
                exam.ResultsReleased = true;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ExamResults), new { courseId });
        }
    }
}
