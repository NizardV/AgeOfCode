using System.Collections;
using System.Collections.Specialized;

using Terminal.Gui;

namespace Client.Screens;

public class MainMenuScreen(Window target)
{
    public Window Target { get; } = target;
    private readonly MainMenuActionList ActionList = new();

    public MainMenuActionList.Action? Action { get; private set; } = null;

    public async Task Show()
    {
        await BeforeShow();
        await SelectAction();

        var next = Action switch
        {
            MainMenuActionList.Action.CREATE_GAME => new CreateGameScreen(Target).Show(),
            MainMenuActionList.Action.JOIN_GAME => new JoinGameScreen(Target).Show(),
            MainMenuActionList.Action.STATISTICS => new StatisticsScreen(Target).Show(),
            MainMenuActionList.Action.END => new EndGameScreen(Target).Show(),
            MainMenuActionList.Action.QUIT => Task.Run(() => Application.RequestStop()),
            _ => Task.Run(() => Application.RequestStop())
        };

        await next;
    }

    private Task BeforeShow()
    {
        Target.RemoveAll();
        Target.Title = $"{MainWindow.Title} - [Main Menu]";
        return Task.CompletedTask;
    }

    private async Task SelectAction()
    {
        ActionList.X = ActionList.Y = Pos.Center();
        ActionList.Width = 13;
        ActionList.Height = 5;

        ActionList.OpenSelectedItem += (_, selected) => { Action = (MainMenuActionList.Action) selected.Value; };

        Target.Add(ActionList);

        ActionList.SetFocus();
        ActionList.MoveHome();

        while (Action is null) { await Task.Delay(100); };
    }
}

public class MainMenuActionList : ListView
{
    public enum Action
    {
        CREATE_GAME,
        JOIN_GAME,
        STATISTICS,
        END,
        QUIT
    }

    private readonly MainMenuActionListDataSource Actions = [
        Action.CREATE_GAME,
        Action.JOIN_GAME,
        Action.STATISTICS,
        Action.END,
        Action.QUIT
    ];

    public MainMenuActionList()
    {
        Source = Actions;
    }
}

public class MainMenuActionListDataSource : List<MainMenuActionList.Action>, IListDataSource
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
            case (int) MainMenuActionList.Action.CREATE_GAME:
                driver.AddStr("Create a game");
                break;
            case (int) MainMenuActionList.Action.JOIN_GAME:
                driver.AddStr("Join a game");
                break;
            case (int) MainMenuActionList.Action.STATISTICS:
                driver.AddStr("view stats");
                break;
            case (int) MainMenuActionList.Action.END:
                driver.AddStr("view endgame");
                break;
            case (int) MainMenuActionList.Action.QUIT:
                driver.AddStr("Quit");
                break;
        }
    }

    public void SetMark(int item, bool value) { }

    public IList ToList()
    {
        return this;
    }
}
