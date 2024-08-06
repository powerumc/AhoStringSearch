using Xunit.Abstractions;

namespace AhoTextSearch.Tests;

public class AhoStringSearchTests(ITestOutputHelper output)
{
    private const string Input = "He gave her a cookie, but his dog ate it before she could say thanks.";

    [Fact]
    public void SearchTest()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("him");
        trie.AddString("it");
        trie.AddString("his");
        trie.Build();

        var actual = search.Search(Input);
        Assert.Equal("his", actual);
    }

    [Fact]
    public void SearchRangeTest()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("him");
        trie.AddString("it");
        trie.AddString("his");
        trie.Build();

        var actual = search.SearchRange(Input);
        output.WriteLine("Start: {0}, End: {1}", actual.Start, actual.End);
        output.WriteLine(Input[actual]);
        Assert.Equal("his", Input[actual]);
    }

    [Fact]
    public void SearchAllTest()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("her");
        trie.AddString("dog");
        trie.AddString("his");
        trie.Build();

        var results = search.SearchAll(Input).ToArray();
        foreach (var str in results)
        {
            output.WriteLine(str);
        }

        Assert.Equal(results, ["her", "his", "dog"]);
    }

    [Fact]
    public void SearchAllRangeTest()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("her");
        trie.AddString("dog");
        trie.AddString("his");
        trie.Build();

        var results = search.SearchAllRanges(Input).ToArray();
        foreach (var range in results)
        {
            output.WriteLine(Input[range]);
        }

        var actuals = results.Select(r => Input[r]);
        Assert.Equal(actuals, ["her", "his", "dog"]);
    }
}