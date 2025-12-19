using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace App.Core.Master
{
    public partial class AudioMaster : SingletonMono<AudioMaster>
    {
        private AudioClip _audioClip;

        private string _deviceName;

        // 音频相关参数
        private const int targetSampleRate = 16000; // 目标采样率 (8000或16000)
        private const int bufferSizeSeconds = 1; // 音频缓冲区大小(秒)
        private bool isRecording = false; // 是否正在录制
        private int lastSamplePosition = 0;
        private float[] audioBuffer;
        private int bufferSize;
        private int _timeId = -1;

        public void StartRecording()
        {
#if UNITY_ANDROID
            if (!PlayerPrefs.HasKey(UnityEngine.Android.Permission.Microphone))
            {
                if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission
                        .Microphone))
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                }

                PlayerPrefs.SetString(UnityEngine.Android.Permission.Microphone, "1");
            }
#endif
            if (Microphone.devices.Length > 0)
            {
                _deviceName = Microphone.devices[0];
                Log.I("麦克风名称", ("device", _deviceName));
            }
            else
            {
                Log.E("没有找到麦克风");
                return;
            }

            if (isRecording) return;

            // 创建音频缓冲区
            bufferSize = targetSampleRate * bufferSizeSeconds;
            audioBuffer = new float[bufferSize];

            _audioClip = Microphone.Start(_deviceName, true, bufferSizeSeconds, targetSampleRate);

            if (!_audioClip)
            {
                Debug.LogError("无法启动麦克风录音");
                return;
            }

            // 等待麦克风初始化
            while (Microphone.GetPosition(_deviceName) <= 0)
            {
            }

            lastSamplePosition = 0;
            isRecording = true;
            _timeId = TimeUpdateMaster.Instance.StartTimer(ReadAudioData);
        }

        private void ReadAudioData(float time)
        {
            if (!isRecording) return;
            if (_audioClip == null) return;
            VisualAudio();
            ProcessAudioData();
        }

        private void VisualAudio()
        {
            //剪切音频
            var volumeData = new float[128];
            var offset = Microphone.GetPosition(_deviceName) - 128 + 1;
            if (offset < 0)
            {
                offset = 0;
            }

            _audioClip.GetData(volumeData, offset);

            if (EventDispatcher.HasEventListener("AudioWavaData"))
            {
                EventDispatcher.TriggerEvent("AudioWavaData", volumeData);
            }
        }

        /// <summary>
        /// 处理音频数据
        /// </summary>
        private void ProcessAudioData()
        {
            var currentPosition = Microphone.GetPosition(_deviceName);
            var sampleCount = currentPosition - lastSamplePosition;

            // 处理缓冲区环绕
            if (sampleCount < 0)
            {
                sampleCount = _audioClip.samples - lastSamplePosition + currentPosition;
            }

            // 没有新数据
            if (sampleCount <= 0) return;

            // 确保缓冲区足够大
            if (audioBuffer.Length < sampleCount)
            {
                audioBuffer = new float[sampleCount];
            }

            // 获取原始音频数据
            _audioClip.GetData(audioBuffer, lastSamplePosition);

            // 处理音频数据
            var processedAudio = ProcessAudioBuffer(audioBuffer, sampleCount);
            // 触发事件
            if (processedAudio != null && processedAudio.Length > 0)
            {
                if (EventDispatcher.HasEventListener("ReceiveAudioDataBytes"))
                {
                    EventDispatcher.TriggerEvent("ReceiveAudioDataBytes", processedAudio);
                }
            }

            // 更新位置
            lastSamplePosition = currentPosition % _audioClip.samples;
        }

        /// <summary>
        /// 处理音频缓冲区：转换为16位PCM、单声道
        /// </summary>
        private byte[] ProcessAudioBuffer(float[] buffer, int sampleCount)
        {
            // 如果原始音频是立体声，转换为单声道（取平均值）
            if (_audioClip.channels > 1)
            {
                var monoCount = sampleCount / _audioClip.channels;
                var monoBuffer = new float[monoCount];

                for (var i = 0; i < monoCount; i++)
                {
                    float sum = 0;
                    for (var c = 0; c < _audioClip.channels; c++)
                    {
                        sum += buffer[i * _audioClip.channels + c];
                    }

                    monoBuffer[i] = sum / _audioClip.channels;
                }

                buffer = monoBuffer;
                sampleCount = monoCount;
            }

            // 将浮点音频数据转换为16位PCM
            var pcmData = new byte[sampleCount * 2]; // 16位 = 2字节/样本

            for (var i = 0; i < sampleCount; i++)
            {
                // 将浮点音频(-1.0到1.0)转换为16位整数(-32768到32767)
                var pcmValue = (short)(buffer[i] * short.MaxValue);

                // 小端字节序
                pcmData[i * 2] = (byte)(pcmValue & 0xFF);
                pcmData[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
            }

            return pcmData;
        }

        public void StopRecording()
        {
            if (!isRecording) return;
            Microphone.End(_deviceName);
            TimeUpdateMaster.Instance.EndTimer(_timeId);
            _audioClip = null;
            isRecording = false;
        }

        public bool IsRecording()
        {
            return isRecording;
        }
    }
}