namespace BTree;

public class Node<T> where T : IComparable<T>
{
    private readonly int _amount;
    public bool IsLeaf { get; }

    public Node<T>? Parent { get; set; }
    public readonly List<T> Keys;
    public readonly List<Node<T>?>? Children;


    public Node(T value, int amount, bool isLeaf)
    {
        _amount = amount;
        IsLeaf = isLeaf;
        
        Keys = new List<T>(2 * _amount) { value };
        if (!IsLeaf)
            Children = new List<Node<T>?>(2 * _amount + 1);
    }
    
    public Node(IEnumerable<T> keys, int amount)
    {
        _amount = amount;
        IsLeaf = true;
        
        Keys = new List<T>(2 * _amount);

        Keys.AddRange(keys);

        if (Keys.Count >= 2 * _amount)
            throw new Exception();
    }
    
    public Node(IEnumerable<T> keys, IEnumerable<Node<T>?> children, int amount)
    {
        _amount = amount;
        IsLeaf = false;
        
        Keys = new List<T>(2 * _amount);
        Children = new List<Node<T>?>(2 * _amount + 1);
        
        Keys.AddRange(keys);
        Children.AddRange(children);
        
        foreach (var child in Children.Where(child => child is not null))
            child!.Parent = this;
        
        if (Keys.Count >= 2 * _amount || Children.Count >= 2 * _amount + 1)
            throw new Exception();
    }

    public Node<T>? Find(T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        for (var i = 0; i < Keys.Count; i++)
        {
            var comparison = value.CompareTo(Keys[i]);
                
            if (comparison == 0)
                return this;

            if (comparison < 0)
            {
                return IsLeaf ? this : Children![i]!.Find(value);
            }
            
            if (i + 1 == Keys.Count)
                return IsLeaf ? this : Children![i + 1]!.Find(value);
        }
        
        return this;
    }

    public (Node<T> Left, T Middle, Node<T> Right) Split()
    {
        Node<T> left;
        Node<T> right;
        if (IsLeaf)
        {
            left = new Node<T>(
                Keys.GetRange(0, _amount - 1),
                _amount
            ) { Parent = Parent };
            right = new Node<T>(
                Keys.GetRange(_amount, _amount - 1),
                _amount
            ) { Parent = Parent };
        }
        else
        {
            left = new Node<T>(
                Keys.GetRange(0, _amount - 1),
                Children!.GetRange(0, _amount),
                _amount
            ) { Parent = Parent };
            right = new Node<T>(
                Keys.GetRange(_amount, _amount - 1),
                Children!.GetRange(_amount, Children.Count - _amount),
                _amount
            ) { Parent = Parent };
        }
        return (left, Keys[_amount - 1], right);
    }

    public static Node<T> Merge(Node<T> left, Node<T> right)
    {
        if (left.Parent is null
            || right.Parent is null
            || left.Parent != right.Parent
            || left.IsLeaf != right.IsLeaf)
            throw new Exception();

        var newKeys = new T[left.Keys.Count + right.Keys.Count];

        left.Keys.CopyTo(newKeys);
        right.Keys.CopyTo(newKeys, left.Keys.Count);

        if (left.IsLeaf)
            return new Node<T>(newKeys, left._amount) {Parent = left.Parent};
        
        var newChildren = new Node<T>[left.Children!.Count + right.Children!.Count];
        left.Children.CopyTo(newChildren);
        right.Children.CopyTo(newChildren, left.Children.Count);
        return new Node<T>(newKeys, newChildren, left._amount) {Parent = left.Parent};
    }
    
    public static Node<T> MergeWithRoot(Node<T> root)
    {
        if (root.Parent is not null
            || root.Children!.Count != 2)
            throw new Exception();

        var left = root.Children[0]!;
        var right = root.Children[1]!;

        if (!left.IsLeaf || !right.IsLeaf)
            throw new Exception();
        
        var newKeys = new T[left.Keys.Count + root.Keys.Count + right.Keys.Count];

        left.Keys.CopyTo(newKeys);
        root.Keys.CopyTo(newKeys, left.Keys.Count);
        right.Keys.CopyTo(newKeys, left.Keys.Count + root.Keys.Count);
        
        return new Node<T>(newKeys, left._amount) {Parent = null};
    }

    public void AddKey(T value)
    {
        var i = 0;
        while (i < Keys.Count && value.CompareTo(Keys[i]) > 0)
            i++;
        Keys.Insert(i, value);
    }

    public override string ToString()
    {
        return $"Node [{String.Join(';', Keys)}]";
    }
}