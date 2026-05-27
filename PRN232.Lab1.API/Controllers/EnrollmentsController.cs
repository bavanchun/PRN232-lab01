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
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _svc;
    private readonly ResponseMappers _map;
    private readonly LinkBuilder _links;

    public EnrollmentsController(IEnrollmentService svc, ResponseMappers map, LinkBuilder links)
    { _svc = svc; _map = map; _links = links; }

    /// <summary>List enrollments with search (status), sort, page, fields, expand=student|course.</summary>
    [HttpGet(Name = "enrollments.List")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentResponse>>), 200)]
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
            Links = _links.ForCollection("enrollments.List", page.Page, page.PageSize, page.TotalPages,
                HttpContext.Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString()))
        });
    }

    /// <summary>Get enrollment by id; supports expand=student|course.</summary>
    [HttpGet("{id:int}", Name = "enrollments.GetById")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand, [FromQuery] string? fields)
    {
        var result = await _svc.GetByIdAsync(id, expand);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        var r = _map.Map(result.Data);
        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = FieldShaper.Shape(r, fields) });
    }

    /// <summary>Create a new enrollment.</summary>
    [HttpPost(Name = "enrollments.Create")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest req)
    {
        var input = new EnrollmentBusiness { StudentId = req.StudentId, CourseId = req.CourseId, EnrollDate = req.EnrollDate, Status = req.Status };
        var result = await _svc.CreateAsync(input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return CreatedAtRoute("enrollments.GetById", new { id = result.Data.EnrollmentId, version = "1.0" },
            ApiResponseFactory.Created(_map.Map(result.Data), "Enrollment created"));
    }

    /// <summary>Update an enrollment.</summary>
    [HttpPut("{id:int}", Name = "enrollments.Update")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentRequest req)
    {
        var input = new EnrollmentBusiness { EnrollmentId = id, StudentId = req.StudentId, CourseId = req.CourseId, EnrollDate = req.EnrollDate, Status = req.Status };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<EnrollmentResponse> { Success = true, Message = result.Message, Data = _map.Map(result.Data) });
    }

    /// <summary>Patch an enrollment.</summary>
    [HttpPatch("{id:int}", Name = "enrollments.Patch")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UpdateEnrollmentRequest>? patch)
    {
        if (patch is null) return BadRequest(ApiResponseFactory.Error("Patch document required"));
        patch.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var current = await _svc.GetByIdAsync(id, null);
        if (!current.Success || current.Data is null) return NotFound(ApiResponseFactory.Error(current.Message));
        var dto = new UpdateEnrollmentRequest { StudentId = current.Data.StudentId, CourseId = current.Data.CourseId, EnrollDate = current.Data.EnrollDate, Status = current.Data.Status };
        patch.ApplyTo(dto, e => ModelState.AddModelError("patch", e.ErrorMessage));
        if (!ModelState.IsValid) return BadRequest(ApiResponseFactory.Error("Invalid patch", ModelState));
        var input = new EnrollmentBusiness { EnrollmentId = id, StudentId = dto.StudentId, CourseId = dto.CourseId, EnrollDate = dto.EnrollDate, Status = dto.Status };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<EnrollmentResponse> { Success = true, Message = "Patched", Data = _map.Map(result.Data) });
    }

    /// <summary>Delete an enrollment.</summary>
    [HttpDelete("{id:int}", Name = "enrollments.Delete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound(ApiResponseFactory.Error(result.Message));
    }
}
