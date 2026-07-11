using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        [HttpGet("ping")]
        public ActionResult Ping() => Ok(new { status = "alive" });
    }
}
