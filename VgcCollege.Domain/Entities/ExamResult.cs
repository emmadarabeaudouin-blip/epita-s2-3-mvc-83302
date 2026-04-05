using System;
using System.Collections.Generic;
using System.Text;

namespace VgcCollege.Domain.Entities
{ 
    public class ExamResult
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;
        public int StudentProfileId { get; set; }
        public StudentProfile StudentProfile { get; set; } = null!;
        public double Score { get; set; }
        public string Grade { get; set; } = string.Empty; // A, B, C, D, F
    }
}
