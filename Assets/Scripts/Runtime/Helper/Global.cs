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
    aliyun,
    xfyun,
}

public static partial class Global
{
    public const AsrChannel WSAsrChannel = AsrChannel.aliyun;
    public static ServerData ServerData;
}
