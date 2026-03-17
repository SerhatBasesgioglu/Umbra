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
    public async Task<List<PipelineRun>> Get()
    {
        // var response = await _client.GetAsync<AdoList<PipelineRun>>(
        //  _endpoint
        // );
        var response = await _client.GetAsync<AdoList<PipelineRun>>(_endpoint);
        var runs = response.Value;
        Console.WriteLine("yo");
        

        return runs;
    }
}
