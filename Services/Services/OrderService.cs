using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Utils;
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
