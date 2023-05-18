using AutoMapper;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace  Integr8ed.AutoMapperProfileConfiguration
{
    public class AutoMapperProfileConfiguration : Profile
    {
        public AutoMapperProfileConfiguration()
        {
            CreateMap<Room_TypeDto, Room_Type>()
                .ForMember(des => des.Title, opt => opt.MapFrom(src => src.RoomTitle))
            .ReverseMap();

            CreateMap<InvoiceDetailDto, InvoiceDetail>().ReverseMap();
            CreateMap<SecurityDto, Security>().ReverseMap();
            CreateMap<ContactDetailsDto, ContactDetails>().ReverseMap();
            CreateMap<BookingDetailsDto, BookingDetails>().ReverseMap();
            CreateMap<CallLogsDto, CallLogs>().ReverseMap();
            CreateMap<BookingStatusDto, BookingStatus>().ReverseMap();
        }
    }

    public static class MappingExpression
    {
        public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(this IMappingExpression<TSource, TDestination> map, Expression<Func<TDestination, object>> selector)
        {
            map.ForMember(selector, config => config.Ignore());
            return map;
        }
    }

}
