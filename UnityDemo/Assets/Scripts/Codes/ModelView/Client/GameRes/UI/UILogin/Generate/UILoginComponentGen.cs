using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public partial class UILoginComponent : Entity, IAwake, IDestroy, ILoad
	{
		public InputField Account;
		public InputField Password;
		public Button LoginBtn;
	}
}
