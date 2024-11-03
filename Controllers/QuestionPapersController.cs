using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.EnterpriseServices;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PaperPlanner_Application.Models;
using PaperPlanner_Application.Filters;
using System.Diagnostics;

namespace PaperPlanner_Application.Controllers
{
    // Filter that makes sure that the user is logged in
    [Authorize]
    public class QuestionPapersController : Controller
    {
        private PaperPlannerDBEntities db = new PaperPlannerDBEntities();



        // --------
        // Function to Show all the Questions papers of the User
        // GET Request : QuestionPapers/
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult Index()
        {
            // For Teacher Role
            if ((Session["tempData"] as User).role == "Teacher")
            {
                User TeacherData = Session["tempData"] as User;
                // Getting all the Question Paper of particular Teacher
                var questionPapers = db.QuestionPapers.Where(model => model.qpCreatorID == TeacherData.userID);
                return View(questionPapers.ToList());
            }
            // For Admin Role
            else
            {
                // Getting all the Question Paper of all the Users
                var questionPapers = db.QuestionPapers.Include(q => q.User);
                return View(questionPapers.ToList());
            }
        }



        // --------
        // Function to Get the Details of the Question Paper
        // GET Request : QuestionPapers/Details/:id
        // Filter to make sure that the logged in user is Either Teacher, Admin or Student
        [RoleAuthenticationFilter("Teacher", "Admin", "Student")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question Paper or Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);

            var questionsForQP = db.Questions.Where(q => q.quesPaperID == id).ToList();
            ViewBag.QuestionsForQP = questionsForQP;

            if (questionPaper == null)
            {
                // If the Question Paper doesn't Exists, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            return View(questionPaper);
        }



        // --------
        // Function to Create the New Question Paper
        // GET Request : QuestionPapers/Create
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult Create()
        {
            ViewBag.qpCreatorID = new SelectList(db.Users, "userID", "username");
            return View();
        }



