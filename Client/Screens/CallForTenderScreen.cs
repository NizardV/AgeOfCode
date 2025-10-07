using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;

using Terminal.Gui;

namespace Client.Screens;
public class CallForTenderScreen(Window target, int gameId, string playerName)
{
    private Window Target { get; } = target;
    private int GameId { get; } = gameId;
    private string PlayerName { get; } = playerName;

    public async Task Show()
    {
        await BeforeShow();
        await DisplayForm();
    }
    private Task BeforeShow()
    {
        Target.RemoveAll();
        Target.Title = $"{MainWindow.Title} - [Call For Tender]";
        return Task.CompletedTask;
    }
    private Task DisplayForm()
    {
        var label = new Label()
        {
            Text = "Call For Tender Screen - To be implemented",
            X = Pos.Center(),
            Y = Pos.Center()
        };
        var backButton = new Button
        {
            Text = "Back",
            X = Pos.Center(),
            Y = Pos.Bottom(label) + 2,
        };
        // Go back to the appropriate screen
        backButton.Accept += async (_, __) =>
        {
            if (GameId > 0 && !string.IsNullOrWhiteSpace(PlayerName))
            {
                var currentGameScreen = new CurrentGameScreen(Target, GameId, PlayerName);
                await currentGameScreen.Show();
            }
            else
            {
                var mainMenuScreen = new MainMenuScreen(Target);
                await mainMenuScreen.Show();
            }
        };
        Target.Add(label, backButton);
        return Task.CompletedTask;
    }
}
