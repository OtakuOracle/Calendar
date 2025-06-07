using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Models;

public partial class User7Context : DbContext
{
    public User7Context()
    {
    }

    public User7Context(DbContextOptions<User7Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentSubdivision> DepartmentSubdivisions { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeRelation> EmployeeRelations { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=45.67.56.214;Port=5421;Database=user7;Username=user7;Password=a8yLONBC");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Departmentid).HasName("department_pkey");

            entity.ToTable("department", "calend");

            entity.Property(e => e.Departmentid)
                .ValueGeneratedNever()
                .HasColumnName("departmentid");
            entity.Property(e => e.Departmentname)
                .HasMaxLength(100)
                .HasColumnName("departmentname");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<DepartmentSubdivision>(entity =>
        {
            entity.HasKey(e => e.Subdivisionid).HasName("department_subdivision_pkey");

            entity.ToTable("department_subdivision", "calend");

            entity.Property(e => e.Subdivisionid)
                .ValueGeneratedNever()
                .HasColumnName("subdivisionid");
            entity.Property(e => e.Departmentid).HasColumnName("departmentid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Headpositionid).HasColumnName("headpositionid");
            entity.Property(e => e.Subdivisionname)
                .HasMaxLength(100)
                .HasColumnName("subdivisionname");

            entity.HasOne(d => d.Department).WithMany(p => p.DepartmentSubdivisions)
                .HasForeignKey(d => d.Departmentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("department_subdivision_departmentid_fkey");

            entity.HasOne(d => d.Headposition).WithMany(p => p.DepartmentSubdivisions)
                .HasForeignKey(d => d.Headpositionid)
                .HasConstraintName("fk_head_position");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Employeeid).HasName("employee_pkey");

            entity.ToTable("employee", "calend");

            entity.Property(e => e.Employeeid)
                .ValueGeneratedNever()
                .HasColumnName("employeeid");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(50)
                .HasColumnName("firstname");
            entity.Property(e => e.IsManager).HasColumnName("is_manager");
            entity.Property(e => e.Lastname)
                .HasMaxLength(50)
                .HasColumnName("lastname");
            entity.Property(e => e.Middlename)
                .HasMaxLength(50)
                .HasColumnName("middlename");
            entity.Property(e => e.Mobiletel)
                .HasMaxLength(20)
                .HasColumnName("mobiletel");
            entity.Property(e => e.Office)
                .HasMaxLength(10)
                .HasColumnName("office");
            entity.Property(e => e.Personalnumber)
                .HasMaxLength(20)
                .HasColumnName("personalnumber");
            entity.Property(e => e.Positionid).HasColumnName("positionid");
            entity.Property(e => e.Subdivisionid).HasColumnName("subdivisionid");
            entity.Property(e => e.Worktel)
                .HasMaxLength(20)
                .HasColumnName("worktel");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Positionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_position_id");

            entity.HasOne(d => d.Subdivision).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Subdivisionid)
                .HasConstraintName("employee_subdivisionid_fkey");
        });

        modelBuilder.Entity<EmployeeRelation>(entity =>
        {
            entity.HasKey(e => e.Relationid).HasName("employee_relations_pkey");

            entity.ToTable("employee_relation", "calend");

            entity.Property(e => e.Relationid)
                .ValueGeneratedNever()
                .HasColumnName("relationid");
            entity.Property(e => e.Assistantid).HasColumnName("assistantid");
            entity.Property(e => e.Employeeid).HasColumnName("employeeid");
            entity.Property(e => e.Managerid).HasColumnName("managerid");

            entity.HasOne(d => d.Assistant).WithMany(p => p.EmployeeRelationAssistants)
                .HasForeignKey(d => d.Assistantid)
                .HasConstraintName("employee_relations_assistantid_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeRelationEmployees)
                .HasForeignKey(d => d.Employeeid)
                .HasConstraintName("employee_relations_employeeid_fkey");

            entity.HasOne(d => d.Manager).WithMany(p => p.EmployeeRelationManagers)
                .HasForeignKey(d => d.Managerid)
                .HasConstraintName("employee_relations_managerid_fkey");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Positionid).HasName("position_pkey");

            entity.ToTable("position", "calend");

            entity.Property(e => e.Positionid)
                .ValueGeneratedNever()
                .HasColumnName("positionid");
            entity.Property(e => e.Ismanager).HasColumnName("ismanager");
            entity.Property(e => e.Positionname)
                .HasMaxLength(100)
                .HasColumnName("positionname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
