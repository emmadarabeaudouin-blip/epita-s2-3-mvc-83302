using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Entities;
using VgcCollege.MVC.Data;

namespace VgcCollege.Tests
{
    public class UnitTest1
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<MVC.Data.AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void Can_Create_StudentProfile()
        {
            using var db = GetDbContext();

            var student = new StudentProfile
            {
                Name = "Emma",
                Email = "emma@test.com",
                Phone = "123456789"
            };

            db.StudentProfiles.Add(student);
            db.SaveChanges();

            Assert.Equal(1, db.StudentProfiles.Count());
            Assert.Equal("Emma", db.StudentProfiles.First().Name);
        }

        [Fact]
        public void Can_Create_FacultyProfile()
        {
            using var db = GetDbContext();

            var faculty = new FacultyProfile
            {
                Name = "Dr Smith",
                Email = "smith@test.com",
                Phone = "987654321"
            };

            db.FacultyProfiles.Add(faculty);
            db.SaveChanges();

            Assert.Equal(1, db.FacultyProfiles.Count());
            Assert.Equal("Dr Smith", db.FacultyProfiles.First().Name);
        }

        [Fact]
        public void Can_Create_Branch_And_Course()
        {
            using var db = GetDbContext();

            var branch = new Branch
            {
                Name = "Computer Science"
            };

            db.Branches.Add(branch);
            db.SaveChanges();

            var course = new Course
            {
                Name = "Software Engineering",
                BranchId = branch.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3)
            };

            db.Courses.Add(course);
            db.SaveChanges();

            Assert.Equal(1, db.Branches.Count());
            Assert.Equal(1, db.Courses.Count());
            Assert.Equal("Software Engineering", db.Courses.First().Name);
        }

        [Fact]
        public void Can_Enroll_Student_In_Course()
        {
            using var db = GetDbContext();

            var student = new StudentProfile
            {
                Name = "Emma",
                Email = "emma@test.com",
                Phone = "123456789"
            };

            var branch = new Branch
            {
                Name = "Business"
            };

            db.StudentProfiles.Add(student);
            db.Branches.Add(branch);
            db.SaveChanges();

            var course = new Course
            {
                Name = "Marketing",
                BranchId = branch.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(4)
            };

            db.Courses.Add(course);
            db.SaveChanges();

            var enrolment = new CourseEnrolment
            {
                StudentProfileId = student.Id,
                CourseId = course.Id,
                EnrolDate = DateTime.Today,
                Status = "Active"
            };

            db.CourseEnrolments.Add(enrolment);
            db.SaveChanges();

            Assert.Equal(1, db.CourseEnrolments.Count());
            Assert.Equal(student.Id, db.CourseEnrolments.First().StudentProfileId);
            Assert.Equal(course.Id, db.CourseEnrolments.First().CourseId);
        }

        [Fact]
        public void Can_Assign_Faculty_To_Course()
        {
            using var db = GetDbContext();

            var faculty = new FacultyProfile
            {
                Name = "Dr Brown",
                Email = "brown@test.com",
                Phone = "555555555"
            };

            var branch = new Branch
            {
                Name = "Engineering"
            };

            db.FacultyProfiles.Add(faculty);
            db.Branches.Add(branch);
            db.SaveChanges();

            var course = new Course
            {
                Name = "Databases",
                BranchId = branch.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(5)
            };

            db.Courses.Add(course);
            db.SaveChanges();

            var facultyAssignment = new FacultyCourseAssignment
            {
                FacultyProfileId = faculty.Id,
                CourseId = course.Id
            };

            db.FacultyCourseAssignments.Add(facultyAssignment);
            db.SaveChanges();

            Assert.Equal(1, db.FacultyCourseAssignments.Count());
            Assert.Equal(faculty.Id, db.FacultyCourseAssignments.First().FacultyProfileId);
            Assert.Equal(course.Id, db.FacultyCourseAssignments.First().CourseId);
        }

        [Fact]
        public void Can_Create_Assignment_For_Course()
        {
            using var db = GetDbContext();

            var branch = new Branch
            {
                Name = "IT"
            };

            db.Branches.Add(branch);
            db.SaveChanges();

            var course = new Course
            {
                Name = "Web Development",
                BranchId = branch.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3)
            };

            db.Courses.Add(course);
            db.SaveChanges();

            var assignment = new Assignment
            {
                Title = "MVC Project",
                CourseId = course.Id,
                DueDate = DateTime.Today.AddDays(7),
                MaxScore = 100
            };

            db.Assignments.Add(assignment);
            db.SaveChanges();

            Assert.Equal(1, db.Assignments.Count());
            Assert.Equal("MVC Project", db.Assignments.First().Title);
        }

        [Fact]
        public void Can_Create_Exam_For_Course()
        {
            using var db = GetDbContext();

            var branch = new Branch
            {
                Name = "Science"
            };

            db.Branches.Add(branch);
            db.SaveChanges();

            var course = new Course
            {
                Name = "Physics",
                BranchId = branch.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3)
            };

            db.Courses.Add(course);
            db.SaveChanges();

            var exam = new Exam
            {
                Title = "Final Exam",
                CourseId = course.Id,
                MaxScore = 100
            };

            db.Exams.Add(exam);
            db.SaveChanges();

            Assert.Equal(1, db.Exams.Count());
            Assert.Equal("Final Exam", db.Exams.First().Title);
        }

        [Fact]
        public void Can_Add_Assignment_Result()
        {
            using var db = GetDbContext();

            var student = new StudentProfile
            {
                Name = "Emma",
                Email = "emma@test.com",
                Phone = "123456789"
            };

            var branch = new Branch
            {
                Name = "IT"
            };

            db.StudentProfiles.Add(student);
            db.Branches.Add(branch);
            db.SaveChanges();

            var course = new Course
            {
                Name = "C# Programming",
                BranchId = branch.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3)
            };

            db.Courses.Add(course);
            db.SaveChanges();

            var assignment = new Assignment
            {
                Title = "Lab 1",
                CourseId = course.Id,
                DueDate = DateTime.Today.AddDays(5),
                MaxScore = 50
            };

            db.Assignments.Add(assignment);
            db.SaveChanges();

            var result = new AssignmentResult
            {
                AssignmentId = assignment.Id,
                StudentProfileId = student.Id,
                Score = 45,
                Feedback = "Good work"
            };

            db.AssignmentResults.Add(result);
            db.SaveChanges();

            Assert.Equal(1, db.AssignmentResults.Count());
            Assert.Equal(45, db.AssignmentResults.First().Score);
        }

        [Fact]
        public void Can_Add_Exam_Result()
        {
            using var db = GetDbContext();

            var student = new StudentProfile
            {
                Name = "Emma",
                Email = "emma@test.com",
                Phone = "123456789"
            };

            var branch = new Branch
            {
                Name = "Math"
            };

            db.StudentProfiles.Add(student);
            db.Branches.Add(branch);
            db.SaveChanges();

            var course = new Course
            {
                Name = "Algebra",
                BranchId = branch.Id,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3)
            };

            db.Courses.Add(course);
            db.SaveChanges();

            var exam = new Exam
            {
                Title = "Midterm",
                CourseId = course.Id,
                MaxScore = 100
            };

            db.Exams.Add(exam);
            db.SaveChanges();

            var result = new ExamResult
            {
                ExamId = exam.Id,
                StudentProfileId = student.Id,
                Score = 88
            };

            db.ExamResults.Add(result);
            db.SaveChanges();

            Assert.Equal(1, db.ExamResults.Count());
            Assert.Equal(88, db.ExamResults.First().Score);
        }
    }
}
