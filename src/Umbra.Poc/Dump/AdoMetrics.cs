using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

public class AdoMetrics
{
    private readonly Counter<long> _runsProcessedCounter;
    private readonly ConcurrentDictionary<string, int> _lastSeenRuns = new();

    public AdoMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Umbra.Poc.Ado");
        _runsProcessedCounter = meter.CreateCounter<long>(
            "ado_runs_processed_total",
            unit: "{run}",
            description: "Total number of runs seen by scraper"
        );

    }
    
    public void ProcessNewRuns(ProjectDto project, IEnumerable<PipelineRunDto> runs)
    {
        var sortedRuns = runs.OrderBy(r => r.Id).ToList();
        _lastSeenRuns.TryGetValue(project.Id, out int lastProcessedId);
        var newRuns = runs.Where(r => r.Id > lastProcessedId).OrderBy(r => r.Id).ToList();

        foreach (var run in newRuns)
        {
            _runsProcessedCounter.Add(1, new KeyValuePair<string, object?>("result", run.Result),
                new KeyValuePair<string, object?>("project", project.Name),
                new KeyValuePair<string, object?>("pipeline", run.Definition.Name));
            _lastSeenRuns[project.Id] = run.Id;
        }
    }
}
