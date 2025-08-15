using System.Threading;
using System.Threading.Tasks;

namespace TicketSystemApi.Repositories
{
    public interface IPostalRepository
    {
        Task<bool> ExistsZipCityDistrictAsync(string zipCode, string city, string district, CancellationToken cancellationToken = default);
    }
}
