using AutoMapper;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Driver;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Mapper
{
    public class ModelProfile : Profile
    {
        public ModelProfile()
        {
            CreateMap<Driver, DriverModel>().ForMember(t => t.CarName, 
                src => src.MapFrom(t => t.Car != null ? t.Car.Plate.ToUpper() : ""));           
            CreateMap<Driver, DriverInfoModel>();
            CreateMap<Trailer, TrailerModel>().ForMember(trailer => trailer.Plate, src => src.MapFrom(t => t.Plate.ToUpper()));
            CreateMap<Car, CarModel>().ForMember(car => car.Plate, src => src.MapFrom(t => t.Plate.ToUpper()));
            CreateMap<Car, DriverCarModel>().ForMember(car => car.Plate, src => src.MapFrom(t => t.Plate.ToUpper()));
            CreateMap<User, UserModel>();
            CreateMap<Questionary, QuestionaryModel>();
            CreateMap<Questionary, QuestionaryCarModel>();
            CreateMap<Material, MaterialModel>();
            CreateMap<DriverTask, DriverTaskOrderModel>();
            CreateMap<Order, OrderModel>();
        }
    }
}
