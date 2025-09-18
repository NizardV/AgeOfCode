using Microsoft.EntityFrameworkCore;

using Server.Endpoints.Contracts;
using Server.Models;
using Server.Persistence;

namespace Server.Endpoints;

public class GameStatistics : IEndpoint
{

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // GET /games/stats
        app.MapGet("games/stats", Handler)
           .WithTags("Games")
           .WithName("GetGamesStatistics");
    }

    public static async Task<IResult> Handler(WssDbContext context)
    {
        var query = context.Games
                           .AsNoTracking()
                           .Select(g => new
                           {
                               g.Id,
                               g.Status,
                               PlayersCount = g.Players.Count,
                               RoundsPlanned = g.Rounds,
                               RoundsPlayed = g.RoundsCollection.Count,
                               ConsultantsCount = g.Consultants.Count
                           });

        var list = await query.ToListAsync();

        var totalGames = list.Count;

        var waiting = list.Count(g => g.Status == GameStatus.Waiting);
        var inProgress = list.Count(g => g.Status == GameStatus.InProgress);
        var finished = list.Count(g => g.Status == GameStatus.Finished);

        var totalPlayers = list.Sum(g => g.PlayersCount);
        var avgPlayersPerGame = totalGames == 0 ? 0.0 : (double) totalPlayers / totalGames;

        var totalRoundsPlanned = list.Sum(g => g.RoundsPlanned);
        var totalRoundsPlayed = list.Sum(g => g.RoundsPlayed);
        var avgRoundsPlayed = totalGames == 0 ? 0.0 : (double) totalRoundsPlayed / totalGames;

        var joinable = list.Count(g =>
            g.Status == GameStatus.Waiting && g.PlayersCount < 3
        );

        var payload = new GamesStatsResponse(
            TotalGames: totalGames,
            WaitingGames: waiting,
            InProgressGames: inProgress,
            FinishedGames: finished,
            JoinableGames: joinable,
            TotalPlayers: totalPlayers,
            AvgPlayersPerGame: Math.Round(avgPlayersPerGame, 2),
            TotalRoundsPlanned: totalRoundsPlanned,
            TotalRoundsPlayed: totalRoundsPlayed,
            AvgRoundsPlayed: Math.Round(avgRoundsPlayed, 2),
            TotalConsultants: list.Sum(g => g.ConsultantsCount)
        );

        return Results.Ok(payload);
    }
}

public sealed record GamesStatsResponse(
    int TotalGames,
    int WaitingGames,
    int InProgressGames,
    int FinishedGames,
    int JoinableGames,
    int TotalPlayers,
    double AvgPlayersPerGame,
    int TotalRoundsPlanned,
    int TotalRoundsPlayed,
    double AvgRoundsPlayed,
    int TotalConsultants
);
