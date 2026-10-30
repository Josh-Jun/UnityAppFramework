/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年3月12 8:53
 * function    : 
 * ===============================================
 * */
using App.Runtime.UDP;

public enum AsrChannel
{
    AL_YUN,
    XF_YUN,
}

public static partial class Global
{
    public const bool RunUDPClient = false;
    public const AsrChannel WSAsrChannel = AsrChannel.AL_YUN;
    public static ServerData ServerData;
}
