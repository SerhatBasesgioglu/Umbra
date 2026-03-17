public class AdoList<T> : IEnumerable<T>
{
    public int Count { get; set; }
    public List<T> Value { get; set; } = new();

    public IEnumerator<T> GetEnumerator() => Value.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
