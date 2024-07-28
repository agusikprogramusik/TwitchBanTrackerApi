using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchBanTrackerApi.Common.Domain;
using TwitchBanTrackerApi.Infrastructure.Persistence;
using TwitchBanTrackerApi.Services;
using TwitchLib.PubSub.Models.Responses;

namespace TwitchBanTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwitchController : ControllerBase
    {
        //TODO: Implement UserHandler

        private readonly TwitchBanTrackerDbContext _context;
        private readonly TwitchUserMessages _twitchUserMessages;
        private readonly IMapper _mapper;

        public TwitchController(TwitchBanTrackerDbContext context, TwitchUserMessages twitchUserMessages, IMapper mapper)
        {
            _context = context;
            _twitchUserMessages = twitchUserMessages;
            _mapper = mapper;
        }

        [HttpGet("banned-users")]
        public async Task<ActionResult<List<UserDto>>> GetBannedUsers()
        {
            var bannedUsers = await _context.Users.ToListAsync();
            var bannedUsersDto = _mapper.Map<List<UserDto>>(bannedUsers).OrderByDescending(u => u.BannedAt).ToList();
            return Ok(bannedUsersDto);
        }

        [HttpGet("last-messages/{username}")]
        public async Task<ActionResult<List<Message>>> GetLastMessages(string username)
        {
            // TODO: Replace the placeholder with your channelId or move it to environment variables
            var messages = await _twitchUserMessages.GetUserMessagesAsync("#", username);
            return Ok(messages);
        }

        //TODO: Implement BanStatistics

        //[HttpGet("ban-statistics")]
        //public async Task<ActionResult<Dictionary<string, int>>> GetBanStatistics()
        //{
        //    var statistics = await _context.Users.Where(u => u.IsBanned)
        //        .GroupBy(u => u.BannedAt.Date)
        //        .Select(g => new { Date = g.Key, Count = g.Count() })
        //        .ToDictionaryAsync(g => g.Date.ToShortDateString(), g => g.Count);

        //    return statistics;
        //}
    }
}