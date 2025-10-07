using Client.Screens;

using Microsoft.Extensions.Configuration;

using Terminal.Gui;

namespace Client;

public class Program
{
    static void Main()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"{Environment.CurrentDirectory}/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{AppContext.BaseDirectory}/appsettings.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();

        var apiConfig = configuration.GetSection("WebApiServer");
        WssConfig.WebApiServerScheme = apiConfig["Scheme"] is null ? "https" : apiConfig["Scheme"]!;
        WssConfig.WebApiServerDomain = apiConfig["Domain"] is null ? "localhost" : apiConfig["Domain"]!;
        WssConfig.WebApiServerPort = apiConfig["Port"] is null ? "7032" : apiConfig["Port"]!;

        var socketConfig = configuration.GetSection("WebSocketServer");
        WssConfig.WebSocketServerScheme = apiConfig["Scheme"] is null ? "wss" : apiConfig["Scheme"]!;
        WssConfig.WebSocketServerDomain = apiConfig["Domain"] is null ? "localhost" : apiConfig["Domain"]!;
        WssConfig.WebSocketServerPort = apiConfig["Port"] is null ? "7032" : apiConfig["Port"]!;

        Application.Init();
        Application.Invoke(async () =>
        {
            var mainWindow = (MainWindow) Application.Top;

            var titleScreen = new TitleScreen(mainWindow);
            await titleScreen.Show();

            var quit = false;
            while (!quit)
            {
                var mainMenuScreen = new MainMenuScreen(mainWindow);
                await mainMenuScreen.Show();
                quit = mainMenuScreen.Action == MainMenuActionList.Action.QUIT;
            }

            Application.RequestStop();
        });
        Application.Run<MainWindow>();
        Application.Shutdown();
    }
}

public static class WssConfig
{
    public static string WebApiServerScheme { get; set; } = "";
    public static string WebApiServerDomain { get; set; } = "";
    public static string WebApiServerPort { get; set; } = "";

    public static string WebSocketServerScheme { get; set; } = "";
    public static string WebSocketServerDomain { get; set; } = "";
    public static string WebSocketServerPort { get; set; } = "";
}

public class MainWindow : Window
{
    public static new string Title => $"Why So Serious ? ({Application.QuitKey} to quit)";

    public MainWindow()
    {
        Width = Dim.Fill();
        Height = Dim.Fill();
        ColorScheme = new ColorScheme(
            new Terminal.Gui.Attribute(Color.Blue, Color.Black),
            new Terminal.Gui.Attribute(Color.Black, Color.White),
            new Terminal.Gui.Attribute(Color.Black, Color.White),
            new Terminal.Gui.Attribute(Color.Blue, Color.Black),
            new Terminal.Gui.Attribute(Color.Black, Color.White)
        );
    }
}
