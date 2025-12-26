using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using TravelAppApi.Models;
namespace TravelAppApi.Data
{
    public class AddTripDto
    {

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string DestinationName { get; set; }
        public string DestinationId { get; set; }
        public double MinLon { get; set; }
        public double MinLat { get; set; }
        public double MaxLon { get; set; }
        public double MaxLat { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);
        public decimal Budget { get; set; }
        public int UserId { get; set; }
    }

    public class TripModelFrontend
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DestinationName { get; set; }
        public string DestinationId { get; set; }
        public double MinLon { get; set; }
        public double MinLat { get; set; }
        public double MaxLon { get; set; }
        public double MaxLat { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);
        public decimal Budget { get; set; }
        public int UserId { get; set; }
        public ObservableCollection<StopModelFrontend> Stops { get; set; }
    }
}
