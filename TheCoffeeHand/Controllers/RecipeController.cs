using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/recipe")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] RecipeRequestDTO recipeDTO)
        {
            var recipe = await _recipeService.CreateRecipeAsync(recipeDTO);
            return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.Id }, recipe);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(Guid id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null) return NotFound();
            return Ok(recipe);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetRecipes()
        //{
        //    var recipes = await _recipeService.GetRecipesAsync();
        //    return Ok(recipes);
        //}

        [HttpGet("paginated")]
        public async Task<IActionResult> GetRecipesPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }
            var recipes = await _recipeService.GetRecipesAsync(pageNumber, pageSize);
            return Ok(recipes);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRecipe(Guid id, [FromBody] RecipeRequestDTO recipeDTO)
        {
            var updatedRecipe = await _recipeService.UpdateRecipeAsync(id, recipeDTO);
            return Ok(updatedRecipe);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(Guid id)
        {
            await _recipeService.DeleteRecipeAsync(id);
            return NoContent();
        }
    }
}
