using System.Text.Json.Serialization;

namespace Umbra.Common.Dto;

public class WiqlDto
{
    public List<WorkItemDto> WorkItems { get; set; }
}

public class WorkItemDto
{
    public int Id { get; set; }
    public string Url { get; set; }
    public FieldDto Fields { get; set; }
}

public class FieldDto
{
    [JsonPropertyName("System.State")]
    public string State { get; set; }
    [JsonPropertyName("System.Title")]
    public string Title { get; set; }
}

public class PatchDto
{
    public string Op { get; set; }
    public string Path { get; set; }
    public string Value { get; set; }
}
