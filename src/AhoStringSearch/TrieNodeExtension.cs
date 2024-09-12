using AhoTextSearch.Serialization;

namespace AhoTextSearch;

public static class TrieNodeExtension
{
    /// <summary>
    ///     Save trie to file.
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="path">File path</param>
    public static void SaveTrie(this TrieNode root, string path)
    {
        using var fs = new FileStream(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        var context = new TrieSerializationContext();
        context.Write(root, bw);
    }

    /// <summary>
    ///     Load trie from file.
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="path">File path</param>
    public static void LoadFrom(this TrieNode root, string path)
    {
        using var fs = new FileStream(path, FileMode.Open);
        using var br = new BinaryReader(fs);
        var context = new TrieSerializationContext();
        context.Read(br, ref root);
    }
}