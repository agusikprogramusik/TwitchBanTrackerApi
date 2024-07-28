namespace TwitchBanTrackerApi.Common.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime BannedAt { get; set; }
        public string BanDuration { get; set; }
        public List<string> LastMessages { get; set; }
    }
}
