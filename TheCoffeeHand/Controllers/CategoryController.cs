using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Interfaces.Interfaces;
using Interfracture.DTOs;
using Interfracture.PaggingItems;

namespace TheCoffeeHand.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IRedisCacheServices _cacheService;

        public CategoryController(ICategoryService categoryService, IRedisCacheServices cacheService)
        {
            _categoryService = categoryService;
            _cacheService = cacheService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestDTO categoryDTO)
        {
            var category = await _categoryService.CreateCategoryAsync(categoryDTO);

            // Invalidate all cached categories pages
            await RemoveAllCategoryCacheAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            string cacheKey = $"category_{id}";

            // Try to get from cache first
            var category = await _cacheService.GetAsync<CategoryResponseDTO>(cacheKey);
            if (category != null)
                return Ok(category);

            // Fetch from database if not found in cache
            category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            // Store in cache for future requests
            await _cacheService.SetAsync(cacheKey, category, TimeSpan.FromMinutes(30));

            return Ok(category);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            string cacheKey = $"categories_{pageNumber}_{pageSize}";

            // Try to get from cache first
            var cachedCategories = await _cacheService.GetAsync<PaginatedList<CategoryResponseDTO>>(cacheKey);
            if (cachedCategories != null)
                return Ok(cachedCategories);

            // Fetch from database if not found in cache
            var paginatedCategories = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize);

            // Store in cache for future requests
            await _cacheService.SetAsync(cacheKey, paginatedCategories, TimeSpan.FromMinutes(30));

            return Ok(paginatedCategories);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryRequestDTO categoryDTO)
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDTO);

            // Invalidate cache for this category
            await _cacheService.RemoveAsync($"category_{id}");

            // Invalidate all cached category pages
            await RemoveAllCategoryCacheAsync();

            return Ok(updatedCategory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            if (!deleted)
                return NotFound();

            // Invalidate cache for this category
            await _cacheService.RemoveAsync($"category_{id}");

            // Invalidate all cached category pages
            await RemoveAllCategoryCacheAsync();

            return Ok(new { message = "Category deleted successfully." });
        }


        private async Task RemoveAllCategoryCacheAsync()
        {
            var cacheKeys = await _cacheService.GetKeysAsync("categories_*"); // Get all cached category keys
            foreach (var key in cacheKeys)
            {
                await _cacheService.RemoveAsync(key);
            }
        }
    }
}
