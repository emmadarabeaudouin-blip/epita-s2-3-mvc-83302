using System;
using System.Collections.Generic;
using System.Text;

namespace VgcCollege.Domain.Entities
{ 
    public class ExamResult
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }
        public int StudentProfileId { get; set; }
        public StudentProfile? StudentProfile { get; set; }
        public double Score { get; set; }
        
    }
}
