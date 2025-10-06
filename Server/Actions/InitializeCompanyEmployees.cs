using System.Linq;
using FluentResults;
using FluentValidation;

using Server.Actions.Contracts;
using Server.Enumes;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

public sealed record InitializeCompanyEmployeesParams(int CompanyId, CompanyType Type);

public class InitializeCompanyEmployeesValidator : AbstractValidator<InitializeCompanyEmployeesParams>
{
    public InitializeCompanyEmployeesValidator()
    {
        RuleFor(p => p.CompanyId).GreaterThan(0);
        RuleFor(p => p.Type).IsInEnum();
    }
}

public class InitializeCompanyEmployees(
    ICompaniesRepository companiesRepository,
    IEmployeesRepository employeesRepository,
    ISkillsRepository skillsRepository,
    IGameHubService gameHubService
) : IAction<InitializeCompanyEmployeesParams, Result<int>>
{
    // optionnel : éviter de recréer le validator à chaque appel
    private static readonly InitializeCompanyEmployeesValidator _validator = new();

    public async Task<Result<int>> PerformAsync(InitializeCompanyEmployeesParams actionParams)
    {
        var validation = await _validator.ValidateAsync(actionParams);
        if (!validation.IsValid)
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));

        var company = await companiesRepository.GetById(actionParams.CompanyId);
        if (company is null)
            return Result.Fail<int>($"Company with Id \"{actionParams.CompanyId}\" not found.");

        var target = TypeCompany.From(actionParams.Type).NumberConsultant;

        var current = company.Employees.Count;
        var toCreate = Math.Max(0, target - current);
        if (toCreate == 0)
            return Result.Ok(0);

        var rnd = new Random();

        var salaries = Enumerable
            .Range(0, ((100000 - 29000) / 500) + 1)
            .Select(i => 29000 + i * 500)
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
                employee.Skills.Add(new LeveledSkill(s.Name, rnd.Next(8))); // #todo: équilibrage
            }

            await employeesRepository.SaveEmployee(employee);
            company.Employees.Add(employee);
        }

        await gameHubService.UpdateCurrentGame(gameId: company.Player.GameId);
        return Result.Ok(toCreate);
    }
}
