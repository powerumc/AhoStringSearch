using Xunit.Abstractions;

namespace AhoTextSearch.Tests;

public class AhoStringSearchTests
{
    private readonly ITestOutputHelper _output;

    public AhoStringSearchTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SearchTest()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("her");
        trie.AddString("he");
        trie.AddString("his");
        trie.Build();

        var actual = search.Search("my his he is good");
        Assert.Equal("his", actual);
    }

    [Fact]
    public void SearchAllTest()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        trie.AddString("her");
        trie.AddString("he");
        trie.AddString("his");
        trie.Build();

        var results = search.SearchAll("mu his he is good").ToArray();
        foreach (var str in results)
        {
            _output.WriteLine(str);
        }
        
        Assert.Equal(results, ["his", "he"]);
    }
}