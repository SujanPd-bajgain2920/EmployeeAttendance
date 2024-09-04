namespace EmployeeAttendance.Models
{
    public class AttendanceEdit
    {
        public long EmpId { get; set; }

        public DateOnly DateIn { get; set; }

        public DateOnly? DateOut { get; set; }

        public TimeOnly? TimeIn { get; set; }

        public TimeOnly? TimeOut { get; set; }

        public string Status { get; set; }

        public bool isOfficeIn { get; set; }

        public string employeeName { get; set; } = null!;
        public string post { get; set; } = null!;

        /* public DateTime OfficeOpeningTime { get; set; }
         public DateTime OfficeClosingTime { get; set; }
         public bool IsOfficeOpen { get; set; }
         public bool CanMarkAttendance { get; set; }

         public bool HasMarkedArrival { get; set; }*/

       
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public EmployeeListEdit Employee { get; set; } // Assuming you have an Employee class
        public List<AttendanceEdit> AttendanceRecords { get; set; }
        public int TotalPresent { get; set; }

    }

}