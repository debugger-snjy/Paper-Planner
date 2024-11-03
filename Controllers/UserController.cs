using PaperPlanner_Application.Helpers;
using PaperPlanner_Application.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PaperPlanner_Application.Controllers
{
    public class UserController : Controller
    {
        PaperPlannerDBEntities db = new PaperPlannerDBEntities();



        // ---------
        // Function to List all the Users in the database
        // GET: User
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Index()
        {

            return View(db.Users.ToList());

        }



        // ---------
        // Function returns view to Create or Add the New User
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Create()
        {
            return View();

        }



        // ---------
        // Function to save the user details and add it to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Create(User newUser)
        {

            // Checking the Validation of the Form
            if (ModelState.IsValid)
            {
                newUser.password_hash = UtilityFunction.EncryptPassword(newUser.password_hash);
                newUser.createdDate = DateTime.Now;
                db.Users.Add(newUser);

                // TODO NOTE : We have to Convert the DateTime to datetime2 in the Database Manually !!
                try
                {
                    int rowAffected = db.SaveChanges();

                    if (rowAffected > 0)
                    {
                        // Showing the Alert Box on Inserting the User in the DB
                        TempData["alertMsg"] = "User is Added !!";
                        TempData["alertType"] = "success";

                        // Redirecting the User to Index of Todo Controller
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception)
                {
                    TempData["alertMsg"] = "User is NOT Added !!";
                    TempData["alertType"] = "danger";
                }
            }
            else
            {
                TempData["alertMsg"] = "Your Form is Invalid !!";
                TempData["alertType"] = "danger";
            }

            return View();


        }



        // ---------
        // Function returns view for Deletion Process to Display Data
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Delete(int? id)
        {

            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the User or User Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "User");
            }
            else
            {

                // Deleting the AnswerTable Related to User
                var userAnswers = db.AnswersTables.Where(q => q.UserID == id);
                foreach (var ans in userAnswers)
                {
                    db.AnswersTables.Remove(ans);
                }
                db.SaveChanges();

                // Deleting the Question Paper Attempted Records From the Table :
                var recordsInAttemptedpapertable = db.QuesPaper_Attempted_By_Users.Where(q => q.UserID == id);

                foreach (var record in recordsInAttemptedpapertable)
                {
                    db.QuesPaper_Attempted_By_Users.Remove(record);
                }

                // Deleting the Question Papers Related to User
                var userQuestionPapers = db.QuestionPapers.Where(q => q.qpCreatorID == id);

                foreach (var que in userQuestionPapers)
                {
                    // Deleting the Questions Related to User
                    var Questions = db.Questions.Where(q => q.quesPaperID == que.quesPaperID);
                    foreach (var oneQue in Questions)
                    {
                        var AnsQuestion = db.AnswersTables.Where(q => q.QuesID == oneQue.quesPaperID);
                        foreach (var item in AnsQuestion)
                        {
                            db.AnswersTables.Remove(item);
                        }
                        db.Questions.Remove(oneQue);
                    }
                    db.QuestionPapers.Remove(que);
                }
                db.SaveChanges();

                User deletedUser = db.Users.Where(model => model.userID == id).FirstOrDefault();
                return View(deletedUser);
            }

        }



        // ---------
        // Function redirect to page after Deleteing the Record on Delete Button from the database
        [HttpPost]
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Delete(int id)
        {
            User deletedRecord = db.Users.Find(id);
            if (deletedRecord == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the User or User Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "User");
            }

            db.Users.Remove(deletedRecord);

            int rowAffected = db.SaveChanges();

            if (rowAffected > 0)
            {
                TempData["alertMsg"] = "User Record Deleted Successfully";
                TempData["alertType"] = "success";

                // If the User Record Inserted Successfully then Redirecting it to the Index Page of User Controller
                return RedirectToAction("Index");
            }
            else
            {
                TempData["alertMsg"] = "User Record Deletion Failed";
                TempData["alertType"] = "danger";
            }

            return RedirectToAction("Index");

        }



        // ---------
        // Function to return view to show the user data in the edit form
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the User or User Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "User");
            }
            else
            {
                User editedUser = db.Users.Where(model => model.userID == id).FirstOrDefault();
                if (editedUser == null)
                {
                    // If the ID is null in the URL, then moving back to the index of the Question Papers
                    TempData["alertMsg"] = "Invalid ID for the User or User Doesn't Exists";
                    TempData["alertType"] = "failed";

                    return RedirectToAction("Index", "User");
                }
                return View(editedUser);
            }

        }



        // ---------
        // Function to save the new or updated data from the edit form on edit button click in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Edit(User editedUser)
        {
            editedUser.updatedDate = DateTime.Now;
            db.Entry(editedUser).State = EntityState.Modified;

            int rowAffected = db.SaveChanges();

            if (rowAffected > 0)
            {
                // Showing the Alert Box on Inserting the User in the DB
                TempData["alertMsg"] = "User Edited Successfully !!";
                TempData["alertType"] = "success";

                // Redirecting the User to Index of Todo Controller
                return RedirectToAction("Index");
            }
            else
            {
                TempData["alertMsg"] = "User is NOT Edited !!";
                TempData["alertType"] = "failed";
            }

            return View();

        }



        // ---------
        // Function returns the view to see the details of the user by its id
        // Filter that makes sure that the user is logged in
        [Authorize]
        public ActionResult Details(int id)
        {
            var userIdRecord = db.Users.Where(model => model.userID == id).FirstOrDefault();

            if (userIdRecord == null)
            {
                // If the ID is null in the URL, then moving back to the index of the Question Papers
                TempData["alertMsg"] = "Invalid ID for the User or User Doesn't Exists";
                TempData["alertType"] = "failed";

                return RedirectToAction("Index", "User");
            }
            return View(userIdRecord);

        }
    }
}