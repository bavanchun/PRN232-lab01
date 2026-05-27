using Asp.Versioning;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using PRN232.Lab1.API.Common;
using PRN232.Lab1.API.Mapping;
using PRN232.Lab1.API.Models.Request;
using PRN232.Lab1.API.Models.Response;
using PRN232.Lab1.Services.Interfaces;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _svc;
    private readonly ResponseMappers _map;
    private readonly LinkBuilder _links;

    public StudentsController(IStudentService svc, ResponseMappers map, LinkBuilder links)
    { _svc = svc; _map = map; _links = links; }

    /// <summary>List students with search (name/email), sort, page, fields, expand=enrollments.</summary>
    [HttpGet(Name = "students.List")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponse>>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> List([FromQuery] QueryOptions opts, [FromQuery] string? fields)
    {
        var result = await _svc.ListAsync(opts);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        var page = result.Data;
        var responses = page.Items.Select(_map.Map).ToList();
        var shaped = FieldShaper.ShapeMany(responses, fields).ToList();
        return Ok(new ApiResponse<object>
        {
            Success = true, Message = result.Message, Data = shaped,
            Pagination = new() { Page = page.Page, PageSize = page.PageSize, TotalItems = page.TotalItems, TotalPages = page.TotalPages },
            Links = _links.ForCollection("students.List", page.Page, page.PageSize, page.TotalPages,
                HttpContext.Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString()))
        });
    }

    /// <summary>Get a student by id; supports expand=enrollments.</summary>
    [HttpGet("{id:int}", Name = "students.GetById")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand, [FromQuery] string? fields)
    {
        var result = await _svc.GetByIdAsync(id, expand);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        var r = _map.Map(result.Data);
        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = FieldShaper.Shape(r, fields) });
    }

    /// <summary>Create a new student.</summary>
    [HttpPost(Name = "students.Create")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest req)
    {
        var input = new StudentBusiness { FullName = req.FullName, Email = req.Email, DateOfBirth = req.DateOfBirth };
        var result = await _svc.CreateAsync(input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return CreatedAtRoute("students.GetById", new { id = result.Data.StudentId, version = "1.0" },
            ApiResponseFactory.Created(_map.Map(result.Data), "Student created"));
    }

    /// <summary>Update an existing student.</summary>
    [HttpPut("{id:int}", Name = "students.Update")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest req)
    {
        var input = new StudentBusiness { StudentId = id, FullName = req.FullName, Email = req.Email, DateOfBirth = req.DateOfBirth };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<StudentResponse> { Success = true, Message = result.Message, Data = _map.Map(result.Data) });
    }

    /// <summary>Patch a student.</summary>
    [HttpPatch("{id:int}", Name = "students.Patch")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UpdateStudentRequest>? patch)
    {
        if (patch is null) return BadRequest(ApiResponseFactory.Error("Patch document required"));
        patch.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var current = await _svc.GetByIdAsync(id, null);
        if (!current.Success || current.Data is null) return NotFound(ApiResponseFactory.Error(current.Message));
        var dto = new UpdateStudentRequest { FullName = current.Data.FullName, Email = current.Data.Email, DateOfBirth = current.Data.DateOfBirth };
        patch.ApplyTo(dto, e => ModelState.AddModelError("patch", e.ErrorMessage));
        if (!ModelState.IsValid) return BadRequest(ApiResponseFactory.Error("Invalid patch", ModelState));
        var input = new StudentBusiness { StudentId = id, FullName = dto.FullName, Email = dto.Email, DateOfBirth = dto.DateOfBirth };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<StudentResponse> { Success = true, Message = "Patched", Data = _map.Map(result.Data) });
    }

    /// <summary>Delete a student.</summary>
    [HttpDelete("{id:int}", Name = "students.Delete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound(ApiResponseFactory.Error(result.Message));
    }

    /// <summary>List enrollments for a student (nested).</summary>
    [HttpGet("{id:int}/enrollments", Name = "students.Enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentResponse>>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ListEnrollments(int id, [FromQuery] QueryOptions opts, [FromQuery] string? fields)
    {
        var result = await _svc.ListEnrollmentsAsync(id, opts);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        var page = result.Data;
        var responses = page.Items.Select(_map.Map).ToList();
        var shaped = FieldShaper.ShapeMany(responses, fields).ToList();
        return Ok(new ApiResponse<object>
        {
            Success = true, Message = result.Message, Data = shaped,
            Pagination = new() { Page = page.Page, PageSize = page.PageSize, TotalItems = page.TotalItems, TotalPages = page.TotalPages }
        });
    }
}
