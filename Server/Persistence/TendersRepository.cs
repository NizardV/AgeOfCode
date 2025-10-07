// Server/Persistence/TendersRepository.cs
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Persistence;

public class TendersRepository(WssDbContext ctx) : ITendersRepository
{
    public async Task AddMany(IEnumerable<Tender> tenders) => await ctx.Tenders.AddRangeAsync(tenders);
    public async Task SaveChanges() => await ctx.SaveChangesAsync();
}
