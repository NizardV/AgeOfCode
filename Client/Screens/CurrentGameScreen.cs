using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Net.Http.Json;

using Client.Records;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Terminal.Gui;

namespace Client.Screens;

public class CurrentGameScreen(Window target, int gameId, string playerName)
{
    private readonly Window Target = target;
    private CurrentGameView? CurrentView = null;

    private readonly int GameId = gameId;
    private readonly string PlayerName = playerName;
    private GameOverview? CurrentGame = null;

    private bool CurrentGameLoading = true;
    private bool CurrentGameStarted = false;
    private bool CurrentGameEnded = false;

    private CurrentGameActionList.Action? CurrentRoundAction = null;

    public async Task Show()
    {
        await BeforeShow();
        await LoadGame();
        await DisplayMainView();
        await DisplayCompanyView();
    }

    private Task BeforeShow()
    {
        Target.RemoveAll();

        ReloadWindowTitle();

        return Task.CompletedTask;
    }

    private void ReloadWindowTitle()
    {
        var gameName = CurrentGame is null ? "..." : CurrentGame.Name;
        Target.Title = $"{MainWindow.Title} - [Game {gameName}]";
    }

    private async Task LoadGame()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{WssConfig.WebSocketServerScheme}://{WssConfig.WebSocketServerDomain}:{WssConfig.WebSocketServerPort}/games/{GameId}"), opts =>
            {
                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                    {
                        clientHandler.ServerCertificateCustomValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    }

                    return message;
                };
            })
            .AddJsonProtocol()
            .Build();

        hubConnection.On<GameOverview>("CurrentGameUpdated", data =>
        {
            CurrentGame = data;
            ReloadWindowTitle();
            CurrentGameLoading = false;
            CurrentRoundAction = null;
            if (data.Status == "InProgress") { CurrentGameStarted = true; }
            if (data.Status == "Ended") { CurrentGameEnded = true; }
        });

        await hubConnection.StartAsync();

        var loadingDialog = new Dialog()
        {
            Width = 17,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Loading game...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Target.Add(loadingDialog);

        while (CurrentGameLoading) { await Task.Delay(100); }

        Target.Remove(loadingDialog);
    }

    private async Task DisplayMainView()
    {
        Target.RemoveAll();

        var mainView = new CurrentGameMainView(CurrentGame!, PlayerName);

        CurrentView = mainView;

        mainView.X = mainView.Y = Pos.Center();
        mainView.OnStart = (_, __) => { CurrentGameStarted = true; };

        Target.Add(mainView);

        while (!CurrentGameStarted)
        {
            await Task.Delay(100);
        }
    }

    private async Task DisplayCompanyView()
    {
        Target.RemoveAll();

        var companyView = new CurrentGameCompanyView(CurrentGame!, PlayerName);

        CurrentView = companyView;

        companyView.X = companyView.Y = 5;
        companyView.Width = companyView.Height = Dim.Fill() - 5;
        companyView.OnRoundAction = (_, roundAction) => { CurrentRoundAction = roundAction; };

        Target.Add(companyView);

        while (CurrentRoundAction is null && !CurrentGameEnded)
        {
            await Task.Delay(100);
        }

        // If the player wants to view Call For Tenders, open that screen and come back here.
        if (CurrentRoundAction == CurrentGameActionList.Action.ParticipateInCallForTenders)
        {
            // Reset the selection before navigating so we don't immediately re-trigger on return.
            CurrentRoundAction = null;

            var callForTenderScreen = new CallForTenderScreen(Target, GameId, PlayerName);
            await callForTenderScreen.Show();

            // Re-open the company view after returning from the Call For Tender screen.
            await DisplayCompanyView();
            return;
        }

        var lastRound = CurrentGame!.CurrentRound;

        await ActInRound();

        while (
            !CurrentGameEnded &&
            // CurrentGame!.CurrentRound != CurrentGame.MaximumRounds &&
            CurrentGame!.CurrentRound == lastRound
        )
        {
            await Task.Delay(100);
        }

        if (!CurrentGameEnded)
        {
            await DisplayCompanyView();
        }
    }

    private async Task ActInRound()
    {
        Target.RemoveAll();

        var loadingDialog = new Dialog()
        {
            Width = 18,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Waiting for other players...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Target.Add(loadingDialog);

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
        };

        var request = httpClient.PostAsJsonAsync($"/rounds/{CurrentGame!.Rounds.MaxBy(r => r.Id)!.Id}/act", new
        {
            ActionType = CurrentRoundAction!.ToString(),
            ActionPayload = "{}",
            PlayerId = CurrentGame.Players.First(p => p.Name == PlayerName).Id
        });

        await request;
    }
}

