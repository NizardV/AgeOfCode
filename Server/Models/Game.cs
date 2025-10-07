using Server.Hubs.Records;

namespace Server.Models;

public enum GameStatus
{
    Waiting,
    InProgress,
    Finished
}

public class Game(string name, int rounds = 15)
{
    public int? Id { get; private set; }

    public string Name { get; set; } = name;

    public int Rounds { get; set; } = rounds;

    public GameStatus Status { get; set; } = GameStatus.Waiting;

    public ICollection<Player> Players { get; } = [];

    public ICollection<Round> RoundsCollection { get; } = [];

    public ICollection<Consultant> Consultants { get; } = [];

    public ICollection<Tender> Tenders { get; } = new List<Tender>();

    public bool CanBeJoined() => Status == GameStatus.Waiting && Players.Count < 3;
    public bool CanBeStarted() => Status == GameStatus.Waiting;

    public bool CanStartANewRound()
        => Status == GameStatus.InProgress && RoundsCollection.Count < Rounds;

    public GameOverview ToOverview()
    {
        return new GameOverview(
            Id ?? 0,
            Name,
            Players.Select(p => p.ToOverview()).ToList(),
            Players.Count,
            3,
            Rounds,
            RoundsCollection.Count,
            Status.ToString(),
            RoundsCollection.Select(r => r.ToOverview()).ToList(),
            Consultants.Select(c => c.ToOverview()).ToList(),
            Tenders.Select(t => t.ToOverview()).ToList()
        );
    }
}
