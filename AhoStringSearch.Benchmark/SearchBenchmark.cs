using BenchmarkDotNet.Attributes;

namespace AhoTextSearch.Benchmark;

[SimpleJob]
public class SearchBenchmark
{
    private const string Input = "You are zombie";
    private AhoStringSearch _search = null!;
    private string[] _strings = null!;

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
    public void AhoTextSearchAll()
    {
        _ = _search.SearchAll(Input).ToArray();
    }

    [Benchmark]
    public void AhoTextSearch()
    {
        _ = _search.Search(Input);
    }

    [Benchmark]
    public void StringContains()
    {
        foreach (var str in _strings)
        {
            if (!str.Contains(Input))
            {
                continue;
            }

            _ = str;
            return;
        }
    }
}