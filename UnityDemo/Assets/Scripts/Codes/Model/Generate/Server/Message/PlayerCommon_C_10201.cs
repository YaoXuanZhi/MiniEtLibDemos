using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
	[ResponseType(nameof(G2C_GmCommand))]
	[Message(PlayerCommon.C2G_GmCommand)]
	[ProtoContract]
	public partial class C2G_GmCommand: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string Command { get; set; }

		[ProtoMember(3)]
		public List<string> CommandArgs { get; set; }

	}

	[Message(PlayerCommon.G2C_GmCommand)]
	[ProtoContract]
	public partial class G2C_GmCommand: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(PlayerCommon.G2C_Message)]
	[ProtoContract]
	public partial class G2C_Message: ProtoObject, IMessage
	{
		[ProtoMember(1)]
		public string Message { get; set; }

	}

	public static class PlayerCommon
	{
		 public const ushort C2G_GmCommand = 10202;
		 public const ushort G2C_GmCommand = 10203;
		 public const ushort G2C_Message = 10204;
	}
}
