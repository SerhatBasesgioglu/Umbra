public class PipelineRunDto
{
    public string State { get; set; }
    public string Result { get; set; }
    public string Name { get; set; }
    public int Id { get; set; }
    public PipelineDto Definition { get; set; }
}
