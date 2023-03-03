namespace ET.Server
{
	[ComponentOf(typeof(Session))]
	public class SessionPlayerComponent : Entity, IAwake, IDestroy
	{
		public long PlayerId { get; set; }
		
		public Player Player => this.DomainScene().GetComponent<PlayerComponent>().GetChild<Player>(this.PlayerId);

		public Player OnlinePlayer
		{
			get
			{
				var player = Player;
				return player is { IsOnline: true } ? player : null;
			}
		}
	}
}