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
public class CoursesController : ControllerBase
{
    private readonly ICourseService _svc;
    private readonly ResponseMappers _map;
    private readonly LinkBuilder _links;

    public CoursesController(ICourseService svc, ResponseMappers map, LinkBuilder links)
    { _svc = svc; _map = map; _links = links; }

    /// <summary>List courses with search/sort/page/fields/expand=semester|enrollments.</summary>
    [HttpGet(Name = "courses.List")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseResponse>>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> List([FromQuery] QueryOptions opts, [FromQuery] string? fields)
    {
        var result = await _svc.ListAsync(opts);
        if (!result.Success || result.Data is null)
            return BadRequest(ApiResponseFactory.Error(result.Message, result.Errors));
        var page = result.Data;
        var responses = page.Items.Select(_map.Map).ToList();
        var shaped = FieldShaper.ShapeMany(responses, fields).ToList();
        return Ok(new ApiResponse<object>
        {
            Success = true, Message = result.Message, Data = shaped,
            Pagination = new() { Page = page.Page, PageSize = page.PageSize, TotalItems = page.TotalItems, TotalPages = page.TotalPages },
            Links = _links.ForCollection("courses.List", page.Page, page.PageSize, page.TotalPages,
                HttpContext.Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString()))
        });
    }

    /// <summary>Get a course by id; supports expand=semester.</summary>
    [HttpGet("{id:int}", Name = "courses.GetById")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand, [FromQuery] string? fields)
    {
        var result = await _svc.GetByIdAsync(id, expand);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        var r = _map.Map(result.Data);
        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = FieldShaper.Shape(r, fields) });
    }

    /// <summary>Create a new course.</summary>
    [HttpPost(Name = "courses.Create")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest req)
    {
        var input = new CourseBusiness { CourseName = req.CourseName, SemesterId = req.SemesterId };
        var result = await _svc.CreateAsync(input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return CreatedAtRoute("courses.GetById", new { id = result.Data.CourseId, version = "1.0" },
            ApiResponseFactory.Created(_map.Map(result.Data), "Course created"));
    }

    /// <summary>Update an existing course.</summary>
    [HttpPut("{id:int}", Name = "courses.Update")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequest req)
    {
        var input = new CourseBusiness { CourseId = id, CourseName = req.CourseName, SemesterId = req.SemesterId };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<CourseResponse> { Success = true, Message = result.Message, Data = _map.Map(result.Data) });
    }

    /// <summary>Patch a course.</summary>
    [HttpPatch("{id:int}", Name = "courses.Patch")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UpdateCourseRequest>? patch)
    {
        if (patch is null) return BadRequest(ApiResponseFactory.Error("Patch document required"));
        patch.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var current = await _svc.GetByIdAsync(id, null);
        if (!current.Success || current.Data is null) return NotFound(ApiResponseFactory.Error(current.Message));
        var dto = new UpdateCourseRequest { CourseName = current.Data.CourseName, SemesterId = current.Data.SemesterId };
        patch.ApplyTo(dto, e => ModelState.AddModelError("patch", e.ErrorMessage));
        if (!ModelState.IsValid) return BadRequest(ApiResponseFactory.Error("Invalid patch", ModelState));
        var input = new CourseBusiness { CourseId = id, CourseName = dto.CourseName, SemesterId = dto.SemesterId };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<CourseResponse> { Success = true, Message = "Patched", Data = _map.Map(result.Data) });
    }

    /// <summary>Delete a course.</summary>
    [HttpDelete("{id:int}", Name = "courses.Delete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound(ApiResponseFactory.Error(result.Message));
    }

    /// <summary>List enrollments for a course (nested). Supports expand=student.</summary>
    [HttpGet("{id:int}/enrollments", Name = "courses.Enrollments")]
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
