using System.Collections.Concurrent;

namespace AhoTextSearch.Serialization;

public class TrieNodeCache<TKey, TValue>
    where TKey : notnull
    where TValue : notnull, TrieNode
{
    public ConcurrentDictionary<int, TValue> IdToNode = new();
    public ConcurrentDictionary<TValue, int> NodeToId = new();

    public void Add(TValue item)
    {
        IdToNode[item.Id] = item;
        NodeToId[item] = item.Id;
    }
    
    public bool TryAdd(TValue item)
    {
        var added = true;
        added &= IdToNode.TryAdd(item.Id, item);
        added &= NodeToId.TryAdd(item, item.Id);

        return added;
    }

    public bool TryGetValue(int id, out TValue? value)
    {
        return IdToNode.TryGetValue(id, out value);
    }

    public bool TryGetValue(TValue node, out int id)
    {
        return NodeToId.TryGetValue(node, out id);
    }
}

public struct TrieNodeContext
{
    public ConcurrentDictionary<TrieNode, int> NodeToId = new();
    public ConcurrentDictionary<int, TrieNode> IdToNode = new();
    private TrieNodeCache<int, TrieNode> Cache = new();
    private Dictionary<TrieNode, int> failureNodeUpdates = new();
    
    private int _nextId = 0;

    public TrieNodeContext()
    {
    }

    public int NextId()
    {
        return Interlocked.Increment(ref _nextId);
    }

    // public void Write(TrieNode root, BinaryWriter writer)
    // {
    //     var queue = new Queue<TrieNode>();
    //     NodeToId[root] = root.Id;
    //     queue.Enqueue(root);
    //
    //     while (queue.Count > 0)
    //     {
    //         var node = queue.Dequeue();
    //         new TrieNodeInfo(this).Write(node, writer);
    //
    //         foreach (var (_, childNode) in node.Children)
    //         {
    //             if (!NodeToId.ContainsKey(childNode))
    //             {
    //                 NodeToId[childNode] = childNode.Id;
    //             }
    //
    //             queue.Enqueue(childNode);
    //         }
    //     }
    // }

    public void Write(TrieNode node, BinaryWriter writer)
    {
        var info = new TrieNodeInfo(this);
        info.Write(node, writer);

        foreach (var (key, child) in node.Children)
        {
            Write(child, writer);
        }
    }

    // public TrieNode Read(BinaryReader reader)
    // {
    //     var queue = new Queue<TrieNode>();
    //     var root = new TrieNode();
    //     queue.Enqueue(root);
    //
    //     while (queue.Count > 0)
    //     {
    //         var parent = queue.Dequeue();
    //         var childNode = new TrieNodeInfo(this).Read(reader);
    //
    //         foreach (var (key, child) in childNode.Children)
    //         {
    //             parent.Children.Add(key, child);
    //             // TODO: Child 가 있어야 queue 에 추가
    //             queue.Enqueue(child);
    //         }
    //     }
    //
    //     return root;
    // }

    public TrieNode Read(BinaryReader reader)
    {
        var root = new TrieNode();
        ReadInternal(root, reader);

        foreach (var failNode in failureNodeUpdates)
        {
            if (Cache.TryGetValue(failNode.Value, out var node))
            {
                failNode.Key.Fail = node;
            }
            else
            {
                throw new Exception("Failure node not found");
            }
        }

        return root;
    }

    private TrieNode ReadInternal(TrieNode parent, BinaryReader reader)
    {
        var info = new TrieNodeInfo(this);
        var nodeInfo = info.Read(reader);
        var node = new TrieNode(nodeInfo);
        Cache.TryAdd(node);
        
        if (nodeInfo.FailId != -1)
        {
            if (Cache.TryGetValue(nodeInfo.FailId, out var failNode))
            {
                node.Fail = failNode;
            }
            else
            {
                //failureNodeUpdates[childNodeItem] = childNode.Fail?.Id ?? -1;
                failureNodeUpdates[node] = nodeInfo.FailId;
            }
        }
        
        foreach (var (key, childNodeInfo) in nodeInfo.Children)
        {
            var childNode = new TrieNode(childNodeInfo);
            parent.Children[key] = childNode;
            parent.Outputs.AddRange(childNode.Outputs);

            // if (childNode.FailId != -1 &&
            //     Cache.TryGetValue(childNode.FailId, out var failNode))
            // {
            //     childNodeItem.Fail = failNode;
            // }
            // else
            // {
            //     failureNodeUpdates[childNodeItem] = childNode.Fail?.Id ?? -1;
            // }

            ReadInternal(childNode, reader);
        }

        return parent;
    }
}

