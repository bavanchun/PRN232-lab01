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
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _svc;
    private readonly ICourseService _courseSvc;
    private readonly ResponseMappers _map;
    private readonly LinkBuilder _links;

    public SemestersController(ISemesterService svc, ICourseService courseSvc, ResponseMappers map, LinkBuilder links)
    { _svc = svc; _courseSvc = courseSvc; _map = map; _links = links; }

    /// <summary>List semesters with search/sort/page/fields/expand.</summary>
    [HttpGet(Name = "semesters.List")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SemesterResponse>>), 200)]
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
            Links = _links.ForCollection("semesters.List", page.Page, page.PageSize, page.TotalPages,
                HttpContext.Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString()))
        });
    }

    /// <summary>Get a semester by id.</summary>
    [HttpGet("{id:int}", Name = "semesters.GetById")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand, [FromQuery] string? fields)
    {
        var result = await _svc.GetByIdAsync(id, expand);
        if (!result.Success || result.Data is null)
            return NotFound(ApiResponseFactory.Error(result.Message));
        var r = _map.Map(result.Data);
        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = FieldShaper.Shape(r, fields) });
    }

    /// <summary>Create a new semester.</summary>
    [HttpPost(Name = "semesters.Create")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest req)
    {
        var input = new SemesterBusiness { SemesterName = req.SemesterName, StartDate = req.StartDate, EndDate = req.EndDate };
        var result = await _svc.CreateAsync(input);
        if (!result.Success || result.Data is null)
            return BadRequest(ApiResponseFactory.Error(result.Message, result.Errors));
        var r = _map.Map(result.Data);
        return CreatedAtRoute("semesters.GetById", new { id = result.Data.SemesterId, version = "1.0" },
            ApiResponseFactory.Created(r, "Semester created"));
    }

    /// <summary>Update an existing semester.</summary>
    [HttpPut("{id:int}", Name = "semesters.Update")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSemesterRequest req)
    {
        var input = new SemesterBusiness { SemesterId = id, SemesterName = req.SemesterName, StartDate = req.StartDate, EndDate = req.EndDate };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null)
            return NotFound(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<SemesterResponse> { Success = true, Message = result.Message, Data = _map.Map(result.Data) });
    }

    /// <summary>Patch (partial update) a semester.</summary>
    [HttpPatch("{id:int}", Name = "semesters.Patch")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UpdateSemesterRequest>? patch)
    {
        if (patch is null) return BadRequest(ApiResponseFactory.Error("Patch document required"));
        patch.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var current = await _svc.GetByIdAsync(id, null);
        if (!current.Success || current.Data is null) return NotFound(ApiResponseFactory.Error(current.Message));
        var dto = new UpdateSemesterRequest { SemesterName = current.Data.SemesterName, StartDate = current.Data.StartDate, EndDate = current.Data.EndDate };
        patch.ApplyTo(dto, e => ModelState.AddModelError(e.AffectedObject?.GetType().Name ?? "patch", e.ErrorMessage));
        if (!ModelState.IsValid) return BadRequest(ApiResponseFactory.Error("Invalid patch", ModelState));
        var input = new SemesterBusiness { SemesterId = id, SemesterName = dto.SemesterName, StartDate = dto.StartDate, EndDate = dto.EndDate };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<SemesterResponse> { Success = true, Message = "Patched", Data = _map.Map(result.Data) });
    }

    /// <summary>Delete a semester.</summary>
    [HttpDelete("{id:int}", Name = "semesters.Delete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound(ApiResponseFactory.Error(result.Message));
    }

    /// <summary>List courses belonging to a semester (nested resource).</summary>
    [HttpGet("{id:int}/courses", Name = "semesters.Courses")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseResponse>>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ListCourses(int id, [FromQuery] QueryOptions opts, [FromQuery] string? fields)
    {
        var result = await _svc.ListCoursesAsync(id, opts);
        if (!result.Success || result.Data is null)
            return NotFound(ApiResponseFactory.Error(result.Message));
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
