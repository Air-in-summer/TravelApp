using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;

namespace TravelAppApi.Repository
{
    public class StopRepository
    {
        private readonly ApplicationDbContext _context;

        public StopRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stop> AddStopAsync(Stop newStop)
        {
            _context.Stops.Add(newStop);
            await _context.SaveChangesAsync();
            return newStop;
        }

        public async Task<IEnumerable<Stop>> GetStopsByTripIdAsync(int tripId)
        {
            return await _context.Stops
                        .Where(s => s.TripId == tripId)
                        .OrderByDescending(s => s.Id)
                        .ToListAsync();
        }

        public async Task<Stop> GetStopByIdAsync(int id)
        {
            var stop = await _context.Stops
                        .FirstOrDefaultAsync(t => t.Id == id);
            return stop;
        }

        public async Task UpdateStopAsync(int id, StopModelFrontend stopModel)
        {
            var content = await _context.Stops.FindAsync(id);
            content.ArrivalDate = stopModel.ArrivalDate.ToUniversalTime();
            content.DepartureDate = stopModel.DepartureDate.ToUniversalTime();
            content.EstimatedCost = stopModel.EstimatedCost;
            content.Notes = stopModel.Notes;
            _context.Stops.Update(content);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStopAsync(int id)
        {
            var content = await _context.Stops.FindAsync(id);
            if (content != null)
            {
                _context.Stops.Remove(content);
                await _context.SaveChangesAsync();
            }
        }
    }
}
