using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User
{
    public class Login
    {
        public class Query : IRequest<Domain.User>
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.Email).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Query, Domain.User>
        {
            private readonly DataContext _context;
            private readonly ICookieGenerator _cookieGenerator;
            private readonly IPasswordHasher _passwordHasher;

            public Handler(IPasswordHasher passwordHasher, DataContext context,
                ICookieGenerator cookieGenerator)
            {
                _passwordHasher = passwordHasher;
                _context = context;
                _cookieGenerator = cookieGenerator;
            }

            public async Task<Domain.User> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.Where(x => x.Email == request.Email)
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized);

                if (!user.Hash.SequenceEqual(await _passwordHasher.Hash(request.Password, user.Salt)))
                    throw new RestException(HttpStatusCode.Unauthorized, new {Error = "Invalid email / password."});

                _cookieGenerator.GenerateCookie(user.Username);

                return user;
            }
        }
    }
}