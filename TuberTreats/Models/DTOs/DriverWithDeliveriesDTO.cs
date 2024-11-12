namespace TuberTreats.Models.DTOs;

public class DriverWithDeliveriesDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<TuberOrderDTO> Deliveries { get; set; }
}
