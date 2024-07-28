using System.Text.RegularExpressions;

namespace TwitchBanTrackerApi.Services
{
    public class TwitchUserMessages
    {
        private readonly HttpClient _client;

        public TwitchUserMessages(HttpClient httpClient)
        {
            _client = httpClient;
        }

        public async Task<List<string>> GetUserMessagesAsync(string channelId, string user)
        {
            string url = $"https://harambelogs.pl/channelid/{channelId}/user/{user}";

            _client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.39.0");
            _client.DefaultRequestHeaders.Add("Accept", "*/*");

            HttpResponseMessage response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            string[] lines = responseBody.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string lastMessage = lines.Length > 0 ? lines[^1] : null;

            int messageCount = lines.Length;
            int startIndex = Math.Max(0, messageCount - 5);

            List<string> resultMessages = new List<string>();
            if (lastMessage != null && (lastMessage.Contains($"{user} has been banned") || lastMessage.Contains($"{user} has been timed out for")))
            {
                startIndex = Math.Max(0, messageCount - 6);
                for (int i = startIndex; i < messageCount - 1; i++)
                {
                    resultMessages.Add(ProcessMessage(lines[i]));
                }
            }
            else
            {
                for (int i = startIndex; i < messageCount; i++)
                {
                    resultMessages.Add(ProcessMessage(lines[i]));
                }
            }

            return resultMessages;
        }

        private string ProcessMessage(string message)
        {
            string pattern = @"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\] #[^\s]+ [^:]+: (.+)";
            Match match = Regex.Match(message, pattern);

            return match.Success ? $"{match.Groups[1].Value}: {match.Groups[2].Value}" : message;
        }
    }
}
