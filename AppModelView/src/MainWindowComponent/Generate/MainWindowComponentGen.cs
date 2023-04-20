using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using AppClientGUI;

namespace ET.Client
{
	[ComponentOf]
	public partial class MainWindowComponent : Entity, IAwake<MainWindowProxy>, IDestroy, ILoad, IUnload
	{
		public Window proxy;
		
		public Button btn_login;
		public Button btn_logout;
		public Button btn_gm;
		public TextBox textbox_message;
		public TextBox input_gm;
	}
}
