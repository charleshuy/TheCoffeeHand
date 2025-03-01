﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Utils;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Interfracture.PaggingItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services.DTOs;
using Services.ServiceInterfaces;
using static Interfracture.Base.BaseException;

namespace Services.Services
{
    public class DrinkService : IDrinkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheServices _cacheService;

        public DrinkService(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<DrinkResponseDTO> CreateDrinkAsync(DrinkRequestDTO drinkDTO)
        {
            var existingDrink = await _unitOfWork.GetRepository<Drink>().Entities.FirstOrDefaultAsync(d => d.Name != null && d.Name.ToLower() == drinkDTO.Name.ToLower());
            if (existingDrink != null)
            {
                throw new BadRequestException("bad_request", "Drink with the same name already exists.");
            }
            var drink = _mapper.Map<Drink>(drinkDTO);
            await _unitOfWork.GetRepository<Drink>().InsertAsync(drink);
            await _unitOfWork.SaveAsync();

            // Clear cache when data changes
            await _cacheService.RemoveByPrefixAsync("drinks_");

            var result = await GetDrinkByIdAsync(drink.Id);

            return _mapper.Map<DrinkResponseDTO>(result);
        }

        public async Task<DrinkResponseDTO> GetDrinkByIdAsync(Guid id)
        {
            string cacheKey = $"drink_{id}";

            // Try to get from cache
            var cachedDrink = await _cacheService.GetAsync<DrinkResponseDTO>(cacheKey);
            if (cachedDrink != null)
            {
                return cachedDrink;
            }

            var drink = await _unitOfWork.GetRepository<Drink>()
                .Entities
                .Include(d => d.Category).Include(d => d.Recipes)
                .FirstOrDefaultAsync(d => d.Id == id);

            var drinkDTO = _mapper.Map<DrinkResponseDTO>(drink);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, drinkDTO, TimeSpan.FromMinutes(30));

            return drinkDTO;
        }

        public async Task<List<DrinkResponseDTO>> GetDrinksAsync()
        {
            string cacheKey = "drinks_all";

            // Try to get from cache
            var cachedDrinks = await _cacheService.GetAsync<List<DrinkResponseDTO>>(cacheKey);
            if (cachedDrinks != null)
            {
                return cachedDrinks;
            }

            var drinkDTOs = await _unitOfWork.GetRepository<Drink>()
                .Entities
                .Where(d => d.DeletedTime == null)
                .ProjectTo<DrinkResponseDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();


            // Store in cache
            await _cacheService.SetAsync(cacheKey, drinkDTOs, TimeSpan.FromMinutes(30));

            return drinkDTOs;
        }

        public async Task<PaginatedList<DrinkResponseDTO>> GetDrinksAsync(int pageNumber, int pageSize)
        {
            string cacheKey = $"drinks_{pageNumber}_{pageSize}";

            // Try to get from cache
            var cachedDrinks = await _cacheService.GetAsync<PaginatedList<DrinkResponseDTO>>(cacheKey);
            if (cachedDrinks != null)
            {
                return cachedDrinks;
            }

            var drinkRepo = _unitOfWork.GetRepository<Drink>();
            var query = drinkRepo.Entities.Where(d => d.DeletedTime == null);

            var paginatedDrinks = await PaginatedList<DrinkResponseDTO>.CreateAsync(
                query.OrderBy(d => d.Name != null ? d.Name.ToLower() : string.Empty).ProjectTo<DrinkResponseDTO>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize
            );

            // Store in cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, paginatedDrinks, TimeSpan.FromMinutes(30));

            return paginatedDrinks;
        }

        public async Task<DrinkResponseDTO> UpdateDrinkAsync(Guid id, DrinkRequestDTO drinkDTO)
        {
            var existingDrink = await _unitOfWork.GetRepository<Drink>().Entities.FirstOrDefaultAsync(d => d.Id != id && d.Name != null && d.Name.ToLower() == drinkDTO.Name.ToLower());
            if (existingDrink != null)
            {
                throw new BadRequestException("bad_request", "Drink with the same name already exists.");
            }
            var drink = _mapper.Map<Drink>(drinkDTO);
            drink.Id = id;


            await _unitOfWork.GetRepository<Drink>().UpdateAsync(drink);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"drink_{id}");
            await _cacheService.RemoveByPrefixAsync("drinks_");

            return _mapper.Map<DrinkResponseDTO>(drink);
        }

        public async Task DeleteDrinkAsync(Guid id)
        {
            var drink = await _unitOfWork.GetRepository<Drink>().GetByIdAsync(id);
            if (drink == null || drink.DeletedTime != null)
                throw new NotFoundException("not_found", "Drink not found");

            drink.DeletedTime = CoreHelper.SystemTimeNow;

            await _unitOfWork.GetRepository<Drink>().UpdateAsync(drink);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"drink_{id}");
            await _cacheService.RemoveByPrefixAsync("drinks_");
        }
    }
}
