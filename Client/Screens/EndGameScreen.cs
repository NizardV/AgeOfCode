using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Terminal.Gui;

namespace Client.Screens;
internal class EndGameScreen (Window target)
{

    public Window Target { get; } = target;

    public async Task Show()
    {
        await BeforeShow();
        await ShowVictory();
    }

    private Task BeforeShow()
    {
        Target.RemoveAll();
        Target.Title = "";
        return Task.CompletedTask;
    }

    private async Task ShowVictory()
    {

        var victoryTextLines = new List<string>
        {
            "             ███            █████                                 ",
            "             ▒▒▒            ▒▒███                                  ",
            " █████ █████ ████   ██████  ███████    ██████  ████████  █████ ████",
            "▒▒███ ▒▒███ ▒▒███  ███▒▒███▒▒▒███▒    ███▒▒███▒▒███▒▒███▒▒███ ▒███ ",
            " ▒███  ▒███  ▒███ ▒███ ▒▒▒   ▒███    ▒███ ▒███ ▒███ ▒▒▒  ▒███ ▒███ ",
            " ▒▒███ ███   ▒███ ▒███  ███  ▒███ ███▒███ ▒███ ▒███      ▒███ ▒███ ",
            "  ▒▒█████    █████▒▒██████   ▒▒█████ ▒▒██████  █████     ▒▒███████ ",
            "   ▒▒▒▒▒    ▒▒▒▒▒  ▒▒▒▒▒▒     ▒▒▒▒▒   ▒▒▒▒▒▒  ▒▒▒▒▒       ▒▒▒▒▒███ ",
            "                                                          ███ ▒███ ",
            "                                                         ▒▒██████  ",
            "                                                          ▒▒▒▒▒▒   "
        };

var labels = new List<Label>();
        for (int i = 0; i < victoryTextLines.Count; i++)
        {
            var label = new Label() // Correct: utiliser le constructeur par défaut
            {
                X = Pos.Center(),
                Y = Pos.Center() + (i - victoryTextLines.Count / 2), // Centrer verticalement
                Text = victoryTextLines[i] // Correct: définir le texte via la propriété Text
            };
            Target.Add(label);
            labels.Add(label);
        }



        foreach (var label in labels)
        {
            label.Visible = false; 
        }

        foreach (var label in labels)
        {
            label.Visible = true;
            await Task.Delay(150); 
        }

        await Task.Delay(10000); 
    }

    
    private async Task ShowDefeat()
    {

        var victoryTextLines = new List<string>
        {
            "   ▄██████▄     ▄████████   ▄▄▄▄███▄▄▄▄      ▄████████       ▄██████▄   ▄█    █▄     ▄████████    ▄████████ ",
            "  ███    ███   ███    ███ ▄██▀▀▀███▀▀▀██▄   ███    ███      ███    ███ ███    ███   ███    ███   ███    ███ ",
            "  ███    █▀    ███    ███ ███   ███   ███   ███    █▀       ███    ███ ███    ███   ███    █▀    ███    ███ ",
            " ▄███          ███    ███ ███   ███   ███  ▄███▄▄▄          ███    ███ ███    ███  ▄███▄▄▄      ▄███▄▄▄▄██▀ ",
            "▀▀███ ████▄  ▀███████████ ███   ███   ███ ▀▀███▀▀▀          ███    ███ ███    ███ ▀▀███▀▀▀     ▀▀███▀▀▀▀▀   ",
            "  ███    ███   ███    ███ ███   ███   ███   ███    █▄       ███    ███ ███    ███   ███    █▄  ▀███████████ ",
            "  ███    ███   ███    ███ ███   ███   ███   ███    ███      ███    ███ ███    ███   ███    ███   ███    ███ ",
            "  ████████▀    ███    █▀   ▀█   ███   █▀    ██████████       ▀██████▀   ▀██████▀    ██████████   ███    ███ ",
            "                                                                                                 ███    ███ "
        };

var labels = new List<Label>();
        for (int i = 0; i < victoryTextLines.Count; i++)
        {
            var label = new Label() // Correct: utiliser le constructeur par défaut
            {
                X = Pos.Center(),
                Y = Pos.Center() + (i - victoryTextLines.Count / 2), // Centrer verticalement
                Text = victoryTextLines[i] // Correct: définir le texte via la propriété Text
            };
            Target.Add(label);
            labels.Add(label);
        }



        foreach (var label in labels)
        {
            label.Visible = false; 
        }

        foreach (var label in labels)
        {
            label.Visible = true;
            await Task.Delay(150); 
        }

        await Task.Delay(10000); 
    }

}
