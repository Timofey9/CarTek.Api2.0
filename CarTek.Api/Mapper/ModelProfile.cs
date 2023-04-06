﻿using AutoMapper;
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
                src => src.MapFrom(t => t.Car != null ? t.Car.Plate.ToUpper() : ""));
            CreateMap<Trailer, TrailerModel>().ForMember(trailer => trailer.Plate, src => src.MapFrom(t => t.Plate.ToUpper()));
            CreateMap<Car, CarModel>().ForMember(car => car.Plate, src => src.MapFrom(t => t.Plate.ToUpper()));
            CreateMap<Car, DriverCarModel>().ForMember(car => car.Plate, src => src.MapFrom(t => t.Plate.ToUpper()));
            CreateMap<User, UserModel>();
            CreateMap<Questionary, QuestionaryModel>();
            CreateMap<Questionary, QuestionaryCarModel>();
        }
    }
}
