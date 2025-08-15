using Microsoft.EntityFrameworkCore;
using TicketSystemApi.Data;
using TicketSystemApi.Repositories;

namespace TicketSystemApi.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AppDbContext _context;

        public LocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetAllCitiesAsync()
        {
            return await _context.Postal
                .Select(p => p.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> GetDistrictsByCityAsync(string city)
        {
            return await _context.Postal
                .Where(p => p.City == city)
                .Select(p => p.District)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        public async Task<string?> GetZipCodeAsync(string city, string district)
        {
            return await _context.Postal
                .Where(p => p.City == city && p.District == district)
                .Select(p => p.ZipCode)
                .FirstOrDefaultAsync();
        }
    }
}