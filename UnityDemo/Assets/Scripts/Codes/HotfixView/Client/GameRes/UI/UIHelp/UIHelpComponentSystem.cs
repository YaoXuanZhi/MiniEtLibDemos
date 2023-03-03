using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[FriendOf(typeof(UIHelpComponent))]
	public static partial class UIHelpComponentSystem
	{
#region 事件绑定/移除
		public static void AddListener(this UIHelpComponent self)
		{
		}

		public static void RemoveListener(this UIHelpComponent self)
		{
		}
#endregion

#region 界面显示/隐藏
		public static void OnInit(this UIHelpComponent self)
		{
			Log.Debug("-----> OnInit");
		}

		public static void OnDispose(this UIHelpComponent self)
		{
			Log.Debug("-----> OnDispose");
		}

		public static void OnShow(this UIHelpComponent self, params object[] args)
		{
			Log.Debug("-----> OnShow 222");
		}

		public static void OnHide(this UIHelpComponent self)
		{
		}
#endregion
	}
}