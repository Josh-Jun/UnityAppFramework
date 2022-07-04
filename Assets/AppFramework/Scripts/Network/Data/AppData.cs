using System.Collections;
using System.Collections.Generic;

#region Https
/// <summary>
/// Https请求结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class HttpsRequestResult<T>
{
    public bool success;
    public string msg;  // 请求状态消息
    public T data;      // 数据
    public string time;
    public override string ToString()
    {
        return $"success:{success},msg:{msg},time:{time}";
    }
}

#endregion
