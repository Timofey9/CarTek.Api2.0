using AutoMapper;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Driver;

namespace CarTek.Api.Mapper
{
    public class ModelProfile : Profile
    {
        public ModelProfile()
        {
            CreateMap<Driver, DriverModel>().ForMember(t => t.CarName, 
                src => src.MapFrom(t => t.Car != null ? t.Car.Plate : ""));
            CreateMap<Trailer, TrailerModel>();
            CreateMap<Car, CarModel>();
            CreateMap<Car, DriverCarModel>();
            CreateMap<User, UserModel>();
            CreateMap<Questionary, QuestionaryModel>();
            CreateMap<Questionary, QuestionaryCarModel>();
        }
    }
}
