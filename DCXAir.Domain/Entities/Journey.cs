namespace DCXAir.Domain.Entities
{
    public class Journey
    {
        public List<Flight> Flights { get; set; }
        public string Origin { get; set; }
        public decimal Destination { get; set; }
        public string Price { get; set; }
    }
}
