using System;
using System.Collections.Generic;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int CourseEnrolmentId { get; set; }
        public CourseEnrolment CourseEnrolment { get; set; } = null!;
        public int WeekNumber { get; set; }
        public DateTime Date { get; set; }
        public bool Present { get; set; }
        public string? Notes { get; set; }
    }
}
