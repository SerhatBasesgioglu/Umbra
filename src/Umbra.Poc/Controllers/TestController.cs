using Microsoft.AspNetCore.Mvc;
using Umbra.Poc.Dump;

namespace Umbra.Poc.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly PipelineFetcher _pipelineFetcher;
    private readonly WorkItemFetcher _workItemFetcher;

    public TestController(PipelineFetcher pipelineFetcher, WorkItemFetcher workItemFetcher)
    {
        _workItemFetcher = workItemFetcher;
        _pipelineFetcher = pipelineFetcher;
    }

    [HttpGet]
    public async Task<string> Get()
    {
        //await _pipelineFetcher.Fetch();
        await _workItemFetcher.Fetch();
        return "Fetched";
    }
}
