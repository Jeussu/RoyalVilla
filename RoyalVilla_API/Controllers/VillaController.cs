using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
    //[Authorize(Roles = "Customer,Admin")]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public VillaController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> Getvillas()
        {
            var vilas = await _db.Villas.ToListAsync();
            var dtoResponseVilla = _mapper.Map<IEnumerable<VillaDTO>>(vilas);
            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoResponseVilla, "Villas retrieved successfully");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0."));
                }

                var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} not found"));
                }

                return Ok(ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villa), "Record retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa: " + ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
                }

                var duplicateVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower());

                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"A villa with the name '{villaDTO.Name}' already exists."));
                }

                Villa villa = _mapper.Map<Villa>(villaDTO);
                await _db.Villas.AddAsync(villa);
                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaDTO>.CreatedAt(_mapper.Map<VillaDTO>(villa), "Villa created successfully");
                return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while creating the villa: " + ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, VillaUpdateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));
                }

                if (id != villaDTO.Id)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa ID in URL does not match villa ID in request body"));
                }

                var existingVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} not found"));
                }

                var duplicateVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower()
                && u.Id != id);

                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"A villa with the name '{villaDTO.Name}' already exists."));
                }

                _mapper.Map(villaDTO, existingVilla);
                existingVilla.UpdatedDate = DateTime.Now;

                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villaDTO), "Villa updated successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while updating the villa: " + ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa with ID {id} not found"));
                }

                _db.Villas.Remove(existingVilla);
                await _db.SaveChangesAsync();

                var response = ApiResponse<object>.NoContent("Villa deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occurred while deleting the villa: " + ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}




