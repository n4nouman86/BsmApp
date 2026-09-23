using Microsoft.AspNetCore.Mvc;

namespace BsmJoin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}