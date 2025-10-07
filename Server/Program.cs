using FluentResults;

using Server.Actions;
using Server.Actions.Contracts;
using Server.Endpoints.Extensions;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence;
using Server.Persistence.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenLocalhost(5176);
    o.ListenLocalhost(7032, lo => lo.UseHttps());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WssDbContext>();
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddSignalR();

builder.Services.AddTransient<ICompaniesRepository, CompaniesRepository>();
builder.Services.AddTransient<IConsultantsRepository, ConsultantsRepository>();
builder.Services.AddTransient<IEmployeesRepository, EmployeesRepository>();
builder.Services.AddTransient<IGamesRepository, GamesRepository>();
builder.Services.AddTransient<IPlayersRepository, PlayersRepository>();
builder.Services.AddTransient<IRoundsRepository, RoundsRepository>();
builder.Services.AddTransient<ISkillsRepository, SkillsRepository>();

builder.Services.AddTransient<IAction<ActInRoundParams, Result<Round>>, ActInRound>();
builder.Services.AddTransient<IAction<ApplyRoundActionParams, Result>, ApplyRoundAction>();
builder.Services.AddTransient<IAction<CreateCompanyParams, Result<Company>>, CreateCompany>();
builder.Services.AddTransient<IAction<CreateEmployeeParams, Result<Employee>>, CreateEmployee>();
builder.Services.AddTransient<IAction<CreateGameParams, Result<Game>>, CreateGame>();
builder.Services.AddTransient<IAction<CreatePlayerParams, Result<Player>>, CreatePlayer>();
builder.Services.AddTransient<IAction<FinishGameParams, Result<Game>>, FinishGame>();
builder.Services.AddTransient<IAction<FinishRoundParams, Result<Round>>, FinishRound>();
builder.Services.AddTransient<IAction<JoinGameParams, Result<Player>>, JoinGame>();
builder.Services.AddTransient<IAction<StartGameParams, Result<Game>>, StartGame>();
builder.Services.AddTransient<IAction<StartRoundParams, Result<Round>>, StartRound>();

builder.Services.AddTransient<IGameHubService, GameHubService>();
builder.Services.AddTransient<IMainHubService, MainHubService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpoints();

app.MapHub<MainHub>("/main");
app.MapHub<GameHub>("/games/{gameId}");

app.UseHttpsRedirection();

app.Run();
