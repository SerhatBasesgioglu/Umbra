using Umbra.Common.Dto;
using Umbra.Common.Dump;

namespace Umbra.Metrics.Dump;

public class WorkItemFetcher : BackgroundService
{
    private readonly AzureDevOpsHttpClient _client;
    private readonly AdoMetrics _metrics;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(2);

    public WorkItemFetcher(IConfiguration config, AdoMetrics metrics)
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
        var allWorkItems = new List<WorkItemDto>();
        var query = """
              select 
                [system.title]
              from workitems
              where
                [system.workitemtype] = 'Product Backlog Item'
              and not [system.state] in ('Done', 'Closed', 'New', 'Removed', 'Committed', 'Approved', 'Suspended')
            """;
        var requestBody = new { query };
        var wiqlResponse = await _client.PostAsync<WiqlDto>("Sandbox/_apis/wit/wiql", requestBody);
        var pbiList = wiqlResponse.WorkItems.Select(x => x.Id).ToList();
        foreach (var batch in pbiList.Chunk(200))
        {
            Console.WriteLine("yo");
            var body = new { ids = batch, fields = new[] { "System.Id", "System.State" } };

            var workItemResult = await _client.PostAsync<AdoList<WorkItemDto>>(
                "Sandbox/_apis/wit/workitemsbatch",
                body
            );
            var workItems = workItemResult.Value;
            allWorkItems.AddRange(workItems);
        }

        _metrics.ProcessWorkItems(allWorkItems);
        Console.WriteLine($"Metrics updated for {allWorkItems.Count} items.");
    }
}
