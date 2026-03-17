using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbra.Poc.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<string> Get()
    {
        return "yo";
    }
}
