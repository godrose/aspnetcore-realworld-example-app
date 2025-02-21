using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Profiles;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Followers
{
    public class Add
    {
        public class Command : IRequest<ProfileEnvelope>
        {
            public Command(string username)
            {
                Username = username;
            }

            public string Username { get; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Username).NotNull().NotEmpty();
            }
        }

        public class QueryHandler : IRequestHandler<Command, ProfileEnvelope>
        {
            private readonly ConduitContext _context;
            private readonly ICurrentUserAccessor _currentUserAccessor;
            private readonly IProfileReader _profileReader;

            public QueryHandler(ConduitContext context, ICurrentUserAccessor currentUserAccessor, IProfileReader profileReader)
            {
                _context = context;
                _currentUserAccessor = currentUserAccessor;
                _profileReader = profileReader;
            }

            public async Task<ProfileEnvelope> Handle(Command message, CancellationToken cancellationToken)
            {
                var target = await _context.Persons.FirstOrDefaultAsync(x => x.Username == message.Username, cancellationToken);

                if (target == null)
                {
                    throw new RestException(HttpStatusCode.NotFound, new { User = Constants.NOT_FOUND });
                }

                var observer = await _context.Persons.FirstOrDefaultAsync(x => x.Username == _currentUserAccessor.GetCurrentUsername(), cancellationToken);

                var followedPeople = await _context.FollowedPeople.FirstOrDefaultAsync(x => x.ObserverId == observer.PersonId && x.TargetId == target.PersonId, cancellationToken);

                if (followedPeople == null)
                {
                    followedPeople = new FollowedPeople
                    {
                        Observer = observer,
                        ObserverId = observer.PersonId,
                        Target = target,
                        TargetId = target.PersonId
                    };
                    await _context.FollowedPeople.AddAsync(followedPeople, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return await _profileReader.ReadProfile(message.Username);
            }
        }
    }
}