namespace TT_Proj_Server.Models
{
    public class Country
    {
        private String id;
        private String name;
        private String region;
        private String city;
        private long population;
        private String currency;
        private String language;
        private Double area;
        private string flagimageUrl;

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Region { get => region; set => region = value; }
        public string City { get => city; set => city = value; }
        public long Population { get => population; set => population = value; }
        public string Currency { get => currency; set => currency = value; }
        public string Language { get => language; set => language = value; }
        public Double Area { get => area; set => area = value; }
        public string FlagImageUrl { get => flagimageUrl; set => flagimageUrl = value; }

        public Country() { }

        public Country(string id, string name, string region, string city, long population, string currency, string language, double area, string flagImageUrl)
        {
            Id = id;
            Name = name;
            Region = region;
            City = city;
            Population = population;
            Currency = currency;
            Language = language;
            Area = area;
            FlagImageUrl = flagImageUrl;
        }

    }
}