using AhoTextSearch.Serialization;
using Xunit.Abstractions;

namespace AhoTextSearch.Tests;

public class SerializeTests
{
    private readonly ITestOutputHelper _output;

    public SerializeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SerializeTest()
    {
        SaveTrie();

        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.LoadFrom("dic.trie");

        var results = search.SearchAll("he").ToArray();

        foreach (var str in results)
        {
            _output.WriteLine(str);
        }
    }

    [Fact]
    public void ContextWriteTest()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("her");
        trie.AddString("he");
        trie.AddString("his");
        trie.Build();

        using var fs = new FileStream("test.trie", FileMode.Create);
        using var bw = new BinaryWriter(fs);
        var context = new TrieNodeContext();
        context.Write(trie, bw);
    }

    [Fact]
    public void ContextReadTest()
    {
        ContextWriteTest();
        using var fs = new FileStream("test.trie", FileMode.Open);
        using var br = new BinaryReader(fs);
        var context = new TrieNodeContext();
        var root = context.Read(br);

        var search = AhoStringSearch.CreateFrom(root);
        var results = search.SearchAll("my his he is good").ToArray();
        foreach (var str in results)
        {
            _output.WriteLine(str);
        }
        
        Assert.Equal(results, ["his", "he"]);
    }

    private static void SaveTrie()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("her");
        trie.AddString("he");
        trie.AddString("his");
        trie.Build();

        trie.SaveTrie("dic.trie");
    }

    private static TrieNode LoadTrie()
    {
        var trie = new TrieNode();
        trie.LoadFrom("dic.trie");

        return trie;
    }
}