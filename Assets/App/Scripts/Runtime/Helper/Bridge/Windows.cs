/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年3月5 11:26
 * function    : 
 * ===============================================
 * */

public class Windows
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    public static bool StartProcess(string exePath, string args)
    {
        var workingDir = System.IO.Path.GetDirectoryName(exePath);
        if (workingDir == null) return false;
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDir, // 设置工作目录
                CreateNoWindow = false, // 显示窗口
                UseShellExecute = true,  // 使用操作系统shell启动
                Arguments = args,
            };
            System.Diagnostics.Process.Start(startInfo);
            return true;
        }
        catch (System.Exception message)
        {
            UnityEngine.Debug.LogError($"启动EXE程序失败: EXE路径: {exePath} 错误信息:{message}");
            return false;
        }
    }
#endif
}
