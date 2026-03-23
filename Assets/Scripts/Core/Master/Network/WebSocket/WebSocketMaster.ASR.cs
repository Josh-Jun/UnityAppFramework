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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    /// <summary>
    /// 科大讯飞语音识别功能
    /// </summary>
    public partial class WebSocketMaster
    {
        private const string ASR_WS_BASE_URL = "wss://rtasr.xfyun.cn/v1/ws";
        private const string APP_ID = "535283ab";
        private const string API_KEY = "93ce61a1adccc0516c06cb299322c479";
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
        /// <summary>
        /// 开始语音识别
        /// </summary>
        public void StartASR()
        {
            var timestamp = DateTime.Now.ToTimeStamp(false);
            var sign = GenerateSignStr1(timestamp);
            asrURL = $"{ASR_WS_BASE_URL}?appid={APP_ID}&ts={timestamp}&signa={sign}";
            Log.I(asrURL);
            Connect(asrURL, OnConnectCompleted, OnReceiveMessage);
            AddEventMsg<byte[]>("ReceiveAudioDataBytes", OnReceiveAudioDataBytes);
            timeTaskId = TimeTaskMaster.Instance.AddTimeTask(SendAudioData, intervalTime, TimeUnit.Millisecond, 0);
        }
        /// <summary>
        /// 接收音频字节流
        /// </summary>
        /// <param name="audioData"></param>
        private void OnReceiveAudioDataBytes(byte[] audioData)
        {
            lock (queueLock)
            {
                foreach (var data in audioData)
                {
                    audioBuffer.Enqueue(data);
                }
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

        private string sentence;
        private readonly StringBuilder sentenceBuilder = new();
        private bool isBeginTranscription = false;
        /// <summary>
        /// WebSocket接收消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnReceiveMessage(string msg)
        {
            Log.I("WebSocket收到消息", ("ws_asr_msg", msg));
            var response = JsonUtility.FromJson<ASRResponseData>(msg);
            switch (response.action)
            {
                case ASR_STARTED:
                    Log.I("开始录音", ("data", response.data));
                    AudioMaster.Instance.StartRecording();
                    isBeginTranscription = false;
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
                            isBeginTranscription = false;
                        }
                        else
                        {
                            if (!isBeginTranscription)
                            {
                                isBeginTranscription = true;
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
                        isBeginTranscription = false;
                        Disconnect(asrURL);
                    }
                    break;
                case ASR_ERROR:
                    Log.E("Error", ("code", response.code), ("desc", response.desc));
                    break;
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
        /// WebSocket连接完成（可能连接失败）
        /// </summary>
        private void OnConnectCompleted()
        {
            Log.I("WebSocket连接完成，开始接收消息");
        }

        /// <summary>
        /// 停止语音识别
        /// </summary>
        public void StopASR()
        {
            AudioMaster.Instance.StopRecording();
            TimeTaskMaster.Instance.DeleteTimeTask(timeTaskId);
            var end = $"{{\"end\": true}}";
            Send(asrURL, end);
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
    }
}