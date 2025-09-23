using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Enumes;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateCompanyParams(
    string CompanyName,
    CompanyType type,
    int? PlayerId = null,
    Player? Player = null
    );

public class CreateCompanyValidator : AbstractValidator<CreateCompanyParams>
{
    public CreateCompanyValidator()
    {
        RuleFor(p => p.CompanyName).NotEmpty();
        RuleFor(p => p.type).NotEmpty().IsInEnum();
        RuleFor(p => p.PlayerId).NotEmpty().When(p => p.Player is null);
        RuleFor(p => p.Player).NotEmpty().When(p => p.PlayerId is null);
    }
}

public class CreateCompany(
  ICompaniesRepository companiesRepository,
  IPlayersRepository playersRepository,
  IAction<CreateEmployeeParams, Result<Employee>> createEmployeeAction,
  IGameHubService gameHubService
) : IAction<CreateCompanyParams, Result<Company>>
{
    public async Task<Result<Company>> PerformAsync(CreateCompanyParams actionParams)
    {
        var actionValidator = new CreateCompanyValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (companyName, type, playerId, player) = actionParams;

        player ??= await playersRepository.GetById(playerId!.Value);

        if (player is null)
        {
            Result.Fail($"Player with Id \"{playerId}\" not found.");
        }

        if (player!.CompanyId is not null)
        {
            return Result.Fail("Player already has a company.");
        }

        var isCompanyNameAvailable = await companiesRepository.IsCompanyNameAvailable(companyName, player.GameId);

        if (!isCompanyNameAvailable)
        {
            return Result.Fail("'Company Name' is already in use.");
        }

        var company = new Company(companyName, player.Id!.Value, type );

        await companiesRepository.SaveCompany(company);

        foreach (var index in Enumerable.Range(1, 3))
        {
            var createEmployeeParams = new CreateEmployeeParams("John Smith", Company: company);
            var createEmployeeResult = await createEmployeeAction.PerformAsync(createEmployeeParams);

            if (createEmployeeResult.IsFailed)
            {
                return Result.Fail(createEmployeeResult.Errors);
            }
        }

        await gameHubService.UpdateCurrentGame(gameId: player.GameId);

        return Result.Ok(company);
    }
}
