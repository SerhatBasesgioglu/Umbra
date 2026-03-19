using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Umbra.Poc.Dump;

public class AdoMetrics
{
    private readonly Counter<long> _runsProcessedCounter;
    private readonly ConcurrentDictionary<string, int> _lastSeenRuns = new();
    private readonly Gauge<int> _workItemGauge;

    public AdoMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Umbra.Poc.Ado");
        _runsProcessedCounter = meter.CreateCounter<long>(
            "ado_runs_processed_total",
            unit: "{run}",
            description: "Total number of runs seen by scraper"
        );

        _workItemGauge = meter.CreateGauge<int>(
            "ado_pbi_state_count",
            unit: "{items}",
            description: "Current count of PBIs by state"
        );
    }

    public void ProcessNewRuns(ProjectDto project, IEnumerable<PipelineRunDto> runs)
    {
        var sortedRuns = runs.OrderBy(r => r.Id).ToList();
        _lastSeenRuns.TryGetValue(project.Id, out int lastProcessedId);
        var newRuns = sortedRuns.Where(r => r.Id > lastProcessedId).OrderBy(r => r.Id).ToList();

        foreach (var run in newRuns)
        {
            string pipelineName = SanitizeLabel(run.Definition.Name);
            _runsProcessedCounter.Add(
                1,
                new KeyValuePair<string, object?>("result", run.Result),
                new KeyValuePair<string, object?>("project", project.Name),
                new KeyValuePair<string, object?>("pipeline", pipelineName)
            );
            _lastSeenRuns[project.Id] = run.Id;
        }
    }

    public void ProcessWorkItems(List<WorkItemDto> workItems)
    {
        var stateTotals = workItems
            .GroupBy(p => p.Fields.State)
            .Select(g => new { State = g.Key, Count = g.Count() });
        foreach (var group in stateTotals)
        {
            _workItemGauge.Record(
                group.Count,
                new KeyValuePair<string, object?>("state", group.State)
            );
        }
    }

    private string SanitizeLabel(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "unknown";
        return new string(
            value.Where(c => !char.IsControl(c) && !char.IsSurrogate(c)).ToArray().Trim()
        );
    }
}
