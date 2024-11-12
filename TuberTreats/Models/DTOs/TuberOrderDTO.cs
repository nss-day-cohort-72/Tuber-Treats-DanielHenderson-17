using System;
using System.Collections.Generic;

namespace TuberTreats.Models
{
    public class TuberOrderDTO
    {
        public int Id { get; set; }
        public DateTime OrderPlacedOnDate { get; set; }
        public int CustomerId { get; set; }
        public int? TuberDriverId { get; set; }
        public DateTime? DeliveredOnDate { get; set; }
        public List<ToppingDTO> Toppings { get; set; } = new List<ToppingDTO>();
    }
}
