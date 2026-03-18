using Microsoft.AspNetCore.Mvc;
using Umbra.Poc.Dump;

namespace Umbra.Poc.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly PipelineFetcher _fetcher;

    public TestController(PipelineFetcher fetcher)
    {
        _fetcher = fetcher;
    }

    [HttpGet]
    public async Task<string> Get()
    {
        _fetcher.Fetch();
        return "Fetched";
    }
}
