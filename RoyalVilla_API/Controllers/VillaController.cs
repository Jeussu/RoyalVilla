using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
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
        public async Task<ActionResult<IEnumerable<VillaDTO>>> Getvillas()
        {
            var vilas = await _db.Villas.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<VillaDTO>>(vilas));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VillaDTO>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Villa ID must be greater than 0");
                }


                var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound($"Villa with ID {id} not found");
                }

                return Ok(_mapper.Map<VillaDTO>(villa));
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while processing your request: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<VillaDTO>> CreateVilla(VillaCreateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest("Villa data is required");
                }

                var duplicateVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower());

                if (duplicateVilla != null)
                {
                    return Conflict($"A villa with the name '{villaDTO.Name}' already exists.");
                }

                Villa villa = _mapper.Map<Villa>(villaDTO);
                await _db.Villas.AddAsync(villa);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, _mapper.Map<VillaDTO>(villa));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while creating the villa: {ex.Message}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<VillaUpdateDTO>> UpdateVilla(int id, VillaUpdateDTO villaDTO)
        {
            try
            {
                if (villaDTO == null)
                {
                    return BadRequest("Villa data is required");
                }

                if (id != villaDTO.Id)
                {
                    return BadRequest("Villa ID in URL does not match villa ID in request body");
                }

                var existingVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return NotFound($"Villa with ID {id} not found");
                }

                var duplicateVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDTO.Name.ToLower()
                && u.Id != id);

                if (duplicateVilla != null)
                {
                    return Conflict($"A villa with the name '{villaDTO.Name}' already exists.");
                }

                _mapper.Map(villaDTO, existingVilla);
                existingVilla.UpdatedDate = DateTime.Now;

                await _db.SaveChangesAsync();

                return Ok(villaDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while updating the villa: {ex.Message}");
            }
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return NotFound($"Villa with ID {id} not found");
                }

                _db.Villas.Remove(existingVilla);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while deleting the villa: {ex.Message}");
            }
        }
    }
}




