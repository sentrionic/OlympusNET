using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Profile
{
    public class Edit
    {
        public class Command : IRequest<Domain.User>
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Bio { get; set; }
            public IFormFile Image { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Username).NotEmpty().Length(3, 30);
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Bio).MaximumLength(250);
            }
        }

        public class Handler : IRequestHandler<Command, Domain.User>
        {
            private readonly DataContext _context;
            private readonly ICookieGenerator _cookieGenerator;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor, ICookieGenerator cookieGenerator,
                IPhotoAccessor photoAccessor)
            {
                _userAccessor = userAccessor;
                _cookieGenerator = cookieGenerator;
                _photoAccessor = photoAccessor;
                _context = context;
            }

            public async Task<Domain.User> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.Username == _userAccessor.GetCurrentUsername());

                if (user.Email != request.Email)
                {
                    if (await _context.Users.Where(x => x.Email == request.Email).AnyAsync())
                        throw new RestException(HttpStatusCode.BadRequest, new {Email = "Email already exists"});
                    user.Email = request.Email;
                }

                if (user.Username != request.Username)
                {
                    if (await _context.Users.Where(x => x.Username == request.Username).AnyAsync())
                        throw new RestException(HttpStatusCode.BadRequest, new {Username = "Username already exists"});

                    user.Username = request.Username;
                }

                user.Bio = request.Bio ?? user.Bio;
                user.UpdatedAt = DateTime.Now;

                if (request.Image != null)
                {
                    var directory = $"dotnet/{user.Id}/avatar";
                    user.Image = await _photoAccessor.AddProfileImage(request.Image, directory);
                }

                var success = await _context.SaveChangesAsync() > 0;

                if (!success)
                    throw new Exception("Problem saving changes");

                _cookieGenerator.GenerateCookie(user.Username);

                return user;
            }
        }
    }
}