using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Application.User
{
    public class Logout
    {
        public class Command : IRequest<bool>
        {
        }

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public Handler(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                if (_httpContextAccessor.HttpContext?.Request.Cookies["oblog"] != null)
                {
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("oblog");
                    _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }

                return Task.FromResult(true);
            }
        }
    }
}