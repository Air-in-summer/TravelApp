namespace TravelAppApi.Data
{
    public class AddStopDto
    {
        public string Location { get; set; }
        public string LocationId { get; set; }
        public string Category { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Address { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Notes { get; set; }
        public int TripId { get; set; }
    }

    public class StopModelFrontend
    {
        public int Id { get; set; }
        public string Location { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int TripId { get; set; }
    }
}
