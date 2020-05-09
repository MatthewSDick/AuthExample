using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthExample.Controllers
{
  // [Route("api/[controller]")]
  [Route("auth")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    [HttpPost("signup")]
    public async Task<ActionResult> SignUpUser()
    {
      return Ok();
    }
  }
}