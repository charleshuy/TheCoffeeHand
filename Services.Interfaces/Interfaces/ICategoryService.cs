﻿using Interfracture.Base;
using Interfracture.DTOs;


namespace Services.Interfaces.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDTO> CreateCategoryAsync(CategoryRequestDTO categoryDTO);
        Task<CategoryResponseDTO?> GetCategoryByIdAsync(Guid id);
        Task<BasePaginatedList<CategoryResponseDTO>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        Task<bool> UpdateCategoryAsync(Guid id, CategoryRequestDTO categoryDTO);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}
