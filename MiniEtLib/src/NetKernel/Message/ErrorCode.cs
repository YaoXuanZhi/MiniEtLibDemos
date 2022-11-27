namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;

        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-109999是Core层的错误
        
        // 110000以下的错误请看ErrorCore.cs
        
        // 这里配置逻辑层的错误码
        // 110000 - 200000是抛异常的错误
        // 200001以上不抛异常
        
        
        public const int ERR_INVALID_OPERATION = 200003; // 操作有误
        public const int ERR_INVALID_PARAMETER = 200004; // 参数有误
        public const int ERR_GATE_IS_ONLINE = 200204;

        public const int ERR_GATE_NOT_LOGIN = 200201;
        public const int ERR_CENTER_NOT_ONLINE = 200202;
        public const int ERR_CENTER_NOT_OFFLINE = 200203;
        
        public const int ERR_PROTO_VERSION_ERROR = 200005; // 客户端和服务器协议版本不一致
    }
}