using AutoMapper;
using Conduit.Domain;

namespace Conduit.Features.Profiles
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<Person, Profile>(MemberList.None);
        }
    }
}
