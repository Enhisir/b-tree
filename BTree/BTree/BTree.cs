namespace BTree;

public class BTree<T> : IBTree<T> where T : IComparable<T>
{
    private readonly int _amount;
    private Node<T>? _root;
    
    public BTree(int amount)
    {
        _amount = amount;
    }

    private Node<T>? Find(T value)
    {
        return _root?.Find(value);
    }

    public bool Contains(T value)
    {
        var node =  Find(value);
        
        return node is not null && node.Keys.Contains(value);
    }

    public void Insert(T value)
    {
        var node = Find(value);

        if (node is null)
        {
            _root = new Node<T>(value, _amount, true);
            return;
        }
        
        if (node.Keys.Contains(value))
            return;

        if (node.Keys.Count < 2 * _amount - 1)
        {
            node.AddKey(value);
            return;
        }
        
        while (node.Keys.Count == 2 * _amount - 1)
        {
            if (node.Parent == null)
            {
                RebuildRoot(value);
                return;
            }
            
            var data = node.Split();
            
            var i = 0;
            while (node != node.Parent!.Children![i])
                i++;
            
            node.Parent.Children[i] = data.Left;
            node.Parent.Children.Insert(i + 1, data.Right);

            if (value.CompareTo(data.Middle) < 0)
                data.Left.AddKey(value);
            else
                data.Right.AddKey(value);

            node = node.Parent;
            value = data.Middle;
        }
        
        node.AddKey(value);
    }

    private void RebuildRoot(T newRootValue)
    {
        if (_root is null)
            throw new Exception();
        
        var newRoot = new Node<T>(newRootValue, _amount, false);
        _root.Parent = newRoot;
        
        var data = _root.Split();
        newRoot.Children!.Add(data.Left);
        newRoot.Children!.Add(data.Right);
        newRoot.Keys[0] = data.Middle;
        
        if (newRootValue.CompareTo(data.Middle) < 0)
            data.Left.AddKey(newRootValue);
        else
            data.Right.AddKey(newRootValue);
        
        _root = newRoot;
    }

    public void Remove(T value)
    {
        var node = Find(value);
        
        if (node is null || !node.Keys.Contains(value))
            throw new Exception();

        if (node.IsLeaf)
        {
            if (node == _root || node.Keys.Count > _amount - 1)
            {
                node.Keys.Remove(value);
                return;
            }

            var i = 0;
            while (node.Parent!.Children![i] != node)
                i++;
            if (i > 0 && node.Parent!.Children![i - 1]!.Keys.Count > _amount - 1)
            {
                var neighbour = node.Parent.Children[i - 1]!;
                
                var keyNeighbour = neighbour.Keys[^1];
                var keyParent = node.Parent.Keys[i - 1];
                
                node.Keys.Remove(value);
                node.AddKey(keyParent);

                node.Parent.Keys[i - 1] = keyNeighbour;
                neighbour.Keys.Remove(keyNeighbour);    
            }
            else if (i < node.Parent!.Children!.Count - 1 && 
                     node.Parent.Children[i + 1]!.Keys.Count > _amount - 1)
            {
                var neighbour = node.Parent.Children[i + 1]!;
                
                var keyNeighbour = neighbour.Keys[0];
                var keyParent = node.Parent.Keys[i];
                
                node.Keys.Remove(value);
                node.AddKey(keyParent);
                
                node.Parent.Keys[i] = keyNeighbour;
                neighbour.Keys.Remove(keyNeighbour);
            }
            else if (node.Parent == _root && _root.Children!.Count == 2)
            {
                node.Keys.Remove(value);
                _root = Node<T>.MergeWithRoot(_root);
            }
            else if (i > 0)
            {
                node.Keys.Remove(value);
                var neighbour = node.Parent.Children[i - 1]!;
                var newNode = Node<T>.Merge(neighbour, node);
                var keyParent = node.Parent.Keys[i - 1];
                newNode.AddKey(keyParent);
                node.Parent.Children[i] = newNode;
                
                Remove(keyParent);
                node.Parent.Children.Remove(node);
               
            }
            else
            {
                node.Keys.Remove(value);
                var neighbour = node.Parent.Children[i + 1]!;
                var newNode = Node<T>.Merge(node, neighbour);
                var keyParent = node.Parent.Keys[i];
                
                
                node.Parent.Children[i + 1] = newNode;
                Remove(keyParent);
                newNode.AddKey(keyParent);
                node.Parent.Children.Remove(neighbour);
            }
        }
        else
        {
            var i = 0;
            while (!node.Keys[i].Equals(value))
                i++;
            
            if (node.Children![i]!.Keys.Count > _amount - 1)
            {
                var childKey = node.Children[i]!.Keys[^1];
                Remove(childKey);
                node.Keys[i] = childKey;
            }
            else if (i < node.Children!.Count - 1
                     && node.Children[i + 1]!.Keys.Count > _amount - 1)
            {
                var childKey = node.Children[i + 1]!.Keys[0];
                Remove(childKey);
                node.Keys[i] = childKey;
            }
            else
            {
                var newNode = Node<T>.Merge(node.Children[i]!, node.Children[i + 1]!);

                if (node == _root && node.Children.Count == 2)
                {
                    newNode.Parent = null;
                    _root = newNode;
                    return;
                }

                newNode.AddKey(value);
                
                node.Keys.Remove(value);
                node.Children[i] = newNode;
                node.Children.Remove(node.Children[i + 1]);
                
                Remove(value);
            }
        }
    }

    public void PrintTree()
    {
        if (_root is null)
            throw new Exception();
        Stack<(int level, Node<T>)> stackTrace = new();
        stackTrace.Push((0, _root));
        while (stackTrace.TryPop(out (int level, Node<T> Node) current))
        {
            Console.Write(new string(' ', current.level * 4));
            Console.WriteLine(current.Node);
            if (current.Node.Children is null)
                continue;
            
            var elements = current.Node.Children.ToArray();
            foreach (var el in elements.Reverse())
                stackTrace.Push((current.level + 1, el)!);
        }
    }
}