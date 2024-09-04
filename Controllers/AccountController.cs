using EmployeeAttendance.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EmployeeAttendance.Models;
using System.Net.Mail;
using System.Net;

namespace EmployeeAttendance.Controllers
{
    public class AccountController : Controller
    {

        private readonly EmployeeManagementSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;

        public AccountController(EmployeeManagementSystemContext context, IWebHostEnvironment env, DataSecurityProvider key, IDataProtectionProvider provider)
        {
            _context = context;
            _env = env;
            _protector = provider.CreateProtector(key.Key);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register(EmployeeListEdit u)
        {

            try
            {
                var users = _context.EmployeeLists.Where(x => x.EmpEmail == u.EmpEmail).FirstOrDefault();
                if (users == null)
                {
                    short maxid;
                    if (_context.EmployeeLists.Any())
                        maxid = Convert.ToInt16(_context.EmployeeLists.Max(x => x.EmpId) + 1);
                    else
                        maxid = 1;
                    u.EmpId = maxid;


                    if (u.EmpFile != null)
                    {
                        string fileName = "EmpImage" + Guid.NewGuid() + Path.GetExtension(u.EmpFile.FileName);
                        string filePath = Path.Combine(_env.WebRootPath, "EmpImage", fileName);
                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            u.EmpFile.CopyTo(stream);
                        }
                        u.ProfilePicture = fileName;
                    }


                    EmployeeList employeeList = new()
                    {
                        EmpId = u.EmpId,
                        EmpPhone = u.EmpPhone,
                        EmpEmail = u.EmpEmail,
                        EmpName = u.EmpName,
                        Designation = u.Designation,
                        ProfilePicture = u.ProfilePicture,
                        LoginPassword = _protector.Protect(u.LoginPassword),
                        LoginStatus = "Active",
                        UserRole = "User" 
                    };


                    _context.Add(employeeList);
                    _context.SaveChanges();



                    return RedirectToAction("Login", "Account");

                }
                else
                {
                    ModelState.AddModelError("", "User already exist with this email.!");
                    return View(u);
                }
            }
            catch
            {
                ModelState.AddModelError("", "User Registration Failed. Please try again");
                return View(u);
            }
        }

        //partial view
        public IActionResult ProfileImage()
        {
            var p = _context.EmployeeLists.Where(u => u.EmpId.Equals(Convert.ToInt16(User.Identity!.Name))).FirstOrDefault();
            ViewData["img"] = p.ProfilePicture;
            return PartialView("_Profile");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(EmployeeListEdit uEdit)
        {
            var users = _context.EmployeeLists.ToList();
            if (users != null)
            {

                var u = users.Where(x => x.EmpEmail.ToUpper().Equals(uEdit.EmpEmail.ToUpper()) && _protector.Unprotect(x.LoginPassword).Equals(uEdit.LoginPassword)).FirstOrDefault();
                if (u != null)
                {
                    List<Claim> claims = new()
                    {
                        new Claim(ClaimTypes.Name,u.EmpId.ToString()),
                         new Claim(ClaimTypes.Role,u.UserRole),
                        new Claim("EmpName",u.EmpName),
                        new Claim("image",u.ProfilePicture),
                        new Claim("email",u.EmpEmail),
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity));

                    return RedirectToAction("Dashboard");

                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid User");

            }
            return View(uEdit);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            if(User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
           
        }

       


        // change password after login
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(ChangePassword c)
        {
            

            var u = _context.EmployeeLists.Where(e => e.EmpId == Convert.ToInt16(User.Identity!.Name)).First();
            if (_protector.Unprotect(u.LoginPassword) != c.CurrentPassword)
            {
                ModelState.AddModelError("", "Check your current password");
            }
            else
            {
                if (c.NewPassword == c.ConfirmPassword)
                {
                    u.LoginPassword = _protector.Protect(c.NewPassword);
                    _context.Update(u);
                    _context.SaveChanges();
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Confirm Password doesnot match");
                    return View(c);
                }

            }
            ModelState.AddModelError("", "Password Changed Failed. Please Try Again!");
            return View();
        }

        // reset password before login
        [HttpGet]

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(EmployeeListEdit edit)
        {

            if (edit.EmpEmail != null)
            {
                Random r = new Random();
                HttpContext.Session.SetString("token", r.Next(9999).ToString());
                var token = HttpContext.Session.GetString("token");
                var user = _context.EmployeeLists.Where(u => u.EmpEmail == edit.EmpEmail).FirstOrDefault();
                if (user != null)
                {
                    SmtpClient s = new()
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        Credentials = new NetworkCredential("ghastlybarely2356@gmail.com", "pfkz vkkg jrpg xlpn"),
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network
                    };

                    MailMessage m = new()
                    {
                        From = new MailAddress("ghastlybarely2356@gmail.com"),
                        Subject = "Forgot Password Token",
                        Body = $@"<p class='text-red-800' style='background-color:red;'>Forgot Password</p>
                        <a href='https://localhost:44321/Account/ResetPassword?UserId=0&EmailAddress={user.EmpEmail}&EmailToken={_protector.Protect(token)}' style='background-color:blue;' >ResetPassword</a>:{token}",
                        IsBodyHtml = true,

                    };


                    m.To.Add(user.EmpEmail);
                    s.Send(m);
                    // return Json("success");
                    return RedirectToAction("VerifyToken", new { email = user.EmpEmail });

                }
                else
                {
                    ModelState.AddModelError("", "This Email is not registered Email.");
                    return View(edit);
                }
            }
            return Json("Failed");
        }

        // token verification 
        [HttpGet]
        public IActionResult VerifyToken(string email)
        {
            return View(new EmployeeListEdit { EmpEmail = email });
        }

        [HttpPost]
        public IActionResult VerifyToken(EmployeeListEdit e)
        {
            var token = HttpContext.Session.GetString("token");

            if (token == e.EmailToken)
            {
                var et = _protector.Protect(e.EmailToken!);
                return RedirectToAction("ResetPassword", new EmployeeListEdit { EmpEmail = e.EmpEmail, EmailToken = et });
            }
            else
            {
                return Json("Failed");
            }
        }

        // reset password
        [HttpGet]
        public IActionResult ResetPassword(EmployeeListEdit e)
        {
            try
            {
                // return Json(e);
                var token = HttpContext.Session.GetString("token");
                var eToken = _protector.Unprotect(e.EmailToken);
                if (token == eToken)
                {
                    return View(new ChangePassword { EmailAddress = e.EmpEmail });
                }
                else
                {
                    return RedirectToAction("ForgotPassword");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ForgotPassword");
            }
        }


        [HttpPost]
        public IActionResult ResetPassword(ChangePassword model)
        {


            if (model.NewPassword == model.ConfirmPassword)
            {
                var user = _context.EmployeeLists.FirstOrDefault(u => u.EmpEmail == model.EmailAddress);
                if (user != null)
                {
                    user.LoginPassword = _protector.Protect(model.NewPassword);
                    _context.Update(user);
                    _context.SaveChanges();
                    return RedirectToAction("Login");
                }
            }
            else
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View(model);
            }


            // return RedirectToAction("ForgotPassword");
            return Json("error");
        }

    }
}
