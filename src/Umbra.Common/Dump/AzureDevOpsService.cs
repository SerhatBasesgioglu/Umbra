using Umbra.Common.Dto;

namespace Umbra.Common.Dump;

public class AzureDevOpsService(AzureDevOpsHttpClient client)
{
    private readonly AzureDevOpsHttpClient _client = client;

    public async Task<List<ProjectDto>> GetProjectsAsync()
    {
        var response = await _client.GetAsync<AdoList<ProjectDto>>("_apis/projects");
        return response.Value;
    }

    public async Task<List<WorkItemDto>> QueryWorkItemsAsync(string query)
    {
        var uri = "Sandbox/_apis/wit/wiql";
        var body = new { query };
        var response = await _client.PostAsync<WiqlDto>(uri, body);
        var workItems = response.WorkItems.Select(x => x.Id).ToList();

        var result = GetWorkItemDetailsBatchAsync(workItems);
        return await result;
    }

    public string UpdateWorkItemsAsync(int id, List<PatchDto> patches, string? project = null)
    {
        return "yo";
    }

    private async Task<List<WorkItemDto>> GetWorkItemDetailsBatchAsync(List<int> pbis)
    {
        var allWorkItems = new List<WorkItemDto>();
        var uri = "Sandbox/_apis/wit/workitemsbatch";
        foreach (var batch in pbis.Chunk(200))
        {
            var body = new { ids = batch, fields = new[] { "System.Id", "System.State" } };
            var result = await _client.PostAsync<AdoList<WorkItemDto>>(uri, body);
            allWorkItems.AddRange(result.Value);
        }
        return allWorkItems;
    }
}
