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
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _svc;
    private readonly ResponseMappers _map;
    private readonly LinkBuilder _links;

    public SubjectsController(ISubjectService svc, ResponseMappers map, LinkBuilder links)
    { _svc = svc; _map = map; _links = links; }

    /// <summary>List subjects with search/sort/page/fields.</summary>
    [HttpGet(Name = "subjects.List")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubjectResponse>>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> List([FromQuery] QueryOptions opts, [FromQuery] string? fields)
    {
        var result = await _svc.ListAsync(opts);
        if (!result.Success || result.Data is null)
            return BadRequest(ApiResponseFactory.Error(result.Message));
        var page = result.Data;
        var responses = page.Items.Select(_map.Map).ToList();
        var shaped = FieldShaper.ShapeMany(responses, fields).ToList();
        return Ok(new ApiResponse<object>
        {
            Success = true, Message = result.Message, Data = shaped,
            Pagination = new() { Page = page.Page, PageSize = page.PageSize, TotalItems = page.TotalItems, TotalPages = page.TotalPages },
            Links = _links.ForCollection("subjects.List", page.Page, page.PageSize, page.TotalPages,
                HttpContext.Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString()))
        });
    }

    /// <summary>Get a subject by id.</summary>
    [HttpGet("{id:int}", Name = "subjects.GetById")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? fields)
    {
        var result = await _svc.GetByIdAsync(id);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        var r = _map.Map(result.Data);
        return Ok(new ApiResponse<object> { Success = true, Message = result.Message, Data = FieldShaper.Shape(r, fields) });
    }

    /// <summary>Create a new subject.</summary>
    [HttpPost(Name = "subjects.Create")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest req)
    {
        var input = new SubjectBusiness { SubjectCode = req.SubjectCode, SubjectName = req.SubjectName, Credit = req.Credit };
        var result = await _svc.CreateAsync(input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return CreatedAtRoute("subjects.GetById", new { id = result.Data.SubjectId, version = "1.0" },
            ApiResponseFactory.Created(_map.Map(result.Data), "Subject created"));
    }

    /// <summary>Update a subject.</summary>
    [HttpPut("{id:int}", Name = "subjects.Update")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest req)
    {
        var input = new SubjectBusiness { SubjectId = id, SubjectCode = req.SubjectCode, SubjectName = req.SubjectName, Credit = req.Credit };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return NotFound(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<SubjectResponse> { Success = true, Message = result.Message, Data = _map.Map(result.Data) });
    }

    /// <summary>Patch a subject.</summary>
    [HttpPatch("{id:int}", Name = "subjects.Patch")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UpdateSubjectRequest>? patch)
    {
        if (patch is null) return BadRequest(ApiResponseFactory.Error("Patch document required"));
        patch.ContractResolver = new CamelCasePropertyNamesContractResolver();
        var current = await _svc.GetByIdAsync(id, null);
        if (!current.Success || current.Data is null) return NotFound(ApiResponseFactory.Error(current.Message));
        var dto = new UpdateSubjectRequest { SubjectCode = current.Data.SubjectCode, SubjectName = current.Data.SubjectName, Credit = current.Data.Credit };
        patch.ApplyTo(dto, e => ModelState.AddModelError("patch", e.ErrorMessage));
        if (!ModelState.IsValid) return BadRequest(ApiResponseFactory.Error("Invalid patch", ModelState));
        var input = new SubjectBusiness { SubjectId = id, SubjectCode = dto.SubjectCode, SubjectName = dto.SubjectName, Credit = dto.Credit };
        var result = await _svc.UpdateAsync(id, input);
        if (!result.Success || result.Data is null) return BadRequest(ApiResponseFactory.Error(result.Message));
        return Ok(new ApiResponse<SubjectResponse> { Success = true, Message = "Patched", Data = _map.Map(result.Data) });
    }

    /// <summary>Delete a subject.</summary>
    [HttpDelete("{id:int}", Name = "subjects.Delete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteAsync(id);
        return result.Success ? NoContent() : NotFound(ApiResponseFactory.Error(result.Message));
    }
}
