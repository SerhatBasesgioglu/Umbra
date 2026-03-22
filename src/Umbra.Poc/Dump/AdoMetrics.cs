using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using OpenTelemetry.Trace;

namespace Umbra.Poc.Dump;

public class AdoMetrics
{
    private readonly Counter<long> _runsQueuedCounter;
    private readonly Counter<long> _runsStartedCounter;
    private readonly Counter<long> _runsFinishedCounter;
    private readonly Histogram<double> _runDuration;
    private readonly Histogram<double> _queueDuration;
    private readonly Gauge<int> _workItemGauge;

    private readonly ConcurrentDictionary<int, DateTime> _queuedRuns = new();
    private readonly ConcurrentDictionary<int, DateTime> _startedRuns = new();
    private readonly ConcurrentDictionary<int, DateTime> _finishedRuns = new();

    public AdoMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Umbra.Poc.Ado");
        _runsQueuedCounter = meter.CreateCounter<long>(
            "ado_runs_queued_total",
            unit: "{run}",
            description: "Total number of runs queued."
        );
        _runsStartedCounter = meter.CreateCounter<long>(
            "ado_runs_started_total",
            unit: "{run}",
            description: "Total number of runs started."
        );
        _runsFinishedCounter = meter.CreateCounter<long>(
            "ado_runs_finished_total",
            unit: "{run}",
            description: "Total number of runs finished."
        );

        _runDuration = meter.CreateHistogram<double>(
            "ado_run_duration_seconds",
            unit: "s",
            description: "Duration of completed runs"
        );

        _queueDuration = meter.CreateHistogram<double>(
            "ado_queue_duration_seconds",
            unit: "s",
            description: "Queue duration of completed runs"
        );

        _workItemGauge = meter.CreateGauge<int>(
            "ado_pbi_state_count",
            unit: "{items}",
            description: "Current count of PBIs by state"
        );
    }

    public void ProcessRuns(ProjectDto project, IEnumerable<PipelineRunDto> runs)
    {
        if (!runs.Any())
            return;

        var newQueuedRuns = runs.Where(r => r.QueueTime > DateTime.MinValue).ToList();
        var newStartedRuns = runs.Where(r => r.StartTime > DateTime.MinValue).ToList();
        var newFinishedRuns = runs.Where(r => r.FinishTime > DateTime.MinValue).ToList();

        RecordDurations(project, newFinishedRuns);
        ProcessCount(newQueuedRuns, project, _runsQueuedCounter, _queuedRuns);
        ProcessCount(newStartedRuns, project, _runsStartedCounter, _startedRuns);
        ProcessCount(newFinishedRuns, project, _runsFinishedCounter, _finishedRuns);
    }

    private void RecordDurations(ProjectDto project, IEnumerable<PipelineRunDto> runs)
    {
        foreach (var run in runs)
        {
            string pipelineName = SanitizeLabel(run.Definition.Name);
            var queueDuration = (run.StartTime - run.QueueTime).TotalSeconds;
            var runDuration = (run.FinishTime - run.StartTime).TotalSeconds;

            _queueDuration.Record(
                queueDuration,
                new KeyValuePair<string, object?>("project", project.Name),
                new KeyValuePair<string, object?>("pipeline", pipelineName)
            );
            _runDuration.Record(
                runDuration,
                new KeyValuePair<string, object?>("project", project.Name),
                new KeyValuePair<string, object?>("pipeline", pipelineName)
            );
        }
    }

    private void ProcessCount(
        IEnumerable<PipelineRunDto> runs,
        ProjectDto project,
        Counter<long> counter,
        ConcurrentDictionary<int, DateTime> processed
    )
    {
        var now = DateTime.UtcNow;
        foreach (var run in runs)
        {
            string pipelineName = SanitizeLabel(run.Definition.Name);

            var isNewRun = processed.TryAdd(run.Id, now);
            if (isNewRun)
            {
                counter.Add(
                    1,
                    new KeyValuePair<string, object?>("result", run.Result),
                    new KeyValuePair<string, object?>("project", project.Name),
                    new KeyValuePair<string, object?>("pipeline", pipelineName)
                );
            }
            else
            {
                processed[run.Id] = now;
            }
        }

        var cutoff = now.AddHours(-12);
        var expiredRuns = processed
            .Where(kvp => kvp.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();
        foreach (var run in expiredRuns)
        {
            processed.TryRemove(run, out _);
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
