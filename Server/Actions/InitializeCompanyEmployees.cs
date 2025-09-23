using FluentResults;
using FluentValidation;

using Server.Actions.Contracts;
using Server.Enumes;
using Server.Models;
using Server.Persistence.Contracts;
using Server.Hubs.Contracts;

namespace Server.Actions;

public sealed record InitializeCompanyEmployeesParams(int CompanyId);

public class InitializeCompanyEmployeesValidator : AbstractValidator<InitializeCompanyEmployeesParams>
{
    public InitializeCompanyEmployeesValidator()
    {
        RuleFor(p => p.CompanyId).GreaterThan(0);
    }
}


public class InitializeCompanyEmployees(
    ICompaniesRepository companiesRepository,
    IEmployeesRepository employeesRepository,
    ISkillsRepository skillsRepository,
    IGameHubService gameHubService
) : IAction<InitializeCompanyEmployeesParams, Result<int>>
{
    public async Task<Result<int>> PerformAsync(InitializeCompanyEmployeesParams actionParams)
    {
        var validator = new InitializeCompanyEmployeesValidator();
        var validation = await validator.ValidateAsync(actionParams);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var rnd = new Random();

        var company = await companiesRepository.GetById(actionParams.CompanyId);
        if (company is null)
            return Result.Fail<int>($"Company with Id \"{actionParams.CompanyId}\" not found.");

        var target = TypeCompany.From(company.Type).NumberConsultant;
        var current = company.Employees.Count;
        var toCreate = Math.Max(0, target - current);

        if (toCreate == 0)
        {
            return Result.Ok(0);
        }

        //#todo adapt value of equilibrage
        var salaries = Enumerable.Range(0, (2000 - 100000) / 500 + 1)
                                 .Select(step => 29000 + step * 500)
                                 .ToList();

        for (int i = 0; i < toCreate; i++)
        {
            var randomSalary = salaries[rnd.Next(salaries.Count)];

            var employee = new Employee(
                $"Employee {current + i + 1}",
                company.Id!.Value,
                company.Player.GameId,
                randomSalary
            );

            var randomSkills = await skillsRepository.GetRandomSkills(3);
            foreach (var s in randomSkills)
            {
                //#todo adapt value of equilibrage
                employee.Skills.Add(new LeveledSkill(s.Name, rnd.Next(8)));
            }

            await employeesRepository.SaveEmployee(employee);
            company.Employees.Add(employee);
        }

        await gameHubService.UpdateCurrentGame(gameId: company.Player.GameId);

        return Result.Ok(toCreate);
    }
}
