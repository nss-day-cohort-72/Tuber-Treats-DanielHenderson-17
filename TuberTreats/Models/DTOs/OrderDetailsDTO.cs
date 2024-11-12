namespace TuberTreats.Models.DTOs;

public class OrderDetailsDTO
{
    public TuberOrder Order { get; set; }
    public Customer Customer { get; set; }
    public TuberDriver Driver { get; set; }
    public List<Topping> Toppings { get; set; }
}
