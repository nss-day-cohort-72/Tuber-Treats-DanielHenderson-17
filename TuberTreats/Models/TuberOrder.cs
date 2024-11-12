namespace TuberTreats.Models
{
    public class TuberOrder
    {
        public int Id { get; set; }
        public DateTime OrderPlacedOnDate { get; set; }
        public int CustomerId { get; set; }
        public int? TuberDriverId { get; set; }
        public DateTime? DeliveredOnDate { get; set; }
        public List<Topping> Toppings { get; set; } = new List<Topping>();
    }
}