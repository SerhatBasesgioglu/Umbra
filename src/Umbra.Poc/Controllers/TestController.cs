using Microsoft.AspNetCore.Mvc;
using Umbra.Common.Dto;
using Umbra.Common.Dump;

namespace Umbra.Poc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(AzureDevOpsHttpClient client) : ControllerBase
{
    private readonly AzureDevOpsHttpClient _client = client;

    [HttpGet]
    public async Task<List<WorkItemDto>> Get()
    {
        var allWorkItems = new List<WorkItemDto>();
        var query = """
              select 
                [system.title]
              from workitems
              where
                [System.WorkItemType] = "Task"
                and [system.state] not in ('Done', 'Removed')
                AND [System.AssignedTo] = "user"
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
        return allWorkItems;
    }
}
