using System;
using System.Collections.Generic;
using System.Linq;

namespace ET.Client
{
	/// <summary>
	/// 管理Scene上的UI
	/// </summary>
	[FriendOf(typeof(UIComponent))]
	public static class UIComponentSystem
	{
		public class UIComponentDestroySystem : DestroySystem<UIComponent>
		{
			protected override void Destroy(UIComponent self)
			{
				self.DestroyAllWindow();
			}
		}

		public class UIComponentLoadSystem : LoadSystem<UIComponent>
		{
			protected override void Load(UIComponent self)
			{
				self.Load();
			}
		}

		public static void Load(this UIComponent self)
		{
			UIEventComponent.Instance.RemoveUIEvents();
			UIEventComponent.Instance.LoadUIEvents();
			self.TryOnShowAfterLoad();
		}
		
		static void TryOnShowAfterLoad(this UIComponent self)
		{
			foreach (var kvp in self.UIs)
			{
				var uiType = kvp.Key;
				if(kvp.Value.IsShow)
					UIEventComponent.Instance.OnShow(self, uiType, kvp.Value.uiData);
			}
		}

		public static async ETTask<UI> Create(this UIComponent self, string uiType, UILayer uiLayer, params object[] args)
		{
			UI ui = null;

			if (self.UIs.TryGetValue(uiType, out ui))
			{
				if (ui.IsShow)
				{
					return ui;
				}
				
				ui.GameObject.SetActive(true);
				
				ui.uiData = args;
				ui.IsShow = true;
				UIEventComponent.Instance.OnShow(self, uiType, args);
			}
			else
			{
				ui = await UIEventComponent.Instance.OnCreate(self, uiType, uiLayer);
				ui.uiData = args;
				self.UIs.Add(uiType, ui);

				ui.IsShow = true;
				UIEventComponent.Instance.OnShow(self, uiType, args);
			}
			
			if (uiLayer == UILayer.Mid)
			{
				var topStackWindow = self.GetTopStackWindow();
				if (topStackWindow != null && topStackWindow.Name != uiType)
				{
					self.WindowPop();
					self.Remove(topStackWindow.Name, true, true);
				}
			}
			
			if (!self.ShowList.Contains(ui))
			{
				self.ShowList.Add(ui);
			}
			
			if (uiLayer == UILayer.Mid)
			{
				self.WindowPush(uiType);
			}
			return ui;
		}

		public static void Remove(this UIComponent self, string uiType, bool isHide = false, bool isForceDestroy = false)
		{
			if (!self.UIs.TryGetValue(uiType, out UI ui))
			{
				return;
			}

			try
			{
				if (ui.IsDisposed)
				{
					return;
				}

				if (!ui.IsShow && !isForceDestroy)
				{
					return;
				}
				ui.IsShow = false;

				self.ShowList.Remove(ui);

				UIEventComponent.Instance.OnRemove(self, uiType);

				self.UIs.Remove(uiType);
				ui.Dispose();
			}
			catch (Exception e)
			{
				throw new Exception($"on remove ui error: {uiType}", e);
			}
		}

		public static UI Get(this UIComponent self, string name)
		{
			UI ui = null;
			self.UIs.TryGetValue(name, out ui);
			return ui;
		}
		
		public static K GetUI<K>(this UIComponent self, string uiTypeName) where K : Entity
		{
			UI ui = self.Get(uiTypeName);
			if (ui == null) return null;
			return ui.GetComponent<K>();
		}
		
		public static void Hide(this UIComponent self, string uiType)
		{
			self.Remove(uiType, true);
		}

		private static void WindowPush(this UIComponent self, string uiType)
		{
			if (self.WindowStack.Contains(uiType))
			{
				return;
			}
			self.WindowStack.Push(uiType);
		}
		
		private static void WindowPop(this UIComponent self)
		{
			self.WindowStack.Pop();
		}

		private static UI GetTopStackWindow(this UIComponent self)
		{
			if (self.WindowStack.TryPeek(out var uiType))
			{
				self.UIs.TryGetValue(uiType, out UI uiWindow);
				return uiWindow;
			}

			return null;
		}
		
		/// <summary>
		/// 删除所有界面
		/// </summary>
		public static void DestroyAllWindow(this UIComponent self)
		{
			foreach (var uiType in self.UIs.Keys.ToList())
			{
				self.Remove(uiType, true, true);
			}

			self.ShowList.Clear();
			self.WindowStack.Clear();
			self.UIs.Clear();
		}
	}
}