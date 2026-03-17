using Microsoft.AspNetCore.Mvc;

namespace Umbra.Poc.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly AzureDevOpsHttpClient _client;
    private readonly AdoMetrics _metrics;
    private readonly string _endpoint;
    private readonly string _project1;
    private readonly string _project2;

    public TestController(IConfiguration config, AdoMetrics metrics)
    {
        _client = new AzureDevOpsHttpClient(config);
        _metrics = metrics;
        _endpoint = config["ado:endpoint"];
        _project1 = config["ado:project1"];
        _project2 = config["ado:project2"];
    }

    [HttpGet]
    public async Task<List<PipelineRunDto>> Get()
    {
        // var response = await _client.GetAsync<AdoList<PipelineRun>>(
        //  _endpoint
        // );
        var projectResponse = await _client.GetAsync<AdoList<ProjectDto>>("_apis/projects?$top=1000");
        var projects = projectResponse.Value;
        var count = 0;
        foreach (var project in projects)
        {
            if (project.Name != _project1 && project.Name != _project2) continue;
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
                    $"{project.Id}/_apis/pipelines/{pipeline.Id}/runs?$top=10"
                );

                if (pipelineRunResponse.Count == 0)
                    continue;
                var pipelineRuns = pipelineRunResponse.Value;
                _metrics.ProcessNewRuns(pipeline.Id, pipelineRuns, project.Name);
                count += pipelineRunResponse.Count;
            }
        }
        Console.WriteLine(count);

        return null;
    }
}
