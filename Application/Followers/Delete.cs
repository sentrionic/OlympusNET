using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Profile;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Followers
{
    public class Delete
    {
        public class Command : IRequest<Profile.Profile>
        {
            public string Username { get; set; }
        }

        public class Handler : IRequestHandler<Command, Profile.Profile>
        {
            private readonly DataContext _context;
            private readonly IProfileReader _profileReader;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor, IProfileReader profileReader)
            {
                _userAccessor = userAccessor;
                _profileReader = profileReader;
                _context = context;
            }

            public async Task<Profile.Profile> Handle(Command request, CancellationToken cancellationToken)
            {
                var observer =
                    await _context.Users.SingleOrDefaultAsync(x => x.Username == _userAccessor.GetCurrentUsername());

                var target = await _context.Users
                    .Include(x => x.Followers)
                    .Include(x => x.Followings)
                    .SingleOrDefaultAsync(x => x.Username == request.Username);

                if (target == null)
                    throw new RestException(HttpStatusCode.NotFound, new {User = "Not found"});

                var following =
                    await _context.Followings.SingleOrDefaultAsync(x =>
                        x.ObserverId == observer.Id && x.TargetId == target.Id);

                if (following == null)
                    throw new RestException(HttpStatusCode.BadRequest, new {User = "You are not following this user"});

                _context.Followings.Remove(following);

                var success = await _context.SaveChangesAsync() > 0;

                if (success) return await _profileReader.ReadProfile(target);

                throw new Exception("Problem saving changes");
            }
        }
    }
}