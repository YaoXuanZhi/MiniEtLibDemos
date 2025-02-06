using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[FriendOf(typeof(UILoginComponent))]
	public static partial class UILoginComponentSystem
	{
#region 事件绑定/移除
		public static void AddListener(this UILoginComponent self)
		{
			self.LoginBtn.onClick.AddListener(self.OnClickLoginBtn);
		}

		public static void RemoveListener(this UILoginComponent self)
		{
			self.LoginBtn.onClick.RemoveAllListeners();
		}
#endregion

#region 界面显示/隐藏
		public static void OnInit(this UILoginComponent self)
		{
		}

		public static void OnDispose(this UILoginComponent self)
		{
		}

		public static void OnShow(this UILoginComponent self, params object[] args)
		{
		}

		public static void OnHide(this UILoginComponent self)
		{
		}
#endregion

#region 事件回调
		public static async void OnClickLoginBtn(this UILoginComponent self)
		{
			Log.Debug("模拟登录");
			var account = "test";
			try
			{
				await LoginHelper.Login(self.ClientScene(), account, "");
				self.Account.text = $"用户已经登录完成";
			}
			catch (Exception exception)
			{
				var finalMessage = "服务器未响应：" + exception.Message;
				Log.Error(exception);
				self.Account.text = finalMessage;
				await LoginHelper.Logout(self.ClientScene());
			}
		}
#endregion
	}
}