﻿using CarTek.Api.Model.Dto.Driver;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model.Dto
{
    public class DriverTaskCarModel
    {
        public long Id { get; set; }

        public OrderModel Order { get; set; }

        public Guid UniqueId { get; set; }

        // night/day
        public ShiftType Shift { get; set; }

        public DriverTaskStatus Status { get; set; }

        public DateTime StartDate { get; set; }

        public double Volume { get; set; }

        public Unit Unit { get; set; }

        public string AdminComment { get; set; }

        public DateTime DateCreated { get; set; }

        public DriverInfoModel Driver { get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }

        public Address LocationA { get; set; }

        public Address LocationB { get; set; }

        public int SubTasksCount { get; set; }

        public DriverTaskNote LastNote { get; set; }
    }

    public class DriverTaskExportModel
    {
        public long Id { get; set; }

        public OrderModel Order { get; set; }

        public ClientModel OrderCustomer { get; set; }

        public Guid UniqueId { get; set; }

        public bool IsCanceled { get; set; }

        public string TransportAmountMessage { get; set; }

        // night/day
        public ShiftType Shift { get; set; }

        public DriverTaskStatus Status { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DateCreated { get; set; }

        public int Volume { get; set; }

        public Address LocationA { get; set; }

        public Address LocationB { get; set; }

        public string AdminComment { get; set; }

        public Unit Unit { get; set; }

        public DriverInfoModel Driver { get; set; }

        public CarInfoModel Car { get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }

        public ICollection<SubTaskModel> SubTasks { get; set; }

        public int SubTasksCount { get; set; }

        public string Price { get; set; }

        public string DriverPrice { get; set; }
    }

    public class DriverTaskSubTaskModel
    {
        public long Id { get; set; }

        public OrderModel Order { get; set; }

        public ClientModel OrderCustomer { get; set; }

        public Guid UniqueId { get; set; }

        public bool IsCanceled { get; set; }

        // night/day
        public ShiftType Shift { get; set; }

        public DriverTaskStatus Status { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DateCreated { get; set; }

        public int Volume { get; set; }

        public Address LocationA { get; set; }

        public Address LocationB { get; set; }

        public string AdminComment { get; set; }

        public Unit Unit { get; set; }

        public DriverInfoModel Driver { get; set; }

        public CarInfoModel Car { get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }

        public int SubTasksCount { get; set; }

        public string Price { get; set; }
        public string DriverPrice { get; set; }

    }
}
