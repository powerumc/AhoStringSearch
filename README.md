# Aho Search String [[한국어](./README.ko.md)]

## Description

This project is a C# implementation of the Aho-Corasick algorithm, a powerful and efficient string search algorithm used
to find all occurrences of a set of string patterns within a string. This algorithm is particularly useful in
applications that need to match multiple patterns simultaneously, such as spam filters and intrusion detection systems.

The Aho-Corasick algorithm was invented by Alfred V. Aho and Margaret J. Corasick in 1975. It allows efficient
multi-pattern matching within text by constructing a finite state machine similar to a dictionary.

## Installation

```bash
dotnet add package AhoStringSearch
```

## API

Checks if the `input` string matches any of the string patterns.

```csharp
var input = "He gave her a cookie, but his dog ate it before she could say thanks.";

var search = new AhoStringSearch();

// Build Trie
var trie = search.CreateTrie();
trie.AddString("him");
trie.AddString("it");
trie.AddString("his");
trie.Build();

var actual = search.Search(input);
Assert.Equal("his", actual);
```

## Performance

Searching for the string 4,783rd `zombie` from 4,783 string word
rules ([negative-words.txt](src/AhoStringSearch.Benchmark/negative-words.txt))

| Method           |        Mean |     Error |    StdDev |
|------------------|------------:|----------:|----------:|
| AhoTextSearchAll |    253.4 ns |   3.37 ns |   3.15 ns |
| AhoTextSearch    |    145.8 ns |   1.03 ns |   0.96 ns |
| StringContains   | 16,971.0 ns | 114.98 ns | 101.92 ns |

![Benchmark Results](./assets/benchmark-results.png)

## Serialization/Deserialization (Experimental Feature)

Since building the Trie for the Aho-Corasick algorithm can be time-consuming, the Trie can be serialized to a file and
reloaded when needed.

```csharp
// Save trie nodes to a file
var search = new AhoStringSearch();
var trie = search.CreateTrie();
trie.AddString("his");
trie.Build();

using var fs = new FileStream("test.trie", FileMode.Create);
using var bw = new BinaryWriter(fs);
var context = new TrieSerializationContext();
context.Write(trie, bw);

// Load trie nodes from a file
using var fs = new FileStream("test.trie", FileMode.Open);
using var br = new BinaryReader(fs);
var context = new TrieSerializationContext();
var root = context.Load(br);

var search = AhoStringSearch.CreateFrom(root);
```
