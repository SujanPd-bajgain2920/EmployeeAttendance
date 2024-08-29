using EmployeeAttendance.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EmployeeAttendance.Models;

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
                        LoginStatus = "Active"
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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
