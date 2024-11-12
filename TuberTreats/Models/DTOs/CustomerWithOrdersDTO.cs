namespace TuberTreats.Models.DTOs;

public class CustomerWithOrdersDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public List<TuberOrderDTO> Orders { get; set; }
}
