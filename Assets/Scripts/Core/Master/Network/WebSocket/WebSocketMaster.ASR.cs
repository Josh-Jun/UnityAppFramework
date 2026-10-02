/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年3月16 10:54
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    /// <summary>
    /// 语音识别功能
    /// </summary>
    public partial class WebSocketMaster
    {
        #region 讯飞语音识别
        
        private const string ASR_WS_BASE_URL = "wss://rtasr.xfyun.cn/v1/ws";
        private const string APP_ID = "";
        private const string API_KEY = "";
        private string asrURL; // asr ws请求地址
        
        private const int bufferSize = 1280;
        private const int intervalTime = 40;// ms
        private readonly Queue<byte> audioBuffer = new();//音频字节流队列
        private int timeTaskId = -1; // 定时任务id
        
        private readonly object queueLock = new(); // 线程安全锁

        /// <summary> 语音识别事件类型 </summary>
        private const string ASR_STARTED = "started";
        private const string ASR_RESULT = "result";
        private const string ASR_ERROR = "error";

        private string sentence;
        
        #endregion

        #region 阿里语音识别

        private const string APP_KEY = "";
        private const string ACCESS_KEY_ID = "";
        private const string ACCESS_KEY_SECRET = "";
        
        private const string WS_URL = "wss://nls-gateway.aliyuncs.com/ws/v1";
        private const string START_TRANSCRIPTION = "StartTranscription";
        private const string STOP_TRANSCRIPTION = "StopTranscription";
        private const string TRANSCRIPTION_STARTED = "TranscriptionStarted";
        private const string SENTENCE_BEGIN = "SentenceBegin";
        private const string TRANSCRIPTION_RESULT_CHANGED = "TranscriptionResultChanged";
        private const string SENTENCE_END = "SentenceEnd";
        private const string TRANSCRIPTION_COMPLETED = "TranscriptionCompleted";

        private string _taskId;
        
        #endregion
        
        private bool isStartTranscription = false;
        private readonly StringBuilder sentenceBuilder = new();
        
        /// <summary>
        /// 开始语音识别
        /// </summary>
        public void StartASR()
        {
            switch (Global.WSAsrChannel)
            {
                case AsrChannel.xfyun:
                {
                    var timestamp = DateTime.Now.ToTimeStamp(false);
                    var sign = GenerateSignStr1(timestamp);
                    asrURL = $"{ASR_WS_BASE_URL}?appid={APP_ID}&ts={timestamp}&signa={sign}";
                    Log.I(asrURL);
                    Connect(asrURL, OnConnectCompleted, OnReceiveMessage);
                    timeTaskId = TimeTaskMaster.Instance.AddTimeTask(SendAudioData, intervalTime, TimeUnit.Millisecond, 0);
                    break;
                }
                case AsrChannel.aliyun:
                {
                    _taskId = Guid.NewGuid().ToString("N");

                    GetWsToken((token, expireTime) =>
                    {
                        Debug.Log($"token: {token}, expireTime: {expireTime}");
                        asrURL = $"{WS_URL}?token={token}";
                        Connect(asrURL, StartTranscription, OnReceiveMessage);
                    }, (error) =>
                    {
                        Debug.Log($"error: {error}");
                    });
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            AddEventMsg<byte[]>("ReceiveAudioDataBytes", OnReceiveAudioDataBytes);
        }
        
        /// <summary>
        /// 停止语音识别
        /// </summary>
        public void StopASR()
        {
            AudioMaster.Instance.StopRecording();
            switch (Global.WSAsrChannel)
            {
                case AsrChannel.xfyun:
                {
                    EndTranscription();
                    break;
                }
                case AsrChannel.aliyun:
                {
                    StopTranscription();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 接收音频字节流
        /// </summary>
        /// <param name="audioData"></param>
        private void OnReceiveAudioDataBytes(byte[] audioData)
        {
            switch (Global.WSAsrChannel)
            {
                case AsrChannel.xfyun:
                {
                    lock (queueLock)
                    {
                        foreach (var data in audioData)
                        {
                            audioBuffer.Enqueue(data);
                        }
                    }
                    break;
                }
                case AsrChannel.aliyun:
                {
                    Send(asrURL, audioData);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// WebSocket接收消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnReceiveMessage(string msg)
        {
            // Log.I("WebSocket收到消息", ("ws_asr_msg", msg));
            switch (Global.WSAsrChannel)
            {
                case AsrChannel.xfyun:
                {
                    XF_OnReceiveMessage(msg);
                    break;
                }
                case AsrChannel.aliyun:
                {
                    AL_OnReceiveMessage(msg);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        /// <summary>
        /// WebSocket连接完成（可能连接失败）
        /// </summary>
        private void OnConnectCompleted()
        {
            Log.I("WebSocket连接完成，开始接收消息");
        }

        #region 讯飞语音识别

        private void EndTranscription()
        {
            TimeTaskMaster.Instance.DeleteTimeTask(timeTaskId);
            var end = $"{{\"end\": true}}";
            Send(asrURL, end);
            isStartTranscription = false; 
        }

        private void XF_OnReceiveMessage(string msg)
        {
            var response = JsonUtility.FromJson<ASRResponseData>(msg);
            switch (response.action)
            {
                case ASR_STARTED:
                    Log.I("开始录音", ("data", response.data));
                    AudioMaster.Instance.StartRecording();
                    isStartTranscription = false;
                    break;
                case ASR_RESULT:
                    // Log.I("result:", ("data", response.data));
                    var data = JsonUtility.FromJson<ASRResultData>(response.data);
                    if (!data.ls)
                    {
                        sentence = GetFullSentence(data.cn.st.rt);
                        // 一句话识别结束
                        if (data.cn.st.type == "0")
                        {
                            if (HasEvent("OnTranscriptionSentenceEnd"))
                            {
                                SendEventMsg("OnTranscriptionSentenceEnd", sentence);
                            }
                            sentenceBuilder.Append(sentence);
                            isStartTranscription = false;
                        }
                        else
                        {
                            if (!isStartTranscription)
                            {
                                isStartTranscription = true;
                                if (HasEvent("OnTranscriptionSentenceBegin"))
                                {
                                    SendEventMsg("OnTranscriptionSentenceBegin");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (sentenceBuilder.Length == 0)
                        {
                            sentence = GetFullSentence(data.cn.st.rt);
                            sentenceBuilder.Append(sentence);
                        }
                        // 识别完成
                        if (HasEvent("OnTranscriptionCompleted"))
                        {
                            SendEventMsg("OnTranscriptionCompleted", sentenceBuilder.ToString());
                        }
                        Log.I("result:", ("sentence", sentenceBuilder.ToString()));
                        sentenceBuilder.Length = 0;
                        Disconnect(asrURL);
                    }
                    break;
                case ASR_ERROR:
                    Log.E("Error", ("code", response.code), ("desc", response.desc));
                    break;
            }
        }
        
        /// <summary>
        /// 每40ms发送1280字节
        /// </summary>
        private void SendAudioData()
        {
            var bytes = new List<byte>();
            lock (queueLock)
            {
                var takeCount = Math.Min(bufferSize, audioBuffer.Count);
                for (var i = 0; i < takeCount; i++)
                {
                    bytes.Add(audioBuffer.Dequeue());
                }
                Send(asrURL, bytes.ToArray());
            }
        }

        /// <summary>
        /// 获取完整句子
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private string GetFullSentence(List<WorldListData> list)
        {
            var stringBuilder = new StringBuilder();
            foreach (var world in from data in list from worldData in data.ws from world in worldData.cw select world)
            {
                stringBuilder.Append(world.w);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 生成加密签名
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private string GenerateSignStr1(long timestamp)
        {
            var baseString = $"{APP_ID}{timestamp}";
            var md5String = MD5Tools.Str2MD5(baseString);
            var keyByte = Encoding.UTF8.GetBytes(API_KEY);
            var sourceBytes = Encoding.UTF8.GetBytes(md5String);
            var hmac_sha1 = new HMACSHA1(keyByte);
            var signBytes =  hmac_sha1.ComputeHash(sourceBytes);
            var sign = Convert.ToBase64String(signBytes);
            return sign;
        }

        #endregion

        #region 阿里语音识别
        
        private void StartTranscription()
        {
            if (isStartTranscription) return;
            var msg = new TranscriptionData<StartTranscriptionData>
            {
                header = new Header
                {
                    appkey = APP_KEY,
                    message_id = Guid.NewGuid().ToString("N"),
                    task_id = _taskId,
                    name = START_TRANSCRIPTION,
                },
                payload = new StartTranscriptionData
                {
                    format = "wav",
                    sample_rate = 16000,
                    enable_intermediate_result = false,
                    enable_punctuation_prediction = true,
                    enable_inverse_text_normalization = true,
                }
            };
            Log.W("StartTranscription", (START_TRANSCRIPTION, JsonUtility.ToJson(msg)));
            Send(asrURL, JsonUtility.ToJson(msg));
            isStartTranscription = true;
        }
        
        private void StopTranscription()
        {
            if (!isStartTranscription) return;
            var msg = new TranscriptionData
            {
                header = new Header
                {
                    appkey = APP_KEY,
                    message_id = Guid.NewGuid().ToString("N"),
                    task_id = _taskId,
                    name = STOP_TRANSCRIPTION,
                }
            };
            Log.W("StopTranscription", (STOP_TRANSCRIPTION, JsonUtility.ToJson(msg)));
            Send(asrURL, JsonUtility.ToJson(msg));
            isStartTranscription = false;
        }

        private void AL_OnReceiveMessage(string msg)
        {
            if (!IsValidJson(msg)) return;
            var data = JsonUtility.FromJson<TranscriptionData>(msg);
            switch (data.header.name)
            {
                // 服务端已经准备好了进行识别，客户端可以发送音频数据了。
                case TRANSCRIPTION_STARTED:
                    Log.W("开始录音");
                    // 开始录音
                    AudioMaster.Instance.StartRecording();
                    break;
                // 服务端检测到了一句话的开始。
                case SENTENCE_BEGIN:
                {
                    Log.W("识别开始");
                    if (HasEvent("OnTranscriptionSentenceBegin"))
                    {
                        SendEventMsg("OnTranscriptionSentenceBegin");
                    }

                    break;
                }
                // 识别结果发生了变化。
                case TRANSCRIPTION_RESULT_CHANGED:
                    Log.W("识别结果改变");
                    break;
                // 服务端检测到了一句话的结束。
                case SENTENCE_END:
                {
                    Log.W("识别结束");
                    var result = JsonUtility.FromJson<TranscriptionData<SentenceEndData>>(msg);
                
                    if (HasEvent("OnTranscriptionSentenceEnd"))
                    {
                        SendEventMsg("OnTranscriptionSentenceEnd", result.payload.result);
                    }
                    sentenceBuilder.Append(result.payload.result);
                    break;
                }
                // 服务端已停止了语音转写。只有发送了StopTranscription指令，才能接收到该事件。
                case TRANSCRIPTION_COMPLETED:
                {
                    Log.W("识别完成");
                    if (HasEvent("OnTranscriptionCompleted"))
                    {
                        SendEventMsg("OnTranscriptionCompleted", sentenceBuilder.ToString());
                    }
                    sentenceBuilder.Length = 0;
                    Disconnect(asrURL);
                    break;
                }
            }
        }
        
        /// <summary>
        /// 判断字符串是否为有效的JSON格式
        /// </summary>
        /// <param name="jsonString">要验证的字符串</param>
        /// <returns>如果是有效的JSON格式返回true，否则返回false</returns>
        private bool IsValidJson(string jsonString)
        {
            // 基本检查：空字符串、null或空白字符串
            if (string.IsNullOrWhiteSpace(jsonString))
                return false;

            // 去除首尾空白字符
            jsonString = jsonString.Trim();

            // 基本格式检查：JSON必须以 { } 或 [ ] 开始和结束
            if (!((jsonString.StartsWith("{") && jsonString.EndsWith("}")) ||
                  (jsonString.StartsWith("[") && jsonString.EndsWith("]"))))
                return false;

            try
            {
                // 尝试使用Unity的JsonUtility解析（适用于对象格式的JSON）
                if (jsonString.StartsWith("{"))
                {
                    // 对于对象类型的JSON，先尝试解析为通用对象
                    JsonUtility.FromJson<object>(jsonString);
                    return true;
                }

                // 对于数组类型的JSON，使用更简单的验证方式
                if (jsonString.StartsWith("["))
                {
                    // 检查是否为有效的JSON数组格式
                    return IsValidJsonArray(jsonString);
                }
            }
            catch (System.ArgumentException)
            {
                // JsonUtility解析失败
                return false;
            }
            catch (System.Exception)
            {
                // 其他异常也视为无效JSON
                return false;
            }

            return false;
        }

        /// <summary>
        /// 验证JSON数组格式的辅助方法
        /// </summary>
        /// <param name="jsonArray">JSON数组字符串</param>
        /// <returns>是否为有效的JSON数组</returns>
        private bool IsValidJsonArray(string jsonArray)
        {
            try
            {
                // 简单的JSON数组验证：检查括号匹配和基本格式
                var bracketCount = 0;
                var inString = false;
                var escapeNext = false;

                foreach (var c in jsonArray)
                {
                    if (escapeNext)
                    {
                        escapeNext = false;
                        continue;
                    }

                    switch (c)
                    {
                        case '\\':
                            escapeNext = true;
                            continue;
                        case '"':
                            inString = !inString;
                            continue;
                    }

                    if (inString) continue;
                    switch (c)
                    {
                        case '[':
                            bracketCount++;
                            break;
                        case ']':
                            bracketCount--;
                            break;
                    }
                }

                return bracketCount == 0 && !inString;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试将JSON字符串解析为指定类型的对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="jsonString">JSON字符串</param>
        /// <param name="result">解析结果</param>
        /// <returns>是否解析成功</returns>
        private bool TryParseJson<T>(string jsonString, out T result)
        {
            result = default(T);

            if (!IsValidJson(jsonString))
                return false;

            try
            {
                result = JsonUtility.FromJson<T>(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region 获取WebSocket鉴权Token

        /// <summary>阿里云语音服务端点</summary>
        private const string BASE_URL = "http://nls-meta.cn-shanghai.aliyuncs.com/";

        /// <summary>API动作</summary>
        private const string ACTION = "CreateToken";

        /// <summary>API版本</summary>
        private const string VERSION = "2019-02-28";

        /// <summary>响应格式</summary>
        private const string FORMAT = "JSON";

        /// <summary>区域ID</summary>
        private const string REGION_ID = "cn-shanghai";

        /// <summary>签名方法</summary>
        private const string SIGNATURE_METHOD = "HMAC-SHA1";

        /// <summary>签名版本</summary>
        private const string SIGNATURE_VERSION = "1.0";

        /// <summary>当前Token</summary>
        private string WsToken { get; set; }

        /// <summary>Token过期时间戳（秒）</summary>
        private long ExpireTime { get; set; }

        /// <summary>Token是否有效</summary>
        private bool IsTokenValid => !string.IsNullOrEmpty(WsToken) &&
                                     DateTimeOffset.UtcNow.ToUnixTimeSeconds() < ExpireTime;
        /// <summary>
        /// 获取Token（异步回调方式）
        /// </summary>
        /// <param name="onSuccess">成功回调</param>
        /// <param name="onError">失败回调</param>
        private void GetWsToken(Action<string, long> onSuccess, Action<string> onError)
        {
            // 如果当前Token仍然有效，直接返回
            if (IsTokenValid)
            {
                onSuccess?.Invoke(WsToken, ExpireTime);
                return;
            }

            try
            {
                // 构建请求参数
                var parameters = BuildRequestParameters(ACCESS_KEY_ID);

                // 计算签名
                var signature = CalculateSignature(parameters, ACCESS_KEY_SECRET);

                // 构建完整URL
                var requestUrl = BuildRequestUrl(parameters, signature);

                // 发送HTTP请求
                SendHttpRequest(requestUrl, onSuccess, onError);
            }
            catch (Exception e)
            {
                Debug.LogError($"[AliyunVoiceTokenManager] 获取Token异常: {e.Message}");
                onError?.Invoke($"获取Token异常: {e.Message}");
            }
        }

        /// <summary>
        /// 清除当前Token缓存
        /// </summary>
        public void ClearToken()
        {
            WsToken = null;
            ExpireTime = 0;
        }

        /// <summary>
        /// 构建请求参数
        /// </summary>
        private Dictionary<string, string> BuildRequestParameters(string accessKeyId)
        {
            var parameters = new Dictionary<string, string>
            {
                ["AccessKeyId"] = accessKeyId,
                ["Action"] = ACTION,
                ["Format"] = FORMAT,
                ["RegionId"] = REGION_ID,
                ["SignatureMethod"] = SIGNATURE_METHOD,
                ["SignatureNonce"] = GenerateNonce(),
                ["SignatureVersion"] = SIGNATURE_VERSION,
                ["Timestamp"] = GenerateTimestamp(),
                ["Version"] = VERSION
            };

            return parameters;
        }

        /// <summary>
        /// 生成时间戳（ISO 8601格式）
        /// </summary>
        private string GenerateTimestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 生成唯一随机数（UUID格式）
        /// </summary>
        private string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 计算签名
        /// </summary>
        private string CalculateSignature(Dictionary<string, string> parameters, string accessKeySecret)
        {
            // 1. 对参数进行排序并编码
            var sortedParams = new SortedDictionary<string, string>(parameters);
            var queryString = BuildCanonicalQueryString(sortedParams);

            // 2. 构造待签名字符串
            var stringToSign = $"GET&{UrlEncode("/")}&{UrlEncode(queryString)}";

            // 3. 计算HMAC-SHA1签名
            var key = $"{accessKeySecret}&";
            var signature = ComputeHmacSha1(stringToSign, key);

            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// 构建规范化查询字符串
        /// </summary>
        private string BuildCanonicalQueryString(SortedDictionary<string, string> parameters)
        {
            var queryParts = parameters.Select(kvp => $"{UrlEncode(kvp.Key)}={UrlEncode(kvp.Value)}").ToList();
            return string.Join("&", queryParts);
        }

        /// <summary>
        /// URL编码（符合RFC 3986标准）
        /// </summary>
        private string UrlEncode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var encoded = new StringBuilder();
            foreach (var c in input)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') || c == '-' || c == '_' ||
                    c == '.' || c == '~')
                {
                    encoded.Append(c);
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(c.ToString());
                    foreach (var b in bytes)
                    {
                        encoded.Append($"%{b:X2}");
                    }
                }
            }
            return encoded.ToString();
        }

        /// <summary>
        /// 计算HMAC-SHA1签名
        /// </summary>
        private byte[] ComputeHmacSha1(string data, string key)
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// 构建请求URL
        /// </summary>
        private string BuildRequestUrl(Dictionary<string, string> parameters, string signature)
        {
            var sortedParams = new SortedDictionary<string, string>(parameters);
            var queryString = BuildCanonicalQueryString(sortedParams);

            return $"{BASE_URL}?Signature={UrlEncode(signature)}&{queryString}";
        }

        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        private void SendHttpRequest(string url, Action<string, long> onSuccess, Action<string> onError)
        {
            var requester = Master.HttpsMaster.Uwr;

            requester.Get(url, response =>
            {
                try
                {
                    if (string.IsNullOrEmpty(response))
                    {
                        onError?.Invoke("服务器返回空响应");
                        return;
                    }

                    // 解析JSON响应
                    var tokenResponse = JsonUtility.FromJson<TokenResponse>(response);

                    if (tokenResponse?.Token != null)
                    {
                        WsToken = tokenResponse.Token.Id;
                        ExpireTime = tokenResponse.Token.ExpireTime;

                        Debug.Log($"[WebSocket] Token获取成功: {WsToken}");
                        Debug.Log($"[WebSocket] Token过期时间: {DateTimeOffset.FromUnixTimeSeconds(ExpireTime).ToLocalTime():yyyy-MM-dd HH:mm:ss}");

                        onSuccess?.Invoke(WsToken, ExpireTime);
                    }
                    else
                    {
                        Debug.LogError($"[WebSocket] 解析Token响应失败: {response}");
                        onError?.Invoke("解析Token响应失败");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WebSocket] 处理响应异常: {e.Message}");
                    onError?.Invoke($"处理响应异常: {e.Message}");
                }
                finally
                {
                    requester?.Destroy();
                }
            });
        }

        /// <summary>
        /// 获取Token过期时间的可读格式
        /// </summary>
        public string GetTokenExpireTimeString()
        {
            if (ExpireTime <= 0) return "无效";

            var expireDateTime = DateTimeOffset.FromUnixTimeSeconds(ExpireTime).ToLocalTime();
            return expireDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取Token剩余有效时间（秒）
        /// </summary>
        public long GetTokenRemainingTime()
        {
            if (!IsTokenValid) return 0;

            return ExpireTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        #endregion

        #endregion
    }
}