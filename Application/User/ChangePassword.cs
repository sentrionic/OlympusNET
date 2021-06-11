using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Validators;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User
{
    public class ChangePassword
    {
        public class Command : IRequest<Domain.User>
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmNewPassword { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.CurrentPassword).Password();
                RuleFor(x => x.NewPassword).Password();
                RuleFor(x => x.ConfirmNewPassword).Password().Equal(x => x.NewPassword);
            }
        }
        
        public class Handler : IRequestHandler<Command, Domain.User>
        {
            private readonly DataContext _context;
            private readonly ICookieGenerator _cookieGenerator;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, ICookieGenerator cookieGenerator, IPasswordHasher passwordHasher, IUserAccessor userAccessor)
            {
                _cookieGenerator = cookieGenerator;
                _passwordHasher = passwordHasher;
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Domain.User> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.Username == _userAccessor.GetCurrentUsername());
                
                if (!user.Hash.SequenceEqual(await _passwordHasher.Hash(request.CurrentPassword, user.Salt)))
                    throw new RestException(HttpStatusCode.Unauthorized, new {Error = "Invalid email / password."});
                
                var salt = Guid.NewGuid().ToByteArray();
                user.Salt = salt;
                user.Hash = await _passwordHasher.Hash(request.NewPassword, salt);
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success) throw new Exception("Problem changing password");

                _cookieGenerator.GenerateCookie(user.Username);

                return user;
            }
        }
    }
}