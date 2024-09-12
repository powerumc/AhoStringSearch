using System.Runtime.CompilerServices;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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