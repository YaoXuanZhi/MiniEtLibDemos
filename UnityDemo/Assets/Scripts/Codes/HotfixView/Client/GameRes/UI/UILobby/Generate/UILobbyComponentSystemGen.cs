using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
	[FriendOf(typeof(UILobbyComponent))]
	public static partial class UILobbyComponentSystem
	{
		public static void Binder(this UILobbyComponent self)
		{
			ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			self.GmApplyButton = rc.Get<Button>("GmApplyButton");
			self.LogoutButton = rc.Get<Button>("LogoutButton");
			self.GmInput = rc.Get<InputField>("GmInput");
			self.MessageText = rc.Get<Text>("MessageText");
		}
		[ObjectSystem]
		public class UILobbyComponentAwakeSystem : AwakeSystem<UILobbyComponent>
		{
			protected override void Awake(UILobbyComponent self)
			{
				self.Binder();
				self.AddListener();
				self.OnInit();
			}
		}

		[ObjectSystem]
		public class UILobbyComponentDestroySystem : DestroySystem<UILobbyComponent>
		{
			protected override void Destroy(UILobbyComponent self)
			{
				self.RemoveListener();
				self.OnDispose();
			}
		}


		[ObjectSystem]
		public class UILobbyComponentLoadSystem : LoadSystem<UILobbyComponent>
		{
			protected override void Load(UILobbyComponent self)
			{
				self.Load();
			}
		}

		public static void Load(this UILobbyComponent self)
		{
			self.RemoveListener();
			self.AddListener();
			self.OnShow();
		}

	}
}
