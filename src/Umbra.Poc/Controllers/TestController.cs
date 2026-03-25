using Microsoft.AspNetCore.Mvc;

namespace Umbra.Poc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public string Get()
    {
        return "yo";
    }
}
