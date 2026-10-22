using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.Common.Exceptions;
using SpotBook.Application.Common.Models;
using SpotBook.Application.DTOs.Request.Space;
using SpotBook.Application.DTOs.Response.Space;
using SpotBook.Application.Interfaces.Spaces;

namespace SpotBook.Api.Controllers;

public class SpaceController(ISpaceService spaceService) : BaseApiController
{
    private readonly ISpaceService _spaceService = spaceService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SpaceResponse>>>> GetAll()
    {
        var result = await _spaceService.GetAllActiveSpacesAsync();
        return HandleSuccess(result, "Daftar ruangan berhasil diambil.");
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SpaceResponse>>> GetById(Guid id)
    {
        var result = await _spaceService.GetSpaceByIdAsync(id)
            ?? throw new NotFoundException("Ruangan tidak ditemukan.");
        return HandleSuccess(result, "Detail ruangan berhasil diambil.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SpaceResponse>>> Create([FromBody] CreateSpaceRequest request)
    {
        var result = await _spaceService.CreateSpaceAsync(request);
        return HandleCreated(nameof(GetById), new { id = result.Id }, result, "Ruangan berhasil ditambahkan.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SpaceResponse>>> Update(Guid id, [FromBody] UpdateSpaceRequest request)
    {
        var result = await _spaceService.UpdateSpaceAsync(id, request);
        return HandleSuccess(result, "Ruangan berhasil diperbarui.");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var success = await _spaceService.DeleteSpaceAsync(id);
        if (!success)
        {
            throw new NotFoundException("Ruangan tidak ditemukan.");
        }
        return HandleSuccess("Ruangan berhasil dinonaktifkan.");
    }
}
