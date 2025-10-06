using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Json;

using Client.Records;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Terminal.Gui;

namespace Client.Screens;

public class JoinGameScreen(Window target)
{
    private Window Target { get; } = target;
    private readonly ListView GamesList = new();
    private readonly Button ListReturnButton = new() { Text = "Return" };
    private readonly JoinGameForm Form = new();

    private ICollection<JoinableGame> _joinableGames = [];
    private ICollection<JoinableGame> JoinableGames
    {
        get => _joinableGames;
        set { _joinableGames = value; ReloadListData(); }
    }
    private int? GameId = null;

    private bool Loading = true;
    private bool Errored = false;
    private bool ListReturned = false;
    private bool FormReturned = false;
    private bool FormSubmitted = false;

    public async Task Show()
    {
        await BeforeShow();
        await LoadGames();
        await SelectGame();

        if (ListReturned)
        {
            await Return();
            return;
        }

        await DisplayForm();

        if (FormReturned)
        {
            await SelectGame();
            return;
        }

        await JoinGame();

        if (Errored)
        {
            FormSubmitted = false;
            FormReturned = false;

            await DisplayForm(true);

            if (FormReturned)
            {
                await SelectGame();
                return;
            }

            return;
        }

        var currentGameScreen = new CurrentGameScreen(Target, (int) GameId!, Form.PlayerNameField.Text.ToString()!);

        await currentGameScreen.Show();
    }

    private Task BeforeShow()
    {
        Target.RemoveAll();
        Target.Title = $"{MainWindow.Title} - [Join Game]";
        return Task.CompletedTask;
    }

    private async Task Return()
    {
        var mainMenuScreen = new MainMenuScreen(Target);
        await mainMenuScreen.Show();
    }

    private void ReloadListData()
    {
        var dataSource = new JoinGameChoiceListDataSource();
        dataSource.AddRange(JoinableGames);
        GamesList.Source = dataSource;
    }

    private async Task LoadGames()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{WssConfig.WebSocketServerScheme}://{WssConfig.WebSocketServerDomain}:{WssConfig.WebSocketServerPort}/main"), opts =>
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

        hubConnection.On<ICollection<JoinableGame>>("JoinableGamesListUpdated", data =>
        {
            JoinableGames = data;
            Loading = false;
        });

        await hubConnection.StartAsync();

        var loadingDialog = new Dialog()
        {
            Width = 18,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Loading games...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Target.Add(loadingDialog);

        while (Loading) { await Task.Delay(100); }

        Target.Remove(loadingDialog);
    }

    private async Task SelectGame()
    {
        GamesList.X = GamesList.Y = Pos.Center();
        GamesList.Width = JoinableGames.Max(g => g.Name.Length);
        GamesList.Height = Math.Min(JoinableGames.Count, 20);

        ListReturnButton.X = Pos.Center();
        ListReturnButton.Y = Pos.Bottom(GamesList) + 1;
        ListReturnButton.Accept += (_, __) => ListReturned = true;

        Target.Add(GamesList);
        Target.Add(ListReturnButton);

        GamesList.OpenSelectedItem += (_, selected) => GameId = ((JoinableGame) selected.Value).Id;
        GamesList.SetFocus();
        GamesList.MoveHome();

        while (GameId is null && !ListReturned) { await Task.Delay(100); };
    }

    private async Task DisplayForm(bool errored = false)
    {
        Target.RemoveAll();

        Form.OnReturn = (_, __) => FormReturned = true;
        Form.OnSubmit = (_, __) => FormSubmitted = true;

        Form.FormView.X = Form.FormView.Y = Pos.Center();
        Form.FormView.Width = 50;
        Form.FormView.Height = 12;

        Target.Add(Form.FormView);

        while (!FormReturned && !FormSubmitted)
        {
            await Task.Delay(100);
        }
    }

