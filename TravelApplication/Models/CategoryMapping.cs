using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelApplication.Models
{
    public static class CategoryMapping
    {
        public static readonly Dictionary<string, string> CategoryMapper = new()
        {
            // Stay
            { "accommodation", "stay" },

            // Food
            { "catering", "food" },

            // Attraction
            { "activity", "attraction" },
            { "commercial", "attraction" },
            { "entertainment", "attraction" },
            { "heritage", "attraction" },
            { "leisure", "attraction" },
            { "man_made", "attraction" },
            { "natural", "attraction" },
            { "national_park", "attraction" },
            { "pet", "attraction" },
            { "production", "attraction" },
            { "service", "attraction" },
            { "tourism", "attraction" },
            { "religion", "attraction" },
            { "camping", "attraction" },
            { "amenity", "attraction" },
            { "beach", "attraction" },
            { "adult", "attraction" },
            { "building", "attraction" },
            { "ski", "attraction" },
            { "sport", "attraction" },
            { "populated_place", "attraction" },
            { "memorial", "attraction" }
        };
    }
}
