using System.ComponentModel;
using ModelContextProtocol.Server;
using Umbra.Common.Dto;
using Umbra.Common.Dump;

namespace Umbra.Mcp.Tools;

[McpServerToolType]
public class Core(AzureDevOpsService service)
{
    private readonly AzureDevOpsService _service = service;

    [McpServerTool(Name = "core_projects_get")]
    [Description(
        @"Fetches the official list of Azure DevOps projects. 
MANDATORY: Call this first if the user provides a nickname or abbreviation (e.g., 'devops', 'test'). 
INSTRUCTION: If a user says 'sndbox', search the results for a project containing that word (like 'Sandbox'). 
You must use the EXACT name from the list for all subsequent tool calls. 
Do not ask the user for confirmation if a clear best match exists in the list."
    )]
    public async Task<List<ProjectDto>> GetProjects() => await _service.GetProjectsAsync();
}
