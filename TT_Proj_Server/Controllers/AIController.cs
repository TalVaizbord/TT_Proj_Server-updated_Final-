using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace TT_Proj_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public AIController(IConfiguration configuration)
        {
            _apiKey = configuration["GoogleSettings:ApiKey"];
            _apiUrl = configuration["GoogleSettings:ApiUrl"];
        }
        public class AiRequest
        {
            public string CountryName { get; set; }
            public string UserPrompt { get; set; }
        }

        [HttpPost("AskAI")]
        public async Task<IActionResult> AskAI([FromBody] AiRequest request)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return BadRequest("יש להגדיר מפתח API של ה-AI בשרת.");
            }

            string systemPrompt = $@"
            אתה סוכן נסיעות מומחה. המשתמש שואל אותך על המדינה: {request.CountryName}.
            ענה לו בעברית, בצורה מקצועית וידידותית.
            השתמש בפורמט HTML נקי (כמו תגיות <ul>, <li>, <strong>, <h3>) כדי שהתשובה תיראה יפה. אל תשתמש בתגיות ```html.
            הנה הבקשה/השאלה של המשתמש:
            {request.UserPrompt}
        ";

            var requestBody = new
            {
                contents = new[]
                {
                new { parts = new[] { new { text = systemPrompt } } }
            }
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{_apiUrl}?key={_apiKey}", content);

                    string responseText = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonDocument doc = JsonDocument.Parse(responseText))
                        {
                            var root = doc.RootElement;
                            var generatedText = root.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                            return Ok(new { Itinerary = generatedText });
                        }
                    }

                    return StatusCode(500, $"שגיאה משרתי גוגל: {responseText}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאת שרת פנימית: {ex.Message}");
            }
        }
    }
}