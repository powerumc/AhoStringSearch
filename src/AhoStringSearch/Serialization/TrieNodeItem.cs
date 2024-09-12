using System.Collections.Concurrent;

namespace AhoTextSearch.Serialization;

public class TrieNodeCache<TKey, TValue>
    where TKey : notnull
    where TValue : TrieNode
{
    private readonly ConcurrentDictionary<TKey, TValue> _idToNode = new();
    private readonly ConcurrentDictionary<TValue, TKey> _nodeToId = new();

    public void Add(TKey key, TValue item)
    {
        _idToNode[key] = item;
        _nodeToId[item] = key;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        var added = true;
        added &= _idToNode.TryAdd(key, value);
        added &= _nodeToId.TryAdd(value, key);

        return added;
    }

    public bool TryGetValue(TKey id, out TValue? value)
    {
        return _idToNode.TryGetValue(id, out value);
    }

    public bool TryGetValue(TValue node, out TKey id)
    {
        return _nodeToId.TryGetValue(node, out id!);
    }
}

public struct TrieNodeItem
{
    public int Id;
    public int FailId;
    public readonly Dictionary<char, TrieNodeItem> Children;
    public readonly List<string> Outputs;

    public TrieNodeItem()
    {
        Id = -1;
        FailId = -1;
        Children = new Dictionary<char, TrieNodeItem>();
        Outputs = new List<string>();
    }

    public TrieNodeItem(TrieNode node)
    {
        Id = node.Id;
        FailId = node.Fail?.Id ?? -1;
        Children = node.Children.ToDictionary(x => x.Key, x => new TrieNodeItem(x.Value));
        Outputs = node.Outputs.ToList();
    }

    public bool HasChildren => Children.Count != 0;

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);

        writer.Write(FailId);

        writer.Write(Children.Count);
        foreach (var children in Children)
        {
            var @char = children.Key;
            var childNode = children.Value;
            
            writer.Write(childNode.Id);
            writer.Write(@char);
            writer.Write(childNode.HasChildren);
        }

        writer.Write(Outputs.Count);
        foreach (var output in Outputs)
        {
            writer.Write(output);
        }
    }

    public TrieNodeItem Read(BinaryReader reader)
    {
        var id = reader.ReadInt32();
        Id = id;

        var failId = reader.ReadInt32();
        FailId = failId;

        var childrenCount = reader.ReadInt32();

        for (var i = 0; i < childrenCount; i++)
        {
            var childId = reader.ReadInt32();
            var @char = reader.ReadChar();
            var hasChildren = reader.ReadBoolean();

            Children.Add(@char, new TrieNodeItem { Id = childId });
        }

        var outputsCount = reader.ReadInt32();
        for (var i = 0; i < outputsCount; i++)
        {
            var output = reader.ReadString();
            Outputs.Add(output);
        }

        return this;
    }
}