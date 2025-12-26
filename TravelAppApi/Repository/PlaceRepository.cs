
using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;

namespace TravelAppApi.Repository
{
    public class PlaceRepository
    {
        private readonly ApplicationDbContext _context;

        public PlaceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Places> GetPlaceAsync(string locationId)
        {
            var place = await _context.Places
                .FirstOrDefaultAsync(d => d.Id == locationId);
            return place;
        }

        public async Task AddPlaceAsync(Places place)
        {
            _context.Places.Add(place);
            await _context.SaveChangesAsync(); // Đảm bảo Place được save ngay
        }
    }
}
