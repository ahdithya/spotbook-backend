using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBook.Application.DTOs.Request.Space;
using SpotBook.Application.DTOs.Response.Space;
using SpotBook.Application.Interfaces.Spaces;
using SpotBook.Application.Services.Spaces;
using SpotBook.Domain.Entities;

namespace SpotBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpaceController : ControllerBase
    {
        private readonly ISpaceService _spaceService;
        public SpaceController(ISpaceService spaceService)
        {
            _spaceService = spaceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpaceResponse>>> GetAll()
        {
            var result = await _spaceService.GetAllActiveSpacesAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SpaceResponse>> GetById(Guid id)
        {
            var result = await _spaceService.GetSpaceByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Ruangan tidak ditemukan." });
            }
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<SpaceResponse>> Create([FromBody] CreateSpaceRequest request)
        {
            try
            {
                var result = await _spaceService.CreateSpaceAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SpaceResponse>> Update(Guid id, [FromBody] UpdateSpaceRequest request)
        {
            try
            {
                var result = await _spaceService.UpdateSpaceAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _spaceService.DeleteSpaceAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Ruangan tidak ditemukan." });
            }
            return NoContent();
        }
    }




}