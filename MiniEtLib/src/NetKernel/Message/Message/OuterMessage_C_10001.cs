using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
	[ResponseType(nameof(G2C_Ping))]
	[Message(OuterMessage.C2G_Ping)]
	[ProtoContract]
	public partial class C2G_Ping: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Ping)]
	[ProtoContract]
	public partial class G2C_Ping: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public long Time { get; set; }

	}

	[ResponseType(nameof(R2C_Login))]
	[Message(OuterMessage.C2R_Login)]
	[ProtoContract]
	public partial class C2R_Login: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string Account { get; set; }

		[ProtoMember(3)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.R2C_Login)]
	[ProtoContract]
	public partial class R2C_Login: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public string Address { get; set; }

		[ProtoMember(5)]
		public long Key { get; set; }

		[ProtoMember(6)]
		public long GateId { get; set; }

	}

	[ResponseType(nameof(G2C_LoginGate))]
	[Message(OuterMessage.C2G_LoginGate)]
	[ProtoContract]
	public partial class C2G_LoginGate: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

		[ProtoMember(3)]
		public long GateId { get; set; }

	}

	[Message(OuterMessage.G2C_LoginGate)]
	[ProtoContract]
	public partial class G2C_LoginGate: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public long PlayerId { get; set; }

	}

	[ResponseType(nameof(G2C_Benchmark))]
	[Message(OuterMessage.C2G_Benchmark)]
	[ProtoContract]
	public partial class C2G_Benchmark: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Benchmark)]
	[ProtoContract]
	public partial class G2C_Benchmark: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2C_GmCommand))]
	[Message(OuterMessage.C2G_GmCommand)]
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

	[Message(OuterMessage.G2C_GmCommand)]
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

	[Message(OuterMessage.G2C_Message)]
	[ProtoContract]
	public partial class G2C_Message: ProtoObject, IMessage
	{
		[ProtoMember(1)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2C_CreateRole))]
	[Message(OuterMessage.C2G_CreateRole)]
	[ProtoContract]
	public partial class C2G_CreateRole: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string Name { get; set; }

	}

	[Message(OuterMessage.G2C_CreateRole)]
	[ProtoContract]
	public partial class G2C_CreateRole: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2C_RoleLogin))]
	[Message(OuterMessage.C2G_RoleLogin)]
	[ProtoContract]
	public partial class C2G_RoleLogin: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_RoleLogin)]
	[ProtoContract]
	public partial class G2C_RoleLogin: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2C_RoleLogout))]
	[Message(OuterMessage.C2G_RoleLogout)]
	[ProtoContract]
	public partial class C2G_RoleLogout: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_RoleLogout)]
	[ProtoContract]
	public partial class G2C_RoleLogout: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	public static class OuterMessage
	{
		 public const ushort C2G_Ping = 10002;
		 public const ushort G2C_Ping = 10003;
		 public const ushort C2R_Login = 10004;
		 public const ushort R2C_Login = 10005;
		 public const ushort C2G_LoginGate = 10006;
		 public const ushort G2C_LoginGate = 10007;
		 public const ushort C2G_Benchmark = 10008;
		 public const ushort G2C_Benchmark = 10009;
		 public const ushort C2G_GmCommand = 10010;
		 public const ushort G2C_GmCommand = 10011;
		 public const ushort G2C_Message = 10012;
		 public const ushort C2G_CreateRole = 10013;
		 public const ushort G2C_CreateRole = 10014;
		 public const ushort C2G_RoleLogin = 10015;
		 public const ushort G2C_RoleLogin = 10016;
		 public const ushort C2G_RoleLogout = 10017;
		 public const ushort G2C_RoleLogout = 10018;
	}
}
