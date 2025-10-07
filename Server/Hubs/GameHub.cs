using Microsoft.AspNetCore.SignalR;

using Server.Hubs.Contracts;
using Server.Hubs.Records;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Hubs;

public class GameHub(IGameHubService gameHubService, IGamesRepository gamesRepository) : Hub<IGameHubClient>()
{
    public override async Task OnConnectedAsync()
    {
        var gameId = Context.GetHttpContext()?.GetRouteData().Values["gameId"];

        if (gameId is null) { Context.Abort(); return; }

        var game = await gamesRepository.GetById(int.Parse((string) gameId));

        if (game is null) { Context.Abort(); return; }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"{game.Id}__{game.Name}");

        await gameHubService.UpdateCurrentGame(Clients.Caller, gameId: int.Parse((string) gameId));

        await base.OnConnectedAsync();
    }
}

public class GameHubService(IHubContext<GameHub, IGameHubClient> gameHubContext, IGamesRepository gamesRepository, ILogger<GameHubService> logger) : IGameHubService
{
    public async Task UpdateCurrentGame(IGameHubClient? caller = null, int? gameId = null, Game? game = null)
    {
        if (game is null && gameId is null) { return; }

        game = await gamesRepository.GetForOverviewById((int) gameId!);

        if (game is null) { return; }

        var data = game.ToOverview();

        var groupName = $"{game.Id}__{game.Name}";
        logger.LogInformation("Sending game update to group: {GroupName}", groupName);

        caller ??= gameHubContext.Clients.Group(groupName);

        await caller.CurrentGameUpdated(data);
    }
}
