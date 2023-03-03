using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public partial class UILobbyComponent : Entity, IAwake, IDestroy, ILoad
	{
		public Button GmApplyButton;
		public Button LogoutButton;
		public InputField GmInput;
		public Text MessageText;
	}
}
