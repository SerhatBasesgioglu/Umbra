public class AdoList<T>
{
    public int Count { get; set; }
    public List<T> Value { get; set; } = new();
}