public abstract class CurrentGameView : View
{
    public abstract Task Refresh(GameOverview game);
}

public class CurrentGameMainView : CurrentGameView
{
    private GameOverview Game;
    private readonly string PlayerName;

    private FrameView? Players;
    private FrameView? Status;
    private Button? StartButton;

    public EventHandler<HandledEventArgs> OnStart = (_, __) => { };

    public CurrentGameMainView(GameOverview game, string playerName)
    {
        Game = game;
        PlayerName = playerName;

        Width = Dim.Auto(DimAutoStyle.Auto);
        Height = Dim.Auto(DimAutoStyle.Auto);

        SetupPlayers();
        SetupStatus();
        SetupStartButton();
    }

    public override Task Refresh(GameOverview game)
    {
        Game = game;

        Remove(Players);
        Remove(Status);
        Remove(StartButton);

        SetupPlayers();
        SetupStatus();
        SetupStartButton();

        return Task.CompletedTask;
    }

    private void SetupPlayers()
    {
        Players = new()
        {
            Title = $"Players ({Game.PlayersCount}/{Game.MaximumPlayersCount})",
            X = 0,
            Y = 0,
            Width = 100,
            Height = 6 + Game.Players.Count,
            Enabled = false
        };

        Add(Players);

        var dataTable = new DataTable();

        dataTable.Columns.Add("Name");
        dataTable.Columns.Add("Company");
        dataTable.Columns.Add("Treasury");
        dataTable.Columns.Add("⭐");

        foreach (var player in Game.Players.ToList())
        {
            dataTable.Rows.Add([
                player.Name,
                player.Company.Name,
                $"{player.Company.Treasury} $",
                PlayerName == player.Name ? "⭐" : ""
            ]);
        }

        var dataTableSource = new DataTableSource(dataTable);

        var tableView = new TableView()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Game.Players.Max(p => p.Name.Length)
                + Game.Players.Max(p => p.Company.Name.Length)
                + Game.Players.Max(p => p.Company.Treasury.ToString().Length)
                + " $".Length
                + "⭐".Length
                + 6,
            Height = Dim.Fill(),
            Table = dataTableSource,
            Style = new TableStyle
            {
                ShowHorizontalBottomline = true,
                ExpandLastColumn = false,
            }
        };

        Players.Add(tableView);
    }

    private void SetupStatus()
    {
        Status = new()
        {
            Title = "Status",
            X = Pos.Left(Players!),
            Y = Pos.Bottom(Players!) + 2,
            Width = Players!.Width,
            Height = 3
        };

        Add(Status);

        var statusLabel = new Label() { Text = Game.Status is null ? "" : Game.Status, X = Pos.Center(), Y = Pos.Center() };

        Status.Add(statusLabel);
    }

    private void SetupStartButton()
    {
        if (PlayerName != Game.Players.First().Name) { return; }

        StartButton = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(Status!) + 2,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = Dim.Auto(DimAutoStyle.Text),
            Text = "Start Game",
        };

        StartButton.Accept += async (_, __) => await StartGame();

        Add(StartButton);

        StartButton.SetFocus();
    }

    private async Task StartGame()
    {
        RemoveAll();

        var loadingDialog = new Dialog()
        {
            Width = 18,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Starting game...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Add(loadingDialog);

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
        };

        var request = httpClient.PostAsJsonAsync($"/games/{Game.Id}/start", new { });
        var response = await request;

        if (!response.IsSuccessStatusCode)
        {
            await Refresh(Game);
        }
        else
        {
            OnStart(null, new HandledEventArgs());
        }
    }
}