    private async Task JoinGame()
    {
        Target.Remove(Form.FormView);

        var loadingDialog = new Dialog()
        {
            Width = 17,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Joining game...",
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

        var playerName = Form.PlayerNameField.Text.ToString();
        var companyName = Form.CompanyNameField.Text.ToString();
        var type = (int)Form.SelectedCompanyType;

        var requestBody = new { playerName, companyName, type };

        var response = await httpClient.PostAsJsonAsync($"/games/{GameId}/join", requestBody);


        if (!response.IsSuccessStatusCode)
        {
            Errored = true;
        }
    }
}

public class JoinGameChoiceListDataSource : List<JoinableGame>, IListDataSource
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
        var game = this[item];
        driver.AddStr($"{game.Name} ({game.PlayersCount}/{game.MaximumPlayersCount})");
    }

    public void SetMark(int item, bool value) { }

    public IList ToList()
    {
        return this;
    }
}

public class JoinGameForm
{
    private EventHandler<HandledEventArgs> _onSubmit = (_, __) => { };
    private EventHandler<HandledEventArgs> _onReturn = (_, __) => { };

    public EventHandler<HandledEventArgs> OnSubmit
    {
        get => _onSubmit;
        set { SubmitButton.Accept -= _onSubmit; SubmitButton.Accept += value; _onSubmit = value; }
    }
    public EventHandler<HandledEventArgs> OnReturn
    {
        get => _onReturn;
        set { ReturnButton.Accept -= _onReturn; ReturnButton.Accept += value; _onReturn = value; }
    }

    public View FormView { get; }
    public Button SubmitButton { get; }
    public Button ReturnButton { get; }
    public Label PlayerNameLabel { get; }
    public Label CompanyNameLabel { get; }
    public Label TypeLabel { get; }
    public TextField PlayerNameField { get; }
    public TextField CompanyNameField { get; }
    public RadioGroup TypeGroup { get; }

    public enum CompanyType { Startup = 0, SME = 1, Corporation = 2, Enterprise = 3 }
    public CompanyType SelectedCompanyType => (CompanyType)TypeGroup.SelectedItem;

    public JoinGameForm()
    {
        PlayerNameLabel = new Label { X = 0, Y = 0, Width = 20, Text = "Player name :" };
        CompanyNameLabel = new Label { X = Pos.Left(PlayerNameLabel), Y = Pos.Bottom(PlayerNameLabel) + 1, Width = 20, Text = "Company name :" };

        PlayerNameField = new TextField { X = Pos.Right(PlayerNameLabel), Y = Pos.Top(PlayerNameLabel), Width = Dim.Fill(), Text = "" };
        CompanyNameField = new TextField { X = Pos.Right(CompanyNameLabel), Y = Pos.Top(CompanyNameLabel), Width = Dim.Fill(), Text = "" };

        TypeLabel = new Label { X = Pos.Left(CompanyNameLabel), Y = Pos.Bottom(CompanyNameLabel) + 1, Width = 20, Text = "Company Type" };
        TypeGroup = new RadioGroup
        {
            X = Pos.Left(TypeLabel),
            Y = Pos.Bottom(TypeLabel),
            Width = 40,
            Height = 4,
            RadioLabels = new[] { "Startup", "SME", "Corporation", "Enterprise" },
            SelectedItem = 0
        };

        SubmitButton = new Button
        {
            X = Pos.Center(),
            Y = Pos.Bottom(TypeGroup) + 1,
            Text = "Submit",
            IsDefault = true
        };

        ReturnButton = new Button
        {
            X = Pos.Right(SubmitButton),
            Y = Pos.Bottom(TypeGroup) + 1,
            Text = "Return",
            IsDefault = false
        };

        SubmitButton.Accept += OnSubmit;
        ReturnButton.Accept += OnReturn;


        FormView = new View { Width = Dim.Fill(), Height = Dim.Fill() };
        FormView.Add(
            PlayerNameLabel, CompanyNameLabel,
            PlayerNameField, CompanyNameField,
            TypeLabel, TypeGroup,
            SubmitButton,ReturnButton
        );
    }
}
