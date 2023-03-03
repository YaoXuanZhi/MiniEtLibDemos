using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
	[FriendOf(typeof(UIHelpComponent))]
	public static partial class UIHelpComponentSystem
	{
		public static void Binder(this UIHelpComponent self)
		{
			ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			self.TxtTips = rc.Get<Text>("TxtTips");
		}
		[ObjectSystem]
		public class UIHelpComponentAwakeSystem : AwakeSystem<UIHelpComponent>
		{
			protected override void Awake(UIHelpComponent self)
			{
				self.Binder();
				self.AddListener();
				self.OnInit();
			}
		}

		[ObjectSystem]
		public class UIHelpComponentDestroySystem : DestroySystem<UIHelpComponent>
		{
			protected override void Destroy(UIHelpComponent self)
			{
				self.RemoveListener();
				self.OnDispose();
			}
		}


		[ObjectSystem]
		public class UIHelpComponentLoadSystem : LoadSystem<UIHelpComponent>
		{
			protected override void Load(UIHelpComponent self)
			{
				self.Load();
			}
		}

		public static void Load(this UIHelpComponent self)
		{
			self.RemoveListener();
			self.AddListener();
			self.OnShow();
		}

	}
}
