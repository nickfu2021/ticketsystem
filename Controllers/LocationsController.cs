using Microsoft.AspNetCore.Mvc;
using TicketSystemApi.Services;

namespace TicketSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _service;

        public LocationsController(ILocationService service)
        {
            _service = service;
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _service.GetCitiesAsync();
            return Ok(cities);
        }

        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return BadRequest("city is required");
            var districts = await _service.GetDistrictsAsync(city);
            return Ok(districts);
        }

        [HttpGet("postal-code")]
        public async Task<IActionResult> GetZipCode([FromQuery] string city, [FromQuery] string district)
        {
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(district))
                return BadRequest("city and district are required");

            var zip = await _service.GetZipCodeAsync(city, district);
            if (zip == null) return NotFound("No zip code found for given city and district");

            return Ok(new { ZipCode = zip });
        }
    }
}
