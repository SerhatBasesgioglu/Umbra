using System.ComponentModel;
using ModelContextProtocol.Server;
using Umbra.Common.Dto;
using Umbra.Common.Dump;


[McpServerToolType]
public class AzureDevOpsTool(AzureDevOpsService service)
{
  private readonly AzureDevOpsService _service = service;

    [McpServerTool, Description("Echoes the message back to the client")]
    public static string Echo(string message) => $"Hello, this was your message: {message}";

    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
    [McpServerTool, Description("Returns all azure devops projects")]
    public async Task<List<ProjectDto>> GetProjects() => await _service.GetProjectsAsync();
}
