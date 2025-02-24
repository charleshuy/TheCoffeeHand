using AutoMapper;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Core.Utils;
using Services.Interfaces.Interfaces;
using Interfracture.Base;
using AutoMapper.QueryableExtensions;
using Services.DTOs;

namespace Services.Services
{
    public class CategoryServices : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CategoryResponseDTO> CreateCategoryAsync(CategoryRequestDTO categoryDTO)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            var category = _mapper.Map<Category>(categoryDTO);
            category.CreatedTime = category.LastUpdatedTime = CoreHelper.SystemTimeNow;

            await categoryRepo.InsertAsync(category);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO?> GetCategoryByIdAsync(Guid id)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();
            var category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);

            return category == null ? null : _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<BasePaginatedList<CategoryResponseDTO>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();
            var query = categoryRepo.Entities.Where(c => c.DeletedTime == null);
            int totalItems = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CategoryResponseDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new BasePaginatedList<CategoryResponseDTO>(categories, totalItems, pageNumber, pageSize);
        }

        public async Task<CategoryResponseDTO> UpdateCategoryAsync(Guid id, CategoryRequestDTO categoryDTO)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            var category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                throw new KeyNotFoundException("Category not found.");

            if (category.DeletedTime != null)
                throw new InvalidOperationException("Cannot update a deleted category.");

            // Map only provided properties while keeping existing values
            _mapper.Map(categoryDTO, category);
            category.LastUpdatedTime = CoreHelper.SystemTimeNow;

            categoryRepo.Update(category);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<CategoryResponseDTO>(category);
        }


        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            var category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null || category.DeletedTime != null)
                return false;

            category.DeletedTime = CoreHelper.SystemTimeNow;

            categoryRepo.Update(category);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
