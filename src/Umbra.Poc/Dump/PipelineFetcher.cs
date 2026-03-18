namespace Umbra.Poc.Dump;

public class PipelineFetcher 
{
    private readonly AzureDevOpsHttpClient _client;
    private readonly AdoMetrics _metrics;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(1);
    private readonly string _endpoint;
    private readonly string _project1;
    private readonly string _project2;

    public PipelineFetcher(IConfiguration config, AdoMetrics metrics)
    {
        _client = new AzureDevOpsHttpClient(config);
        _metrics = metrics;
        _endpoint = config["ado:endpoint"];
        _project1 = config["ado:project1"];
        _project2 = config["ado:project2"];
    }

    public async void Fetch()
    {
        var projectResponse = await _client.GetAsync<AdoList<ProjectDto>>("_apis/projects?$top=1000");
        var projects = projectResponse.Value;
        var count = 0;
        foreach (var project in projects)
        {
            //if (project.Name != _project1 && project.Name != _project2) continue;
            var pipelineResponse = await _client.GetAsync<AdoList<PipelineDto>>(
                $"{project.Id}/_apis/build/definitions"
            );
            if (pipelineResponse.Count == 0)
                continue;
            var pipelines = pipelineResponse.Value;

            var pipelineRunResponse = await _client.GetAsync<AdoList<PipelineRunDto>>(
                $"{project.Id}/_apis/build/builds?$top=50"
            );
            if (pipelineRunResponse.Count == 0)
                continue;
            var pipelineRuns = pipelineRunResponse.Value;
            _metrics.ProcessNewRuns(project, pipelineRuns);
            count += pipelineRunResponse.Count;

        }
        Console.WriteLine(count);
    }

}
