using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profile
{
    public class Details
    {
        public class Query : IRequest<Profile>
        {
            public string Username { get; set; }
        }

        public class Handler : IRequestHandler<Query, Profile>
        {
            private readonly DataContext _context;
            private readonly IProfileReader _profileReader;

            public Handler(DataContext context, IProfileReader profileReader)
            {
                _context = context;
                _profileReader = profileReader;
            }

            public async Task<Profile> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .Include(x => x.Followers)
                    .Include(x => x.Followings)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Username == request.Username);
                
                if (user == null)
                    throw new RestException(HttpStatusCode.NotFound, new { Profile = "Not Found" });

                return await _profileReader.ReadProfile(user);
            }
        }
    }
}