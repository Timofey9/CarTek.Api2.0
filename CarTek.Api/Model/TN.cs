﻿using CarTek.Api.Model.Orders;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarTek.Api.Model
{
    public class TN
    {
        public long Id { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsOrginalReceived { get; set; }
        //Default -> КарТэк
        public string? Transporter { get; set; }
        public string? Number { get; set; }
        //чтобы понять, к чему привязана
        public long? DriverTaskId { get; set; }
        public long? SubTaskId { get; set; }
        public long DriverId { get; set; }
        public long? TransporterId { get; set; } //Id из таблицы Clients, если null => КарТэк

        //м3
        public double? LoadVolume { get; set; }
        //тн
        public double? LoadVolume2 { get; set; }
        //м3
        public double? UnloadVolume { get; set; }
        //тн
        public double? UnloadVolume2 { get; set; }

        public Unit? Unit { get; set; }

        public Unit? Unit2 { get; set; }

        public Unit? UnloadUnit { get; set; }

        public Unit? UnloadUnit2 { get; set; }

        //Id грузоотправителя из клиентов
        public long? GoId { get; set; }

        //Id грузополучателя из клиентов
        public long? GpId { get; set; }

        //Id точки забора груза
        public long? LocationAId { get; set; }

        //Id точки сдачи груза
        public long? LocationBId { get; set; }

        public long? MaterialId { get; set; }

        public DateTime? PickUpArrivalDate { get; set; }

        public DateTime? PickUpDepartureDate { get; set; }

        public string? PickUpArrivalTime { get; set; }

        public string? PickUpDepartureTime { get; set; }

        public DateTime? DropOffArrivalDate { get; set; }

        public DateTime? DropOffDepartureDate { get; set; }

        public string? DropOffArrivalTime { get; set; }

        public string? DropOffDepartureTime { get; set; }

        public DriverTask? DriverTask { get; set; }

        public SubTask? SubTask { get; set; }

        public Material? Material { get; set; }

        public long? OrderId { get; set; }
        public Order? Order { get; set; }

        [InverseProperty("TNLocationA")]
        public Address LocationA { get; set; }
        [InverseProperty("TNLocationB")]
        public Address LocationB { get; set; }
    }
}
