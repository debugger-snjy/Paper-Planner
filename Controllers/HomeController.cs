using Microsoft.Ajax.Utilities;
using PaperPlanner_Application.Helpers;
using PaperPlanner_Application.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using static System.Net.WebRequestMethods;

namespace PaperPlanner_Application.Controllers
{
    public class HomeController : Controller
    {
        PaperPlannerDBEntities _context = new PaperPlannerDBEntities();



        // --------
        // Function to return View or Action Result For Login Form or Login Page (Home Page)
        // GET Request
        [AllowAnonymous]
        public ActionResult Index()
        {
            
            return View();
        }

        
        
        // --------
        // Function to Submit the Login Form & return View or Action Result For Login Form
        // POST Request
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(User loginUser)
        {
            string captchaValue = Request.Form["userCaptcha"];
            Debug.WriteLine(captchaValue);
            string capDigit1 = Request.Form["capDigit1"];
            string capDigit2 = Request.Form["capDigit2"];
            string capDigit3 = Request.Form["capDigit3"];
            string capDigit4 = Request.Form["capDigit4"];
            string capDigit5 = Request.Form["capDigit5"];

            string finalCaptcha = $"{capDigit1}{capDigit2}{capDigit3}{capDigit4}{capDigit5}";

            if (captchaValue == finalCaptcha)
            {
                // Getting the user From the Database
                User dbRecord = _context.Users.FirstOrDefault(model => model.username == loginUser.username);

                // Checking for the NULL Record !
                try
                {
                    if (dbRecord != null)
                    {
                        // Decrypting the User Password in Database
                        string originalPassword = UtilityFunction.DecryptPassword(dbRecord.password_hash);

                        // Comparing the Passwords
                        if (originalPassword == loginUser.password_hash)
                        {
                            // Clearing the Fields
                            ModelState.Clear();

                            TempData["alertMsg"] = "Login Successfully !!";
                            TempData["alertType"] = "success";

                            // We will redirect to the Home Screen
                            // TODO !

                            if (dbRecord.role == "Admin")
                            {
                                //RedirectToAction("Index", "Admin");

                                FormsAuthentication.SetAuthCookie(dbRecord.userID.ToString(), false);
                                Session["tempData"] = dbRecord;

                                // Return a JavaScript snippet that redirects after 2 seconds
                                return Content("<script>window.setTimeout(function(){window.location.href = '" + Url.Action("Index", "Admin") + "';}, 2000);</script>");
                            }
                            else if (dbRecord.role == "Teacher")
                            {
                                //RedirectToAction("Index", "Admin");

                                FormsAuthentication.SetAuthCookie(dbRecord.userID.ToString(), false);
                                Session["tempData"] = dbRecord;

                                // Return a JavaScript snippet that redirects after 2 seconds
                                return Content("<script>window.setTimeout(function(){window.location.href = '" + Url.Action("Index", "Teacher") + "';}, 2000);</script>");
                            }
                            else if (dbRecord.role == "Student")
                            {
                                //RedirectToAction("Index", "Admin");

                                FormsAuthentication.SetAuthCookie(dbRecord.userID.ToString(), false);
                                Session["tempData"] = dbRecord;

                                // Return a JavaScript snippet that redirects after 2 seconds
                                return Content("<script>window.setTimeout(function(){window.location.href = '" + Url.Action("Index", "Student") + "';}, 2000);</script>");
                            }

                        }
                        else
                        {
                            TempData["alertMsg"] = "Login Failed - Incorrect Password !!";
                            TempData["alertType"] = "failed";
                        }
                    }
                    else
                    {
                        TempData["alertMsg"] = "Login Failed - User with this credentials doesn't exists !!";
                        TempData["alertType"] = "failed";
                    }
                }
                catch (Exception)
                {
                    TempData["alertMsg"] = "Login Failed !!";
                    TempData["alertType"] = "failed";
                }

            }

            else
            {

                if (captchaValue == "" || captchaValue == null)
                {
                    TempData["alertMsg"] = "Captcha is Empty !!";
                    TempData["alertType"] = "failed";
                }
                else
                {
                    TempData["alertMsg"] = "Invalid Captcha !!";
                    TempData["alertType"] = "failed";
                }
            }

            return View();
        }

        
        
        // --------
        // Function to return View or Action Result For Signup Form or Signup Page
        // GET Request
        [AllowAnonymous]
        public ActionResult Signup()
        {
            return View();
        }



        // --------
        // Function to Submit the Signup Form & return View or Action Result For Signup Form or Signup Page
        // POST Request
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Signup(User newUser)
        {
            // If all form fields are Valid
            if (ModelState.IsValid)
            {
                // Encrypting the Passwords
                string hashedPassword = UtilityFunction.EncryptPassword(newUser.password_hash);

                // Changing the passwor_hash from plain text to the Hash Value
                newUser.password_hash = hashedPassword;
                newUser.createdDate = DateTime.Now;

                // Variable to get the row affected after adding the record
                int rowAffected = 0;
                // Saving the New User Record
                try
                {
                    _context.Users.Add(newUser);
                    rowAffected = _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.InnerException.Message.Contains("Violation of UNIQUE KEY constraint 'UQ__Users__AB6E6164F58F64D1'. Cannot insert duplicate key in object 'dbo.Users'"))
                    {
                        TempData["alertMsg"] = "Account With this Email ID already Exists !";
                        TempData["alertType"] = "failed";
                        return View();
                    }
                    else
                    {
                        TempData["alertMsg"] = "Registration Failed";
                        TempData["alertType"] = "failed";
                    }
                    return View();
                }

                if (rowAffected > 0)
                {
                    // Clearing the Fields
                    ModelState.Clear();

                    TempData["alertMsg"] = "Registration Successfull !!";
                    TempData["alertType"] = "success";
                }
                else
                {
                    TempData["alertMsg"] = "Registration Failed";
                    TempData["alertType"] = "failed";
                }
            }
            else
            {
                TempData["alertMsg"] = "Registration Failed";
                TempData["alertType"] = "failed";
            }
            return View();
        }



        // --------
        // Function to return Logout and clear all the Session and TempData and Move to Index Page of Home Controller
        // GET Request
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            // Clear session
            Session.Clear();

            // Clearing Temp Data
            TempData.Clear();

            // Optionally, you can also abandon the session
            // Session.Abandon();

            // Redirect to the login page or another appropriate page
            return RedirectToAction("Index", "Home");
        }



        // --------
        // Function to return View for the About Page of the Website
        // GET Request
        [AllowAnonymous]
        public ActionResult About()
        {
            return View();
        }



        // --------
        // Function to return View for the Contact Page of the Website
        // GET Request
        [AllowAnonymous]
        public ActionResult Contact()
        {
            return View();
        }
    }
}