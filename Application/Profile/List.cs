using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profile
{
    public class List
    {
        public class Query : IRequest<List<Profile>>
        {
            public string Search { get; set; }
        }

        public class Handler : IRequestHandler<Query, List<Profile>>
        {
            private readonly DataContext _context;
            private readonly IProfileReader _profileReader;

            public Handler(DataContext context, IProfileReader profileReader)
            {
                _context = context;
                _profileReader = profileReader;
            }

            public async Task<List<Profile>> Handle(Query request, CancellationToken cancellationToken)
            {
                var queryable = _context.Users
                    .Include(x => x.Followers)
                    .Include(x => x.Followings)
                    .AsNoTracking()
                    .Take(20)
                    .OrderByDescending(x => x.Followers.Count)
                    .AsQueryable();

                if (request.Search != null)
                    queryable = queryable
                        .Where(x =>
                            x.Username.ToLower().Contains(request.Search.ToLower()) ||
                            x.Bio.ToLower().Contains(request.Search.ToLower())
                        );

                var profiles = await queryable.ToListAsync(cancellationToken);

                var list = new List<Profile>();
                profiles.ForEach(profile => { list.Add(_profileReader.ReadProfile(profile).Result); });

                return list;
            }
        }
    }
}