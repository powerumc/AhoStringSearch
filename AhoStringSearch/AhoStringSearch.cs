namespace AhoTextSearch;

public class AhoStringSearch
{
    private readonly TrieNode _root = new();

    public AhoStringSearch()
    {
    }

    private AhoStringSearch(TrieNode trie)
    {
        _root = trie;
    }

    public TrieNode CreateTrie()
    {
        return _root;
    }

    public static AhoStringSearch CreateFrom(TrieNode trie)
    {
        return new AhoStringSearch(trie);
    }

    // 일치하는 문자열 하나를 반환
    public string Search(string str)
    {
        var node = _root;
        foreach (var c in str)
        {
            while (node != null && !node.Children.ContainsKey(c))
            {
                node = node.Fail;
            }

            node = node?.Children[c] ?? _root;
            foreach (var output in node.Outputs)
            {
                return output;
            }
        }

        return string.Empty;
    }

    public IEnumerable<string> SearchAll(string text)
    {
        var node = _root;
        foreach (var c in text)
        {
            while (node != null && !node.Children.ContainsKey(c))
            {
                node = node.Fail;
            }

            node = node?.Children[c] ?? _root;
            foreach (var output in node.Outputs)
            {
                yield return output;
            }
        }
    }
}