using TicketSystemApi.Repositories;

namespace TicketSystemApi.Services
{
    public class LocationService(ILocationRepository repo) : ILocationService
    {
        private readonly ILocationRepository _repo = repo;

        public Task<List<string>> GetCitiesAsync() => _repo.GetAllCitiesAsync();

        public Task<List<string>> GetDistrictsAsync(string city) => _repo.GetDistrictsByCityAsync(city);

        public Task<string?> GetZipCodeAsync(string city, string district) => _repo.GetZipCodeAsync(city, district);
    }
}