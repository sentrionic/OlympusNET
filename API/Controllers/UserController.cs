using System.Threading;
using System.Threading.Tasks;
using Application.Profile;
using Application.User;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/users")]
    public class UserController : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Register(Register.Command command, CancellationToken cancellationToken)
        {
            return await Mediator.Send(command, cancellationToken);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Login(Login.Query query, CancellationToken cancellationToken)
        {
            return await Mediator.Send(query, cancellationToken);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> Logout(Logout.Command command, CancellationToken cancellationToken)
        {
            return await Mediator.Send(command, cancellationToken);
        }

        [HttpPut("change-password")]
        public async Task<ActionResult<User>> ChangePassword(ChangePassword.Command command,
            CancellationToken cancellationToken)
        {
            return await Mediator.Send(command, cancellationToken);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ForgotPassword(ForgotPassword.Command command,
            CancellationToken cancellationToken)
        {
            return await Mediator.Send(command, cancellationToken);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> ResetPassword(ResetPassword.Command command,
            CancellationToken cancellationToken)
        {
            return await Mediator.Send(command, cancellationToken);
        }

        [HttpGet]
        [Route("~/api/user")]
        public async Task<ActionResult<User>> CurrentUser(CancellationToken cancellationToken)
        {
            return await Mediator.Send(new CurrentUser.Query(), cancellationToken);
        }

        [HttpPut]
        [Route("~/api/user")]
        public async Task<ActionResult<User>> Edit([FromForm] Edit.Command command)
        {
            return await Mediator.Send(command);
        }
    }
}