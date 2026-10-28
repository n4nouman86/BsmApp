using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}