using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using Client.Records;

using Terminal.Gui;

namespace Client.Screens;
internal class StatisticsScreen
{
    private Window Target { get; }
    private readonly ListView StatsList = new();
    private readonly Button ReturnButton = new() { Text = "Return" };

    private GamesStatsResponse? Stats;
    private bool Loading = true;
    private bool Errored = false;
    private bool Returned = false;

    public StatisticsScreen(Window target) {
        Target = target;
    }

    public async Task Show()
    {
        await BeforeShow();
        await LoadStats();

        if (Errored)
        {
            ShowError();
            await WaitForReturn();
            await GoBack();
        }

        ShowStats();
        await WaitForReturn();
        await GoBack();
    }


    private Task BeforeShow()
    {
        Target.RemoveAll();
        Target.Title = $"{MainWindow.Title} - [Statistics]";
        return Task.CompletedTask;
    }

    private async Task LoadStats()
    {
        var loadingDialog = new Dialog() { Width = 20, Height = 3 };
        var loadingText = new Label() { Text = "Loading stats....", X = Pos.Center(), Y = Pos.Center() };
        loadingDialog.Add(loadingText);
        Target.Add(loadingDialog);

        try
        {
            using var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };

            using var httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}")
            };

            GamesStatsEnvelope? envelope = null;
            try
            {
                envelope = await httpClient.GetFromJsonAsync<GamesStatsEnvelope>("/games/stats");
            }
            catch
            {
                //#todo prevoir le cas d'erreure
            }

            if (envelope?.Stats is not null)
            {
                Stats = envelope.Stats;
            }
            else
            {
                Stats = await httpClient.GetFromJsonAsync<GamesStatsResponse>("/games/stats");
            }

            if (Stats is null)
                Errored = true;
        }
        catch
        {
            Errored = true;
        }
        finally
        {
            Loading = false;
            Target.Remove(loadingDialog);
        }
    }

    private void ShowStats()
    {
        if (Stats is null)
            return;

        var lines = new List<string>
        {
            AlignKV("Total games",            Stats.TotalGames.ToString()),
            AlignKV("Waiting",                Stats.WaitingGames.ToString()),
            AlignKV("In progress",            Stats.InProgressGames.ToString()),
            AlignKV("Finished",               Stats.FinishedGames.ToString()),
            AlignKV("Joinable",               Stats.JoinableGames.ToString()),
            AlignKV("Total players",          Stats.TotalPlayers.ToString()),
            AlignKV("Avg players / game",     Stats.AvgPlayersPerGame.ToString("0.00")),
            AlignKV("Total rounds (planned)", Stats.TotalRoundsPlanned.ToString()),
            AlignKV("Total rounds (played)",  Stats.TotalRoundsPlayed.ToString()),
            AlignKV("Avg rounds / game",      Stats.AvgRoundsPlayed.ToString("0.00")),
            AlignKV("Total consultants",      Stats.TotalConsultants.ToString()),
            AlignKV("Player with the highest treasury",      Stats.HighestTreasury.ToString())
        };

        var dataSource = new StatsListDataSource();
        dataSource.AddRange(lines);

        StatsList.Source = dataSource;

        StatsList.X = Pos.Center();
        StatsList.Y = Pos.Center();
        StatsList.Width = Math.Max(lines.Max(s => s.Length) + 2, 38);
        StatsList.Height = Math.Min(lines.Count, 15);

        ReturnButton.X = Pos.Center();
        ReturnButton.Y = Pos.Bottom(StatsList) + 1;
        ReturnButton.Accept += (_, __) => Returned = true;

        Target.Add(StatsList);
        Target.Add(ReturnButton);
    }






    private void ShowError()
    {
        var dlg = new Dialog()
        {
            Width = 30,
            Height = 5,
            Title = "Error"
        };
        dlg.Add(new Label()
        {
            Text = "Failed to load statistics.",
            X = Pos.Center(),
            Y = Pos.Center()
        });

        ReturnButton.X = Pos.Center();
        ReturnButton.Y = Pos.Bottom(dlg) - 1;
        ReturnButton.Accept += (_, __) => Returned = true;

        Target.Add(dlg);
        Target.Add(ReturnButton);
    }

    private async Task WaitForReturn()
    {
        while (!Returned)
            await Task.Delay(100);
    }

    private async Task GoBack()
    {
        var mainMenu = new MainMenuScreen(Target);
        await mainMenu.Show();
    }

    private static string AlignKV(string key, string value)
    {
        const int keyWidth = 22;
        if (key.Length > keyWidth)
            key = key[..keyWidth];
        return key.PadRight(keyWidth) + ": " + value;
    }
}


public class StatsListDataSource : List<string>, IListDataSource
{
    public int Length => Count;

    public bool SuspendCollectionChangedEvent
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, __) => { };

    public void Dispose() => GC.SuppressFinalize(this);

    public bool IsMarked(int item) => false;

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        var text = this[item];
        driver.AddStr(text);
    }

    public void SetMark(int item, bool value) { }

    public IList ToList() => this;
}
