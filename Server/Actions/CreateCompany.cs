using FluentResults;
using FluentValidation;

using Server.Actions.Contracts;
using Server.Enumes;
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
        RuleFor(p => p.type).IsInEnum();
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
        var validation = await actionValidator.ValidateAsync(actionParams);
        if (!validation.IsValid)
        {
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        }

        var (companyName, type, playerId, player) = actionParams;

        player ??= await playersRepository.GetById(playerId!.Value);
        if (player is null)
        {
            return Result.Fail<Company>($"Player with Id \"{playerId}\" not found.");
        }

        if (player.CompanyId is not null)
        {
            return Result.Fail<Company>("Player already has a company.");
        }

        var isCompanyNameAvailable = await companiesRepository.IsCompanyNameAvailable(companyName, player.GameId);
        if (!isCompanyNameAvailable)
        {
            return Result.Fail<Company>("'Company Name' is already in use.");
        }

        var company = new Company(companyName, player.Id!.Value, type);
        await companiesRepository.SaveCompany(company);

        var employeesToCreate = TypeCompany.From(type).NumberConsultant;
        foreach (var index in Enumerable.Range(1, employeesToCreate))
        {
            var createEmployeeParams = new CreateEmployeeParams($"Employee {index}", Company: company);
            var createEmployeeResult = await createEmployeeAction.PerformAsync(createEmployeeParams);
            if (createEmployeeResult.IsFailed)
            {
                return Result.Fail<Company>(createEmployeeResult.Errors);
            }
        }

        await gameHubService.UpdateCurrentGame(gameId: player.GameId);
        return Result.Ok(company);
    }
}
