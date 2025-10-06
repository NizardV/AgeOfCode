using FluentResults;
using Server.Actions;
using Server.Actions.Contracts;
using Server.Endpoints.Contracts;
using Server.Models;
using Server.Persistence;

namespace Server.Endpoints;

public class JoinGame : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("games/{gameId}/join", Handler).WithTags("Games");
    }

    public static async Task<IResult> Handler(
        int gameId,
        JoinGameParams actionParams,
        WssDbContext context,
        IAction<JoinGameParams, Result<Player>> joinGameAction
    )
    {
        actionParams = actionParams with { GameId = gameId };

        using var transaction = context.Database.BeginTransaction();

        var actionResult = await joinGameAction.PerformAsync(actionParams);

        if (actionResult.IsFailed)
        {
            await transaction.RollbackAsync();
            return Results.BadRequest(new { Errors = actionResult.Errors.Select(e => e.Message) });
        }

        await transaction.CommitAsync();
        return Results.Ok();
    }
}
