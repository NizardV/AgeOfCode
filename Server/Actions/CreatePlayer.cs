using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Enumes;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public record CreatePlayerParams(
    string PlayerName,
    string CompanyName,
    CompanyType type,
    int? GameId = null,
    Game? Game = null
    );

public class CreatePlayerValidator : AbstractValidator<CreatePlayerParams>
{
    public CreatePlayerValidator()
    {
        RuleFor(p => p.PlayerName).NotEmpty();
        RuleFor(p => p.CompanyName).NotEmpty();
        RuleFor(p => p.type).NotEmpty().IsInEnum();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class CreatePlayer(
  IGamesRepository gamesRepository,
  IPlayersRepository playersRepository,
  IAction<CreateCompanyParams, Result<Company>> createCompanyAction,
  IGameHubService gameHubService
) : IAction<CreatePlayerParams, Result<Player>>
{
    public async Task<Result<Player>> PerformAsync(CreatePlayerParams actionParams)
    {
        var actionValidator = new CreatePlayerValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (playerName, companyName, type,gameId, game) = actionParams;

        game ??= await gamesRepository.GetById(gameId!.Value);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        if (!game!.CanBeJoined())
        {
            return Result.Fail("Game is full and cannot be joined.");
        }

        var isPlayerNameAvailable = await playersRepository.IsPlayerNameAvailable(playerName, game.Id!.Value);

        if (!isPlayerNameAvailable)
        {
            return Result.Fail("'Player Name' is already in use.");
        }

        var player = new Player(playerName, game.Id!.Value);

        await playersRepository.SavePlayer(player);

        var createCompanyParams = new CreateCompanyParams(companyName, type, Player: player);// possblement une erreur car il ya a pas plaierId
        var createCompanyResult = await createCompanyAction.PerformAsync(createCompanyParams);

        if (createCompanyResult.IsFailed)
        {
            return Result.Fail(createCompanyResult.Errors);
        }

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        return Result.Ok(player);
    }
}
