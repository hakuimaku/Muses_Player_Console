using Terminal.Gui.App;

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