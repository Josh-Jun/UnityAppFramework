using System;

internal class SocketPackage
{
	public int headLength = 4;

	public byte[] headBuffer = null;

	public int headIndex = 0;

	public int bodyLength = 0;

	public byte[] bodyBuffer = null;

	public int bodyIndex = 0;

	public SocketPackage()
	{
		headBuffer = new byte[4];
	}

	public void InitBodyBuffer()
	{
		bodyLength = BitConverter.ToInt32(headBuffer, 0);
		bodyBuffer = new byte[bodyLength];
	}

	public void ResetData()
	{
		headIndex = 0;
		bodyLength = 0;
		bodyBuffer = null;
		bodyIndex = 0;
	}
}