using AutoMapper;
using Interfracture.Entities;
using Services.DTOs;

namespace Services.MapperProfiles
{
    public class OrderDetailProfile : Profile
    {
        public OrderDetailProfile()
        {
            CreateMap<OrderDetail, OrderDetailResponselDTO>().ReverseMap();
            CreateMap<OrderDetailRequestDTO, OrderDetail>().ReverseMap();
        }
    }
}
