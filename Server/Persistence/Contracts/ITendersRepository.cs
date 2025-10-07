// Server/Persistence/Contracts/ITendersRepository.cs
using Server.Models;

namespace Server.Persistence.Contracts;

public interface ITendersRepository
{
    Task AddMany(IEnumerable<Tender> tenders);
    Task SaveChanges();
}
