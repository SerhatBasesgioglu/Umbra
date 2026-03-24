public class PipelineRunDto
{
    public int Id { get; set; }
    public string Status { get; set; }
    public string Result { get; set; }
    public string Name { get; set; }
    public DateTime QueueTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime FinishTime { get; set; }
    public PipelineDto Definition { get; set; }
}
