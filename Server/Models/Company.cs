using Server.Enumes;
using Server.Hubs.Records;

namespace Server.Models;

public class Company
{
    private Company() { }

    public Company(string name, int playerId, CompanyType type)
    {
        Name = name;
        PlayerId = playerId;
        Treasury = TypeCompany.From(type).Treasury;
    }

    public int? Id { get; private set; }

    public string Name { get; private set; } = null!;

    public int PlayerId { get; private set; }

    public Player Player { get; private set; } = null!;

    public int Treasury { get; private set; }

    public ICollection<Employee> Employees { get; } = new List<Employee>();

    public CompanyOverview ToOverview()
    {
        return new CompanyOverview(
            Id ?? 0,
            Name,
            Treasury,
            Employees.Select(e => e.ToOverview()).ToList()
        );
    }
}
