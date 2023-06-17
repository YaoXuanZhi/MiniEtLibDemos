using System.Collections.Generic;

namespace ET.Client
{
	/// <summary>
	/// 管理Scene上的UI
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class UIComponent: Entity, IAwake, IDestroy, ILoad
	{
		public Dictionary<string, UI> UIs = new Dictionary<string, UI>();
		public List<UI> ShowList = new List<UI>();
		public Stack<string> WindowStack = new Stack<string>();
	}
}