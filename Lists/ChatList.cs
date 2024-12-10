using GroupBot.Shared;

namespace GroupBot.Lists;

public class ChatList(string name)
{
    private readonly List<string> _list = [];

    public IReadOnlyList<string> List => _list;

    public string Name { get; } = name;


    public void Add(string word)
    {
        if (_list.Contains(word)) return;

        _list.Add(word);
    }

    public void Remove(string word)
    {
        _list.Remove(word);
    }

    public void Shuffle()
    {
        _list.Shuffle();
    }
}