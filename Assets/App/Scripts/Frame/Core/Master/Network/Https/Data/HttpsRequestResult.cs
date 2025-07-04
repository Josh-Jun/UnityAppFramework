using System;

namespace App.Core.Master
{
    /// <summary>
    /// Https请求结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class HttpsRequestResult<T>
    {
        public int code; // 请求状态码
        public string msg; // 请求状态消息
        public T data; // 数据
        public long timestamp; // 请求时间戳
    }
}
