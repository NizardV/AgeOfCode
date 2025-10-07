using FluentResults;
using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record StartRoundParams(int? GameId = null, Game? Game = null);

public class StartRoundValidator : AbstractValidator<StartRoundParams>
{
    public StartRoundValidator()
    {
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class StartRound(
    IGamesRepository gamesRepository,
    IRoundsRepository roundsRepository,
    IGameHubService gameHubService,
    // 🔥 injecte l’action qui génère les tenders
    IAction<GenerateTendersForNewRoundParams, Result<int>> generateTendersForNewRound
) : IAction<StartRoundParams, Result<Round>>
{
    public async Task<Result<Round>> PerformAsync(StartRoundParams actionParams)
    {
        var actionValidator = new StartRoundValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (gameId, game) = actionParams;

        game ??= await gamesRepository.GetById(gameId!.Value);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        if (!game.CanStartANewRound())
        {
            return Result.Fail("Game cannot start a new round.");
        }

        var round = new Round(game.Id!.Value, game.RoundsCollection.Count + 1);

        await roundsRepository.SaveRound(round);

        // ✅ génère 2 tenders aléatoires par joueur pour ce nouveau round
        var gen = await generateTendersForNewRound.PerformAsync(
            new GenerateTendersForNewRoundParams(round.GameId, 2)
        );
        if (gen.IsFailed)
        {
            return Result.Fail<Round>(gen.Errors.Select(e => e.Message));
        }

        // ✅ broadcast pour rafraîchir CurrentGame côté front (incluant Tenders)
        await gameHubService.UpdateCurrentGame(gameId: round.GameId);

        return Result.Ok(round);
    }
}