public class CurrentGameCompanyView : CurrentGameView
{
    private GameOverview Game;
    private PlayerOverview CurrentPlayer;
    private readonly string PlayerName;
    public EventHandler<CurrentGameActionList.Action> OnRoundAction = (_, __) => { };

    private View? Header;
    private View? Body;
    private View? LeftBody;
    private View? RightBody;

    private FrameView? Player;
    private FrameView? Company;
    private FrameView? Treasury;
    private FrameView? Rounds;
    private FrameView? Employees;
    private FrameView? Consultants;
    private FrameView? CallForTenders;
    private FrameView? Actions;

    public CurrentGameCompanyView(GameOverview game, string playerName)
    {
        Game = game;
        PlayerName = playerName;
        CurrentPlayer = Game.Players.First(p => p.Name == PlayerName);

        Width = Dim.Auto(DimAutoStyle.Auto);
        Height = Dim.Auto(DimAutoStyle.Auto);

        SetupHeader();
        SetupBody();
    }

    public override Task Refresh(GameOverview game)
    {
        Game = game;
        CurrentPlayer = Game.Players.First(p => p.Name == PlayerName);

        RemoveHeader();
        RemoveBody();

        SetupHeader();
        SetupBody();

        return Task.CompletedTask;
    }

    private void SetupHeader()
    {
        Header = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Auto(DimAutoStyle.Content)
        };

        SetupPlayer();
        SetupCompany();
        SetupTreasury();
        SetupRounds();

