using System.ComponentModel.DataAnnotations;

namespace EmployeeAttendance.Models
{
    public class EmployeeListEdit
    {
        public long EmpId { get; set; }

        public string EmpName { get; set; } = null!;

        public string EmpPhone { get; set; } = null!;

        public string EmpEmail { get; set; } = null!;

        public string Designation { get; set; } = null!;

        public string LoginPassword { get; set; } = null!;

        public string? ProfilePicture { get; set; }

        public string LoginStatus { get; set; } = null!;

        [DataType(DataType.Upload)]
        public IFormFile? EmpFile { get; set; } = null!;
    }
}
