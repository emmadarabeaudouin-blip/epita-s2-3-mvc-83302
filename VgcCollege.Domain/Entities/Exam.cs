using System;
using System.Collections.Generic;
using System.Text;
using VgcCollege.Domain.Entities;

namespace VgcCollege.Domain.Entities
{
    public class Exam
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double MaxScore { get; set; }
        public bool ResultsReleased { get; set; } = false; 
        public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
    }
}
