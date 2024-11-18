namespace DCXAir.Domain.Entities
{
    public class Journey
    {
        public List<Flight>? Flights { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public Double Price { get; set; }
    }
}
