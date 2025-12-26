using Microsoft.EntityFrameworkCore;
using TravelAppApi.Data;
using TravelAppApi.Models;

namespace TravelAppApi.Repository
{
    public class UserPreferencesRepository
    {
        private readonly ApplicationDbContext _context;
        public UserPreferencesRepository(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<IEnumerable<UserPreferences>> GetUserPreferencesAsync(int tripId)
        {
            return await _context.UserPreferences
                         .Where(u => u.TripId == tripId)
                         .ToListAsync();
        }

        public async Task AddUserPreferencesAsync(int tripID, List<string> preferences)
        {
            foreach (var pref in preferences)
            {
                var userPref = new UserPreferences
                {
                    TripId = tripID,
                    Preference = pref
                };
                _context.UserPreferences.Add(userPref);
            }
            
            await _context.SaveChangesAsync();
            Console.WriteLine("Added preferences to context.");
        }

        public async Task UpdateUserPreferencesAsync(int tripID, List<string> preferences, IEnumerable<UserPreferences> existingPrefs)
        {
            int i = 0;
            foreach(var pref in existingPrefs)
            {
                pref.Preference = preferences[i];
                _context.UserPreferences.Update(pref);
                i++;
            }
            await _context.SaveChangesAsync();
        }
    }
}
