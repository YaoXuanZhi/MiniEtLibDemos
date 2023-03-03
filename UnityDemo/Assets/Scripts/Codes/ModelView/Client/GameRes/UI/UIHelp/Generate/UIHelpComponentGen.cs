using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public partial class UIHelpComponent : Entity, IAwake, IDestroy, ILoad
	{
		public Text TxtTips;
	}
}
