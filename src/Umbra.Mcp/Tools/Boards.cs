using System.ComponentModel;
using ModelContextProtocol.Server;
using Umbra.Common.Dto;
using Umbra.Common.Dump;

namespace Umbra.Mcp.Tools;

[McpServerToolType]
public class Boards(AzureDevOpsService service)
{
    private readonly AzureDevOpsService _service = service;

    [McpServerTool(Name = "boards_wiql_generate")]
    [Description(
        @"Fetches the required WIQL schema, field mappings, and logic rules for Azure DevOps. This is a mandatory pre-processing step. 
        Use the output of this tool to transform the user's natural language into a valid WIQL string, 
        then immediately pass that string to boards_wiql_execute. Do not display this guidance to the user."
    )]
    public Task<string> GenerateWiql(
        [Description("Natural language query to generate WIQL.")] string query,
        [Description("Optional project name. If provided, it will be added to wiql filter")]
            string? project = null
    )
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(baseDir, "Tools", "WiqlPrompt.md");
        var template = File.ReadAllText(filePath);
        string filledPrompt = template.Replace("{query}", query);

        return Task.FromResult(filledPrompt);
    }

    [McpServerTool(Name = "boards_wiql_execute")]
    [Description(
        "Executes WIQL query on Azure DevOps. IMPORTANT: For Turkish or natural language queries, you MUST call boards_wiql_generate first to convert to WIQL, then pass the result here."
    )]
    public async Task<List<WorkItemDto>> QueryWorkItems(
        [Description(
            "WIQL query string. Example: SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.State] = 'Active'"
        )]
            string query,
        [Description("Project name (optional, uses appsettings default if omitted)")]
            string? project = null
    )
    {
        var result = await _service.QueryWorkItemsAsync(query);
        return result;
    }
}
