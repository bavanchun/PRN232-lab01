using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PRN232.Lab1.Repositories.Entities;

namespace PRN232.Lab1.Repositories.Seed;

public static class BogusSeeder
{
    public static async Task SeedAsync(LmsDbContext db, ILogger logger)
    {
        var allSeeded = await db.Semesters.AnyAsync()
                     && await db.Subjects.AnyAsync()
                     && await db.Courses.AnyAsync()
                     && await db.Students.AnyAsync()
                     && await db.Enrollments.AnyAsync();
        if (allSeeded)
        {
            logger.LogInformation("DB already fully seeded — skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            try
            {
                // 1) Semesters — 5
                var semesters = new List<Semester>
                {
                    new() { SemesterName = "Spring 2024", StartDate = new(2024,1,1),  EndDate = new(2024,4,30) },
                    new() { SemesterName = "Summer 2024", StartDate = new(2024,5,1),  EndDate = new(2024,8,31) },
                    new() { SemesterName = "Fall 2024",   StartDate = new(2024,9,1),  EndDate = new(2024,12,31) },
                    new() { SemesterName = "Spring 2025", StartDate = new(2025,1,1),  EndDate = new(2025,4,30) },
                    new() { SemesterName = "Summer 2025", StartDate = new(2025,5,1),  EndDate = new(2025,8,31) }
                };
                db.Semesters.AddRange(semesters);
                await db.SaveChangesAsync();

                // 2) Subjects — 10
                Randomizer.Seed = new Random(173473);
                var subjectNames = new[] {
                    "Mathematics", "Physics", "Programming", "Database Systems",
                    "Web Development", "Software Engineering", "Algorithms",
                    "Operating Systems", "Networks", "Artificial Intelligence"
                };
                var subjects = subjectNames.Select((name, i) => new Subject
                {
                    SubjectCode = $"SUB{i + 1:000}",
                    SubjectName = name,
                    Credit = new Random(173473 + i).Next(2, 6)
                }).ToList();
                db.Subjects.AddRange(subjects);
                await db.SaveChangesAsync();

                // 3) Courses — 20
                var courseFaker = new Faker<Course>()
                    .UseSeed(173473)
                    .RuleFor(c => c.CourseName, f => $"{f.PickRandom(subjectNames)} {f.PickRandom("A","B","C","D")}")
                    .RuleFor(c => c.SemesterId, f => f.PickRandom(semesters).SemesterId);
                var courses = courseFaker.Generate(20);
                db.Courses.AddRange(courses);
                await db.SaveChangesAsync();

                // 4) Students — 50 (unique emails)
                var studentFaker = new Faker<Student>()
                    .UseSeed(173473)
                    .RuleFor(s => s.FullName, f => f.Name.FullName())
                    .RuleFor(s => s.Email, (f, s) => f.Internet.Email(s.FullName).ToLower())
                    .RuleFor(s => s.DateOfBirth, f => f.Date.Between(new DateTime(2000,1,1), new DateTime(2006,12,31)));
                var students = studentFaker.Generate(50);
                var emailSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in students)
                {
                    var baseEmail = s.Email;
                    var n = 1;
                    while (!emailSet.Add(s.Email))
                    {
                        var at = baseEmail.IndexOf('@');
                        s.Email = $"{baseEmail[..at]}{n}{baseEmail[at..]}";
                        n++;
                    }
                }
                db.Students.AddRange(students);
                await db.SaveChangesAsync();

                // 5) Enrollments — 500 (unique StudentId+CourseId)
                var statuses = new[] { "Active", "Completed", "Dropped", "Pending" };
                var rng = new Random(173473);
                var enrollments = new List<Enrollment>();
                var seen = new HashSet<(int, int)>();
                while (enrollments.Count < 500)
                {
                    var s = students[rng.Next(students.Count)];
                    var c = courses[rng.Next(courses.Count)];
                    if (!seen.Add((s.StudentId, c.CourseId))) continue;
                    enrollments.Add(new Enrollment
                    {
                        StudentId = s.StudentId,
                        CourseId = c.CourseId,
                        EnrollDate = new DateTime(2024, 1, 1).AddDays(rng.Next(0, 600)),
                        Status = statuses[rng.Next(statuses.Length)]
                    });
                }
                db.Enrollments.AddRange(enrollments);
                await db.SaveChangesAsync();

                await tx.CommitAsync();
                logger.LogInformation("Seeded: {Sem} semesters, {Subj} subjects, {Cou} courses, {Stu} students, {Enr} enrollments.",
                    semesters.Count, subjects.Count, courses.Count, students.Count, enrollments.Count);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                logger.LogError(ex, "Seed failed — transaction rolled back.");
                throw;
            }
        });
    }
}
