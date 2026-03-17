using System.Diagnostics.Metrics;

public class AdoMetrics
{
    private readonly Counter<long> _runsProcessedCounter;
    private readonly ObservableGauge<int> _totalRunsGauge;
    private int _currentTotalCount;

    public AdoMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Umbra.Poc.Ado");
        _runsProcessedCounter = meter.CreateCounter<long>(
            "ado_runs_processed_total",
            unit: "{run}",
            description: "Total number of  runs seen by scraper"
        );

        _totalRunsGauge = meter.CreateObservableGauge(
            "ado_pipeline_total_count",
            () => _currentTotalCount,
            unit: "{run}",
            description: "The current total count of runs in the pipeline"
        );
    }

    public void IncrementProcessed(string result)
    {
        _runsProcessedCounter.Add(1, new KeyValuePair<string, object?>("result", result));
    }

    public void SetTotalCount(int count)
    {
        _currentTotalCount = count;
    }
}
