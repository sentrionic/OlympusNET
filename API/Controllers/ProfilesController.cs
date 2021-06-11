using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ProfilesController : BaseController
    {
        [HttpGet("{username}")]
        [AllowAnonymous]
        public async Task<ActionResult<Profile>> Get(string username)
        {
            return await Mediator.Send(new Details.Query {Username = username});
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Profile>>> List(string search)
        {
            return await Mediator.Send(new List.Query {Search = search});
        }
    }
}