using System.Windows;
using ET;
using ET.Client;
namespace AppClientGUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    MainWindowProxy _mainWindowProxy;
    public MainWindow()
    {
        InitializeComponent();
        _mainWindowProxy = new MainWindowProxy(this);
    }

    private void Button_ReloadHotFix_Click(object sender, RoutedEventArgs e)
    {
        //note 热重载逻辑不允许放在HotFix程序集里
        EventSystem.Instance.Unload();
        CodeLoader.Instance.LoadHotfix();
        EventSystem.Instance.Load();
        Log.Console("重载Dll成功");
    }
}