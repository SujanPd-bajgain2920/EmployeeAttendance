using EmployeeAttendance.Models;
using EmployeeAttendance.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace EmployeeAttendance.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly EmployeeManagementSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;

        public AdminController(EmployeeManagementSystemContext context, DataSecurityProvider p, IDataProtectionProvider provider, IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
            _protector = provider.CreateProtector(p.Key);
        }
        public IActionResult Index()
        {
            // Get the current date
            var utcNow = DateTime.UtcNow;
            var nepaliOffset = TimeSpan.FromHours(5) + TimeSpan.FromMinutes(45);
            var nepaliTime = utcNow + nepaliOffset;
            var currentDate = DateOnly.FromDateTime(nepaliTime);

            // Fetch all employees with User role
            var employees = (from employee in _context.EmployeeLists
                             where employee.UserRole == "User"
                             select employee).ToList();

            // Fetch today's attendance for these employees
            var attendanceToday = (from attendance in _context.Attendances
                                   where attendance.DateIn == currentDate
                                   select new
                                   {
                                       EmpId = attendance.EmpId,
                                       TimeIn = attendance.TimeIn
                                   }).ToList();

            // Create a dictionary for quick lookup
            var attendanceDict = attendanceToday.ToDictionary(a => a.EmpId, a => a.TimeIn);

            // Create a list to hold the final result
            var attendanceList = new List<AttendanceEdit>();

            foreach (var employee in employees)
            {
                // Make 'TimeIn' a nullable type
                TimeOnly? timeIn = attendanceDict.TryGetValue(employee.EmpId, out var ti) ? ti : (TimeOnly?)null;

                // Determine the status of the employee
                var status = timeIn == null ? "Absent" : "Present";

                attendanceList.Add(new AttendanceEdit
                {
                    EmpId = employee.EmpId,
                    employeeName = employee.EmpName,
                    post = employee.Designation,
                    TimeIn = timeIn,  // Nullable 'TimeOnly?' will hold either a value or null
                    Status = status
                });
            }

            // Calculate summary
            var totalEmployees = employees.Count;
            var totalPresent = attendanceList.Count(a => a.Status == "Present");
            var totalAbsent = totalEmployees - totalPresent;

            ViewBag.TotalEmployees = totalEmployees;
            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalAbsent = totalAbsent;

            ViewBag.AttendanceToday = attendanceList;

            return View();
        }

        // only current date record
        public IActionResult EmployeeReport(int empId, DateTime? startDate, DateTime? endDate)
        {
            // Fetch employee details
            var employee = _context.EmployeeLists
                .Where(e => e.EmpId == empId)
                .Select(e => new
                {
                    e.EmpName,
                    e.EmpEmail,
                    e.Designation,
                    e.EmpPhone,
                    e.LoginStatus,
                    e.ProfilePicture
                })
                .FirstOrDefault();

            if (employee == null)
            {
                // Handle case when employee is not found
                return NotFound();
            }

            // Get the attendance records for the specified employee and date range
            var attendanceList = _context.Attendances
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

            // Calculate total present and absent
            var totalPresent = attendanceList.Count(a => a.DateOut.HasValue);
            var totalAbsent = attendanceList.Count(a => !a.DateOut.HasValue);

            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalAbsent = totalAbsent;
            ViewBag.EmployeeId = empId;
            ViewBag.EmployeeName = employee.EmpName;
            ViewBag.EmployeeEmail = employee.EmpEmail;
            ViewBag.EmployeeDesignation = employee.Designation;
            ViewBag.EmployeePhone = employee.EmpPhone;
            ViewBag.LoginStatus = employee.LoginStatus;
            ViewBag.EmployeePhotoUrl = employee.ProfilePicture;

            return View(attendanceList);
        }


        // all date record
        public async Task<IActionResult> AttendanceReport(DateOnly? reportDate)
        {
            if (reportDate == null)
            {
                return View(new List<AttendanceEdit>());
            }

            var date = reportDate.Value.ToDateTime(TimeOnly.MinValue);

            // Fetch all records first
            var allAttendanceRecords = await _context.Attendances
                .Include(a => a.Emp)
                .ToListAsync();

            // Filter in-memory
            var filteredRecords = allAttendanceRecords
                .Where(a => a.DateIn.ToDateTime(TimeOnly.MinValue).Date == date.Date ||
                            (a.DateOut.HasValue && a.DateOut.Value.ToDateTime(TimeOnly.MaxValue).Date == date.Date))
                .Select(a => new AttendanceEdit
                {
                    EmpId = a.EmpId,
                    DateIn = a.DateIn,
                    DateOut = a.DateOut,
                    TimeIn = a.TimeIn,
                    TimeOut = a.TimeOut,
                    employeeName = a.Emp.EmpName,
                    post = a.Emp.Designation
                })
                .ToList();
            ViewData["ReportDate"] = reportDate?.ToString("yyyy-MM-dd");
            return View(filteredRecords);
        }


        public IActionResult DownloadPdf(int empId, DateTime? startDate, DateTime? endDate)
        {
            // Fetch attendance records
            var attendanceRecords = _context.Attendances
                .Where(a => a.EmpId == empId)
                .Where(a => !startDate.HasValue || a.DateIn >= DateOnly.FromDateTime(startDate.Value))
                .Where(a => !endDate.HasValue || a.DateIn <= DateOnly.FromDateTime(endDate.Value))
                .Select(a => new AttendanceEdit
                {
                    DateIn = a.DateIn,
                    TimeIn = a.TimeIn,
                    DateOut = a.DateOut,
                    TimeOut = a.TimeOut,
                    
                    employeeName = a.Emp.EmpName,
                    post = a.Emp.Designation
                })
                .ToList();

            // Fetch employee details
            var employee = _context.EmployeeLists
                .Where(e => e.EmpId == empId)
                .Select(e => new EmployeeListEdit
                {
                    EmpId = e.EmpId,
                    EmpName = e.EmpName,
                    Designation = e.Designation,
                    EmpPhone = e.EmpPhone,
                    EmpEmail = e.EmpEmail,
                    // Add other properties if needed
                })
                .FirstOrDefault();

            if (employee == null)
            {
                return NotFound();
            }

            // Calculate total present days
            var totalPresent = attendanceRecords.Count(a => a.DateOut.HasValue);

            // Prepare the ViewModel
            var model = new AttendanceEdit
            {
                Employee = employee,
                AttendanceRecords = attendanceRecords,
                TotalPresent = totalPresent
            };

            // Set IsPdf flag to indicate that this is for a PDF export
            ViewData["IsPdf"] = true;

           
            // Use Rotativa to generate a PDF
            var pdfResult = new ViewAsPdf("EmployeeAttendanceReportPdf", model)
            {
                FileName = "EmployeeAttendanceReport.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking" // Optional: Adjust rendering options
            };

            return pdfResult;
        }


        public IActionResult DownloadReport(DateOnly? reportDate)
        {
            // Check if reportDate is provided
            if (reportDate == null)
            {
                return BadRequest("Report date is required.");
            }

            var date = reportDate.Value.ToDateTime(TimeOnly.MinValue);

            // Fetch all records first
            var allAttendanceRecords = _context.Attendances
                .Include(a => a.Emp)
                .ToList();

            // Filter records based on the selected date
            var filteredRecords = allAttendanceRecords
                .Where(a => a.DateIn.ToDateTime(TimeOnly.MinValue).Date == date.Date ||
                            (a.DateOut.HasValue && a.DateOut.Value.ToDateTime(TimeOnly.MaxValue).Date == date.Date))
                .Select(a => new AttendanceEdit
                {
                    EmpId = a.EmpId,
                    DateIn = a.DateIn,
                    DateOut = a.DateOut,
                    TimeIn = a.TimeIn,
                    TimeOut = a.TimeOut,
                    employeeName = a.Emp.EmpName,
                    post = a.Emp.Designation,
                    Status = a.DateOut.HasValue ? "Present" : "Absent"
                })
                .ToList();

            // Calculate summary
            var totalPresent = filteredRecords.Count(a => a.DateOut.HasValue);

            // Prepare the ViewModel
            var model = new AttendanceEdit
            {
                AttendanceRecords = filteredRecords,
                TotalPresent = totalPresent,
                StartDate = date.Date,
                EndDate = date.Date
            };

            // Set IsPdf flag to indicate that this is for a PDF export
            ViewData["IsPdf"] = true;

            // Use Rotativa to generate a PDF
            var pdfResult = new ViewAsPdf("AttendanceReportPdf", model)
            {
                FileName = "AttendanceReport.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A1,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking" // Optional: Adjust rendering options
            };

            return pdfResult;
        }



        // edit user profile
       

    }






}

