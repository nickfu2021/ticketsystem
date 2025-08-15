namespace TicketSystemApi.Services
{
    public interface ILocationService
    {
        Task<List<string>> GetCitiesAsync();
        Task<List<string>> GetDistrictsAsync(string city);
        Task<string?> GetZipCodeAsync(string city, string district);
    }
}