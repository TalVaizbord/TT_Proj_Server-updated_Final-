using System.Text.Json.Serialization;

namespace TT_Proj_Server.Models
{
    public class User
    {
        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [JsonPropertyName("FavContinent")]
        public string FavContinent { get; set; }

        [JsonPropertyName("LanguageProficiency")]
        public string LanguageProficiency { get; set; }

        [JsonPropertyName("IsAdmin")]
        public bool IsAdmin { get; set; }

        public bool IsLocked { get; set; }

        public bool IsMaster { get; set; }

        public User() { }
    }
}