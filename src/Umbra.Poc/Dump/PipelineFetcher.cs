namespace Umbra.Poc.Dump;

public class PipelineFetcher : BackgroundService
{
    private readonly AzureDevOpsHttpClient _client;
    private readonly AdoMetrics _metrics;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(8);

    public PipelineFetcher(IConfiguration config, AdoMetrics metrics)
    {
        _client = new AzureDevOpsHttpClient(config);
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(_period);
        while (await timer.WaitForNextTickAsync(token))
        {
            try
            {
                await Fetch();
            }
            catch (Exception ex)
            {
                Console.WriteLine("yeey");
            }
        }
    }

    public async Task Fetch()
    {
        var projectResponse = await _client.GetAsync<AdoList<ProjectDto>>(
            "_apis/projects?$top=1000"
        );
        var projects = projectResponse.Value;
        var count = 0;
        foreach (var project in projects)
        {
            //if (project.Name != _project1 && project.Name != _project2) continue;
            Console.WriteLine($"Processing {project.Name}");
            var pipelineResponse = await _client.GetAsync<AdoList<PipelineDto>>(
                $"{project.Id}/_apis/build/definitions"
            );
            if (pipelineResponse.Count == 0)
                continue;
            var pipelines = pipelineResponse.Value;

            string minTime = DateTime.UtcNow.AddMinutes(-15).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var pipelineRunResponse = await _client.GetAsync<AdoList<PipelineRunDto>>(
                $"{project.Id}/_apis/build/builds?$top=50&minTime={minTime}"
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
