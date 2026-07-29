using TT_Proj_Server.DAL;
using TT_Proj_Server.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TT_Proj_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static List<Country> _cachedCountries = null;
        private static readonly object _lockObj = new object();
        public CountriesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetAllCountries")]
        public IActionResult GetAllCountries()
        {
            if (_cachedCountries == null)
            {
                lock (_lockObj) 
                {
                    if (_cachedCountries == null)
                    {
                        DBServices db = new DBServices(_configuration);
                        _cachedCountries = db.GetAllCountries();
                    }
                }
            }

            return Ok(_cachedCountries);
        }

        [HttpPost("PostCountries")]
        public IActionResult PostCountries([FromBody] Country country)
        {
            DBServices db = new DBServices(_configuration);
            int numEffected = db.InsertCountry(country);
            if (numEffected > 0)
            {
                return Ok("Country Saved Successfully");
            }
            return BadRequest("Error saving state to database.");
        }

        [HttpPut("UpdateCountry")]
        public IActionResult Put(string id, [FromBody] Country itemToUpdate)
        {
            DBServices db = new DBServices(_configuration);
            bool isUpdated = db.UpdateCountry(itemToUpdate);
            if (isUpdated)
            {
                return Ok("Country updated successfully");
            }
            return BadRequest("Error updating country in database.");
        }

        [HttpDelete("DeleteCountry/{id}")]
        public IActionResult Delete(string id)
        {
            DBServices db = new DBServices(_configuration);
            bool isDeleted = db.DeleteCountry(id);
            if (isDeleted)
            {
                return Ok("Country deleted successfully");
            }
            return BadRequest("Error deleting country from database.");
        }

        [HttpGet("SearchCountries")]
        public IActionResult Search(string? name = null, string? region = null, string? language = null, string? currency = null, long? minPopulation = null, double? minArea = null)
        {
            try
            {
                DBServices db = new DBServices(_configuration);
                List<Country> list = db.SearchCountries(name, region, language, currency, minPopulation, minArea);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ImportCountriesFromApi")]
        public async Task<IActionResult> ImportCountriesFromApi()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Tal_CS_Project_Ruppin/1.0");
                    var response = await client.GetAsync("https://countries.dev/countries");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();

                        using (System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(jsonString))
                        {
                            if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array)
                            {
                                return BadRequest("Expected an Array from countries.dev, but got something else.");
                            }

                            List<Country> importedCountries = new List<Country>();
                            importedCountries.Add(new Country());

                            foreach (System.Text.Json.JsonElement element in doc.RootElement.EnumerateArray())
                            {
                                Country newCountry = new Country();

                                if (element.TryGetProperty("alpha3Code", out System.Text.Json.JsonElement alpha3Code))
                                    newCountry.Id = alpha3Code.GetString();

                                if (element.TryGetProperty("name", out System.Text.Json.JsonElement nameProp))
                                    newCountry.Name = nameProp.GetString();

                                if (element.TryGetProperty("region", out System.Text.Json.JsonElement region))
                                    newCountry.Region = region.GetString();

                                if (element.TryGetProperty("capital", out System.Text.Json.JsonElement capital))
                                    newCountry.City = capital.GetString();

                                if (element.TryGetProperty("population", out System.Text.Json.JsonElement pop) && pop.ValueKind == System.Text.Json.JsonValueKind.Number)
                                    newCountry.Population = pop.GetInt64();

                                if (element.TryGetProperty("area", out System.Text.Json.JsonElement area) && area.ValueKind == System.Text.Json.JsonValueKind.Number)
                                    newCountry.Area = area.GetDouble();

                                if (element.TryGetProperty("flags", out System.Text.Json.JsonElement flags) && flags.TryGetProperty("png", out System.Text.Json.JsonElement png))
                                    newCountry.FlagImageUrl = png.GetString();

                                if (element.TryGetProperty("languages", out System.Text.Json.JsonElement languages) && languages.ValueKind == System.Text.Json.JsonValueKind.Array && languages.GetArrayLength() > 0)
                                {
                                    if (languages[0].TryGetProperty("name", out System.Text.Json.JsonElement langName))
                                        newCountry.Language = langName.GetString();
                                }

                                if (element.TryGetProperty("currencies", out System.Text.Json.JsonElement currencies) && currencies.ValueKind == System.Text.Json.JsonValueKind.Array && currencies.GetArrayLength() > 0)
                                {
                                    if (currencies[0].TryGetProperty("name", out System.Text.Json.JsonElement currName))
                                        newCountry.Currency = currName.GetString();
                                }

                                importedCountries.Add(newCountry);
                            }

                            DBServices db = new DBServices(_configuration);
                            int successCount = 0;

                            for (int l = 1; l < importedCountries.Count; l++)
                            {
                                Country currentCountry = importedCountries[l];
                                int numEffected = db.InsertCountry(currentCountry);
                                if (numEffected > 0)
                                {
                                    successCount++;
                                }
                            }

                            return Ok($"Successfully imported {successCount} countries to the database.");
                        }
                    }
                    else
                    {
                        var errorDetails = await response.Content.ReadAsStringAsync();
                        return BadRequest($"API Error (Status {response.StatusCode}): {errorDetails}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}