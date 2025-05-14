using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service) =>
            _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> Get() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CategoryDto>> Get(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            return dto is null
                ? NotFound(new { Message = $"Category {id} not found." })
                : Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Post(CreateCategoryDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, UpdateCategoryDto dto)
        {
            if (!await _service.UpdateAsync(id, dto))
                return NotFound(new { Message = $"Category {id} not found." });
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _service.DeleteAsync(id))
                return NotFound(new { Message = $"Category {id} not found." });
            return NoContent();
        }
    }
}
