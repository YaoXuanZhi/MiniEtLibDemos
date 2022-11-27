using System;
using System.ComponentModel;
using System.IO;
using ProtoBuf.Meta;

namespace ET
{
    public static class ProtobufHelper
    {
		public static void Init()
		{
		}
        
		static ProtobufHelper()
		{
		}
		
		public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			using MemoryStream stream = new MemoryStream(bytes, index, count);
			object o = ProtoBuf.Serializer.Deserialize(type, stream);
			if (o is ISupportInitialize supportInitialize)
			{
				supportInitialize.EndInit();
			}
			return o;
		}

        public static byte[] Serialize(object message)
		{
			using MemoryStream stream = new MemoryStream();
			ProtoBuf.Serializer.Serialize(stream, message);
			return stream.ToArray();
		}

        public static void Serialize(object message, Stream stream)
        {
            ProtoBuf.Serializer.Serialize(stream, message);
        }

        public static object Deserialize(Type type, Stream stream)
        {
	        object o = ProtoBuf.Serializer.Deserialize(type, stream);
	        if (o is ISupportInitialize supportInitialize)
	        {
		        supportInitialize.EndInit();
	        }
	        return o;
        }
    }
}