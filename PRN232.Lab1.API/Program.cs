using System.Reflection;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PRN232.Lab1.API.Common;
using PRN232.Lab1.API.Mapping;
using PRN232.Lab1.Repositories;
using PRN232.Lab1.Repositories.Implementations;
using PRN232.Lab1.Repositories.Interfaces;
using PRN232.Lab1.Repositories.Seed;
using PRN232.Lab1.Services.Implementations;
using PRN232.Lab1.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ─── Routing + JSON ─────────────────────────────────────────────
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddControllers()
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
    });

// CORS for grader testing
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LinkBuilder>();
builder.Services.AddScoped<ResponseMappers>();

// ─── API Versioning ─────────────────────────────────────────────
builder.Services.AddApiVersioning(opts =>
{
    opts.DefaultApiVersion = new ApiVersion(1, 0);
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.ReportApiVersions = true;
    opts.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version")
    );
}).AddApiExplorer(opts =>
{
    opts.GroupNameFormat = "'v'VVV";
    opts.SubstituteApiVersionInUrl = true;
});

// ─── Swagger / OpenAPI ──────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 Lab 1 — LMS REST API",
        Version = "v1",
        Description = "Learning Management System REST API. " +
                      "3-layer architecture, 4 model types, HATEOAS HAL links, URL versioning. " +
                      "Built for PRN232 Lab 1 — SE173473.",
        Contact = new OpenApiContact { Name = "SE173473" }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    c.SupportNonNullableReferenceTypes();
    c.UseInlineDefinitionsForEnums();
});

// ─── DbContext ──────────────────────────────────────────────────
builder.Services.AddDbContext<LmsDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: null)));

// ─── Repositories ───────────────────────────────────────────────
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// ─── Services ───────────────────────────────────────────────────
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

var app = builder.Build();

// ─── Startup: migrate + seed with retry ─────────────────────────
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<LmsDbContext>();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    const int maxAttempts = 20;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation("Applying migrations (attempt {N}/{Max})...", attempt, maxAttempts);
            var strategy = db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await db.Database.MigrateAsync());
            await BogusSeeder.SeedAsync(db, logger);
            logger.LogInformation("DB ready.");
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning("DB not ready: {Msg}. Retrying in 5s...", ex.Message);
            await Task.Delay(5000);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DB initialization failed after {Max} attempts.", maxAttempts);
            break;
        }
    }
}

// ─── Middleware ─────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS API v1");
    c.DocumentTitle = "PRN232 Lab 1 — Swagger UI";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
