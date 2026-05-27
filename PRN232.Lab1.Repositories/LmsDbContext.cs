using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Entities;

namespace PRN232.Lab1.Repositories;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Semester>(e =>
        {
            e.ToTable("Semester");
            e.HasKey(x => x.SemesterId);
            e.Property(x => x.SemesterName).HasMaxLength(100).IsRequired();
            e.Property(x => x.StartDate).HasColumnType("datetime2");
            e.Property(x => x.EndDate).HasColumnType("datetime2");
        });

        mb.Entity<Course>(e =>
        {
            e.ToTable("Course");
            e.HasKey(x => x.CourseId);
            e.Property(x => x.CourseName).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Semester).WithMany(s => s.Courses)
                .HasForeignKey(x => x.SemesterId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Subject>(e =>
        {
            e.ToTable("Subject");
            e.HasKey(x => x.SubjectId);
            e.Property(x => x.SubjectCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.SubjectName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.SubjectCode).IsUnique();
        });

        mb.Entity<Student>(e =>
        {
            e.ToTable("Student");
            e.HasKey(x => x.StudentId);
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100).IsRequired();
            e.Property(x => x.DateOfBirth).HasColumnType("datetime2");
            e.HasIndex(x => x.Email).IsUnique();
        });

        mb.Entity<Enrollment>(e =>
        {
            e.ToTable("Enrollment");
            e.HasKey(x => x.EnrollmentId);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.EnrollDate).HasColumnType("datetime2");
            e.HasOne(x => x.Student).WithMany(s => s.Enrollments)
                .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Course).WithMany(c => c.Enrollments)
                .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
