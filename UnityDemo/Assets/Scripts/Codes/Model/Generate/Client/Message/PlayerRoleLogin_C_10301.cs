using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
/// <summary>
/// 创建角色
/// </summary>
	[ResponseType(nameof(G2C_CreateRole))]
	[Message(PlayerRoleLogin.C2G_CreateRole)]
	[ProtoContract]
	public partial class C2G_CreateRole: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string Name { get; set; }

	}

	[Message(PlayerRoleLogin.G2C_CreateRole)]
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

/// <summary>
/// 角色登录
/// </summary>
	[ResponseType(nameof(G2C_RoleLogin))]
	[Message(PlayerRoleLogin.C2G_RoleLogin)]
	[ProtoContract]
	public partial class C2G_RoleLogin: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

	}

	[Message(PlayerRoleLogin.G2C_RoleLogin)]
	[ProtoContract]
	public partial class G2C_RoleLogin: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public int Error { get; set; }

		[ProtoMember(2)]
		public string Message { get; set; }

	}

	[ResponseType(nameof(G2C_RoleLogout))]
	[Message(PlayerRoleLogin.C2G_RoleLogout)]
	[ProtoContract]
	public partial class C2G_RoleLogout: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(PlayerRoleLogin.G2C_RoleLogout)]
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

	public static class PlayerRoleLogin
	{
		 public const ushort C2G_CreateRole = 10302;
		 public const ushort G2C_CreateRole = 10303;
		 public const ushort C2G_RoleLogin = 10304;
		 public const ushort G2C_RoleLogin = 10305;
		 public const ushort C2G_RoleLogout = 10306;
		 public const ushort G2C_RoleLogout = 10307;
	}
}
