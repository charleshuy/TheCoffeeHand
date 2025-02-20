using AutoMapper;
using Interfracture.DTOs;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Core.Utils;
using Services.Interfaces.Interfaces;
using Interfracture.Base;
using AutoMapper.QueryableExtensions;

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
            _unitOfWork.Save();

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO?> GetCategoryByIdAsync(string id)
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
                .OrderBy(c => c.Name) // Sorting by Name, change as needed
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CategoryResponseDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();


            return new BasePaginatedList<CategoryResponseDTO>(categories, totalItems, pageNumber, pageSize);
        }


        public async Task<bool> UpdateCategoryAsync(string id, CategoryRequestDTO categoryDTO)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            var category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null || category.DeletedTime != null)
                return false;

            category.Name = categoryDTO.Name ?? category.Name;
            category.LastUpdatedTime = CoreHelper.SystemTimeNow;

            categoryRepo.Update(category);
            _unitOfWork.Save();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            var category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null || category.DeletedTime != null)
                return false;

            category.DeletedTime = CoreHelper.SystemTimeNow;

            categoryRepo.Update(category);
            _unitOfWork.Save();
            return true;
        }
    }
}
