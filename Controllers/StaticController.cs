using EmployeeAttendance.Models;
using EmployeeAttendance.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAttendance.Controllers
{
    public class StaticController : Controller
    {

        private readonly EmployeeManagementSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;

        public StaticController(EmployeeManagementSystemContext context, IWebHostEnvironment env, DataSecurityProvider key, IDataProtectionProvider provider)
        {
            _context = context;
            _env = env;
            _protector = provider.CreateProtector(key.Key);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
