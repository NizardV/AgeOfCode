using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Records;
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

public sealed class GamesStatsEnvelope
{
    public GamesStatsResponse? Stats { get; init; }
}
