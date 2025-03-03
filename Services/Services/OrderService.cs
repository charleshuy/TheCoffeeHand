﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Constants;
using Core.Utils;
using Interfracture.Base;
using Interfracture.Entities;
using Interfracture.Interfaces;
using Interfracture.PaggingItems;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Services.DTOs;
using Services.ServiceInterfaces;
using System.Security.Claims;
using static Interfracture.Base.BaseException;

namespace Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheServices _cacheService;
        private readonly IUserServices _userServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService, IUserServices userServices, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _userServices = userServices;
            _httpContextAccessor = httpContextAccessor;
        }



        // Create a new order
        public async Task<OrderResponseDTO> CreateOrderAsync(OrderRequestDTO orderDTO)
        {
            var order = _mapper.Map<Order>(orderDTO);
            order.Date = CoreHelper.SystemTimeNow;

            await _unitOfWork.GetRepository<Order>().InsertAsync(order);
            await _unitOfWork.SaveAsync();

            // Clear cache when data changes
            await _cacheService.RemoveByPrefixAsync("orders_");

            return _mapper.Map<OrderResponseDTO>(order);
        }

        public async Task ConfirmOrderAsync(Guid orderId)
        {
            var orderRepository = _unitOfWork.GetRepository<Order>();
            var ingredientRepository = _unitOfWork.GetRepository<Ingredient>();

            // Fetch order with details and drinks
            var order = await orderRepository
                .Entities
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new BaseException.NotFoundException("not_found", "Order not found");

            if (order.Status != EnumOrderStatus.Cart)
                throw new BaseException.BadRequestException("invalid_status", "Order cannot be confirmed at this stage");

            var orderDetails = order.OrderDetails?.ToList() ?? new List<OrderDetail>();

            if (!orderDetails.Any())
                throw new BaseException.BadRequestException("empty_order", "Cannot confirm an empty order");

            // Collect all drink IDs
            var drinkIds = orderDetails.Select(od => od.DrinkId).Distinct().ToList();

            // Fetch all required recipes at once
            var recipes = await _unitOfWork.GetRepository<Recipe>()
                .Entities
                .Where(r => drinkIds.Contains(r.DrinkId))
                .Include(r => r.Ingredient)
                .ToListAsync();

            if (!recipes.Any())
                throw new BaseException.BadRequestException("recipe_not_found", "No recipes found for the drinks in this order");

            // Calculate required ingredient quantities
            var requiredIngredients = new Dictionary<Guid, int>();

            foreach (var orderDetail in orderDetails)
            {
                var drinkRecipes = recipes.Where(r => r.DrinkId == orderDetail.DrinkId);

                foreach (var recipe in drinkRecipes)
                {
                    if (recipe.Ingredient == null)
                        continue;

                    var requiredQuantity = recipe.Quantity * orderDetail.Total;

                    if (requiredIngredients.ContainsKey(recipe.Ingredient.Id))
                        requiredIngredients[recipe.Ingredient.Id] += requiredQuantity;
                    else
                        requiredIngredients[recipe.Ingredient.Id] = requiredQuantity;
                }
            }

            // Fetch required ingredients from DB
            var ingredients = await ingredientRepository
                .Entities
                .Where(i => requiredIngredients.Keys.Contains(i.Id))
                .ToListAsync();

            if (ingredients.Count != requiredIngredients.Count)
                throw new BaseException.BadRequestException("missing_ingredients", "Some required ingredients are missing in the database");

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Deduct stock inside transaction
                    foreach (var ingredient in ingredients)
                    {
                        if (ingredient.Quantity < requiredIngredients[ingredient.Id])
                            throw new BaseException.BadRequestException("not_enough_stock", $"Not enough stock for ingredient: {ingredient.Name}");

                        ingredient.Quantity -= requiredIngredients[ingredient.Id];
                        ingredientRepository.Update(ingredient);
                    }

                    // Update order status
                    order.Status = EnumOrderStatus.Confirmed;
                    orderRepository.Update(order);

                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            await _cacheService.RemoveByPrefixAsync("orders_");
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            var orderRepository = _unitOfWork.GetRepository<Order>();
            var ingredientRepository = _unitOfWork.GetRepository<Ingredient>();

            // Fetch order with details and drinks
            var order = await orderRepository
                .Entities
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new BaseException.NotFoundException("not_found", "Order not found");

            if (order.Status != EnumOrderStatus.Confirmed)
                throw new BaseException.BadRequestException("invalid_status", "Only confirmed orders can be canceled");

            var orderDetails = order.OrderDetails?.ToList() ?? new List<OrderDetail>();

            if (!orderDetails.Any())
                throw new BaseException.BadRequestException("empty_order", "Cannot cancel an empty order");

            // Collect all drink IDs
            var drinkIds = orderDetails.Select(od => od.DrinkId).Distinct().ToList();

            // Fetch all required recipes at once
            var recipes = await _unitOfWork.GetRepository<Recipe>()
                .Entities
                .Where(r => drinkIds.Contains(r.DrinkId))
                .Include(r => r.Ingredient)
                .ToListAsync();

            if (!recipes.Any())
                throw new BaseException.BadRequestException("recipe_not_found", "No recipes found for the drinks in this order");

            // Calculate ingredients to be restocked
            var restockIngredients = new Dictionary<Guid, int>();

            foreach (var orderDetail in orderDetails)
            {
                var drinkRecipes = recipes.Where(r => r.DrinkId == orderDetail.DrinkId);

                foreach (var recipe in drinkRecipes)
                {
                    if (recipe.Ingredient == null)
                        continue;

                    var restockQuantity = recipe.Quantity * orderDetail.Total;

                    if (restockIngredients.ContainsKey(recipe.Ingredient.Id))
                        restockIngredients[recipe.Ingredient.Id] += restockQuantity;
                    else
                        restockIngredients[recipe.Ingredient.Id] = restockQuantity;
                }
            }

            // Fetch required ingredients from DB
            var ingredients = await ingredientRepository
                .Entities
                .Where(i => restockIngredients.Keys.Contains(i.Id))
                .ToListAsync();

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Restock ingredients inside transaction
                    foreach (var ingredient in ingredients)
                    {
                        ingredient.Quantity += restockIngredients[ingredient.Id];
                        ingredientRepository.Update(ingredient);
                    }

                    // Update order status
                    order.Status = EnumOrderStatus.Canceled;
                    orderRepository.Update(order);

                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            await _cacheService.RemoveByPrefixAsync("orders_");
        }




        // Get order by ID
        public async Task<OrderResponseDTO> GetOrderByIdAsync(Guid id)
        {
            string cacheKey = $"order_{id}";

            var cachedOrder = await _cacheService.GetAsync<OrderResponseDTO>(cacheKey);
            if (cachedOrder != null)
            {
                return cachedOrder;
            }

            var order = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            var orderDTO = _mapper.Map<OrderResponseDTO>(order);

            await _cacheService.SetAsync(cacheKey, orderDTO, TimeSpan.FromMinutes(30));

            return orderDTO;
        }

        public async Task<PaginatedList<OrderResponseDTO>> GetOrdersAsync(int pageNumber, int pageSize, Guid? userId, DateTimeOffset? date)
        {
            string cacheKey = $"orders_{pageNumber}_{pageSize}_{userId}_{date}";

            var cachedOrders = await _cacheService.GetAsync<PaginatedList<OrderResponseDTO>>(cacheKey);
            if (cachedOrders != null)
            {
                return cachedOrders;
            }

            var query = _unitOfWork.GetRepository<Order>().Entities;
            if(userId != null)
            {
                query = query.Where(o => o.UserId == userId);
            }
            if (date != null)
            {
                query = query.Where(o => o.Date.HasValue && o.Date.Value.CompareTo(date.Value) >= 0);
            }

            var orders = await query
                .ProjectTo<OrderResponseDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var paginatedOrders = PaginatedList<OrderResponseDTO>.Create(
                orders.OrderBy(o => o.Date).ToList(),
                pageNumber,
                pageSize
            );

            await _cacheService.SetAsync(cacheKey, paginatedOrders, TimeSpan.FromMinutes(30));

            return paginatedOrders;
        }

        public async Task<OrderResponseDTO> GetCartAsync()
        {

            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new BadRequestException("not_found", "User not found");

            var cart = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.UserId.ToString() == userId && o.Status == 0);
            if (cart == null) 
            {
                var newCart = new OrderRequestDTO { UserId = Guid.Parse(userId), Status = 0 };
                return await CreateOrderAsync(newCart);
            }
            return _mapper.Map<OrderResponseDTO>(cart); ;
        }

        // Update order
        public async Task<OrderResponseDTO> UpdateOrderAsync(Guid id, OrderRequestDTO orderDTO)
        {
            var existingOrder = await _unitOfWork.GetRepository<Order>().GetByIdAsync(id);
            if (existingOrder == null)
                throw new NotFoundException("not_found", "Order not found");

            _mapper.Map(orderDTO, existingOrder);

            await _unitOfWork.GetRepository<Order>().UpdateAsync(existingOrder);
            await _unitOfWork.SaveAsync();

            await _cacheService.RemoveAsync($"order_{id}");
            await _cacheService.RemoveByPrefixAsync("orders_");

            return _mapper.Map<OrderResponseDTO>(existingOrder);
        }

        // Delete order (soft delete)
        public async Task DeleteOrderAsync(Guid id)
        {
            var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("not_found", "Order not found");

            order.DeletedTime = CoreHelper.SystemTimeNow;

            await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            await _unitOfWork.SaveAsync();

            await _cacheService.RemoveAsync($"order_{id}");
            await _cacheService.RemoveByPrefixAsync("orders_");
        }
    }
}
