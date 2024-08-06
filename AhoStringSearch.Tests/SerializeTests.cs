using System.Diagnostics;
using AhoTextSearch.Serialization;
using Xunit.Abstractions;

namespace AhoTextSearch.Tests;

public class SerializeTests(ITestOutputHelper output)
{
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
            output.WriteLine(str);
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
        var context = new TrieSerializationContext();
        context.Write(trie, bw);
    }

    [Fact]
    public void ContextReadTest()
    {
        ContextWriteTest();
        using var fs = new FileStream("test.trie", FileMode.Open);
        using var br = new BinaryReader(fs);
        var context = new TrieSerializationContext();
        var root = context.Read(br);

        var search = AhoStringSearch.CreateFrom(root);
        var results = search.SearchAll("my his he is good").ToArray();
        foreach (var str in results)
        {
            output.WriteLine(str);
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

    [Fact]
    public void BuildIteration100()
    {
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
            var search = new AhoStringSearch();
            var trie = search.CreateTrie();
            var words = File.ReadAllLines("../../../../AhoStringSearch.Benchmark/negative-words.txt");

            foreach (var word in words)
            {
                trie.AddString(word);
            }

            trie.Build();
        }

        stopwatch.Stop();

        output.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
    }

    [Fact]
    public void LoadTrieIteration100()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        var words = File.ReadAllLines("../../../../AhoStringSearch.Benchmark/negative-words.txt");

        foreach (var word in words)
        {
            trie.AddString(word);
        }

        trie.Build();
        trie.SaveTrie("./words.trie");

        var stopwatch = Stopwatch.StartNew();
        search = new AhoStringSearch();
        trie = search.CreateTrie();
        for (var i = 0; i < 1000; i++)
        {
            trie.LoadFrom("./words.trie");
        }

        stopwatch.Stop();
        output.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
    }
}