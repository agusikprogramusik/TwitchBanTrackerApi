using AutoMapper;
using TwitchBanTrackerApi.Common.Domain;
using TwitchBanTrackerApi.Common.Entities;

namespace TwitchBanTrackerApi.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}