public struct TrieNodeInternal
{
    public int Id;
    public int FailId;
    public char Char;
    public Dictionary<char, TrieNodeInternal> Children;
    public List<string> Outputs;
    public bool HasChildren;

    public TrieNodeInternal()
    {
        Id = -1;
        FailId = -1;
        Char = '\0';
        Children = new Dictionary<char, TrieNodeInternal>();
        Outputs = new List<string>();
        HasChildren = false;
    }
}

public struct TrieNodeInfo
{
    private readonly TrieNodeContext _context;

    public TrieNodeInfo(TrieNodeContext context)
    {
        _context = context;
    }
    // public void Write(TrieNode node, BinaryWriter writer)
    // {
    //     writer.Write(node.Id);
    //
    //     var failId = node.Fail != null ? context.NodeToId[node.Fail] : -1;
    //     writer.Write(failId);
    //
    //     writer.Write(node.Children.Count);
    //     foreach (var (@char, childNode) in node.Children)
    //     {
    //         if (!context.NodeToId.ContainsKey(childNode))
    //         {
    //             context.NodeToId[childNode] = childNode.Id;
    //         }
    //
    //         writer.Write(context.NodeToId[childNode]);
    //         writer.Write(@char);
    //     }
    //
    //     writer.Write(node.Outputs.Count);
    //     foreach (var output in node.Outputs)
    //     {
    //         writer.Write(output);
    //     }
    // }

    public void Write(TrieNode node, BinaryWriter writer)
    {
        writer.Write(node.Id);

        var failId = node.Fail?.Id ?? -1;
        writer.Write(failId);

        writer.Write(node.Children.Count);
        foreach (var (@char, childNode) in node.Children)
        {
            writer.Write(childNode.Id);
            writer.Write(@char);
            writer.Write(childNode.Children.Any());
        }

        writer.Write(node.Outputs.Count);
        foreach (var output in node.Outputs)
        {
            writer.Write(output);
        }
    }

    // public TrieNode Read(BinaryReader reader)
    // {
    //     var failPointerUpdates = new Dictionary<TrieNode, int>();
    //
    //     var id = reader.ReadInt32();
    //     var node = new TrieNode(id);
    //
    //     var failId = reader.ReadInt32();
    //     if (failId != -1)
    //     {
    //         if (context.IdToNode.TryGetValue(failId, out var failNode))
    //         {
    //             node.Fail = failNode;
    //         }
    //         else
    //         {
    //             failPointerUpdates[node] = failId;
    //         }
    //     }
    //
    //     var childrenCount = reader.ReadInt32();
    //     for (var i = 0; i < childrenCount; i++)
    //     {
    //         var childId = reader.ReadInt32();
    //         var @char = reader.ReadChar();
    //
    //         if (!context.IdToNode.TryGetValue(childId, out var childNode))
    //         {
    //             childNode = new TrieNode();
    //             context.IdToNode[childId] = childNode;
    //         }
    //
    //         node.Children[@char] = childNode;
    //     }
    //
    //     var outputsCount = reader.ReadInt32();
    //     for (var i = 0; i < outputsCount; i++)
    //     {
    //         var output = reader.ReadString();
    //         node.Outputs.Add(output);
    //     }
    //
    //
    //     foreach (var item in failPointerUpdates)
    //     {
    //         if (context.IdToNode.TryGetValue(item.Value, out var failNode))
    //         {
    //             item.Key.Fail = failNode;
    //         }
    //     }
    //
    //     return node;
    // }

    public TrieNodeInternal Read(BinaryReader reader)
    {
        var failPointerUpdates = new Dictionary<TrieNode, int>();

        var id = reader.ReadInt32();
        //var node = new TrieNode(id);
        var node = new TrieNodeInternal();
        node.Id = id;
        
        var failId = reader.ReadInt32();
        node.FailId = failId;

        var childrenCount = reader.ReadInt32();

        for (var i = 0; i < childrenCount; i++)
        {
            var childId = reader.ReadInt32();
            var @char = reader.ReadChar();
            var hasChildren = reader.ReadBoolean();

            // if (!_context.IdToNode.TryGetValue(childId, out var childNode))
            // {
            //     childNode = new TrieNode();
            //     _context.IdToNode[childId] = childNode;
            // }

            // var childNode = new TrieNodeInternal();
            // childNode.Id = childId;
            // childNode.Char = @char;
            // childNode.HasChildren = hasChildren;
            // node.Children[@char] = childNode;

            node.Children.Add(@char, new TrieNodeInternal { Id = childId });
        }

        var outputsCount = reader.ReadInt32();
        for (var i = 0; i < outputsCount; i++)
        {
            var output = reader.ReadString();
            node.Outputs.Add(output);
        }

        return node;
    }
}