        Add(Header);
    }

    private void SetupBody()
    {
        Body = new()
        {
            X = 0,
            Y = Pos.Bottom(Header!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        SetupLeftBody();
        SetupRightBody();

        Add(Body);
    }

    private void SetupLeftBody()
    {
        LeftBody = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(80),
            Height = Dim.Fill()
        };

        SetupEmployees();
        SetupConsultants();
        SetupCallForTenders();

        Body!.Add(LeftBody);
    }

    private void SetupRightBody()
    {
        RightBody = new()
        {
            X = Pos.Right(LeftBody!),
            Y = Pos.Top(LeftBody!),
            Width = Dim.Percent(20),
            Height = Dim.Fill()
        };

        SetupActions();

        Body!.Add(RightBody);
    }

    private void SetupPlayer()
    {
        Player = new()
        {
            Title = "Player",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = 0,
            Y = 0
        };

        Player.Add(new Label { Text = CurrentPlayer.Name });

        Header!.Add(Player);
    }

    private void SetupCompany()
    {
        Company = new()
        {
            Title = "Company",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Player!),
            Y = 0
        };

        Company.Add(new Label { Text = CurrentPlayer.Company.Name });

        Header!.Add(Company);
    }

    private void SetupTreasury()
    {
        Treasury = new()
        {
            Title = "Treasury",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Company!),
            Y = 0
        };

        Treasury.Add(new Label { Text = $"{CurrentPlayer.Company.Treasury} $" });

        Header!.Add(Treasury);
    }

    private void SetupRounds()
    {
        Rounds = new()
        {
            Title = "Rounds",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Treasury!),
            Y = 0
        };

        Rounds.Add(new Label { Text = $"{Game.CurrentRound}/{Game.MaximumRounds}" });

        Header!.Add(Rounds);
    }

    private void SetupEmployees()
    {
        Employees = new()
        {
            Title = "Employees",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(40)
        };

        var employeesTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        var employeesData = new List<TreeNode>();

        foreach (var employee in CurrentPlayer.Company.Employees.ToList())
        {
            var node = new TreeNode($"{employee.Name} | {employee.Salary} $");
            var skills = employee.Skills.ToList();

            foreach (var skill in skills)
            {
                node.Children.Add(new TreeNode($"{skill.Name} | {skill.Level}"));
            }

            employeesData.Add(node);
        }

        employeesTree.BorderStyle = LineStyle.None;
        employeesTree.AddObjects(employeesData);
        employeesTree.ExpandAll();

        Employees.Add(employeesTree);

        LeftBody!.Add(Employees);
    }

    private void SetupConsultants()
    {
        Consultants = new()
        {
            Title = "Consultants",
            X = Pos.Left(Employees!),
            Y = Pos.Bottom(Employees!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(30)
        };

        var consultantsTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        var consultantsData = new List<TreeNode>();

        foreach (var consultant in Game.Consultants.ToList())
        {
            var node = new TreeNode($"{consultant.Name} | {consultant.SalaryRequirement} $");
            var skills = consultant.Skills.ToList();

            foreach (var skill in skills)
            {
                node.Children.Add(new TreeNode($"{skill.Name} | {skill.Level}"));
            }

            consultantsData.Add(node);
        }

        consultantsTree.BorderStyle = LineStyle.None;
        consultantsTree.AddObjects(consultantsData);
        consultantsTree.ExpandAll();

        Consultants.Add(consultantsTree);

        LeftBody!.Add(Consultants);
    }

    private void SetupCallForTenders()
    {
        CallForTenders = new()
        {
            Title = "Call For Tenders",
            X = Pos.Left(Consultants!),
            Y = Pos.Bottom(Consultants!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(30)
        };

        LeftBody!.Add(CallForTenders);
    }

    private void SetupActions()
    {
        Actions = new()
        {
            Title = "Actions",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var actionList = new CurrentGameActionList()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        actionList.OpenSelectedItem += (_, selected) => { OnRoundAction(null, (CurrentGameActionList.Action) selected.Value); };

        Actions.Add(actionList);

        actionList.SetFocus();
        actionList.MoveHome();

        RightBody!.Add(Actions);
    }

    private void RemoveHeader()
    {
        Header!.Remove(Player);
        Header!.Remove(Company);
        Header!.Remove(Treasury);
        Header!.Remove(Rounds);

        Remove(Header);
    }

    private void RemoveBody()
    {
        LeftBody!.Remove(Employees);
        LeftBody!.Remove(Consultants);
        LeftBody!.Remove(CallForTenders);

        RightBody!.Remove(Actions);

        Body!.Remove(LeftBody);
        Body!.Remove(RightBody);

        Remove(Body);
    }
}

public class CurrentGameActionList : ListView
{
    public enum Action
    {
        SendEmployeeForTraining,
        ParticipateInCallForTenders,
        RecruitAConsultant,
        FireAnEmployee,
        PassMyTurn
    }

    private readonly CurrentGameActionListDataSource Actions = [
        Action.SendEmployeeForTraining,
        Action.ParticipateInCallForTenders,
        Action.RecruitAConsultant,
        Action.FireAnEmployee,
        Action.PassMyTurn
    ];

    public CurrentGameActionList()
    {
        Source = Actions;
    }
}

public class CurrentGameActionListDataSource : List<CurrentGameActionList.Action>, IListDataSource
{
    public int Length => Count;

    public bool SuspendCollectionChangedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, __) => { };

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public bool IsMarked(int item)
    {
        return false;
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        switch (item)
        {
            case (int) CurrentGameActionList.Action.SendEmployeeForTraining:
                driver.AddStr("Send Employee For Training");
                break;
            case (int) CurrentGameActionList.Action.ParticipateInCallForTenders:
                driver.AddStr("Participate In Call For Tenders");
                break;
            case (int) CurrentGameActionList.Action.RecruitAConsultant:
                driver.AddStr("Recruit A Consultant");
                break;
            case (int) CurrentGameActionList.Action.FireAnEmployee:
                driver.AddStr("Fire An Employee");
                break;
            case (int) CurrentGameActionList.Action.PassMyTurn:
                driver.AddStr("Pass My Turn");
                break;
        }
    }

    public void SetMark(int item, bool value) { }

    public IList ToList()
    {
        return this;
    }
}
