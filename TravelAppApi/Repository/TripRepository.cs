using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TravelAppApi.Data;
using TravelAppApi.Models;

namespace TravelAppApi.Repository
{
    public class TripRepository
    {
        private readonly ApplicationDbContext _context;

        public TripRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Trip> AddTripAsync(Trip newTrip)
        {
            _context.Trips.Add(newTrip);
            await _context.SaveChangesAsync();
            return newTrip;
        }

        public async Task<IEnumerable<Trip>> GetTripsByUserIdAsync(int ownerId)
        {
            return await _context.Trips
                        .Where(t => t.UserId == ownerId)
                        .OrderByDescending(t => t.Id)
                        .ToListAsync();
        }

        public async Task<Trip> GetTripByIdAsync(int id)
        {
            var trip = await _context.Trips.Include(d => d.Destination)
                        .FirstOrDefaultAsync(t => t.Id == id); 
            trip.Stops = await _context.Stops
                                .Where(s => s.TripId == id)
                                .Include(s => s.Location)
                                .OrderBy(s => s.ArrivalDate)
                                .ToListAsync();
            return trip;
        }

        public async Task DeleteTripAsync(int id)
        {
            var content = await _context.Trips.FindAsync(id);
            if (content != null)
            {
                _context.Trips.Remove(content);
                await _context.SaveChangesAsync();
            } 
        }

        public async Task UpdateTripAsync(int id, Trip trip)
        {
            var content = await _context.Trips.FindAsync(id);
            content.Title = trip.Title;
            content.Description = trip.Description;
            content.StartDate = trip.StartDate.ToUniversalTime();
            content.EndDate = trip.EndDate.ToUniversalTime();
            content.Budget = trip.Budget;
            _context.Trips.Update(content);
            await _context.SaveChangesAsync();

        }
    }
}
