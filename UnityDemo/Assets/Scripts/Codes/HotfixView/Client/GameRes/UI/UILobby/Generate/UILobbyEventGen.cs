using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ET.Client;


namespace ET.Client
{
	[UIEvent(UIType.UILobby)]
	public class UILobbyEvent : AUIEvent
	{
		public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
		{
			string assetsName = $"Assets/Bundles/UI/{UIType.UILobby}.prefab";
			GameObject bundleGameObject = await uiComponent.DomainScene().GetComponent<ResourcesLoaderComponent>().LoadAssetAsync<GameObject>(assetsName);
			GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
			UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILobby, gameObject);
			var pnl = ui.AddComponent<UILobbyComponent>();
			pnl.OnShow();
			return ui;
		}

		public override void OnRemove(UIComponent uiComponent)
		{
		}

		public override void OnShow(UIComponent uiComponent, string uiType, params object[] args)
		{
			UILobbyComponent sample = uiComponent.GetUI<UILobbyComponent>(uiType);
			sample.OnShow(args);
		}

		public override void OnHide(UIComponent uiComponent, string uiType)
		{
			UILobbyComponent sample = uiComponent.GetUI<UILobbyComponent>(uiType);
			sample.OnHide();
		}
	}
}
