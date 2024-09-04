using System;
using System.Collections.Generic;

namespace EmployeeAttendance.Models;

public partial class EmployeeList
{
    public long EmpId { get; set; }

    public string EmpName { get; set; } = null!;

    public string EmpPhone { get; set; } = null!;

    public string EmpEmail { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string LoginPassword { get; set; } = null!;

    public string? ProfilePicture { get; set; }

    public string UserRole { get; set; } = null!;

    public string LoginStatus { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
