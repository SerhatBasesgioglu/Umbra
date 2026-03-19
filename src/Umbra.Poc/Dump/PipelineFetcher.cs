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
            //if (project.Name != "Sandbox") continue;
            Console.WriteLine($"Processing {project.Name}");
            var pipelineResponse = await _client.GetAsync<AdoList<PipelineDto>>(
                $"{project.Id}/_apis/build/definitions"
            );
            if (pipelineResponse.Count == 0)
                continue;
            var pipelines = pipelineResponse.Value;

            //Cok data dondurmemek icin mintime parametresi var, ama bitmemis runlar cok once baslamis olabilir. Bu nedenle completed ve
            //noncompleted runlar farkli query ile donduruluyor. noncompleted az oldugu icin zaman filtesine gerek yok.
            string minTime = DateTime.UtcNow.AddMinutes(-10).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var pipelineRunResponse = await _client.GetAsync<AdoList<PipelineRunDto>>(
                $"{project.Id}/_apis/build/builds?statusFilter=completed&minTime={minTime}&queryOrder=finishTimeAscending"
            );

            var unfinishedPipelineRunResponse = await _client.GetAsync<AdoList<PipelineRunDto>>(
                $"{project.Id}/_apis/build/builds?statusFilter=inProgress,cancelling,postponed,notStarted"
            );

            var pipelineRuns = pipelineRunResponse.Value;
            var unfinishedPipelineRuns = unfinishedPipelineRunResponse.Value;

            var allRuns = pipelineRuns
                .Concat(unfinishedPipelineRuns)
                .DistinctBy(r => r.Id)
                .ToList();

            _metrics.ProcessRuns(project, pipelineRuns, unfinishedPipelineRuns);
            count += pipelineRunResponse.Count;
        }
        Console.WriteLine(count);
    }
}
