namespace TwitchBanTrackerApi.Common.Domain
{
    public class UserDto
    {
        public string Username { get; set; }
        public DateTime BannedAt { get; set; }
        public string BanDuration { get; set; }
        public List<string> LastMessages { get; set; }
    }
}
