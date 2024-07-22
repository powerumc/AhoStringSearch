using BenchmarkDotNet.Attributes;

namespace AhoTextSearch.Benchmark;

[SimpleJob]
public class SearchBenchmark
{
    private AhoStringSearch _search;
    private string[] _strings;

    [GlobalSetup]
    public void Setup()
    {
        var search = new AhoStringSearch();
        var trie = search.CreateTrie();
        var strings = File.ReadAllLines("negative-words.txt");

        foreach (var str in strings)
        {
            trie.AddString(str);
        }

        trie.Build();
        _search = search;


        _strings = strings;
    }

    [Benchmark]
    public void AhoTextSearchSearch()
    {
        _ = _search.SearchAll("You are zombie").ToArray();
    }

    [Benchmark]
    public void StringContains()
    {
        foreach (var str in _strings)
        {
            if (str.Contains("You are zombie"))
            {
                _ = str;
            }
        }
    }
}