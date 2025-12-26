using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelApplication.Models
{
    public class UserPreferences
    {
        public int TripId { get; set; }        

        public List<String> Preferences { get; set; } = new List<String>();
    }
}
