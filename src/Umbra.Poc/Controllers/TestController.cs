using Microsoft.AspNetCore.Mvc;

namespace Umbra.Poc.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly AzureDevOpsHttpClient _client;
    private readonly string _endpoint;

    public TestController(IConfiguration config)
    {
        _client = new AzureDevOpsHttpClient(config);
        _endpoint = config["ado:endpoint"];
    }

    [HttpGet]
    public async Task<object> Get()
    {
        // var response = await _client.GetAsync<AdoList<PipelineRun>>(
        //  _endpoint
        // );
        var responseTest = await _client.GetAsync<AdoList<object>>(_endpoint);

        return responseTest;
    }
}
