using System;

namespace App.Core.Master
{
	[Serializable]
	public abstract class SocketMsg
	{
		public int cmd;
		public int err;
	}
}