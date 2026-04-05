using Umbra.Common.Dto;

namespace Umbra.Common.Dump;

public class AzureDevOpsService(AzureDevOpsHttpClient client)
{
    private readonly AzureDevOpsHttpClient _client = client;

    public async Task<List<ProjectDto>> GetProjectsAsync(){
      var response =  await _client.GetAsync<AdoList<ProjectDto>>("_apis/projects");
      return response.Value;
    }

    public string UpdateWorkItemsAsync(int id, List<PatchDto> patches, string? project = null)
    {
        return "yo";
    }
}
