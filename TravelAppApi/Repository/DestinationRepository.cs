
using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;

namespace TravelAppApi.Repository
{
    public class DestinationRepository
    {
        private readonly ApplicationDbContext _context;

        public DestinationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Destinations> GetDestinationAsync(string destinationId)
        {
            var destination = await _context.Destinations
                .FirstOrDefaultAsync(d => d.Id == destinationId);
            return destination;
        }

        public async Task AddDestinationAsync(Destinations destinations)
        {
            _context.Destinations.Add(destinations);
            await _context.SaveChangesAsync();
        }
    }
}
