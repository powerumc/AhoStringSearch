namespace AhoTextSearch.Serialization;

/// <summary>
///     Trie serialization context.
/// </summary>
public class TrieSerializationContext
{
    private readonly TrieNodeCache<int, TrieNode> _cache = new();
    private readonly Dictionary<TrieNode, int> _failureNodeUpdates = new();

    /// <summary>
    ///     Write trie to binary writer.
    /// </summary>
    /// <param name="node">Root node</param>
    /// <param name="writer">Writer</param>
    public void Write(TrieNode node, BinaryWriter writer)
    {
        var item = new TrieNodeItem(node);
        item.Write(writer);

        foreach (var children in node.Children)
        {
            var child = children.Value;
            Write(child, writer);
        }
    }

    /// <summary>
    ///     Read trie from binary reader.
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="root">Root node</param>
    /// <exception cref="AhoSearchStringException">Failure node not found.</exception>
    public void Read(BinaryReader reader, ref TrieNode root)
    {
        ReadInternal(root, reader);

        foreach (var failNode in _failureNodeUpdates)
        {
            if (_cache.TryGetValue(failNode.Value, out var node) &&
                node != null)
            {
                failNode.Key.UpdateFailurePointer(node);
            }
            else
            {
                throw new AhoSearchStringException("Failure node not found");
            }
        }
    }

    /// <summary>
    ///     Load trie from binary reader and return root node.
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <returns>
    ///     Returns root node.
    /// </returns>
    public TrieNode Load(BinaryReader reader)
    {
        var root = new TrieNode();
        Read(reader, ref root);

        return root;
    }

    /// <summary>
    ///     Binary reader to trie node and build failure pointers.
    /// </summary>
    /// <param name="parent">Parent trie node</param>
    /// <param name="reader">Reader</param>
    /// <returns>
    ///     Returns trie node.
    /// </returns>
    private TrieNode ReadInternal(TrieNode parent, BinaryReader reader)
    {
        var nodeInfo = new TrieNodeItem();
        nodeInfo.Read(reader);

        var node = new TrieNode(nodeInfo);
        _cache.TryAdd(node.Id, node);

        if (nodeInfo.FailId != -1)
        {
            if (_cache.TryGetValue(nodeInfo.FailId, out var failNode) &&
                failNode != null)
            {
                parent.UpdateFailurePointer(failNode);
            }
            else
            {
                _failureNodeUpdates[node] = nodeInfo.FailId;
            }
        }

        parent.UpdateOutputs(nodeInfo.Outputs);

        foreach (var children in nodeInfo.Children)
        {
            var key = children.Key;
            var childNodeInfo = children.Value;
            var childNode = new TrieNode(childNodeInfo);
            parent.Children[key] = childNode;

            ReadInternal(childNode, reader);
        }

        return parent;
    }
}