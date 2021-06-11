using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/articles")]
    public class CommentsController : BaseController
    {
        [HttpPost("{slug}/comments")]
        public Task<CommentResponse> Create(string slug, Create.Command command, CancellationToken cancellationToken)
        {
            command.Slug = slug;
            return Mediator.Send(command, cancellationToken);
        }

        [HttpGet("{slug}/comments")]
        [AllowAnonymous]
        public Task<List<CommentResponse>> Get(string slug, CancellationToken cancellationToken)
        {
            return Mediator.Send(new List.Query {Slug = slug}, cancellationToken);
        }

        [HttpDelete("{slug}/comments/{id}")]
        public Task Delete(string slug, int id, CancellationToken cancellationToken)
        {
            return Mediator.Send(new Delete.Command {CommentId = id}, cancellationToken);
        }
    }
}