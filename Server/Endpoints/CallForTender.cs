
using Server.Endpoints.Contracts;

namespace Server.Endpoints;

public class CallForTender : IEndpoint
{
    // Implementation goes here
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenders/call", Handler).WithTags("Tenders");
    }

    private async Task Handler(HttpContext context)
    {
        await context.Response.WriteAsync("Call for tender endpoint is under construction.");
    }
}
