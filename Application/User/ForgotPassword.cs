using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Persistence;

namespace Application.User
{
    public class ForgotPassword
    {
        public class Command : IRequest<bool>
        {
            public string Email { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
            }
        }
        
        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly DataContext _context;
            private readonly IDistributedCache _distributedCache;
            private readonly IMailSender _mailSender;

            public Handler(DataContext context, IDistributedCache distributedCache, IMailSender mailSender)
            {
                _context = context;
                _distributedCache = distributedCache;
                _mailSender = mailSender;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.Email == request.Email);

                if (user == null)
                    return true;

                var token = Guid.NewGuid();
                
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromDays(3));
                await _distributedCache.SetStringAsync($"forget-password:{token}", user.Id.ToString(), options, cancellationToken);
                
                _mailSender.SendMail(user.Email, $"<a href=\"localhost:3000/reset-password/{token}\">Reset Password</a>");
                
                return true;
            }
        }
    }
}