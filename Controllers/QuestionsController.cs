using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using PaperPlanner_Application.Models;

namespace PaperPlanner_Application.Controllers
{
    public class QuestionsController : Controller
    {
        private PaperPlannerDBEntities db = new PaperPlannerDBEntities();
        private static int qpID;


        // --------
        // Function to return all the Questions of the Question Paper of id in the parameter
        // GET Request : Questions
        public ActionResult Index(int id)
        {
            qpID = id;
            int questionPaperID = id;
            
            var qp = db.QuestionPapers.Find(questionPaperID);
            if (qp == null)
            {
                TempData["alertMsg"] = "Invalid Question Paper !!";
                TempData["alertType"] = "failed";
                return RedirectToAction("Index", "QuestionPapers");
            }
            ViewBag.questionPaperID = questionPaperID;
            var data = db.Questions.Where(model => model.quesPaperID == questionPaperID);
            return View(data);
        }



        // --------
        // Function to return the details of the question paper by its id
        // GET Request : Questions/Details/:id?
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question or Question Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "Questions");
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question or Question Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "Questions");
            }
            ViewBag.questionPaperIDInDetails = question.QuestionPaper.quesPaperID;
            return View(question);
        }



        // --------
        // Function to create a new Question Paper
        // GET Request : Questions/Create
        public ActionResult Create(int? id)
        {
            TempData["questionPaperIDInCreate"] = id;
            return View();
        }

        // --------
        // Function to submit the create question form data
        // POST Request : Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "quesId,quesText,quesOptions,quesAnswer")] Question question, int id)
        {

            TempData["questionPaperIDInCreate"] = id;

            if (question.quesText == "" || question.quesText == null || question.quesOptions=="" || question.quesOptions==null || question.quesAnswer == "" || question.quesAnswer == null)
            {
                TempData["alertMsg"] = "Your Form is Empty, Kindly Fill Your Form";
                TempData["alertType"] = "failed";
                return View(question);
            }
            if (question.quesOptions.EndsWith(","))
            {
                TempData["alertMsg"] = "Invalid Options ( Check for the Comma )";
                TempData["alertType"] = "failed";
                return View(question);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var allOptions = question.quesOptions.Split(',').Select(s => s.Trim()).ToList();
                    if (allOptions.Count() <= 1)
                    {
                        TempData["alertMsg"] = "Your Have Only Added 1 Option, Kindly add atleast 2 Options to add the Question !!";
                        TempData["alertType"] = "failed";
                        return View(question);
                    }
                    if (!allOptions.Contains(question.quesAnswer))
                    {
                        TempData["alertMsg"] = "Your Correct Option Doesn't Match Your Given Options !!";
                        TempData["alertType"] = "failed";
                        return View(question);
                    }
                    else
                    {
                        question.quesOptions = string.Join(",", allOptions);
                    }
                    question.quesCreationDate = DateTime.Now;
                    question.quesPaperID = id;

                    db.Questions.Add(question);
                    // Handle the Exception for any Exception from the SQL Queries or Database
                    try
                    {
                        if (db.SaveChanges() > 0)
                        {
                            // Sending the Status of Paper Creation
                            TempData["alertMsg"] = "Question Created Successfully !!";
                            TempData["alertType"] = "success";
                            return RedirectToAction("Index", new { id });
                        }
                        else
                        {
                            // Sending the Status of Paper Creation
                            TempData["alertMsg"] = "Question NOT Created Successfully !!";
                            TempData["alertType"] = "failed";
                            return View(question);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Sending the Status of Paper Creation
                        if (ex.InnerException.InnerException.Message.Contains("duplicate key") && ex.InnerException.InnerException.Message.Contains("Violation of UNIQUE KEY constraint"))
                        {
                            TempData["alertMsg"] = "Question NOT Created Successfully Because Question with this Text Already Exists !!";
                            TempData["alertType"] = "failed";
                        }
                        else
                        {
                            TempData["alertMsg"] = "Question NOT Created Successfully !!";
                            TempData["alertType"] = "failed";
                        }
                        return View(question);
                    }
                }
            }

            return View(question);
        }



        // --------
        // Function to return the question record details in form view to edit
        // GET Request : Questions/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question or Question Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "Questions");
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question or Question Doesn't Exists 123";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "Questions");
            }
            ViewBag.questionPaperIDInEdit = question.QuestionPaper.quesPaperID;
            return View(question);
        }



        // --------
        // Function to submit the edited question form data to update the question
        // POST Request : Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "quesId,quesText,quesOptions,quesAnswer,quesPaperID")] Question question, string paperid)
        {
            if (question.quesOptions.EndsWith(","))
            {
                TempData["alertMsg"] = "Invalid Options ( Check for the Comma )";
                TempData["alertType"] = "failed";
                return View(question);
            }
            if (ModelState.IsValid)
            {
                var allOptions = question.quesOptions.Split(',').Select(s => s.Trim()).ToList();
                if (allOptions.Count() <= 1)
                {
                    TempData["alertMsg"] = "Your Have Only Added 1 Option, Kindly add atleast 2 Options to add the Question !!";
                    TempData["alertType"] = "failed";
                    return View(question);
                }
                if (!allOptions.Contains(question.quesAnswer))
                {
                    TempData["alertMsg"] = "Your Correct Option Doesn't Match Your Given Options !!";
                    TempData["alertType"] = "failed";
                    return View(question);
                }
                else
                {
                    question.quesOptions = string.Join(",", allOptions);
                }
                question.quesUpdatedDate = DateTime.Now;

                //db.Questions.AddOrUpdate(question);
                db.Entry(question).State = EntityState.Modified;

                // Handle the Exception for any Exception from the SQL Queries or Database
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        // Sending the Status of Paper Creation
                        TempData["alertMsg"] = "Question Edited Successfully !!";
                        TempData["alertType"] = "success";
                        return RedirectToAction("Index", new { id = question.quesPaperID });

                    }
                    else
                    {
                        // Sending the Status of Paper Creation
                        TempData["alertMsg"] = "Question NOT Edited Successfully !!";
                        TempData["alertType"] = "failed";
                        return View(question);
                    }
                }
                catch (Exception ex)
                {
                    // Sending the Status of Paper Creation
                    if (ex.InnerException.InnerException.Message.Contains("duplicate key") && ex.InnerException.InnerException.Message.Contains("Violation of UNIQUE KEY constraint 'UQ__Question__651DD4F4179D3175'"))
                    {
                        TempData["alertMsg"] = "Question NOT Edited Successfully Because Question with this Text Already Exists !!";
                        TempData["alertType"] = "failed";
                    }
                    else
                    {
                        TempData["alertMsg"] = "Question NOT Edited Successfully !!";
                        TempData["alertType"] = "failed";
                    }
                    return View(question);
                }
            }
            return View(question);
        }



        // --------
        // Function to return the question record that has to be deleted
        // GET Request : Questions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question or Question Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "Questions");
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the Question or Question Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "Questions");
            }
            ViewBag.questionPaperIDInDelete = question.QuestionPaper.quesPaperID;
            return View(question);
        }



        // --------
        // Function to delete the question from the database
        // POST Request : Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Question question = db.Questions.Find(id);
            db.Questions.Remove(question);
            db.SaveChanges();
            TempData["alertMsg"] = "Question Deleted Successfully !!";
            TempData["alertType"] = "success";
            return RedirectToAction("Index", new { id = question.quesPaperID });
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
