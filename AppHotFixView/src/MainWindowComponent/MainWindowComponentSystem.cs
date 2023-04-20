using System.Windows;
using System.Windows.Input;

namespace ET.Client
{
    public static partial class MainWindowComponentSystem
    {
#region 事件绑定/移除

		private static void AddListener(this MainWindowComponent self)
		{
			self.btn_login.Click += self.OnClickBtnLogin;
			self.btn_logout.Click += self.OnClickBtnLogout;
			self.btn_gm.Click += self.OnClickBtnGmApply;
			
			self.proxy.Loaded += self.OnShow;
			self.proxy.Closed += self.OnClose;
			self.proxy.MouseDown += self.OnMouseDown;
		}

		private static void OnMouseDown(this MainWindowComponent self, object sender, MouseButtonEventArgs args)
		{
			self.textbox_message.Text = $"{Mouse.GetPosition(self.proxy)}";
		}
		
		private static void RemoveListener(this MainWindowComponent self)
		{
			self.btn_login.Click -= self.OnClickBtnLogin;
			self.btn_logout.Click -= self.OnClickBtnLogout;
			self.btn_gm.Click -= self.OnClickBtnGmApply;
			
			self.proxy.Loaded -= self.OnShow;
			self.proxy.Closed -= self.OnClose;
			self.proxy.MouseDown -= self.OnMouseDown;

		}
#endregion

#region 界面显示/隐藏

		private static void OnInit(this MainWindowComponent self)
		{
		}

		private static void OnDispose(this MainWindowComponent self)
		{
		}

		public static void OnShow(this MainWindowComponent self, object sender, RoutedEventArgs args)
		{
			Console.WriteLine("显示UI");
			self.RefreshUi();
		}

		public static void OnClose(this MainWindowComponent self, object? sender, EventArgs args)
		{
			Console.WriteLine("关闭UI");
		}

		static void RefreshUi(this MainWindowComponent self)
		{
			self.textbox_message.Text = "Hello World";
			self.proxy.Title = "Simple Reload DemoUI";
			self.input_gm.Text = "error_test";
		}
#endregion

#region 事件回调

private static async void OnClickBtnLogin(this MainWindowComponent self, object sender, RoutedEventArgs e)
		{
			self.textbox_message.Text = $"用户登录";
			await TimerComponent.Instance.WaitAsync(2000);

			var account = "test";
			try
			{
				await LoginHelper.Login(Root.Instance.Scene, account, "");
				self.textbox_message.Text = $"用户已经登录完成";
			}
			catch (Exception exception)
			{
				var finalMessage = "服务器未响应：" + exception.Message;
				Console.WriteLine(exception);
				self.textbox_message.Text = finalMessage;
				await LoginHelper.Logout(Root.Instance.Scene);
			}
		}

		private static async void OnClickBtnLogout(this MainWindowComponent self, object sender, RoutedEventArgs e)
		{
			foreach (var row in AIConfigCategory.Instance.DataList)
			{
				Log.Debug($"{row.Name} => {row.Desc}");
			}
			
			await LoginHelper.Logout(Root.Instance.Scene);
			self.textbox_message.Text = "登出";
		}

		private static async void OnClickBtnGmApply(this MainWindowComponent self, object sender, RoutedEventArgs e)
		{
			NetClientComponent netClientComponent = Root.Instance.Scene.GetComponent<NetClientComponent>();
			if (netClientComponent == null)
			{
				self.textbox_message.Text = $"请先登录";
				await ETTask.CompletedTask;
				return;
			}
			foreach (var child in netClientComponent.Children.Values)
			{
				if (child is Session session)
				{
					self.textbox_message.Text = $"应用Gm {self.input_gm.Text}";
					var request = new C2G_GmCommand();
					var args = self.input_gm.Text.Split(" ");
					request.Command = args.First();
					request.CommandArgs = new List<string>();
					request.CommandArgs.Add(args.Last());

					await session.Call(request);
				}
			}
		}
#endregion
	}
}
