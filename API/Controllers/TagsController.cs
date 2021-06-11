using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/articles/tags")]
    public class TagsController : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<string>> Tags(CancellationToken cancellationToken)
        {
            return await Mediator.Send(new List.Query(), cancellationToken);
        }
    }
}