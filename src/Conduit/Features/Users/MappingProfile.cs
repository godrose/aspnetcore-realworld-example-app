using AutoMapper;
using Conduit.Domain;

namespace Conduit.Features.Users
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Person, User>(MemberList.None);
        }
    }
}
