using Microsoft.AspNetCore.Mvc;

namespace Umbra.Poc.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly AzureDevOpsHttpClient _client;
    private readonly AdoMetrics _metrics;
    private readonly string _endpoint;

    public TestController(IConfiguration config, AdoMetrics metrics)
    {
        _client = new AzureDevOpsHttpClient(config);
        _metrics = metrics;
        _endpoint = config["ado:endpoint"];
    }

    [HttpGet]
    public async Task<List<PipelineRunDto>> Get()
    {
        // var response = await _client.GetAsync<AdoList<PipelineRun>>(
        //  _endpoint
        // );
        var projectResponse = await _client.GetAsync<AdoList<ProjectDto>>("_apis/projects");
        var projects = projectResponse.Value;
        foreach (var project in projects)
        {
            var pipelineResponse = await _client.GetAsync<AdoList<PipelineDto>>(
                $"{project.Id}/_apis/pipelines"
            );
            if (pipelineResponse.Count == 0)
                continue;

            var pipelines = pipelineResponse.Value;

            foreach (var pipeline in pipelines)
            {
                // Console.WriteLine(pipeline.Name);
                var pipelineRunResponse = await _client.GetAsync<AdoList<PipelineRunDto>>(
                    $"{project.Id}/_apis/pipelines/{pipeline.Id}/runs"
                );

                if (pipelineRunResponse.Count == 0)
                    continue;
                var pipelineRuns = pipelineRunResponse.Value;
                Console.WriteLine(pipelineRuns.FirstOrDefault().Name);
            }

            //Console.WriteLine($"{project.Id}, {project.Name}");
        }

        var response = await _client.GetAsync<AdoList<PipelineRunDto>>(_endpoint);
        var count = response.Count;
        var runs = response.Value;

        _metrics.SetTotalCount(count);
        var latestRun = runs.FirstOrDefault();
        _metrics.IncrementProcessed(latestRun.Result);
        Console.WriteLine(count);

        return runs;
    }
}
