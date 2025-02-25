using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/ingredient")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        /// <summary>
        /// Get paginated list of ingredients
        /// </summary>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetIngredients([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }
            var ingredients = await _ingredientService.GetIngredientsAsync(pageNumber, pageSize);
            return Ok(ingredients);
        }

        /// <summary>
        /// Get ingredient by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIngredientById(Guid id)
        {
            var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
            if (ingredient == null) return NotFound();
            return Ok(ingredient);
        }

        /// <summary>
        /// Create a new ingredient
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateIngredient([FromBody] IngredientRequestDTO ingredientDTO)
        {
            var createdIngredient = await _ingredientService.CreateIngredientAsync(ingredientDTO);
            return Ok(createdIngredient);
        }

        /// <summary>
        /// Update an ingredient
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIngredient(Guid id, [FromBody] IngredientResponseDTO ingredientDTO)
        {
            var updatedIngredient = await _ingredientService.UpdateIngredientAsync(id, ingredientDTO);
            if (updatedIngredient == null) return NotFound();
            return Ok(updatedIngredient);
        }

        /// <summary>
        /// Delete an ingredient
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngredient(Guid id)
        {
            await _ingredientService.DeleteIngredientAsync(id);
            return NoContent();
        }
    }
}
