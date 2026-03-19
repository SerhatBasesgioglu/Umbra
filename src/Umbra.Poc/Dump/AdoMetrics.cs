using OpenTelemetry.Trace;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Umbra.Poc.Dump;

public class AdoMetrics
{
    private readonly Counter<long> _runsProcessedCounter;
    private readonly Gauge<int> _runsUnfinishedGauge;
    private readonly ConcurrentDictionary<int, DateTime> _processedRuns = new();
    private readonly Gauge<int> _workItemGauge;

    public AdoMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Umbra.Poc.Ado");
        _runsProcessedCounter = meter.CreateCounter<long>(
            "ado_runs_processed_total",
            unit: "{run}",
            description: "Total number of runs seen by scraper"
        );

        _runsUnfinishedGauge = meter.CreateGauge<int>(
            "ado_runs_unfinished_count",
            unit: "{run}",
            description: "Current count of unfinished runs"
        );

        _workItemGauge = meter.CreateGauge<int>(
            "ado_pbi_state_count",
            unit: "{items}",
            description: "Current count of PBIs by state"
        );
    }

    private void ProcessUnFinishedRuns(ProjectDto project, IEnumerable<PipelineRunDto> runs)
    {
        if (!runs.Any()) return;
        var stateTotals = runs.GroupBy(r => r.Status)
            .Select(g => new {Status = g.Key, Count = g.Count() });

        foreach ( var group in stateTotals)
        {
            _runsUnfinishedGauge.Record(
                group.Count,
                new KeyValuePair<string, object?>("project", project.Name)
            );
        }
    }
    private void ProcessFinishedRuns(ProjectDto project, IEnumerable<PipelineRunDto> runs)
    {
        if (!runs.Any()) return;
        var now = DateTime.UtcNow;
        var newRuns = runs.Where(r => !_processedRuns.ContainsKey(r.Id)).ToList();

        foreach (var run in newRuns)
        {
            string pipelineName = SanitizeLabel(run.Definition.Name);
            _runsProcessedCounter.Add(
                1,
                new KeyValuePair<string, object?>("result", run.Result),
                new KeyValuePair<string, object?>("project", project.Name),
                new KeyValuePair<string, object?>("pipeline", pipelineName)
            );
            _processedRuns.TryAdd(run.Id, run.FinishTime);
        }

        var cutoff = now.AddHours(-12);
        var expiredRuns = _processedRuns.Where(kvp => kvp.Value < cutoff).Select(kvp => kvp.Key).ToList();
        foreach (var run in expiredRuns)
        {
            _processedRuns.TryRemove(run, out _);
        }
    }

    public void ProcessRuns(ProjectDto project, IEnumerable<PipelineRunDto> runs, IEnumerable<PipelineRunDto> unfinishedRuns)
    {
        ProcessFinishedRuns(project, runs);
        ProcessUnFinishedRuns(project, unfinishedRuns);
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
