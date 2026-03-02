using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking.Domain.Parcels
{
    public static class ScanStage
    {
        public const string Created = "CREATED";
        public const string Collection = "COLLECTED";
        public const string SortingHub = "SORTING_HUB";
        public const string DeliveryCenter = "DELIVERY_CENTER";
        public const string OutForDelivery = "OUT_FOR_DELIVERY";
        public const string Delivered = "DELIVERED";
        public const string Failed = "FAILED";
    }
}
