using System;
using System.Collections.Generic;

namespace EmployeeAttendance.Models;

public partial class Attendance
{
    public long EmpId { get; set; }

    public DateOnly DateIn { get; set; }

    public DateOnly? DateOut { get; set; }

    public TimeOnly TimeIn { get; set; }

    public TimeOnly? TimeOut { get; set; }

    public virtual EmployeeList Emp { get; set; } = null!;
}
