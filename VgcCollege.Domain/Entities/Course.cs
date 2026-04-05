using System;
using System.Collections.Generic;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<CourseEnrolment> Enrolments { get; set; } = new List<CourseEnrolment>();
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public ICollection<FacultyCourseAssignment> FacultyAssignments { get; set; } = new List<FacultyCourseAssignment>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

}
