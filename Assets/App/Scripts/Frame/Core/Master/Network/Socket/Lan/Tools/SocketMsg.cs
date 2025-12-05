using System;
using System.Collections.Generic;

namespace App.Core.Master
{
	[Serializable]
	public abstract class SocketMsg
	{
		public int cmd;
	}

	/// <summary>消息包 </summary>
	[Serializable]
	public class MsgPackage
	{
		public SocketTcpBase session; //同一个回话
		public SocketMsg msg;

		public MsgPackage(SocketTcpBase session, SocketMsg msg)
		{
			this.session = session;
			this.msg = msg;
		}
	}
     /// <summary> 推送消息 </summary>
     [Serializable]
     public class PushMsg : SocketMsg
     {
         public List<string> clients = new(); //客户端列表, 长度为0则推送给所有
         public string eventName; //事件名
         public string data; //参数数组
     }

     public enum LAN_CMD
     {
         None = 0, //默认
         SCMsg_All = 1, //服务端=>客户端(all)
         SCMsg_One = 2, //服务端=>客户端(one)
         SCMsg_List = 3, //服务端=>客户端(list)
         SCMsg_UnSelf = 4, //服务端=>客户端(!self)
         CSMsg = 5, //客户端<=>服务端
         CCMsg_All = 6, //客户端=>客户端(all)
         CCMsg_One = 7, //客户端=>客户端(one)
         CCMsg_List = 8, //客户端=>客户端(list)
         CCMsg_UnSelf = 9, //客户端=>客户端(!self)
     }
}