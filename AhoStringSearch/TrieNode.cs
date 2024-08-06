using AhoTextSearch.Serialization;

namespace AhoTextSearch;

/// <summary>
///     Aho-Corasick trie node.
/// </summary>
public class TrieNode
{
    public TrieNode()
    {
    }

    private TrieNode(int id)
    {
        Id = id;
    }

    internal TrieNode(TrieNodeItem node)
    {
        Id = node.Id;
        Children = node.Children.ToDictionary(o => o.Key, v => new TrieNode(v.Value));
        Outputs = node.Outputs.ToList();
    }

    public Dictionary<char, TrieNode> Children { get; private set; } = new();
    public TrieNode? Fail { get; private set; }
    public List<string> Outputs { get; private set; } = [];
    public int Id { get; }

    internal void UpdateFailurePointer(TrieNode node)
    {
        Fail = node;
    }

    internal void UpdateChildren(Dictionary<char, TrieNode> children)
    {
        Children = children;
    }

    internal void UpdateOutputs(List<string> outputs)
    {
        Outputs = outputs;
    }

    private int NextId()
    {
        return Id + 1;
    }

    public void AddString(string s)
    {
        var node = this;
        foreach (var c in s)
        {
            if (!node.Children.TryGetValue(c, out var next))
            {
                next = new TrieNode(node.NextId());
                node.Children.Add(c, next);
            }

            node = next;
        }

        node.Outputs.Add(s);
    }

    public void Build()
    {
        var queue = new Queue<TrieNode>();
        foreach (var child in Children.Values)
        {
            child.Fail = this;
            queue.Enqueue(child);
        }

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var children in node.Children)
            {
                var key = children.Key;
                var value = children.Value;
                
                queue.Enqueue(value);

                var fail = node.Fail;
                while (fail != null && !fail.Children.ContainsKey(key))
                {
                    fail = fail.Fail;
                }

                value.Fail = fail?.Children[key] ?? this;
                value.Outputs.AddRange(value.Fail.Outputs);
            }
        }
    }
}

public static class TrieNodeExtension
{
    public static void SaveTrie(this TrieNode root, string path)
    {
        using var fs = new FileStream(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);

        SerializeNode(root, bw);
    }

    public static void LoadFrom(this TrieNode root, string path)
    {
        using var fs = new FileStream(path, FileMode.Open);
        using var br = new BinaryReader(fs);

        DeserializeNode(root, br);
    }

    private static void SerializeNode(this TrieNode root, BinaryWriter bw)
    {
        var queue = new Queue<TrieNode>();
        var nodeToId = new Dictionary<TrieNode, int>();
        var nextId = 0;

        queue.Enqueue(root);
        nodeToId[root] = nextId++;

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            bw.Write(node.Fail != null ? nodeToId[node.Fail] : -1);
            bw.Write(node.Outputs.Count);
            foreach (var output in node.Outputs)
            {
                bw.Write(output);
            }

            bw.Write(node.Children.Count);
            foreach (var children in node.Children)
            {
                var key = children.Key;
                var child = children.Value;
                
                if (!nodeToId.ContainsKey(child))
                {
                    nodeToId[child] = nextId++;
                    queue.Enqueue(child);
                }

                bw.Write(key);
                bw.Write(nodeToId[child]);
            }
        }
    }

    private static void DeserializeNode(this TrieNode root, BinaryReader br)
    {
        var idToNode = new Dictionary<int, TrieNode>();
        var failPointerUpdates = new Dictionary<TrieNode, int>();
        var queue = new Queue<TrieNode>();
        var nextId = 0;

        idToNode[nextId] = root;
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var failId = br.ReadInt32();
            if (failId != -1)
            {
                if (idToNode.TryGetValue(failId, out var value))
                {
                    node.UpdateFailurePointer(value);
                }
                else
                {
                    failPointerUpdates[node] = failId;
                }
            }

            var outputsCount = br.ReadInt32();
            for (var i = 0; i < outputsCount; i++)
            {
                node.Outputs.Add(br.ReadString());
            }

            var childrenCount = br.ReadInt32();
            for (var i = 0; i < childrenCount; i++)
            {
                var key = br.ReadChar();
                var childId = br.ReadInt32();

                var childNode = new TrieNode();
                if (!idToNode.TryGetValue(childId, out var value))
                {
                    idToNode[childId] = childNode;
                    queue.Enqueue(childNode);
                }
                else
                {
                    childNode = value;
                }

                node.Children[key] = childNode;
            }
        }

        foreach (var item in failPointerUpdates)
        {
            if (idToNode.TryGetValue(item.Value, out var failNode))
            {
                item.Key.UpdateFailurePointer(failNode);
            }
        }
    }
}