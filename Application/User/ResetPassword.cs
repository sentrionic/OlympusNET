using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Validators;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Persistence;

namespace Application.User
{
    public class ResetPassword
    {
        public class Command : IRequest<Domain.User>
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmNewPassword { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Token).NotEmpty();
                RuleFor(x => x.NewPassword).Password();
                RuleFor(x => x.ConfirmNewPassword).Password().Equal(x => x.NewPassword);
            }
        }

        public class Handler : IRequestHandler<Command, Domain.User>
        {
            private readonly DataContext _context;
            private readonly IDistributedCache _distributedCache;
            private readonly ICookieGenerator _cookieGenerator;
            private readonly IPasswordHasher _passwordHasher;

            public Handler(DataContext context, IDistributedCache distributedCache, ICookieGenerator cookieGenerator, IPasswordHasher passwordHasher)
            {
                _context = context;
                _distributedCache = distributedCache;
                _cookieGenerator = cookieGenerator;
                _passwordHasher = passwordHasher;
            }

            public async Task<Domain.User> Handle(Command request, CancellationToken cancellationToken)
            {
                var key = $"forget-password:{request.Token}";
                var value = await _distributedCache.GetStringAsync(key, cancellationToken);
                Console.Write(value);

                if (value == null)
                    throw new RestException(HttpStatusCode.BadRequest, new {Token = "Token Expired"});

                var userId = int.Parse(value);

                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.Id == userId);

                if (user == null)
                    throw new RestException(HttpStatusCode.NotFound);
                
                var salt = Guid.NewGuid().ToByteArray();
                user.Salt = salt;
                user.Hash = await _passwordHasher.Hash(request.NewPassword, salt);
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success) throw new Exception("Problem changing password");

                await _distributedCache.RemoveAsync(key, cancellationToken);

                _cookieGenerator.GenerateCookie(user.Username);

                return user;
            }
        }
    }
}