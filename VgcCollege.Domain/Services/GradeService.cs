using System;
using System.Collections.Generic;
using System.Text;

namespace VgcCollege.Domain.Services
{
    public class GradeService
    {
        public string GetLetterGrade(double score, double maxScore)
        {
            var pct = score / maxScore * 100;
            return pct switch
            {
                >= 70 => "A",
                >= 60 => "B",
                >= 50 => "C",
                >= 40 => "D",
                _ => "F"
            };
        }

        public bool CanViewExamResult(bool resultsReleased, string userRole)
        {
            if (userRole == "Admin" || userRole == "Faculty") return true;
            return resultsReleased;
        }
    }
}
