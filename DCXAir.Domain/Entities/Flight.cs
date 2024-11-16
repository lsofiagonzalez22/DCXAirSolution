namespace DCXAir.Domain.Entities
{
    public class Flight
    {
        public Transport Transport { get; set; }
        public string Origin { get; set; }
        public decimal Destination { get; set; }
        public double Price { get; set; }
    }
}
