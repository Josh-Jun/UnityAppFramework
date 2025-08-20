using System;
using UnityEngine;

namespace App.Core.Tools
{
    /// <summary>
    /// 功能:音频工具 
    /// </summary>
    public class AudioTools
    {
        /// <summary>
        /// AudioClip转byte[]，支持声道转换和重采样
        /// </summary>
        /// <param name="audioClip">要转换的音频剪辑</param>
        /// <param name="targetChannels">目标声道数 (1=单声道, 2=双声道)</param>
        /// <param name="targetFrequency">目标采样率 (如: 8000, 16000, 44100等)</param>
        /// <returns>16位PCM格式的字节数组</returns>
        public static byte[] ClipToBytes(AudioClip audioClip, int targetChannels = 1, int targetFrequency = 44100)
        {
            // 参数验证
            if (audioClip == null)
            {
                Debug.LogError("AudioTools.ClipToBytes: audioClip不能为空");
                return null;
            }
            
            if (audioClip.samples <= 0)
            {
                Debug.LogError("AudioTools.ClipToBytes: audioClip样本数必须大于0");
                return null;
            }
            
            if (targetChannels <= 0 || targetChannels > 2)
            {
                Debug.LogError("AudioTools.ClipToBytes: 目标声道数必须在1-2之间");
                return null;
            }
            
            if (targetFrequency <= 0)
            {
                Debug.LogError("AudioTools.ClipToBytes: 目标采样率必须大于0");
                return null;
            }

            // 获取原始音频数据
            int originalChannels = audioClip.channels;
            int originalSamples = audioClip.samples;
            int originalFrequency = audioClip.frequency;
            
            int totalSamples = originalSamples * originalChannels;
            var originalData = new float[totalSamples];

            if (!audioClip.GetData(originalData, 0))
            {
                Debug.LogError("AudioTools.ClipToBytes: 获取音频数据失败");
                return null;
            }

            // 计算重采样后的样本数
            int targetSamples = Mathf.RoundToInt((float)originalSamples * targetFrequency / originalFrequency);
            
            Debug.Log($"AudioTools.ClipToBytes: 转换 {originalChannels}声道->{targetChannels}声道, " +
                     $"{originalFrequency}Hz->{targetFrequency}Hz, " +
                     $"{originalSamples}样本->{targetSamples}样本");

            // 执行声道转换和重采样
            var convertedData = ConvertChannelsAndResample(originalData, originalSamples, originalChannels, 
                                                          targetSamples, targetChannels, originalFrequency, targetFrequency);

            // 转换为字节数组
            const float rescaleFactor = 32767f;
            var bytesData = new byte[convertedData.Length * 2];

            for (var i = 0; i < convertedData.Length; i++)
            {
                float clampedSample = Mathf.Clamp(convertedData[i], -1f, 1f);
                short intSample = (short)(clampedSample * rescaleFactor);
                var byteArr = BitConverter.GetBytes(intSample);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }

        /// <summary>
        /// 声道转换和重采样处理
        /// </summary>
        private static float[] ConvertChannelsAndResample(float[] originalData, int originalSamples, int originalChannels,
                                                          int targetSamples, int targetChannels, int originalFreq, int targetFreq)
        {
            // 先进行声道转换
            var channelConverted = ConvertChannels(originalData, originalSamples, originalChannels, targetChannels);
            
            // 如果采样率相同，直接返回声道转换结果
            if (originalFreq == targetFreq)
            {
                return channelConverted;
            }
            
            // 进行重采样
            return Resample(channelConverted, originalSamples, targetSamples, targetChannels);
        }

        /// <summary>
        /// 声道转换
        /// </summary>
        private static float[] ConvertChannels(float[] data, int samples, int originalChannels, int targetChannels)
        {
            if (originalChannels == targetChannels)
            {
                return data; // 无需转换
            }

            var result = new float[samples * targetChannels];

            if (originalChannels == 1 && targetChannels == 2)
            {
                // 单声道转立体声：复制单声道数据到左右声道
                for (int i = 0; i < samples; i++)
                {
                    result[i * 2] = data[i];     // 左声道
                    result[i * 2 + 1] = data[i]; // 右声道
                }
            }
            else if (originalChannels == 2 && targetChannels == 1)
            {
                // 立体声转单声道：混合左右声道
                for (int i = 0; i < samples; i++)
                {
                    float left = data[i * 2];
                    float right = data[i * 2 + 1];
                    result[i] = (left + right) * 0.5f; // 平均值
                }
            }
            else if (originalChannels > 2 && targetChannels == 1)
            {
                // 多声道转单声道：混合所有声道
                for (int i = 0; i < samples; i++)
                {
                    float sum = 0f;
                    for (int ch = 0; ch < originalChannels; ch++)
                    {
                        sum += data[i * originalChannels + ch];
                    }
                    result[i] = sum / originalChannels;
                }
            }
            else if (originalChannels > 2 && targetChannels == 2)
            {
                // 多声道转立体声：取前两个声道
                for (int i = 0; i < samples; i++)
                {
                    result[i * 2] = data[i * originalChannels];         // 左声道
                    result[i * 2 + 1] = originalChannels > 1 ? 
                        data[i * originalChannels + 1] : data[i * originalChannels]; // 右声道
                }
            }

            return result;
        }

        /// <summary>
        /// 简单线性插值重采样
        /// </summary>
        private static float[] Resample(float[] data, int originalSamples, int targetSamples, int channels)
        {
            if (originalSamples == targetSamples)
            {
                return data;
            }

            var result = new float[targetSamples * channels];
            float ratio = (float)originalSamples / targetSamples;

            for (int i = 0; i < targetSamples; i++)
            {
                float sourceIndex = i * ratio;
                int index1 = Mathf.FloorToInt(sourceIndex);
                int index2 = Mathf.Min(index1 + 1, originalSamples - 1);
                float t = sourceIndex - index1;

                for (int ch = 0; ch < channels; ch++)
                {
                    float sample1 = data[index1 * channels + ch];
                    float sample2 = data[index2 * channels + ch];
                    result[i * channels + ch] = Mathf.Lerp(sample1, sample2, t);
                }
            }

            return result;
        }

        /// <summary>
        /// byte[]转AudioClip
        /// </summary>
        /// <param name="rawData">原始音频数据(16位PCM格式)</param>
        /// <param name="channels">声道数 (1=单声道, 2=双声道)</param>
        /// <param name="frequency">采样率 (如: 8000, 16000, 44100等)</param>
        /// <returns></returns>
        public static AudioClip BytesToClip(byte[] rawData, int channels = 1, int frequency = 44100)
        {
            // 参数验证
            if (rawData == null || rawData.Length == 0)
            {
                Debug.LogError("AudioTools.BytesToClip: rawData不能为空");
                return null;
            }
            
            if (channels <= 0 || channels > 8)
            {
                Debug.LogError("AudioTools.BytesToClip: 声道数必须在1-8之间");
                return null;
            }
            
            if (frequency <= 0)
            {
                Debug.LogError("AudioTools.BytesToClip: 采样率必须大于0");
                return null;
            }
            
            // 处理奇数长度的数据（16位数据需要2字节对齐）
            if (rawData.Length % 2 != 0)
            {
                Debug.LogWarning("AudioTools.BytesToClip: 数据长度为奇数，将在末尾补0以对齐");
                
                // 创建新的偶数长度数组，末尾补0
                var alignedData = new byte[rawData.Length + 1];
                Array.Copy(rawData, alignedData, rawData.Length);
                alignedData[rawData.Length] = 0; // 末尾补0
                rawData = alignedData;
            }

            // 计算总样本数：每个样本占2字节，总样本数需要除以声道数得到每个声道的样本数
            int totalSamples = rawData.Length / 2; // 16位数据，每个样本2字节
            int samplesPerChannel = totalSamples / channels; // 每个声道的样本数
            
            var samples = new float[totalSamples]; // Unity需要交错存储的样本数据
            const float rescaleFactor = 32767f; // 16位有符号整数的最大值

            // 处理音频数据
            for (int i = 0; i < totalSamples; i++)
            {
                // 每2字节读取一个16位样本
                short sample = BitConverter.ToInt16(rawData, i * 2);
                // 转换为-1到1之间的float值
                samples[i] = sample / rescaleFactor;
            }

            // 创建AudioClip
            var audioClip = AudioClip.Create("ConvertedAudio", samplesPerChannel, channels, frequency, false);
            
            if (audioClip == null)
            {
                Debug.LogError("AudioTools.BytesToClip: 创建AudioClip失败");
                return null;
            }
            
            // 设置音频数据
            if (!audioClip.SetData(samples, 0))
            {
                Debug.LogError("AudioTools.BytesToClip: 设置音频数据失败");
                return null;
            }

            return audioClip;
        }
    }
}