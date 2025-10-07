namespace Server.Hubs.Records;

public sealed record GameOverview(
    int Id,
    string Name,
    ICollection<PlayerOverview> Players,
    int PlayersCount,
    int MaximumPlayersCount,
    int MaximumRounds,
    int CurrentRound,
    string Status,
    ICollection<RoundOverview> Rounds,
    ICollection<ConsultantOverview> Consultants,
    // ✅ Nouvel ensemble d'appels d'offres visibles au niveau du jeu
    ICollection<TenderOverview> Tenders
);

public sealed record PlayerOverview(
    int Id,
    string Name,
    CompanyOverview Company
);

public sealed record CompanyOverview(
    int Id,
    string Name,
    int Treasury,
    ICollection<EmployeeOverview> Employees
);

public record ConsultantOverview(
    int Id,
    string Name,
    int SalaryRequirement,
    ICollection<SkillOverview> Skills
);

public sealed record EmployeeOverview(
    int Id,
    string Name,
    int Salary,
    ICollection<SkillOverview> Skills
);

// ✅ Toujours la même structure de skill partagée dans tout le GameOverview
public sealed record SkillOverview(
    string Name,
    int Level
);

public sealed record RoundOverview(
    int Id,
    ICollection<RoundActionOverview> Actions
);

public sealed record RoundActionOverview(
    string ActionType,
    string Payload,
    int PlayerId
);

public sealed record TenderOverview(
    int Id,
    int CompanyId,
    int Gain,
    int Time,
    int ContTime,
    bool IsStart,
    bool IsEnd,
    ICollection<SkillOverview> Skills
);
