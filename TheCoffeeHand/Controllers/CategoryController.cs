using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestDTO categoryDTO)
        {
            var category = await _categoryService.CreateCategoryAsync(categoryDTO);

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var paginatedCategories = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize);
            return Ok(paginatedCategories);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryRequestDTO categoryDTO)
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDTO);
            return Ok(updatedCategory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}
