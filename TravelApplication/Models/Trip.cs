using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
namespace TravelApplication.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string DestinationId { get; set; }
        public double MinLon { get; set; }
        public double MinLat { get; set; }
        public double MaxLon { get; set; }
        public double MaxLat { get; set; }


        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);
        public decimal Budget { get; set; }
        public int UserId { get; set; }
        public ObservableCollection<Stop> Stops { get; set; } = new();
    }
}
