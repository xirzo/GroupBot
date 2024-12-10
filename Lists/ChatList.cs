using GroupBot.Shared;

namespace GroupBot.Lists;

public class ChatList(string name)
{
    private readonly List<Participant> _list = [];

    public IReadOnlyList<Participant> List => _list;

    public string Name { get; } = name;


    public void Add(long id, string name)
    {
        if (_list.Exists(l => l.Id == id)) return;

        _list.Add(new Participant(id, name));
    }

    public void Add(Participant participant)
    {
        if (_list.Exists(l => l == participant)) return;

        _list.Add(participant);
    }

    public void Remove(long id)
    {
        var participant = _list.Find(l => l.Id == id);

        if (participant != null)
            _list.Remove(participant);
    }

    public void Shuffle()
    {
        _list.Shuffle();
    }

    public void Swap(Participant participantAsker, Participant participantGiver)
    {
        var indexAsker = _list.FindIndex(p => p.Id == participantAsker.Id);
        var indexGiver = _list.FindIndex(p => p.Id == participantGiver.Id);
        Console.WriteLine($"{indexAsker}  {indexGiver}");

        if (indexAsker == -1 || indexGiver == -1)
            throw new InvalidOperationException("One or both participants not found in the list.");

        (_list[indexAsker], _list[indexGiver]) = (_list[indexGiver], _list[indexAsker]);
    }
}