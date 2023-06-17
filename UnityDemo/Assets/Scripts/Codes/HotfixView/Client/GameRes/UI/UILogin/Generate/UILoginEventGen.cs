using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ET.Client;


namespace ET.Client
{
	[UIEvent(UIType.UILogin)]
	public class UILoginEvent : AUIEvent
	{
		public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer)
		{
			await uiComponent.DomainScene().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILogin.StringToAB());
			GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILogin.StringToAB(), UIType.UILogin);
			GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
			UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILogin, gameObject);
			var pnl = ui.AddComponent<UILoginComponent>();
			pnl.OnShow();
			return ui;
		}

		public override void OnRemove(UIComponent uiComponent)
		{
		}

		public override void OnShow(UIComponent uiComponent, string uiType, params object[] args)
		{
			UILoginComponent sample = uiComponent.GetUI<UILoginComponent>(uiType);
			sample.OnShow(args);
		}

		public override void OnHide(UIComponent uiComponent, string uiType)
		{
			UILoginComponent sample = uiComponent.GetUI<UILoginComponent>(uiType);
			sample.OnHide();
		}
	}
}
