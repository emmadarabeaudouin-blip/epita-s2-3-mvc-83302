using System;
using System.Collections.Generic;
using System.Text;

namespace VgcCollege.Domain.Entities
{
    public class Assignment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public double MaxScore { get; set; }
        public DateTime DueDate { get; set; }

        public ICollection<AssignmentResult> Results { get; set; } = new List<AssignmentResult>();
    }
}
