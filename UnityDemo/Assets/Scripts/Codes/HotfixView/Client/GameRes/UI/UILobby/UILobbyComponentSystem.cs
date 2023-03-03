using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[FriendOf(typeof(UILobbyComponent))]
	public static partial class UILobbyComponentSystem
	{
#region 事件绑定/移除
		public static void AddListener(this UILobbyComponent self)
		{
			self.GmApplyButton.onClick.AddListener(self.OnClickGmApplyButton);
			self.LogoutButton.onClick.AddListener(self.OnClickLogoutButton);
		}

		public static void RemoveListener(this UILobbyComponent self)
		{
			self.GmApplyButton.onClick.RemoveAllListeners();
			self.LogoutButton.onClick.RemoveAllListeners();
		}
#endregion

#region 界面显示/隐藏
		public static void OnInit(this UILobbyComponent self)
		{
		}

		public static void OnDispose(this UILobbyComponent self)
		{
		}

		public static void OnShow(this UILobbyComponent self, params object[] args)
		{
			Log.Debug("显示UI");
			self.RefreshUi();
		}

		public static void OnHide(this UILobbyComponent self)
		{
			Log.Debug("关闭UI");
		}
#endregion

#region 事件回调
		public static async void OnClickGmApplyButton(this UILobbyComponent self)
		{
			var clientScene = self.ClientScene();
			NetClientComponent netClientComponent = clientScene.GetComponent<NetClientComponent>();
			if (netClientComponent == null)
			{
				self.MessageText.text = $"请先登录";
				await ETTask.CompletedTask;
				return;
			}

			foreach (var child in netClientComponent.Children.Values)
			{
				if (child is Session session)
				{
					self.MessageText.text = $"应用Gm {self.GmInput.text}";
					var request = new C2G_GmCommand();
					var args = self.GmInput.text.Split(" ");
					request.Command = args.First();
					request.CommandArgs = new List<string>();
					request.CommandArgs.Add(args.Last());
					await session.Call(request);
				}
			}
		}

		public static async void OnClickLogoutButton(this UILobbyComponent self)
		{
			var clientScene = self.ClientScene();
			await LoginHelper.Logout(clientScene);
			await UIHelper.Remove(clientScene, UIType.UILobby);
			EventSystem.Instance.PublishAsync(clientScene, new EventType.AppStartInitFinish()).Coroutine();
			Log.Debug("配置读取示例");
			foreach (var row in AIConfigCategory.Instance.DataList)
			{
				Log.Debug($"======> {row.Name} {row.Desc} {row.Id}");
			}
		}
#endregion

#region 其它方法
		static void RefreshUi(this UILobbyComponent self)
		{
			self.MessageText.text = "Hello World";
			self.GmInput.text = "error_test";
		}
#endregion
	}
}