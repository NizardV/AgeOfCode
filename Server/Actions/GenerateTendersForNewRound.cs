using FluentResults;
using Microsoft.EntityFrameworkCore;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence;
using Server.Persistence.Contracts;

public sealed record GenerateTendersForNewRoundParams(int GameId, int TendersPerPlayer = 2);

public class GenerateTendersForNewRound(
    WssDbContext ctx,
    ITendersRepository tendersRepository,
    IGamesRepository gamesRepository,
    IGameHubService gameHubService
) : IAction<GenerateTendersForNewRoundParams, Result<int>>
{
    private static readonly Random Rnd = new();

    public async Task<Result<int>> PerformAsync(GenerateTendersForNewRoundParams p)
    {
        var game = await ctx.Games
            .Include(g => g.Players)
                .ThenInclude(p => p.Company)
                    .ThenInclude(c => c.Employees)
                        .ThenInclude(e => e.Skills)
            .FirstOrDefaultAsync(g => g.Id == p.GameId);

        if (game is null)
            return Result.Fail<int>($"Game {p.GameId} not found.");

        var allSkills = await ctx.Skills.ToListAsync();
        if (allSkills.Count == 0)
            return Result.Fail<int>("No base skills in catalog.");

        var toCreate = new List<Tender>();

        foreach (var player in game.Players)
        {
            var company = player.Company;
            if (company is null || company.Id is null) continue;

            for (int i = 0; i < p.TendersPerPlayer; i++)
            {
                var time = Rnd.Next(10, 21);      // 10..20
                var gain = Rnd.Next(1000, 5001);  // 1 000..5 000

                var reqCount = Rnd.Next(2, 4);    // 2..3 skills requis
                var reqSkills = allSkills
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(reqCount)
                    .Select(s => new LeveledSkill(s.Name, Rnd.Next(5, 16))) // niveau 5..15
                    .ToList();

                var tender = new Tender(
                    gameId: game.Id!.Value,
                    companyId: company.Id.Value,
                    gain: gain,
                    time: time,
                    contTime: time
                );

                foreach (var rs in reqSkills)
                    tender.AddSkill(rs);

                toCreate.Add(tender);
            }
        }

        if (toCreate.Count > 0)
        {
            await tendersRepository.AddMany(toCreate);
            await tendersRepository.SaveChanges();

            // Broadcast pour rafraîchir le front (GameOverview.Tenders)
            await gameHubService.UpdateCurrentGame(gameId: p.GameId);
        }

        return Result.Ok(toCreate.Count);
    }
}
