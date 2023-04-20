using System.Windows;
using System.Windows.Controls;
using ET;
using ET.Client;
namespace AppClientGUI;

/// <summary>
/// 这个类之所以存在是因为wpf框架下，xaml文件中的控件被设为internal了，只能在当前程序集下访问，
/// 所以需要通过这个代理类绕开访问限制，这个问题还导致了ModelView无法被单独拆分成独立程序集，
/// 其实还有个更粗暴的方式是通过反射来访问该程序集的内部变量，看各位需要吧，这里只是提供了一个技术思路
/// 虽然蛋疼，但是可以结合c# source generator来生成这类绑定代码
/// 好消息是，这个问题在Unity上并不存在
/// </summary>
public class MainWindowProxy
{
    public Window owner;
    public Button Button_Login;
    public Button Button_Logout;
    public Button Button_GmApply;
    public TextBox Text_Board;
    public TextBox Input_Gm;

    public MainWindowProxy(MainWindow mainWindow)
    {
        owner = mainWindow;
        Button_Login = mainWindow.Button_Login;
        Button_Logout = mainWindow.Button_Logout;
        Button_GmApply = mainWindow.Button_GmApply;
        Text_Board = mainWindow.Text_Board;
        Input_Gm = mainWindow.Input_Gm;
        
        Root.Instance.Scene.AddComponent<MainWindowComponent, MainWindowProxy>(this);
    }
}