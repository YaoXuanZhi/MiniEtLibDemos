using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
	[FriendOf(typeof(UILoginComponent))]
	public static partial class UILoginComponentSystem
	{
		public static void Binder(this UILoginComponent self)
		{
			ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			self.Account = rc.Get<InputField>("Account");
			self.Password = rc.Get<InputField>("Password");
			self.LoginBtn = rc.Get<Button>("LoginBtn");
		}
		[ObjectSystem]
		public class UILoginComponentAwakeSystem : AwakeSystem<UILoginComponent>
		{
			protected override void Awake(UILoginComponent self)
			{
				self.Binder();
				self.AddListener();
				self.OnInit();
			}
		}

		[ObjectSystem]
		public class UILoginComponentDestroySystem : DestroySystem<UILoginComponent>
		{
			protected override void Destroy(UILoginComponent self)
			{
				self.RemoveListener();
				self.OnDispose();
			}
		}


		[ObjectSystem]
		public class UILoginComponentLoadSystem : LoadSystem<UILoginComponent>
		{
			protected override void Load(UILoginComponent self)
			{
				self.Load();
			}
		}

		public static void Load(this UILoginComponent self)
		{
			self.RemoveListener();
			self.AddListener();
			self.OnShow();
		}

	}
}
