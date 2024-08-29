using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAttendance.Models;

public partial class EmployeeManagementSystemContext : DbContext
{
    public EmployeeManagementSystemContext()
    {
    }

    public EmployeeManagementSystemContext(DbContextOptions<EmployeeManagementSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<EmployeeList> EmployeeLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=Conn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => new { e.EmpId, e.DateIn }).HasName("PK__Attendan__E7DAEE86033F2A09");

            entity.ToTable("Attendance");

            entity.Property(e => e.EmpId).HasColumnName("Emp_Id");
            entity.Property(e => e.DateIn).HasColumnName("Date_In");
            entity.Property(e => e.DateOut).HasColumnName("Date_Out");
            entity.Property(e => e.TimeIn).HasColumnName("Time_In");
            entity.Property(e => e.TimeOut).HasColumnName("Time_Out");

            entity.HasOne(d => d.Emp).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Emp_I__3A81B327");
        });

        modelBuilder.Entity<EmployeeList>(entity =>
        {
            entity.HasKey(e => e.EmpId).HasName("PK__Employee__262359ABDA41D930");

            entity.ToTable("EmployeeList");

            entity.Property(e => e.EmpId)
                .ValueGeneratedNever()
                .HasColumnName("Emp_Id");
            entity.Property(e => e.Designation).HasMaxLength(50);
            entity.Property(e => e.EmpEmail)
                .HasMaxLength(100)
                .HasColumnName("Emp_Email");
            entity.Property(e => e.EmpName)
                .HasMaxLength(100)
                .HasColumnName("Emp_Name");
            entity.Property(e => e.EmpPhone)
                .HasMaxLength(15)
                .HasColumnName("Emp_Phone");
            entity.Property(e => e.LoginPassword).HasColumnName("Login_Password");
            entity.Property(e => e.LoginStatus)
                .HasMaxLength(20)
                .HasColumnName("Login_Status");
            entity.Property(e => e.ProfilePicture).HasColumnName("Profile_Picture");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
