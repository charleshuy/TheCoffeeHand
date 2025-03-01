using AutoMapper;
using AutoMapper.QueryableExtensions;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Interfracture.PaggingItems;
using Microsoft.EntityFrameworkCore;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace Services.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheServices _cacheService;

        public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<OrderDetailResponselDTO> CreateOrderDetailAsync(OrderDetailRequestDTO dto)
        {
            var orderDetail = _mapper.Map<OrderDetail>(dto);

            // Fetch the order/cart
            var cart = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(c => c.Id == orderDetail.OrderId && c.Status == 0);

            if (cart == null)
                throw new Exception("Cart not found");

            // Fetch all recipes for the drink (many-to-many)
            var recipes = await _unitOfWork.GetRepository<Recipe>()
                .Entities
                .Where(r => r.DrinkId == orderDetail.DrinkId)
                .Include(r => r.Ingredient) // Load related ingredient
                .ToListAsync();

            if (!recipes.Any())
                throw new Exception("No recipes found for this drink");

            // Dictionary to track required ingredient quantities
            var requiredIngredients = new Dictionary<Guid, int>(); // {IngredientId, TotalRequired}

            // Calculate total required quantity for each ingredient
            foreach (var recipe in recipes)
            {
                if (recipe.Ingredient == null)
                    throw new Exception($"Ingredient missing for recipe ID: {recipe.Id}");

                int requiredQuantity = orderDetail.Total * recipe.Quantity;

                if (requiredIngredients.ContainsKey(recipe.Ingredient.Id))
                {
                    requiredIngredients[recipe.Ingredient.Id] += requiredQuantity;
                }
                else
                {
                    requiredIngredients[recipe.Ingredient.Id] = requiredQuantity;
                }
            }

            // Fetch all involved ingredients from DB
            var ingredientIds = requiredIngredients.Keys.ToList();
            var ingredients = await _unitOfWork.GetRepository<Ingredient>()
                .Entities
                .Where(i => ingredientIds.Contains(i.Id))
                .ToListAsync();

            if (ingredients.Count != requiredIngredients.Count)
                throw new Exception("Some required ingredients are missing in the database");

            // Validate stock availability
            foreach (var ingredient in ingredients)
            {
                if (ingredient.Quantity < requiredIngredients[ingredient.Id])
                    throw new Exception($"Not enough stock for ingredient: {ingredient.Name}");
            }

            // Deduct ingredient stock
            foreach (var ingredient in ingredients)
            {
                ingredient.Quantity -= requiredIngredients[ingredient.Id];
                _unitOfWork.GetRepository<Ingredient>().Update(ingredient);
            }

            // Use a transaction to ensure atomic updates
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Add the new order detail
                    await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);
                    await _unitOfWork.SaveAsync(); // Save ingredient stock updates

                    // Re-fetch cart after adding order detail
                    cart = await _unitOfWork.GetRepository<Order>()
                        .Entities
                        .Include(o => o.OrderDetails!)
                            .ThenInclude(od => od.Drink)
                        .FirstOrDefaultAsync(c => c.Id == orderDetail.OrderId && c.Status == 0);

                    if (cart == null)
                        throw new Exception("Cart not found");

                    // Recalculate total price
                    cart.TotalPrice = cart.OrderDetails!.Sum(od => (od.Drink != null ? od.Drink.Price : 0) * od.Total);

                    // Save updated cart price
                    _unitOfWork.GetRepository<Order>().Update(cart);
                    await _unitOfWork.SaveAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // Clear cache when data changes
            await _cacheService.RemoveByPrefixAsync("order_details_");
            await _cacheService.RemoveByPrefixAsync("orders_");

            return _mapper.Map<OrderDetailResponselDTO>(orderDetail);
        }



        public async Task<OrderDetailResponselDTO> GetOrderDetailByIdAsync(Guid id)
        {
            string cacheKey = $"order_detail_{id}";

            // Try to get from cache
            var cachedOrderDetail = await _cacheService.GetAsync<OrderDetailResponselDTO>(cacheKey);
            if (cachedOrderDetail != null)
            {
                return cachedOrderDetail;
            }

            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(id);
            var dto = _mapper.Map<OrderDetailResponselDTO>(orderDetail);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

            return dto;
        }

        public async Task<List<OrderDetailResponselDTO>> GetOrderDetailsAsync()
        {
            string cacheKey = "order_details_all";

            // Try to get from cache
            var cachedOrderDetails = await _cacheService.GetAsync<List<OrderDetailResponselDTO>>(cacheKey);
            if (cachedOrderDetails != null)
            {
                return cachedOrderDetails;
            }

            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities.ToListAsync();
            var dtoList = _mapper.Map<List<OrderDetailResponselDTO>>(orderDetails);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, dtoList, TimeSpan.FromMinutes(30));

            return dtoList;
        }

        public async Task<PaginatedList<OrderDetailResponselDTO>> GetOrderDetailsAsync(int pageNumber, int pageSize)
        {
            string cacheKey = $"order_details_{pageNumber}_{pageSize}";

            // Try to get from cache
            var cachedOrderDetails = await _cacheService.GetAsync<PaginatedList<OrderDetailResponselDTO>>(cacheKey);
            if (cachedOrderDetails != null)
            {
                return cachedOrderDetails;
            }

            var orderDetailRepo = _unitOfWork.GetRepository<OrderDetail>();
            var query = orderDetailRepo.Entities;

            var paginatedOrderDetails = await PaginatedList<OrderDetailResponselDTO>.CreateAsync(
                query.OrderBy(o => o.Id).ProjectTo<OrderDetailResponselDTO>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize
            );

            // Store in cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, paginatedOrderDetails, TimeSpan.FromMinutes(30));

            return paginatedOrderDetails;
        }

        public async Task<OrderDetailResponselDTO> UpdateOrderDetailAsync(Guid id, OrderDetailRequestDTO dto)
        {
            var orderDetail = _mapper.Map<OrderDetail>(dto);
            orderDetail.Id = id;

            await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetail);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"order_detail_{id}");
            await _cacheService.RemoveByPrefixAsync("order_details_");

            return _mapper.Map<OrderDetailResponselDTO>(orderDetail);
        }

        public async Task DeleteOrderDetailAsync(Guid id)
        {
            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(id);
            if (orderDetail == null)
                throw new Exception("Order detail not found");

            await _unitOfWork.GetRepository<OrderDetail>().DeleteAsync(orderDetail);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"order_detail_{id}");
            await _cacheService.RemoveByPrefixAsync("order_details_");
        }
    }
}
