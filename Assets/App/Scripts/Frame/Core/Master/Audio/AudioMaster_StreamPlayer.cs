using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    public partial class AudioMaster : SingletonMono<AudioMaster>
    {
        private readonly Queue<float[]> audioStreamQueue = new Queue<float[]>();
        private List<float> audioStreamBuffer = new List<float>();
        private AudioClip streamingClip;

        // 音频参数
        private int streamSampleRate = 8000;
        private int streamChannels = 1;
        private const float initialBufferTime = 0.2f; // 初始缓冲时间（秒）
        private const int streamBufferSize = 24000; // 缓冲区最大缓存样本数（例如约3秒）
        private const int clipLengthSeconds = 300; // 流式 AudioClip 的总时长（秒），避免很快自然播放结束

        // 播放状态
        private bool isInitialized = false;
        private bool isPlaying = false;
        private bool isFirstChunk = true;
        private int bufferReadPosition = 0;
        private int totalSamplesWritten = 0;

        // 同步锁
        private readonly object queueLock = new object();
        private readonly object bufferLock = new object();

        // 性能监控
        private float lastBufferSizeCheck = 0f;
        private const float BUFFER_CHECK_INTERVAL = 0.5f;
        private Action _streamAudioCallback;
        private int streamTimeId = -1;
        private float boostMultiplier = 1.0f; // 音量增益倍数
        
        public AudioSource StreamAudioSource
        {
            get
            {
                if (!streamAudioSource)
                {
                    InitializeStreamAudio();
                }
                return streamAudioSource;
            }
        }
        
        private void InitializeStreamAudio()
        {
            var stream = new GameObject("StreamAudioPlayer", typeof(AudioSource));
            stream.transform.SetParent(background.transform);
            streamAudioSource = stream.GetOrAddComponent<AudioSource>();
            streamAudioSource.playOnAwake = false;

            streamTimeId = TimeUpdateMaster.Instance.StartTimer(StreamUpdate);
        }

        private void StreamUpdate(float time)
        {
            // 定期检查缓冲区状态
            if (!(Time.time - lastBufferSizeCheck > BUFFER_CHECK_INTERVAL)) return;
            MonitorBufferStatus();
            lastBufferSizeCheck = Time.time;
        }

        private AudioSource streamAudioSource;

        public void InitStreamAudioPlayer(int sampleRate, int channels, float multiplier)
        {
            if (isInitialized) return;

            streamSampleRate = sampleRate;
            streamChannels = channels;
            boostMultiplier = multiplier;
            // 预分配缓冲区（只作为我们自己的缓存，不等于 AudioClip 的长度）
            audioStreamBuffer = new List<float>(streamBufferSize);

            // 创建流式AudioClip（长度要足够长，避免 AudioSource 播放到末尾直接停止）
            var clipLengthSamples = sampleRate * channels * clipLengthSeconds;
            streamingClip = AudioClip.Create(
                "StreamingAudio",
                clipLengthSamples,
                channels,
                sampleRate,
                true, // 流式音频
                OnAudioReadCallback
            );

            isInitialized = true;
            Debug.Log($"音频系统初始化完成，采样率：{sampleRate}Hz，声道：{channels}，缓冲区：{streamBufferSize}样本");
        }

        public void AddAudioData(string base64Chunk)
        {
            var pcmData = Convert.FromBase64String(base64Chunk);
            AddAudioData(pcmData);
        }

        /// <summary>
        /// 添加新的音频数据块
        /// </summary>
        public void AddAudioData(byte[] pcmData)
        {
            // 转换数据格式
            var floatData = ConvertByteToFloat16(pcmData);
            AddAudioData(floatData);
        }

        public void AddAudioData(float[] floatData)
        {
            // 处理第一个数据块的特殊逻辑：只写入缓冲，不进入队列，避免被重复读取
            if (isFirstChunk)
            {
                HandleFirstChunk(floatData);
                isFirstChunk = false;
            }
            else
            {
                lock (queueLock)
                {
                    audioStreamQueue.Enqueue(floatData);
                }
            }
        }


        public void SetStreamAudioVolume(float volume)
        {
            if (streamAudioSource)
            {
                streamAudioSource.volume = volume;
            }
        }

        public void SetStreamAudioMute(bool mute)
        {
            if (streamAudioSource)
            {
                streamAudioSource.mute = mute;
            }
        }

        /// <summary>
        /// 处理第一个数据块
        /// </summary>
        private void HandleFirstChunk(float[] firstChunkData)
        {
            // 先填充初始缓冲区，但不立即播放
            lock (bufferLock)
            {
                audioStreamBuffer.AddRange(firstChunkData);
            }

            // 检查是否达到初始缓冲要求
            if (GetBufferDuration() >= initialBufferTime)
            {
                StartPlayback();
            }
            else
            {
                Debug.Log($"等待初始缓冲... 当前: {GetBufferDuration():F2}s / 目标: {initialBufferTime}s");
            }
        }

        /// <summary>
        /// 开始播放
        /// </summary>
        private void StartPlayback()
        {
            if (isPlaying) return;

            // 重置读取位置
            bufferReadPosition = 0;
            totalSamplesWritten = 0;

            // 开始播放
            streamAudioSource.clip = streamingClip;
            streamAudioSource.Play();
            isPlaying = true;

            Debug.Log($"开始播放，初始缓冲: {GetBufferDuration():F2}s");

            // 启动缓冲区刷新协程
            StartCoroutine(BufferManagementCoroutine());
        }

        /// <summary>
        /// 缓冲区管理协程
        /// </summary>
        private IEnumerator BufferManagementCoroutine()
        {
            while (isPlaying)
            {
                // 刷新缓冲区数据
                RefillBufferFromQueue();
                // 控制刷新频率
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// 音频数据读取回调
        /// </summary>
        private void OnAudioReadCallback(float[] data)
        {
            lock (bufferLock)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    if (bufferReadPosition < audioStreamBuffer.Count)
                    {
                        data[i] = audioStreamBuffer[bufferReadPosition];
                        bufferReadPosition++;
                    }
                    else
                    {
                        // 缓冲区不足，填充静音
                        data[i] = 0f;
                    }
                }
            }

            totalSamplesWritten += data.Length;
        }

        /// <summary>
        /// 从队列刷新缓冲区
        /// </summary>
        private void RefillBufferFromQueue()
        {
            lock (bufferLock)
            {
                if (audioStreamQueue.Count == 0) return;
                // 清理已播放的数据
                if (bufferReadPosition > streamChannels / 10) // 超过50ms的数据
                {
                    var samplesToRemove = Mathf.Min(bufferReadPosition, audioStreamBuffer.Count);
                    if (samplesToRemove > 0)
                    {
                        audioStreamBuffer.RemoveRange(0, samplesToRemove);
                        bufferReadPosition -= samplesToRemove;
                        bufferReadPosition = Mathf.Max(0, bufferReadPosition);
                    }
                }

                // 从队列中添加新数据
                while (audioStreamQueue.Count > 0 && audioStreamBuffer.Count < streamBufferSize)
                {
                    var newData = audioStreamQueue.Dequeue();
                    audioStreamBuffer.AddRange(newData);
                }
            }

            // 如果之前没有开始播放，检查是否可以开始
            if (!isPlaying && GetBufferDuration() >= initialBufferTime)
            {
                StartPlayback();
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopPlayback()
        {
            if (streamAudioSource && streamAudioSource.isPlaying)
            {
                streamAudioSource.Stop();
            }

            isPlaying = false;
            isFirstChunk = true;

            lock (queueLock)
            {
                audioStreamQueue.Clear();
            }

            lock (bufferLock)
            {
                audioStreamBuffer.Clear();
                bufferReadPosition = 0;
            }
        }

        /// <summary>
        /// 获取缓冲区时长（秒）
        /// </summary>
        private float GetBufferDuration()
        {
            return (float)audioStreamBuffer.Count / (streamSampleRate * streamChannels);
        }

        /// <summary>
        /// 获取未播放时长（秒）
        /// </summary>
        public float GetUnplayedDuration()
        {
            lock (bufferLock)
            {
                var unplayedSamples = audioStreamBuffer.Count - bufferReadPosition;
                return Mathf.Max(0f, (float)unplayedSamples / (streamSampleRate * streamChannels));
            }
        }

        /// <summary>
        /// 监控缓冲区状态
        /// </summary>
        private void MonitorBufferStatus()
        {
            var bufferDuration = GetBufferDuration();
            var unplayedDuration = GetUnplayedDuration();
            lock (queueLock)
            {
                if (isPlaying)
                {
                    if (unplayedDuration <= 0 && audioStreamQueue.Count == 0)
                    {
                        if (EventDispatcher.HasEventListener("StreamAudioPlayCompleted"))
                        {
                            EventDispatcher.TriggerEvent("StreamAudioPlayCompleted");
                        }
                    }
                }
            }
#if UNITY_EDITOR
            if (!isPlaying) return;
            lock (queueLock)
            {
                Debug.Log($"缓冲区状态 - 总缓冲: {bufferDuration:F2}s, 未播放: {unplayedDuration:F2}s, 队列: {audioStreamQueue.Count}块");
            }
#endif
        }

        /// <summary>
        /// 转换16-bit PCM byte数组为float数组
        /// </summary>
        private float[] ConvertByteToFloat16(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return Array.Empty<float>();

            if (byteArray.Length % 2 != 0)
            {
                Debug.LogWarning($"音频数据长度({byteArray.Length})不是2的倍数");
                // 补齐为偶数长度
                var paddedArray = new byte[byteArray.Length + 1];
                Array.Copy(byteArray, paddedArray, byteArray.Length);
                byteArray = paddedArray;
            }

            var sampleCount = byteArray.Length / 2;
            var floatArray = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                try
                {
                    // 16-bit PCM: 小端字节序
                    var intSample = (short)((byteArray[i * 2]) | (byteArray[i * 2 + 1] << 8));
                    floatArray[i] = Mathf.Clamp(intSample / 32768f, -1f, 1f) * boostMultiplier;
                }
                catch (Exception e)
                {
                    Debug.LogError($"转换音频数据时出错: {e.Message}");
                    floatArray[i] = 0f;
                }
            }

            return floatArray;
        }

        /// <summary>
        /// 转换float数组为16-bit PCM byte数组
        /// </summary>
        private byte[] ConvertFloatToByte16(float[] floatArray)
        {
            var byteArray = new byte[floatArray.Length * 2];

            for (var i = 0; i < floatArray.Length; i++)
            {
                var sample = Mathf.Clamp(floatArray[i], -1f, 1f);
                var intSample = (short)(sample * 32767f);

                // 小端字节序
                byteArray[i * 2] = (byte)(intSample & 0xFF);
                byteArray[i * 2 + 1] = (byte)((intSample >> 8) & 0xFF);
            }

            return byteArray;
        }

        private void ClearStreamAudio()
        {
            StopPlayback();

            if (streamingClip)
            {
                Destroy(streamingClip);
            }

            TimeUpdateMaster.Instance.EndTimer(streamTimeId);
        }

        /// <summary>
        /// 重置播放器状态
        /// </summary>
        public void Reset()
        {
            StopPlayback();
            isFirstChunk = true;
            totalSamplesWritten = 0;
        }
    }
}