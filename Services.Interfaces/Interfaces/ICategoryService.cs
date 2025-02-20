using Interfracture.Base;
using Interfracture.DTOs;


namespace Services.Interfaces.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDTO> CreateCategoryAsync(CategoryRequestDTO categoryDTO);
        Task<CategoryResponseDTO?> GetCategoryByIdAsync(string id);
        Task<BasePaginatedList<CategoryResponseDTO>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        Task<bool> UpdateCategoryAsync(string id, CategoryRequestDTO categoryDTO);
        Task<bool> DeleteCategoryAsync(string id);
    }
}