        // --------
        // Function to Submit the Form of Creating the Question Paper
        // POST Request : QuestionPapers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult Create([Bind(Include = "quesPaperID,qpTitle,qpCreatorID,qpDescription,qpCreatedDate,qpStatus,qpUpdatedDate")] QuestionPaper newQuestionPaper)
        {
            if (ModelState.IsValid)
            {
                newQuestionPaper.qpCreatorID = (Session["tempData"] as User).userID;
                newQuestionPaper.qpCreatedDate = DateTime.Now;

                newQuestionPaper.qpStatus = "Created";

                db.QuestionPapers.Add(newQuestionPaper);

                // Handle the Exception for any Exception from the SQL Queries or Database
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        // Sending the Status of Paper Creation
                        TempData["alertMsg"] = "Question Paper is Created Successfully !!";
                        TempData["alertType"] = "success";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Sending the Status of Paper Creation
                        TempData["alertMsg"] = "Question Paper is NOT Created Successfully !!";
                        TempData["alertType"] = "failed";
                        ViewBag.qpCreatorID = new SelectList(db.Users, "userID", "username", newQuestionPaper.qpCreatorID);
                        return View(newQuestionPaper);
                    }
                }
                catch (Exception ex)
                {
                    // Sending the Status of Paper Creation
                    if (ex.InnerException.InnerException.Message.Contains("duplicate key") && ex.InnerException.InnerException.Message.Contains("Violation of UNIQUE KEY constraint 'UQ__Question__651DD4F4179D3175'"))
                    {
                        TempData["alertMsg"] = "Question Paper is NOT Created Successfully Because Question Paper with this Title Already Exists !!";
                        TempData["alertType"] = "failed";
                    }
                    else
                    {
                        TempData["alertMsg"] = "Question Paper is NOT Created Successfully !!";
                        TempData["alertType"] = "failed";
                    }
                    ViewBag.qpCreatorID = new SelectList(db.Users, "userID", "username", newQuestionPaper.qpCreatorID);
                    return View(newQuestionPaper);
                }
            }
            return View(newQuestionPaper);
        }



        // --------
        // Function to Return the Edit View (cshtml Page)
        // GET: QuestionPapers/Edit/5
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question Paper or Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper == null)
            {
                // If the Question Paper doesn't Exists, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            ViewBag.qpCreatorID = new SelectList(db.Users, "userID", "username", questionPaper.qpCreatorID);
            return View(questionPaper);
        }



        // --------
        // Function to Submit the Form of Editing the Question Paper
        // POST: QuestionPapers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult Edit([Bind(Include = "quesPaperID,qpTitle,qpCreatorID,qpDescription,qpCreatedDate,qpStatus,qpUpdatedDate")] QuestionPaper questionPaper)
        {
            if (ModelState.IsValid)
            {
                questionPaper.qpUpdatedDate = DateTime.Now;
                db.Entry(questionPaper).State = EntityState.Modified;

                // Handle the Exception for any Exception from the SQL Queries or Database
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        var username = db.Users.Where(user=>user.userID == questionPaper.qpCreatorID).FirstOrDefault();
                        questionPaper.User = username;
                        // Sending the Status of Paper Creation
                        TempData["alertMsg"] = "Question Paper is Edited Successfully !!";
                        TempData["alertType"] = "success";
                        return View(questionPaper);
                    }
                    else
                    {
                        // Sending the Status of Paper Creation
                        TempData["alertMsg"] = "Question Paper is NOT Edited Successfully !!";
                        TempData["alertType"] = "failed";
                        ViewBag.qpCreatorID = new SelectList(db.Users, "userID", "username", questionPaper.qpCreatorID);
                        return View(questionPaper);
                    }
                }
                catch (Exception ex)
                {
                    // Sending the Status of Paper Creation
                    if (ex.InnerException.InnerException.Message.Contains("duplicate key") && ex.InnerException.InnerException.Message.Contains("Violation of UNIQUE KEY constraint 'UQ__Question__651DD4F4179D3175'"))
                    {
                        TempData["alertMsg"] = "Question Paper is NOT Edited Successfully Because Question Paper with this Title Already Exists !!";
                        TempData["alertType"] = "failed";
                    }
                    else
                    {
                        TempData["alertMsg"] = "Question Paper is NOT Edited Successfully !!";
                        TempData["alertType"] = "failed";
                    }
                    ViewBag.qpCreatorID = new SelectList(db.Users, "userID", "username", questionPaper.qpCreatorID);
                    return View(questionPaper);
                }
            }
            else
            {
                TempData["alertMsg"] = "Question Paper Edit Form is Invalid !!";
                TempData["alertType"] = "failed";
                ViewBag.qpCreatorID = new SelectList(db.Users, "userID", "username", questionPaper.qpCreatorID);
                return View(questionPaper);
            }
        }



        // --------
        // Function to Return the Delete View (cshtml Page)
        // GET: QuestionPapers/Delete/5
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question Paper or Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper == null)
            {
                // If the Question Paper doesn't Exists, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            return View(questionPaper);
        }



        // --------
        // Function to Submit the Form of Deleting the Question Paper
        // POST: QuestionPapers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Deleting the Questions that were present in that Question Paper
            var paperQuestions = db.Questions.Where(model => model.quesPaperID == id);

            if (paperQuestions != null)
            {
                foreach (var question in paperQuestions)
                {
                    db.Questions.Remove(question);
                }
                db.SaveChanges();
            }

            // Removing the Approval Record After Deleting The Question paper
            ApprovedQuestionPaper approvedQPRecord = db.ApprovedQuestionPapers.Where(model => model.approvedPaperID == id).FirstOrDefault();
            if (approvedQPRecord != null)
            {
                db.ApprovedQuestionPapers.Remove(approvedQPRecord);
                db.SaveChanges();
            }

            // Deleting the Question Paper
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper != null)
            {
                db.QuestionPapers.Remove(questionPaper);
                db.SaveChanges();
            }

            TempData["alertMsg"] = "Question Paper with all the Questions is Deleted Successfully !!";
            TempData["alertType"] = "success";

            return RedirectToAction("Index");
        }



        // --------
        // Function to Return the SendForApproval View (cshtml Page)
        // Send For Approval
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult SendForApproval(int? id)
        {
            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question Paper or Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);

            var questionsForQP = db.Questions.Where(q => q.quesPaperID == id).ToList();
            ViewBag.QuestionsForQP = questionsForQP;

            if (questionPaper == null)
            {
                // If the Question Paper doesn't Exists, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index");
            }
            return View(questionPaper);
        }



        // --------
        // Function to Submit the Form of Send Approval for Question Paper to Admin
        [HttpPost]
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult SendForApproval(int id)
        {
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);

            // Changing the Question Paper Status to Pending For Approval after Sending for Approval
            if (questionPaper != null)
            {
                questionPaper.qpStatus = "Sent for Approval, Approval Pending !";
                questionPaper.qpApprovalSendDate = DateTime.Now;
                db.SaveChanges();

                TempData["alertMsg"] = "Your Question Paper has been Sent for Approval, No Changes are Possible Now !!";
                TempData["alertType"] = "warning";

                return RedirectToAction("Index", "QuestionPapers");
            }
            else
            {
                TempData["alertMsg"] = "Your Question Paper has NOT Sent for Approval !!";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "QuestionPapers");
            }
        }



        // --------
        // Function to Return the Pending Question Papers View (cshtml Page)
        // Filter that makes sure that the user is Admin
        [RoleAuthenticationFilter("Admin")]
        public ActionResult PendingQuestionPapers()
        {
            var pendingQPs = db.QuestionPapers.Where(model => model.qpStatus == "Sent for Approval, Approval Pending !").ToList();
            return View(pendingQPs);
        }



        // --------
        // Function to Approve the Question Paper
        // Filter that makes sure that the user is Admin
        [RoleAuthenticationFilter("Admin")]
        public ActionResult ApprovePaper(int id)
        {
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper == null)
            {

                // If the Question Paper doesn't Exists, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("PendingQuestionPapers", "Questionpapers");
            }
            else
            {
                // Saving the Changes in the Question Paper
                questionPaper.qpStatus = "Question Paper Approved !!";
                questionPaper.qpApprovedDate = DateTime.Now;
                db.SaveChanges();

                // Now Adding the Approval Record in the ApprovedQuestionPapers Table
                ApprovedQuestionPaper approvedQuestionPaperRecord = new ApprovedQuestionPaper();
                approvedQuestionPaperRecord.approverID = (Session["tempData"] as User).userID;
                approvedQuestionPaperRecord.approverName = (Session["tempData"] as User).username;
                approvedQuestionPaperRecord.approvedPaperID = id;
                approvedQuestionPaperRecord.approvalDate = DateTime.Now;
                approvedQuestionPaperRecord.approveStatus = "Approved";

                // Saving the Record in the AprroveQuestionPapers table
                db.ApprovedQuestionPapers.Add(approvedQuestionPaperRecord);
                db.SaveChanges();

                TempData["alertMsg"] = "Question Paper is Approved !!";
                TempData["alertType"] = "success";

                return RedirectToAction("PendingQuestionPapers", "Questionpapers");
            }
        }



        // --------
        // Function to Reject the Question Paper
        // Filter that makes sure that the user is Admin
        [RoleAuthenticationFilter("Admin")]
        public ActionResult RejectPaper(int id)
        {
            QuestionPaper questionPaper = db.QuestionPapers.Find(id);
            if (questionPaper == null)
            {

                // If the Question Paper doesn't Exists, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Question Paper Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("PendingQuestionPapers", "Questionpapers");
            }
            else
            {
                // Saving the Changes in the Question Paper
                questionPaper.qpStatus = "Question Paper Rejected !!";
                questionPaper.qpApprovedDate = DateTime.Now;
                db.SaveChanges();

                // Now Adding the Approval Record in the ApprovedQuestionPapers Table
                ApprovedQuestionPaper approvedQuestionPaperRecord = new ApprovedQuestionPaper();
                approvedQuestionPaperRecord.approverID = (Session["tempData"] as User).userID;
                approvedQuestionPaperRecord.approverName = (Session["tempData"] as User).username;
                approvedQuestionPaperRecord.approvedPaperID = id;
                approvedQuestionPaperRecord.approvalDate = DateTime.Now;
                approvedQuestionPaperRecord.approveStatus = "Rejected";

                // Saving the Record in the AprroveQuestionPapers table
                db.ApprovedQuestionPapers.Add(approvedQuestionPaperRecord);
                db.SaveChanges();

                TempData["alertMsg"] = "Question Paper is Rejected Successfully !!";
                TempData["alertType"] = "success";

                return RedirectToAction("PendingQuestionPapers", "Questionpapers");
            }
        }



        // --------
        // Function to View all the Submission made by the students for the particular Question Paper
        public ActionResult ViewSubmissions(int id)
        {
            // Variable to get the approved Question Paper that are Attempted
            List<QuesPaper_Attempted_By_Users> SubmissionRecords = null;

            // Handling the Exception if no records presents for that Unattempted Question Paper
            try
            {
                SubmissionRecords = db.QuesPaper_Attempted_By_Users.Where(model => model.PaperID == id).ToList();
            }
            catch (Exception ex)
            {
                SubmissionRecords = null;
            }

            return View(SubmissionRecords);
        }



        // --------
        // Function to View Result of Question Paper Submitted by the students
        // Filter that makes sure that the user is either Teacher or Admin
        [RoleAuthenticationFilter("Teacher", "Admin")]
        public ActionResult ViewResult(int id, int userid)
        {
            var UserAttempt = db.QuesPaper_Attempted_By_Users.Where(model => model.UserID == userid && model.PaperID == id).FirstOrDefault();

            int correct = UserAttempt.CorrectQuestions;
            int incorrect = UserAttempt.IncorrectQuestions;
            int total = UserAttempt.TotalQuestions;

            // Getting the Question paper
            var query = from questions in db.Questions
                        join answers in db.AnswersTables on questions.quesId equals answers.QuesID
                        where questions.quesPaperID == id && answers.UserID == userid
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



        // --------
        // Function to Release or Dispose Unmanaged Resources that are not needed (Auto-Generated Function)
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
