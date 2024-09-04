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
            entity.HasKey(e => new { e.EmpId, e.DateIn }).HasName("PK__Attendan__E56FD4B951A446E9");

            entity.ToTable("Attendance");

            entity.HasOne(d => d.Emp).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__EmpId__4D94879B");
        });

        modelBuilder.Entity<EmployeeList>(entity =>
        {
            entity.HasKey(e => e.EmpId).HasName("PK__Employee__AF2DBB99C4C305C0");

            entity.ToTable("EmployeeList");

            entity.Property(e => e.EmpId).ValueGeneratedNever();
            entity.Property(e => e.Designation).HasMaxLength(50);
            entity.Property(e => e.EmpEmail).HasMaxLength(100);
            entity.Property(e => e.EmpName).HasMaxLength(100);
            entity.Property(e => e.EmpPhone).HasMaxLength(15);
            entity.Property(e => e.LoginStatus).HasMaxLength(20);
            entity.Property(e => e.UserRole).HasMaxLength(40);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
