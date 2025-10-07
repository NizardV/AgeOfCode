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

        var nameList = new List<string>
        {
            "Nizard", "John", "Jane", "Bob", "Alice", "Tom", "Sara", "Mike", "Lily", "David", "Emma", "Baptiste", "Valérie", "Lucas", "Julie", "Antoine", "Camille", "Mathieu", "Juliette", "Léa", "Émile", "Michael", "Christopher", "Jessica", "Ashley", "Emily", "Matthew", "Amanda", "Daniel", "James", "Robert", "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "William", "Susan", "Joseph", "Margaret", "Charles", "Thomas", "Sarah", "Karen", "Nancy", "Lisa", "Nicolas", "Alexandre", "Julien", "Maxime", "Pierre", "Hugo", "Léo", "Gabriel", "Arthur", "Louis", "Raphaël", "Adam", "Nathan", "Enzo", "Jules", "Théo", "Ethan", "Sacha", "Maël", "Chloé", "Manon", "Inès", "Louise", "Jade", "Ambre", "Rose", "Mila", "Eva", "Zoé", "Charlotte", "Léonie", "Agathe", "Jeanne", "Margaux", "Clara", "Marie", "Sophie", "Isabelle", "Hélène", "Nathalie", "Sylvie", "Christine", "Françoise", "Monique", "Martine", "Nicole", "Catherine", "Anne", "Céline", "Sandrine", "Aurélie", "Stéphanie", "Laetitia", "Virginie", "Audrey", "Kevin", "Brian", "Steven", "Paul", "Mark", "Laura", "Kimberly", "Deborah", "Jason", "Michelle", "Cynthia", "Angela", "Melissa", "Brenda", "Amy", "Anna", "Rebecca", "Peter", "Charlotte", "Olivia", "Sophia", "Liam", "Noah", "Oliver", "Elijah", "William", "James", "Benjamin", "Lucas", "Henry", "Alexander"
        }

        var lastNameList = new List<string>
        {
            "Nodon", "Payet", "Verdenal","Doe", "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts", "Turner", "Phillips", "Evans", "Edwards", "Stewart", "Collins", "Murphy", "Parker", "Cook", "Morgan", "Bell", "Cooper", "Reed", "Bailey", "Kelly", "Howard", "Ward", "Cox", "Richardson", "Watson", "Brooks", "Wood", "James", "Bennett", "Gray", "Hughes", "Price", "Sanders", "Patel", "Long", "Foster", "Diaz", "Jenkins", "Perry", "Russell", "Sullivan", "Fisher", "Richards", "West", "Marshall", "Fowler", "Warren", "Hunt", "Snyder", "Mason", "Carroll", "Porter", "Stone", "Payne", "Harvey", "Little", "Burton", "Cole", "Hamilton", "Gibson", "Ray", "Reynolds", "Jordan", "Ellis", "Harrison", "Graham", "Wallace", "Simmons", "Hayes", "Myers", "Ford", "Daniels", "Bowman", "Wheeler", "Ferguson", "Lawson", "Burns", "Chavez", "Pierce", "Freeman", "Perkins", "Pearson", "Holt", "Black", "Stevens", "Hunter", "Gordon", "Armstrong", "Stephens", "Murray", "Hart", "Fuller", "Spencer", "Gardner", "Franklin", "Kennedy", "Dunn", "Webb", "Crawford", "Olson", "Curtis", "Greene", "Jensen", "Henry", "Boyd", "Bishop", "Mills", "Nichols", "Grant", "Knight", "Kelley", "Hoffman", "Elliott", "Cunningham", "Duncan", "Carr", "Williamson", "Lucas", "Hudson", "Hansen", "Sharp", "Andrews", "Cain", "Palmer", "Lane", "Berry", "Jacobs", "Matthews", "Arnold", "Wagner", "Owens", "Parks", "Rice", "Mendoza", "Day", "Garza", "Walsh", "Robertson", "Shaw", "Reyes", "Watkins", "Lynch", "Woods", "Riley", "Gilbert", "Montgomery", "Alvarez", "Medina", "Soto", "Meyer", "Chambers", "Barrett", "Strickland", "Patterson", "Barnes", "Gross", "Goodman", "Paul", "Walters", "Mack", "Frazier", "Erickson", "Ball", "Mann", "Walton", "Powers", "Goodwin", "Holman", "Tyler", "Casey", "Swanson", "Stokes", "Fields", "Ortega", "Hale", "Singleton", "Gomez"
        }

        for (int i = 0; i < toCreate; i++)
        {
            var randomSalary = salaries[rnd.Next(salaries.Count)];

            var randomName = nameList[rnd.Next(nameList.Count)];
            var randomLastName = lastNameList[rnd.Next(lastNameList.Count)];

            var fullName = $"{randomName} {randomLastName}";
            var employee = new Employee(
                fullName,
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
