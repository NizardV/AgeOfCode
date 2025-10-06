
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;

namespace Server.Actions;

public sealed record FinishGameParams(int? GameId = null, Game? Game = null);

public class FinishGameValidator : AbstractValidator<FinishGameParams>
{
    public FinishGameValidator()
    {
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class FinishGame(IGameHubService gameHubService) : IAction<FinishGameParams, Result<Game>>
{
    public Task<Result<Game>> PerformAsync(FinishGameParams actionParams)
    {
        var (_, game) = actionParams;

        game!.Status = GameStatus.Finished;

        return Task.FromResult(Result.Ok(game));
    }
}
