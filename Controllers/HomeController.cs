using EmployeeAttendance.Models;
using EmployeeAttendance.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace EmployeeAttendance.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmployeeManagementSystemContext _Context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;

        public HomeController(EmployeeManagementSystemContext context, IWebHostEnvironment env, DataSecurityProvider key, IDataProtectionProvider provider, IHttpContextAccessor httpContextAccessor)
        {
            _Context = context;
            _env = env;
            _protector = provider.CreateProtector(key.Key);
            _httpContextAccessor = httpContextAccessor;
        }

        /* // GET: Attendance Page
         [HttpGet]
         public IActionResult Index()
         {
             // Get current UTC time and adjust for Nepal time zone (UTC +5:45)
             var currentUtcTime = DateTime.UtcNow;
             var nepalTime = currentUtcTime.AddMinutes(345);

             var empId = Convert.ToInt16(User.Identity!.Name);// EmployeeHelper.GetCurrentEmpId(_httpContextAccessor.HttpContext); // Get current empId

             // Check if the employee has already marked their arrival today
             var hasMarkedArrival = _context.Attendances
                 .Any(a => a.EmpId == empId && a.DateIn == DateOnly.FromDateTime(nepalTime) && a.TimeIn != null);

             var viewModel = new AttendanceEdit
             {
                 HasMarkedArrival = hasMarkedArrival
             };

             return View(viewModel);
         }

         // POST: Mark Attendance
         [HttpPost]
         public IActionResult Index(string status)
         {
             // Get current UTC time and adjust for Nepal time zone (UTC +5:45)
             var currentUtcTime = DateTime.UtcNow;
             var nepalTime = currentUtcTime.AddMinutes(345);

             var empId = Convert.ToInt16(User.Identity!.Name);//EmployeeHelper.GetCurrentEmpId(_httpContextAccessor.HttpContext);
             var currentDate = DateOnly.FromDateTime(nepalTime);

             // Check if an attendance record exists for the current date and employee
             var existingRecord = _context.Attendances
                 .FirstOrDefault(a => a.EmpId == empId && a.DateIn == currentDate);

             if (status == "In")
             {
                 if (existingRecord == null)
                 {
                     // Create a new attendance record
                     var newAttendance = new Attendance
                     {
                         EmpId = empId,
                         DateIn = currentDate,
                         TimeIn = TimeOnly.FromDateTime(nepalTime) // Convert Nepal time to TimeOnly
                     };
                     _context.Attendances.Add(newAttendance);
                 }
                 else
                 {
                     ModelState.AddModelError("", "You have already marked your arrival today.");
                 }
             }
             else if (status == "Out")
             {
                 if (existingRecord != null && existingRecord.TimeOut == null)
                 {
                     // Update the existing record with departure time
                     existingRecord.DateOut = currentDate;
                     existingRecord.TimeOut = TimeOnly.FromDateTime(nepalTime); // Convert Nepal time to TimeOnly
                     _context.Attendances.Update(existingRecord);
                 }
                 else
                 {
                     ModelState.AddModelError("", "You must mark your arrival before marking departure or you have already marked your departure.");
                 }
             }
             _context.SaveChanges();
             return Content("success");
         }*/


        /*[Authorize]
        public IActionResult Index()
        {
            var empId = Convert.ToInt16(User.Identity!.Name);
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(345));
            var previousDate = currentDate.AddDays(-1);

            // Check for incomplete attendance record from the previous day
            var incompleteRecord = _Context.Attendances.FirstOrDefault(ea => ea.EmpId == empId && ea.DateIn == previousDate && ea.DateOut == null);

            if (incompleteRecord != null)
            {
                // Mark the toggle switch as Office Out if there's an incomplete record
                ViewBag.isOfficeIn = false;
            }
            else
            {
                // Check if the employee is currently clocked in
                var employeeAttendance = _Context.Attendances.FirstOrDefault(ea => ea.EmpId == empId && ea.DateIn == currentDate && ea.DateOut == null);
                ViewBag.isOfficeIn = employeeAttendance == null;
            }

            ViewBag.Attendance = _Context.Attendances.Where(em => em.EmpId == empId);

            // Calculate total working days by counting unique dates
            var totalWorkingDays = _Context.Attendances
                                            .Where(a => a.EmpId == empId && a.DateOut != null)
                                            .Select(a => a.DateIn)
                                            .Distinct()
                                            .Count();

            ViewBag.TotalWorkingDays = totalWorkingDays;


            return View();
        }*/

        [Authorize]
        public IActionResult Index()
        {
            var empId = Convert.ToInt16(User.Identity!.Name);
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(345)); // Nepal Time
            var previousDate = currentDate.AddDays(-1);

            // Check for incomplete attendance record from the previous day
            var incompleteRecord = _Context.Attendances.FirstOrDefault(ea => ea.EmpId == empId && ea.DateIn == previousDate && ea.DateOut == null);

            if (incompleteRecord != null)
            {
                // Mark incomplete records from the previous day as "N/A" conceptually (keep as null in the database)
                incompleteRecord.DateOut = null;
                incompleteRecord.TimeOut = null;
                _Context.SaveChanges();

                // Mark the toggle switch as Office Out if there's an incomplete record
                ViewBag.isOfficeIn = false;
            }
            else
            {
                // Check if the employee is currently clocked in
                var employeeAttendance = _Context.Attendances.FirstOrDefault(ea => ea.EmpId == empId && ea.DateIn == currentDate && ea.DateOut == null);
                ViewBag.isOfficeIn = employeeAttendance == null;
            }

            // Set Attendance records in ViewBag, converting nulls to "N/A" for display
            var attendanceRecords = _Context.Attendances
                .Where(em => em.EmpId == empId)
                .Select(a => new
                {
                    a.EmpId,
                    a.DateIn,
                    a.TimeIn,
                    DateOut = a.DateOut.HasValue ? a.DateOut.Value.ToString("yyyy-MM-dd") : "N/A",
                    TimeOut = a.TimeOut.HasValue ? a.TimeOut.Value.ToString("HH:mm") : "N/A"
                });

            ViewBag.Attendance = attendanceRecords;

            // Calculate total working days by counting unique dates, excluding "N/A"
            var totalWorkingDays = _Context.Attendances
                                            .Where(a => a.EmpId == empId && a.DateOut != null)
                                            .Select(a => a.DateIn)
                                            .Distinct()
                                            .Count();

            ViewBag.TotalWorkingDays = totalWorkingDays;

            return View();
        }


        [HttpPost]
        public IActionResult Index(AttendanceEdit attendance)
        {
            var CurrentTime = DateTime.UtcNow;
            var nepalTime = CurrentTime.AddMinutes(345);

            var currentDate = DateOnly.FromDateTime(nepalTime);
            var employeeAttendance = _Context.Attendances.FirstOrDefault(ea => ea.EmpId == attendance.EmpId && ea.DateIn == currentDate);

            if (attendance.isOfficeIn)
            {
                if (employeeAttendance != null && employeeAttendance.DateOut != null)
                {
                    TempData["ErrorMessage"] = "You have already clocked in for today.";
                }
                else if (employeeAttendance == null)
                {
                    // Office In: Record new attendance entry
                    Attendance record = new Attendance()
                    {
                        EmpId = attendance.EmpId,
                        DateIn = currentDate,
                        TimeIn = TimeOnly.FromDateTime(nepalTime)
                    };

                    _Context.Attendances.Add(record);
                    _Context.SaveChanges();
                    TempData["SuccessMessage"] = "Clocked in successfully for today!";
                }
            }
            else
            {
                if (employeeAttendance != null && employeeAttendance.DateOut == null)
                {
                    // Office Out: Update the existing attendance entry
                    employeeAttendance.DateOut = currentDate;
                    employeeAttendance.TimeOut = TimeOnly.FromDateTime(nepalTime);
                    _Context.SaveChanges();
                    TempData["SuccessMessage"] = "Clocked out successfully for today!";
                }
                else
                {
                    TempData["ErrorMessage"] = "No clock-in record found for today.";
                }
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: AttendanceReport
        public IActionResult Report(DateTime? startDate, DateTime? endDate)
        {
            var empId = Convert.ToInt16(User.Identity!.Name); //EmployeeHelper.GetCurrentEmpId(_httpContextAccessor.HttpContext);
                                                              //return Json(empId);


            var attendanceList = _Context.Attendances
                    .Where(a => a.EmpId == empId)
                    .Where(a => !startDate.HasValue || a.DateIn >= DateOnly.FromDateTime(startDate.Value))
                    .Where(a => !endDate.HasValue || a.DateIn <= DateOnly.FromDateTime(endDate.Value))
                    .Select(a => new AttendanceEdit
                    {
                        DateIn = a.DateIn,
                        TimeIn = a.TimeIn,
                        DateOut = a.DateOut,
                        TimeOut = a.TimeOut
                    })
                    .ToList();
            // Count total present and absent based on DateOut
            var totalPresent = attendanceList.Count(a => a.DateOut.HasValue);
            var totalAbsent = attendanceList.Count(a => !a.DateOut.HasValue);

            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalAbsent = totalAbsent;

            return View(attendanceList);
         }

        [HttpGet]
        public IActionResult Edit(int id)
        {

            var user = _Context.EmployeeLists.Where(x => x.EmpId == Convert.ToInt16(User.Identity!.Name)).FirstOrDefault();
            EmployeeListEdit userEdit = new()
            {
                EmpId = user.EmpId,
                EmpName = user.EmpName,
                EmpEmail = user.EmpEmail,
                EmpPhone = user.EmpPhone,
                ProfilePicture = user.ProfilePicture,
                Designation = user.Designation
            };
            return View(userEdit);
        }
        [Authorize]
        [HttpPost]
        public IActionResult Edit(EmployeeListEdit u)
        {
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

            EmployeeList user = new()
            {
                EmpId = u.EmpId,
                EmpName = u.EmpName,
                EmpEmail = u.EmpEmail,
                EmpPhone = u.EmpPhone,
                ProfilePicture = u.ProfilePicture
            };

            _Context.Update(user);
            _Context.SaveChangesAsync();
            return RedirectToAction("ProfileUpdate");

        }

    }




}