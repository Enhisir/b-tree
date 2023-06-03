namespace BTree;

public interface IBTree<in T> where T : IComparable<T>
{
    public bool Contains(T value);
    public void Insert(T value);
    public void Remove(T value);
}