using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TwitchBanTrackerApi.Common.Entities;
using TwitchBanTrackerApi.Infrastructure.Persistence;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace TwitchBanTrackerApi.Services;

public class TwitchListenerService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TwitchListenerService> _logger;
    private readonly TwitchUserMessages _twitchUserMessages;

    private TwitchClient _client;
    private TwitchAPI _api;

    // TODO: Replace the placeholders with your own values or move it to environment variables
    private readonly string _channelId = "#";
    private readonly string _clientId = "#";
    private readonly string _clientSecret = "#";
    private readonly string _channelName = "#";
    private readonly string _botUsername = "#";
    private readonly string _botOAuthToken = "#";

    public TwitchListenerService(TwitchUserMessages twitchUserMessages, IServiceProvider services, ILogger<TwitchListenerService> logger)
    {
        _twitchUserMessages = twitchUserMessages;
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string accessToken = await GetAccessToken(_clientId, _clientSecret);

        _api = new TwitchAPI();
        _api.Settings.ClientId = _clientId;
        _api.Settings.AccessToken = accessToken;

        ConnectionCredentials credentials = new ConnectionCredentials(_botUsername, _botOAuthToken);
        _client = new TwitchClient();
        _client.Initialize(credentials, _channelName);

        // TODO: After implementing the OnUserUnbanned event, uncomment the following line
        //_client.OnUserUnbanned; += Client_OnUserUnbanned;

        _client.OnUserBanned += Client_OnUserBanned;
        _client.OnUserTimedout += Client_OnUserTimedOut;
        _client.OnMessageReceived += Client_OnMessageReceived;

        _client.Connect();
        _logger.LogInformation("Twitch listener service started.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Disconnect();
        _logger.LogInformation("Twitch listener service stopped.");
        return Task.CompletedTask;
    }

    //private async void Client_OnUserUnbanned(object sender, ChannelUnban e)
    //{
    //    using (var scope = _services.CreateScope())
    //    {
    //        var context = scope.ServiceProvider.GetRequiredService<TwitchBanTrackerDbContext>();
    //        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == e.UserName);
    //        if (user != null)
    //        {
    //            context.Users.Remove(user);
    //        }
    //        await context.SaveChangesAsync();
    //    }
    //    _logger.LogInformation($"User unbanned: {e.UserName}");
    //}

    private async void Client_OnUserBanned(object sender, OnUserBannedArgs e)
    {
        using (var scope = _services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TwitchBanTrackerDbContext>();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == e.UserBan.Username);
            if (user == null)
            {
                user = new User { Username = e.UserBan.Username, BannedAt = DateTime.Now, BanDuration = "Permanently banned", LastMessages = await _twitchUserMessages.GetUserMessagesAsync(_channelId, e.UserBan.Username) };
                context.Users.Add(user);
            }
            else
            {
                user.BannedAt = DateTime.Now;
                user.BanDuration = "Permanently banned";
                context.Users.Update(user);
            }
            await context.SaveChangesAsync();
        }
        _logger.LogInformation($"User banned: {e.UserBan.Username} (permanent)");
    }

    private async void Client_OnUserTimedOut(object sender, OnUserTimedoutArgs e)
    {
        using (var scope = _services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TwitchBanTrackerDbContext>();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == e.UserTimeout.Username);
            if (e.UserTimeout.TimeoutDuration < 600)
            {
                return;
            }
            if (user == null)
            {
                user = new User { Username = e.UserTimeout.Username, BannedAt = DateTime.Now, BanDuration = $"{e.UserTimeout.TimeoutDuration}s", LastMessages = await _twitchUserMessages.GetUserMessagesAsync(_channelId, e.UserTimeout.Username) };
                context.Users.Add(user);
            }
            else
            {
                user.BannedAt = DateTime.Now;
                user.BanDuration = $"{e.UserTimeout.TimeoutDuration}s";
                context.Users.Update(user);
            }
            await context.SaveChangesAsync();
        }
        _logger.LogInformation($"User timed out: {e.UserTimeout.Username} (duration: {e.UserTimeout.TimeoutDuration}s)");
    }

    private async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        using (var scope = _services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TwitchBanTrackerDbContext>();
            try
            {
                _logger.LogInformation($"Message received from {e.ChatMessage.Username}: {e.ChatMessage.Message}");

                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == e.ChatMessage.Username);
                if (user != null)
                {
                    context.Users.Remove(user);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"User {e.ChatMessage.Username} removed from database.");
                }
                else
                {
                    _logger.LogWarning($"User {e.ChatMessage.Username} not found in database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing user {e.ChatMessage.Username} from database.");
            }
        }
    }

    private async Task<string> GetAccessToken(string clientId, string clientSecret)
    {
        using (HttpClient client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://id.twitch.tv/oauth2/token");
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            request.Content = new FormUrlEncodedContent(keyValues);

            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);
            return json["access_token"].ToString();
        }
    }
}