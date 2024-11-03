using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaperPlanner_Application.Models
{
    public class Marksheet
    {
        public Question Question { get; set; }
        public AnswersTable Answers { get; set; }

        public int TotalQuestions { get; set; }
        public int CorrectQuestions { get; set; }
        public int IncorrectQuestions { get; set; }
    }
}