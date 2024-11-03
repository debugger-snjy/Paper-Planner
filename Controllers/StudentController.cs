using PaperPlanner_Application.Filters;
using PaperPlanner_Application.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Mvc;

namespace PaperPlanner_Application.Controllers
{
    public class StudentController : Controller
    {
        PaperPlannerDBEntities db = new PaperPlannerDBEntities();

        
        
        // ---------
        // Function to return the Approved and Unapproved Question Papers to the Student Dashboard or Index Page
        // GET Request : student
        // Filter that makes sure that the user is logged in
        [Authorize]
        // Filter that makes sure that the user is Student Only
        [RoleAuthenticationFilter("Student")]
        public ActionResult Index()
        {
            int currentUserID = (Session["tempData"] as User).userID;

            // Variable to get the approved Question Paper that are Unattempted
            List<QuestionPaper> approvedUnattempted = null;

            // Variable to get the approved Question Paper that are Attempted
            List<QuestionPaper> approvedAttempted = null;

            // Handling the Exception if no records presents for that Unattempted Question Paper
            try
            {
                approvedUnattempted = db.QuestionPapers
                .Where(qp => qp.qpStatus == "Question Paper Approved !!")
                .Where(qp => !db.QuesPaper_Attempted_By_Users.Any(qa => qa.PaperID == qp.quesPaperID && qa.UserID == currentUserID))
                .ToList();
            }
            catch (Exception ex)
            {
                approvedUnattempted = null;
            }

            // Handling the Exception if no records presents for that Attempted Question Paper
            try
            {
                approvedAttempted = db.QuestionPapers
                .Where(qp => qp.qpStatus == "Question Paper Approved !!")
                .Join(db.QuesPaper_Attempted_By_Users,
                      qp => qp.quesPaperID,
                      qa => qa.PaperID,
                      (qp, qa) => new { QuestionPaper = qp, AttemptedByUser = qa })
                .Where(joined => joined.AttemptedByUser.UserID == currentUserID)
                .Select(joined => joined.QuestionPaper)
                .ToList();
            }
            catch { approvedAttempted = null; }

            // I want to Send Both Attempted and Unattempted Question papers
            // So, We Cannot Send 2 Objects at One Time !
            // Making a tuple of both the objects
            var combinedData = new Tuple<List<QuestionPaper>, List<QuestionPaper>>(approvedUnattempted, approvedAttempted);

            return View(combinedData);
        }

        
        
        // ---------
        // Function to allow user to attempt the Question Paper throught the Question Paper ID
        // GET Request : student/attemptpaper/id
        // Filter that makes sure that the user is logged in
        [Authorize]
        // Filter that makes sure that the user is Student Only
        [RoleAuthenticationFilter("Student")]
        public ActionResult AttemptPaper(int id)
        {
            var questionsForQP = db.Questions.Where(q => q.quesPaperID == id).ToList();
            ViewBag.QuestionsForQP = questionsForQP;

            // Getting Attempted Papers Questions
            var paper = db.QuestionPapers.Where(model => model.quesPaperID == id).FirstOrDefault();
            if (paper == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question Paper or Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "Student");
            }
            return View(paper);
        }



        // ---------
        // Function to save the attempted data for the Question Paper by the student
        // GET Request : student/attemptpaper/id
        // Filter that makes sure that the user is logged in
        [Authorize]
        // Filter that makes sure that the user is Student Only
        [RoleAuthenticationFilter("Student")]
        [HttpPost]
        public ActionResult AttemptPaper(int id, FormCollection submittedData)
        {
            int correctQuestions = 0;
            int totalQuestions = 0;
            int incorrectQuestions = 0;

            for (int i = 1; i < submittedData.Count; i++)
            {

                // Getting the Answers
                int questionID = Convert.ToInt32(submittedData.GetKey(i));
                string optionSelected = submittedData[i];
                Debug.WriteLine(questionID + " : " + optionSelected);

                Question questionRecord = db.Questions.Where(model => model.quesId == questionID).First();
                string correctAns = questionRecord.quesAnswer;

                AnswersTable answer = new AnswersTable();
                answer.AttemptDate = DateTime.Now;
                answer.PaperID = id;
                answer.QuesID = questionID;
                answer.UserID = (Session["tempData"] as User).userID;
                answer.AttemptedAns = optionSelected;
                answer.CorrectAns = correctAns;

                db.AnswersTables.Add(answer);
                db.SaveChanges();

                if (optionSelected == correctAns)
                {
                    correctQuestions++;
                }

                totalQuestions++;

            }

            QuesPaper_Attempted_By_Users attemptedRecord = new QuesPaper_Attempted_By_Users();
            attemptedRecord.TotalQuestions = totalQuestions;
            attemptedRecord.CorrectQuestions = correctQuestions;
            attemptedRecord.IncorrectQuestions = totalQuestions - (correctQuestions);
            attemptedRecord.UnattemptedQuestions = totalQuestions - (correctQuestions + incorrectQuestions);
            attemptedRecord.Attemptedon = DateTime.Now;
            attemptedRecord.PaperID = id;
            attemptedRecord.UserID = (Session["tempData"] as User).userID;

            db.QuesPaper_Attempted_By_Users.Add(attemptedRecord);
            db.SaveChanges();

            TempData["alertMsg"] = "Your Question Paper is Submitted Successfully !!";
            TempData["alertType"] = "success";

            return RedirectToAction("Index", "Student");
        }



        // ---------
        // Function to view the result of the question paper submitted by the student through the id
        // GET Request : student/viewresult/id
        // Filter that makes sure that the user is logged in
        [Authorize]
        // Filter that makes sure that the user is Student Only
        [RoleAuthenticationFilter("Student")]
        public ActionResult ViewResult(int id)
        {
            int userID = (Session["tempData"] as User).userID;

            var UserAttempt = db.QuesPaper_Attempted_By_Users.Where(model => model.UserID == userID && model.PaperID == id).FirstOrDefault();

            int correct = UserAttempt.CorrectQuestions;
            int incorrect = UserAttempt.IncorrectQuestions;
            int total = UserAttempt.TotalQuestions;

            // Getting the Question paper
            var query = from questions in db.Questions
                        join answers in db.AnswersTables on questions.quesId equals answers.QuesID
                        where questions.quesPaperID == id && answers.UserID == userID
                        select new Marksheet
                        {
                            Question = questions,
                            Answers = answers,
                            TotalQuestions = total,
                            IncorrectQuestions = incorrect,
                            CorrectQuestions = correct
                        };

            return View(query.ToList());
        }
    }
}