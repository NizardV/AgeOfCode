// Server/Models/Tender.cs
using Server.Hubs.Records;

namespace Server.Models;

public class Tender
{
    private Tender() { }

    public Tender(int gameId, int companyId, int gain, int time, int? contTime = null)
    {
        GameId = gameId;
        CompanyId = companyId;
        Gain = gain;
        Time = time;
        ContTime = contTime ?? time;
        IsStart = false;
        IsEnd = false;
    }

    public int? Id { get; private set; }

    public int GameId { get; private set; }
    public Game Game { get; private set; } = null!;

    public int CompanyId { get; private set; }
    public Company Company { get; private set; } = null!;

    public int Gain { get; private set; }

    public ICollection<LeveledSkill> Skills { get; } = [];

    public int Time { get; private set; }
    public int ContTime { get; private set; }
    public bool IsStart { get; private set; }
    public bool IsEnd { get; private set; }

    public void Start() {
        if (!IsEnd)
        {
            IsStart = true; if (ContTime <= 0) ContTime = Time;
        } }

    public void Tick(int delta)
    {
        if (IsStart && !IsEnd && delta > 0) { ContTime = Math.Max(0, ContTime - delta); if (ContTime == 0) End(); }
    }

    public void End()
    {
        if (!IsEnd) { IsEnd = true; IsStart = false; ContTime = 0; }
    }

    public void ResetCountdown()
    {
        if (!IsEnd) ContTime = Time;
    }

    public void SetGain(int newGain)
    {
        if (newGain < 0) throw new ArgumentOutOfRangeException(nameof(newGain)); Gain = newGain;
    }
    public void AddSkill(LeveledSkill skill) => Skills.Add(skill);
    public void ClearSkills() => Skills.Clear();

    public TenderOverview ToOverview() => new(
        Id ?? 0, CompanyId, Gain, Time, ContTime, IsStart, IsEnd,
        Skills.Select(s => s.ToOverview()).ToList()
    );
}
