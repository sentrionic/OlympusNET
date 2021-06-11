using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User
{
    public class CurrentUser
    {
        public class Query : IRequest<Domain.User>
        {
        }

        public class Handler : IRequestHandler<Query, Domain.User>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Domain.User> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Users.FirstAsync(x => x.Username == _userAccessor.GetCurrentUsername(),
                    cancellationToken);
            }
        }
    }
}