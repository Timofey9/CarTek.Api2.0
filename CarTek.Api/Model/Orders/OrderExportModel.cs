﻿using CarTek.Api.Model.Dto;

namespace CarTek.Api.Model.Orders
{
    public class OrderExportModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public ClientModel Client { get; set; }

        public double Volume { get; set; }

        public Unit LoadUnit { get; set; }

        public Unit UnloadUnit { get; set; }

        public bool IsComplete { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public Address LocationA { get; set; }

        public Address LocationB { get; set; }

        public double Price { get; set; }

        public string? Note { get; set; }

        public int CarCount { get; set; }

        public int Mileage { get; set; }

        public ServiceType Service { get; set; }

        //Услуга
        public MaterialModel Material { get; set; }

        public ICollection<DriverTaskOrderModel> DriverTasks { get; set; }
    }
}
