using System.Collections.Generic;

namespace TuberTreats.Models
{
    public class TuberDriverDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TuberOrderDTO> TuberDeliveries { get; set; } = new List<TuberOrderDTO>();
    }
}
