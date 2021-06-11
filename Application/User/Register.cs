using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
    public class Register
    {
        private static string CreateMD5(string email)
        {
            // Use input string to calculate MD5 hash
            using var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(email);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            foreach (var t in hashBytes) sb.Append(t.ToString("X2"));
            return sb.ToString();
        }

        public class Command : IRequest<Domain.User>
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Username).NotEmpty().Length(3, 30);
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).Password();
            }
        }

        public class Handler : IRequestHandler<Command, Domain.User>
        {
            private readonly DataContext _context;
            private readonly ICookieGenerator _cookieGenerator;
            private readonly IPasswordHasher _passwordHasher;

            public Handler(DataContext context, ICookieGenerator cookieGenerator, IPasswordHasher passwordHasher)
            {
                _cookieGenerator = cookieGenerator;
                _passwordHasher = passwordHasher;
                _context = context;
            }

            public async Task<Domain.User> Handle(Command request, CancellationToken cancellationToken)
            {
                if (await _context.Users.Where(x => x.Email == request.Email).AnyAsync())
                    throw new RestException(HttpStatusCode.BadRequest, new {Email = "Email already exists"});

                if (await _context.Users.Where(x => x.Username == request.Username).AnyAsync())
                    throw new RestException(HttpStatusCode.BadRequest, new {Username = "Username already exists"});

                var salt = Guid.NewGuid().ToByteArray();
                var user = new Domain.User
                {
                    Email = request.Email,
                    Username = request.Username,
                    Bio = "",
                    Image = $"https://gravatar.com/avatar/{CreateMD5(request.Email)}?d=identicon",
                    Hash = await _passwordHasher.Hash(request.Password, salt),
                    Salt = salt,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };


                await _context.Users.AddAsync(user);
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success) throw new Exception("Problem creating user");

                _cookieGenerator.GenerateCookie(user.Username);

                return user;
            }
        }
    }
}