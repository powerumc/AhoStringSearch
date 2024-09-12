namespace AhoTextSearch;

/// <summary>
///     Aho-Corasick string search algorithm.
/// </summary>
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

    /// <summary>
    ///     Returns one matching string.
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>
    ///     Returns the first matching string.
    /// </returns>
    public string Search(string input)
    {
        var node = _root;
        foreach (var c in input)
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

    /// <summary>
    ///     Returns all matching strings.
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>
    ///     Returns all matching strings.
    /// </returns>
    public IEnumerable<string> SearchAll(string input)
    {
        var node = _root;
        foreach (var c in input)
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
#if NETSTANDARD2_1_OR_GREATER
    /// <summary>
    ///     Returns the range of the first matching string.
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>
    ///     Returns the range of the first matching string.
    /// </returns>
    public Range SearchRange(string input)
    {
        var node = _root;
        var start = 0;
        var end = 0;

        foreach (var c in input)
        {
            while (node != null && !node.Children.ContainsKey(c))
            {
                node = node.Fail;
                start = end;
            }

            node = node?.Children[c] ?? _root;
            foreach (var output in node.Outputs)
            {
                return new Range(start + 1, end + 1);
            }

            end++;
        }

        return new Range(0, 0);
    }

    /// <summary>
    ///     Returns all ranges of matching strings.
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>
    ///     Returns all ranges of matching strings.
    /// </returns>
    public IEnumerable<Range> SearchAllRanges(string input)
    {
        var node = _root;
        var start = 0;
        var end = 0;

        foreach (var c in input)
        {
            while (node != null && !node.Children.ContainsKey(c))
            {
                node = node.Fail;
                start = end;
            }

            node = node?.Children[c] ?? _root;
            foreach (var output in node.Outputs)
            {
                yield return new Range(start + 1, end + 1);
            }

            end++;
        }
    }
#endif
}