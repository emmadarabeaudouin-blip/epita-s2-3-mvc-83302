using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VgcCollege.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace VgcCollege.MVC.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userMgr;

        public StudentController(AppDbContext db, UserManager<ApplicationUser> userMgr)
        {
            _db = db;
            _userMgr = userMgr;
        }

        private async Task<Domain.Entities.StudentProfile?> GetMyProfileAsync()
        {
            var user = await _userMgr.GetUserAsync(User);
            return await _db.StudentProfiles
                .FirstOrDefaultAsync(s => s.IdentityUserId == user!.Id);
        }

        // DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var enrolments = await _db.CourseEnrolments
                .Where(e => e.StudentProfileId == profile.Id)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Branch)
                .Include(e => e.AttendanceRecords)
                .ToListAsync();

            ViewBag.Profile = profile;
            return View(enrolments);
        }

        // MY PROFILE
        public async Task<IActionResult> MyProfile()
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");
            return View(profile);
        }

        // MY RESULTS
        public async Task<IActionResult> MyResults()
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var assignments = await _db.AssignmentResults
                .Where(ar => ar.StudentProfileId == profile.Id)
                .Include(ar => ar.Assignment)
                    .ThenInclude(a => a.Course)
                .ToListAsync();

            // Students only see exam results when ResultsReleased = true
            var exams = await _db.ExamResults
                .Where(er => er.StudentProfileId == profile.Id && er.Exam.ResultsReleased)
                .Include(er => er.Exam)
                    .ThenInclude(e => e.Course)
                .ToListAsync();

            var pendingExams = await _db.ExamResults
                .Where(er => er.StudentProfileId == profile.Id && !er.Exam.ResultsReleased)
                .CountAsync();

            ViewBag.AssignmentResults = assignments;
            ViewBag.ExamResults = exams;
            ViewBag.PendingExams = pendingExams;
            return View();
        }

        //  MY ATTENDANCE 
        public async Task<IActionResult> MyAttendance()
        {
            var profile = await GetMyProfileAsync();
            if (profile == null) return View("NoProfile");

            var enrolments = await _db.CourseEnrolments
                .Where(e => e.StudentProfileId == profile.Id)
                .Include(e => e.Course)
                .Include(e => e.AttendanceRecords)
                .ToListAsync();

            return View(enrolments);
        }
    }
}
