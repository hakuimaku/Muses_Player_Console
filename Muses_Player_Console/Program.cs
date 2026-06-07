using Terminal.Gui.ViewBase;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;

namespace Muses_Player_Console;

class Program
{
    public static void Main(string[] args)
    {
        MusesService musesService = new MusesService();
        
        using (IApplication app = Application.Create().Init())
        {
            TuiDisplay tuiDisplay = new TuiDisplay(app, musesService);
            app.Run(tuiDisplay);
        }
    }
}