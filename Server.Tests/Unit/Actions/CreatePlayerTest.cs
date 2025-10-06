using FluentAssertions;

using FluentResults;

using Moq;

using Server.Actions;
using Server.Actions.Contracts;
using Server.Enumes;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Tests.Unit.Actions;

public class CreatePlayerTest
{
    private readonly Mock<IGamesRepository> _gamesRepositoryMock;
    private readonly Mock<IPlayersRepository> _playersRepositoryMock;
    private readonly Mock<IAction<CreateCompanyParams, Result<Company>>> _createCompanyMock;
    private readonly Mock<IGameHubService> _gameHubServiceMock;

    public CreatePlayerTest()
    {
        _gamesRepositoryMock = new Mock<IGamesRepository>();
        _playersRepositoryMock = new Mock<IPlayersRepository>();
        _createCompanyMock = new Mock<IAction<CreateCompanyParams, Result<Company>>>();
        _gameHubServiceMock = new Mock<IGameHubService>();

        _gamesRepositoryMock
            .Setup(r => r.GetById(It.IsAny<int>()))
            .Returns(Task.Run(() =>
            {
                var game = new Game("Game 1");
                typeof(Game).GetProperty("Id")?.SetValue(game, 1);
                return (Game?) game;
            }));

        _playersRepositoryMock
            .Setup(r => r.IsPlayerNameAvailable(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.Run(() => true));

        _playersRepositoryMock
            .Setup(r => r.SavePlayer(It.IsAny<Player>()))
            .Returns(Task.Run(() => { }));

        _createCompanyMock
            .Setup(a => a.PerformAsync(It.IsAny<CreateCompanyParams>()))
            .Returns(Task.Run(() => Result.Ok(It.IsAny<Company>())));

        _gameHubServiceMock
            .Setup(s => s.UpdateCurrentGame(It.IsAny<IGameHubClient?>(), It.IsAny<int?>(), It.IsAny<Game?>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task ItShouldNotCreatePlayerWithoutAPlayerName()
    {
        var actionParams = new CreatePlayerParams("", "Company 1",CompanyType.Startup,1);
        var action = new CreatePlayer(
          _gamesRepositoryMock.Object,
          _playersRepositoryMock.Object,
          _createCompanyMock.Object,
          _gameHubServiceMock.Object
        );

        var actionResult = await action.PerformAsync(actionParams);

        actionResult.IsFailed.Should().BeTrue();
        actionResult.Errors.Count.Should().Be(1);
        actionResult.Errors.First().Message.Should().Be("'Player Name' must not be empty.");
    }

    [Fact]
    public async Task ItShouldNotCreatePlayerWhenGameDoesNotExist()
    {
        _gamesRepositoryMock
            .Setup(r => r.GetById(It.IsAny<int>()))
            .Returns(Task.Run(() => (Game?) null));

        var actionParams = new CreatePlayerParams("Player 1", "Company 1",CompanyType.Startup,1);
        var action = new CreatePlayer(
          _gamesRepositoryMock.Object,
          _playersRepositoryMock.Object,
          _createCompanyMock.Object,
          _gameHubServiceMock.Object
        );

        var actionResult = await action.PerformAsync(actionParams);

        actionResult.IsFailed.Should().BeTrue();
        actionResult.Errors.Count.Should().Be(1);
        actionResult.Errors.First().Message.Should().Be("Game with Id \"1\" not found.");
    }

    [Fact]
    public async Task ItShouldNotCreatePlayerWithATakenPlayerName()
    {
        _playersRepositoryMock
            .Setup(r => r.IsPlayerNameAvailable(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.Run(() => false));

        var actionParams = new CreatePlayerParams("Player 1", "Company 1",CompanyType.Startup,1);
        var action = new CreatePlayer(
          _gamesRepositoryMock.Object,
          _playersRepositoryMock.Object,
          _createCompanyMock.Object,
          _gameHubServiceMock.Object
        );

        var actionResult = await action.PerformAsync(actionParams);

        actionResult.IsFailed.Should().BeTrue();
        actionResult.Errors.Count.Should().Be(1);
        actionResult.Errors.First().Message.Should().Be("'Player Name' is already in use.");
    }

    [Fact]
    public async Task ItShouldNotCreatePlayerIfCreateCompanyFails()
    {
        _createCompanyMock
            .Setup(a => a.PerformAsync(It.IsAny<CreateCompanyParams>()))
            .Returns(Task.Run(() => Result.Fail<Company>("CreateCompany ERROR")));

        var actionParams = new CreatePlayerParams("Player 1", "Company 1", CompanyType.Startup,1);
        var action = new CreatePlayer(
          _gamesRepositoryMock.Object,
          _playersRepositoryMock.Object,
          _createCompanyMock.Object,
          _gameHubServiceMock.Object
        );

        var actionResult = await action.PerformAsync(actionParams);

        actionResult.IsFailed.Should().BeTrue();
        actionResult.Errors.Count.Should().Be(1);
        actionResult.Errors.First().Message.Should().Be("CreateCompany ERROR");
    }

    [Fact]
    public async Task ItShouldCreatePlayerWithValidData()
    {
        var actionParams = new CreatePlayerParams("Player 1", "Company 1", CompanyType.Startup,1);
        var action = new CreatePlayer(
          _gamesRepositoryMock.Object,
          _playersRepositoryMock.Object,
          _createCompanyMock.Object,
          _gameHubServiceMock.Object
        );

        var actionResult = await action.PerformAsync(actionParams);

        actionResult.IsSuccess.Should().BeTrue();
        actionResult.Errors.Count.Should().Be(0);
    }
}
