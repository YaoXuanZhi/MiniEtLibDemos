using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using ET;

namespace AppClientGUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private long lastTimeFrame = 0;
    public Session session;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        Entry.Init();
        Init.Start();

        CompositionTarget.Rendering += RenderFrame;
    }
    
    private void RenderFrame(object? sender, EventArgs e)
    {
        long currentTimeFrame = TimeHelper.ClientNow();
        if (currentTimeFrame - lastTimeFrame < 100) return;
        lastTimeFrame = currentTimeFrame;
        
        try
        {
            Init.Update();
            Init.LateUpdate();
            Init.FrameFinishUpdate();
        }
        catch (Exception e2)
        {
            Log.Error(e2);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Game.Close();
        base.OnExit(e);
    }
}