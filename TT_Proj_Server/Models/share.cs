namespace TT_Proj_Server.Models
{
    public class Share
    {
        public int Id { get; set; }
        public string? CountryId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? Content { get; set; }
        public DateTime? DatePosted { get; set; }

        public Share() { }
    }
}