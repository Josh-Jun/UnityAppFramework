using System;

namespace AppFramework.Network.Lan
{
	[Serializable]
	public abstract class SocketMsg
	{
		public int cmd;
		public int err;
	}
}