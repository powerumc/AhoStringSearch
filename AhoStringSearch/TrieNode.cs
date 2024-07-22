using AhoTextSearch.Serialization;

namespace AhoTextSearch;

public class TrieNode
{
    public TrieNode()
    {
    }

    internal TrieNode(int id)
    {
        Id = id;
    }

    internal TrieNode(TrieNodeInternal node)
    {
        Id = node.Id;
        Children = node.Children.ToDictionary(o => o.Key, v => new TrieNode(v.Value));
        Outputs = node.Outputs.ToList();
    }
    
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

    public Dictionary<char, TrieNode> Children { get; private set; } = new();
    public TrieNode? Fail { get; set; }
    public List<string> Outputs { get; private set; } = [];
    public int Id { get; }

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
            foreach (var pair in node.Children)
            {
                var c = pair.Key;
                var child = pair.Value;
                queue.Enqueue(child);

                var fail = node.Fail;
                while (fail != null && !fail.Children.ContainsKey(c))
                {
                    fail = fail.Fail;
                }

                child.Fail = fail?.Children[c] ?? this;
                child.Outputs.AddRange(child.Fail.Outputs);
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

    private static void SerializeNode(this TrieNode root, BinaryWriter binaryWriter)
    {
        var queue = new Queue<TrieNode>();
        var nodeToId = new Dictionary<TrieNode, int>();
        var nextId = 0;

        queue.Enqueue(root);
        nodeToId[root] = nextId++;

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            // Serialize Fail pointer as an ID, or -1 if null
            binaryWriter.Write(node.Fail != null ? nodeToId[node.Fail] : -1);
            // Serialize Outputs
            binaryWriter.Write(node.Outputs.Count);
            foreach (var output in node.Outputs)
            {
                binaryWriter.Write(output);
            }

            // Serialize Children
            binaryWriter.Write(node.Children.Count);
            foreach (var kvp in node.Children)
            {
                var child = kvp.Value;
                if (!nodeToId.ContainsKey(child))
                {
                    nodeToId[child] = nextId++;
                    queue.Enqueue(child);
                }

                // Write character key and child node ID
                binaryWriter.Write(kvp.Key);
                binaryWriter.Write(nodeToId[child]);
            }
        }
    }

    private static void DeserializeNode(this TrieNode root, BinaryReader binaryReader)
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
            var failId = binaryReader.ReadInt32();
            if (failId != -1)
            {
                if (idToNode.ContainsKey(failId))
                {
                    node.Fail = idToNode[failId];
                }
                else
                {
                    failPointerUpdates[node] = failId;
                }
            }

            var outputsCount = binaryReader.ReadInt32();
            for (var i = 0; i < outputsCount; i++)
            {
                node.Outputs.Add(binaryReader.ReadString());
            }

            var childrenCount = binaryReader.ReadInt32();
            for (var i = 0; i < childrenCount; i++)
            {
                var key = binaryReader.ReadChar();
                var childId = binaryReader.ReadInt32();

                var childNode = new TrieNode();
                if (!idToNode.ContainsKey(childId))
                {
                    idToNode[childId] = childNode;
                    queue.Enqueue(childNode);
                }
                else
                {
                    childNode = idToNode[childId];
                }

                node.Children[key] = childNode;
            }
        }

        // Resolve Fail pointers that couldn't be set during the initial pass
        foreach (var item in failPointerUpdates)
        {
            if (idToNode.TryGetValue(item.Value, out var failNode))
            {
                item.Key.Fail = failNode;
            }
        }
    }
}