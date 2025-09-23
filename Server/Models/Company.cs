using Server.Enumes;
using Server.Hubs.Records;

namespace Server.Models;

public class Company(string name, int playerId, CompanyType type)
{
    public int? Id { get; private set; }

    public string Name { get; set; } = name;

    public int PlayerId { get; set; } = playerId;

    public Player Player { get; set; } = null!;

    public CompanyType Type { get; } = type;
    public int Treasury { get; set; } = TypeCompany.From(type).Treasury;

    public ICollection<Employee> Employees { get; } = [];

    public CompanyOverview ToOverview()
    {
        return new CompanyOverview(
            Id is null ? 0 : (int)Id,
            Name,
            Treasury,
            Employees.Select(e => e.ToOverview()).ToList()
        );
    }
}
