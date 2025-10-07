
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateEmployeeParams(string EmployeeName, int? CompanyId = null, Company? Company = null);

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeParams>
{
    public CreateEmployeeValidator()
    {
        RuleFor(p => p.EmployeeName).NotEmpty();
        RuleFor(p => p.CompanyId).NotEmpty().When(p => p.Company is null);
        RuleFor(p => p.Company).NotEmpty().When(p => p.CompanyId is null);
    }
}

public class CreateEmployee(
    ICompaniesRepository companiesRepository,
    IEmployeesRepository employeesRepository,
    ISkillsRepository skillsRepository,
    IGameHubService gameHubService
) : IAction<CreateEmployeeParams, Result<Employee>>
{
    public async Task<Result<Employee>> PerformAsync(CreateEmployeeParams actionParams)
    {
        var rnd = new Random();

        var nameList = new List<string>
        {
            "Nizard", "John", "Jane", "Bob", "Alice", "Tom", "Sara", "Mike", "Lily", "David", "Emma", "Baptiste", "Valérie", "Lucas", "Julie", "Antoine", "Camille", "Mathieu", "Juliette", "Léa", "Émile", "Michael", "Christopher", "Jessica", "Ashley", "Emily", "Matthew", "Amanda", "Daniel", "James", "Robert", "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "William", "Susan", "Joseph", "Margaret", "Charles", "Thomas", "Sarah", "Karen", "Nancy", "Lisa", "Nicolas", "Alexandre", "Julien", "Maxime", "Pierre", "Hugo", "Léo", "Gabriel", "Arthur", "Louis", "Raphaël", "Adam", "Nathan", "Enzo", "Jules", "Théo", "Ethan", "Sacha", "Maël", "Chloé", "Manon", "Inès", "Louise", "Jade", "Ambre", "Rose", "Mila", "Eva", "Zoé", "Charlotte", "Léonie", "Agathe", "Jeanne", "Margaux", "Clara", "Marie", "Sophie", "Isabelle", "Hélène", "Nathalie", "Sylvie", "Christine", "Françoise", "Monique", "Martine", "Nicole", "Catherine", "Anne", "Céline", "Sandrine", "Aurélie", "Stéphanie", "Laetitia", "Virginie", "Audrey", "Kevin", "Brian", "Steven", "Paul", "Mark", "Laura", "Kimberly", "Deborah", "Jason", "Michelle", "Cynthia", "Angela", "Melissa", "Brenda", "Amy", "Anna", "Rebecca", "Peter", "Charlotte", "Olivia", "Sophia", "Liam", "Noah", "Oliver", "Elijah", "William", "James", "Benjamin", "Lucas", "Henry", "Alexander"
        };

        var lastNameList = new List<string>
        {
            "Nodon", "Payet", "Verdenal","Doe", "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts", "Turner", "Phillips", "Evans", "Edwards", "Stewart", "Collins", "Murphy", "Parker", "Cook", "Morgan", "Bell", "Cooper", "Reed", "Bailey", "Kelly", "Howard", "Ward", "Cox", "Richardson", "Watson", "Brooks", "Wood", "James", "Bennett", "Gray", "Hughes", "Price", "Sanders", "Patel", "Long", "Foster", "Diaz", "Jenkins", "Perry", "Russell", "Sullivan", "Fisher", "Richards", "West", "Marshall", "Fowler", "Warren", "Hunt", "Snyder", "Mason", "Carroll", "Porter", "Stone", "Payne", "Harvey", "Little", "Burton", "Cole", "Hamilton", "Gibson", "Ray", "Reynolds", "Jordan", "Ellis", "Harrison", "Graham", "Wallace", "Simmons", "Hayes", "Myers", "Ford", "Daniels", "Bowman", "Wheeler", "Ferguson", "Lawson", "Burns", "Chavez", "Pierce", "Freeman", "Perkins", "Pearson", "Holt", "Black", "Stevens", "Hunter", "Gordon", "Armstrong", "Stephens", "Murray", "Hart", "Fuller", "Spencer", "Gardner", "Franklin", "Kennedy", "Dunn", "Webb", "Crawford", "Olson", "Curtis", "Greene", "Jensen", "Henry", "Boyd", "Bishop", "Mills", "Nichols", "Grant", "Knight", "Kelley", "Hoffman", "Elliott", "Cunningham", "Duncan", "Carr", "Williamson", "Lucas", "Hudson", "Hansen", "Sharp", "Andrews", "Cain", "Palmer", "Lane", "Berry", "Jacobs", "Matthews", "Arnold", "Wagner", "Owens", "Parks", "Rice", "Mendoza", "Day", "Garza", "Walsh", "Robertson", "Shaw", "Reyes", "Watkins", "Lynch", "Woods", "Riley", "Gilbert", "Montgomery", "Alvarez", "Medina", "Soto", "Meyer", "Chambers", "Barrett", "Strickland", "Patterson", "Barnes", "Gross", "Goodman", "Paul", "Walters", "Mack", "Frazier", "Erickson", "Ball", "Mann", "Walton", "Powers", "Goodwin", "Holman", "Tyler", "Casey", "Swanson", "Stokes", "Fields", "Ortega", "Hale", "Singleton", "Gomez"
        };


        var actionValidator = new CreateEmployeeValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (employeeName, companyId, company) = actionParams;

        company ??= await companiesRepository.GetById(companyId!.Value);

        if (company is null)
        {
            Result.Fail($"Company with Id \"{companyId}\" not found.");
        }

        IEnumerable<int> salaries = [];

        for (var salary = 29000; salary <= 100000; salary += 500)
        {
            salaries = salaries.Append(salary);
        }

        var randomSalary = salaries.ToList()[rnd.Next(salaries.Count() - 1)];

        var randomName = nameList[rnd.Next(nameList.Count)];
        var randomLastName = lastNameList[rnd.Next(lastNameList.Count)];

        var fullName = $"{randomName} {randomLastName}";

        var employee = new Employee(fullName, company!.Id!.Value, company!.Player.GameId, randomSalary);

        var randomSkills = await skillsRepository.GetRandomSkills(3);

        foreach (var randomSkill in randomSkills)
        {
            employee.Skills.Add(new LeveledSkill(randomSkill.Name, rnd.Next(11)));
        }

        await employeesRepository.SaveEmployee(employee);

        await gameHubService.UpdateCurrentGame(gameId: company.Player.GameId);

        return Result.Ok(employee);
    }
}
