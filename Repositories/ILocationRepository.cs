using TicketSystemApi.Dtos;

namespace TicketSystemApi.Repositories
{
    public interface ILocationRepository
    {
        Task<List<string>> GetAllCitiesAsync();
        Task<List<string>> GetDistrictsByCityAsync(string city);
        Task<string?> GetZipCodeAsync(string city, string district);
    }
}