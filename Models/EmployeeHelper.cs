using System.Security.Claims;

namespace EmployeeAttendance.Models
{
    public static class EmployeeHelper
    {
        public static int GetCurrentEmpId(HttpContext httpContext)
        {
            var identity = httpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var empIdClaim = identity.FindFirst(ClaimTypes.Name); // Retrieve EmpId stored in Name claim
                if (empIdClaim != null && int.TryParse(empIdClaim.Value, out var empId))
                {
                    return empId;
                }
            }
            throw new InvalidOperationException("Employee ID claim not found or invalid.");
        }
    }
}

