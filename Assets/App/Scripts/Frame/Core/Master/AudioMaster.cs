using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace App.Core.Master
{
    public class AudioMaster : SingletonMono<AudioMaster>
    {
        private GameObject background;
        private AudioSource backgroundAudio;
        private AudioSource defaultEffectAudio;
        private readonly Dictionary<string, AudioSource> effectAudios = new();


        private int _timeIdStream;
        private string _streamSourceName;
        private int _streamSampleRate;
        private readonly Queue<float[]> _audioQueue = new Queue<float[]>();

        private Action _streamAudioCallback;

        private void Awake()
        {
            background = new GameObject("AudioSource", typeof(AudioSource));
            background.transform.SetParent(transform);
            backgroundAudio = background.GetOrAddComponent<AudioSource>();
            backgroundAudio.playOnAwake = false;

            var defaultEffect = new GameObject("Default", typeof(AudioSource));
            defaultEffect.transform.SetParent(background.transform);
            defaultEffectAudio = defaultEffect.GetOrAddComponent<AudioSource>();
            defaultEffectAudio.playOnAwake = false;

            _timeIdStream = TimeUpdateMaster.Instance.StartTimer(PlayStreamAudio);
        }

        private void OnDestroy()
        {
            TimeUpdateMaster.Instance.EndTimer(_timeIdStream);
        }

        public void InitStreamAudio(string sourceName, int sampleRate = 16000, Action callback = null)
        {
            _streamSourceName = sourceName;
            _streamSampleRate = sampleRate;
            _streamAudioCallback = callback;
        }

        public void PushAudioBase64(string base64Chunk)
        {
            var wavBytes = Convert.FromBase64String(base64Chunk);
            PushAudioBytes(wavBytes);
        }

        public void PushAudioBytes(byte[] wavBytes)
        {
            // 转成 short[]
            var pcm16 = new short[wavBytes.Length / 2];
            Buffer.BlockCopy(wavBytes, 0, pcm16, 0, wavBytes.Length);
            // 转 float [-1,1]
            var samples = new float[pcm16.Length];
            for (var i = 0; i < pcm16.Length; i++)
                samples[i] = pcm16[i] / 32768f;
            PushAudio(samples);
        }

        public void PushAudio(float[] samples)
        {
            _audioQueue.Enqueue(samples);
        }

        private bool isStreamAudioPlaying = false;

        private void PlayStreamAudio(float time)
        {
            lock (_audioQueue)
            {
                if (_audioQueue.Count > 0 && !GetEffectAudio(_streamSourceName).isPlaying)
                {
                    isStreamAudioPlaying = true;
                    float[] samples;
                    lock (_audioQueue)
                    {
                        samples = _audioQueue.Dequeue();
                    }

                    var clip = AudioClip.Create("tts", samples.Length, 1, _streamSampleRate, false);
                    clip.SetData(samples, 0);
                    GetEffectAudio(_streamSourceName).clip = clip;
                    GetEffectAudio(_streamSourceName).Play();
                }
                else if (isStreamAudioPlaying && _audioQueue.Count == 0 && !GetEffectAudio(_streamSourceName).isPlaying)
                {
                    isStreamAudioPlaying = false;
                    _streamAudioCallback?.Invoke();
                }
            }
        }
        
        public void StopStreamAudio(string sourceName)
        {
            if(string.IsNullOrEmpty(sourceName)) return;
            _audioQueue.Clear();
            isStreamAudioPlaying = false;
            GetEffectAudio(sourceName).clip = null;
            GetEffectAudio(sourceName).Stop();
            _streamAudioCallback?.Invoke();
        }

        public void CreateEffectAudio(string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName)) return;
            if (effectAudios.ContainsKey(sourceName)) return;
            var effect = new GameObject(sourceName, typeof(AudioSource));
            effect.transform.SetParent(background.transform);
            effectAudios[sourceName] = effect.GetOrAddComponent<AudioSource>();
            effectAudios[sourceName].playOnAwake = false;
        }

        private AudioSource GetEffectAudio(string sourceName = null)
        {
            return string.IsNullOrEmpty(sourceName) ? defaultEffectAudio : effectAudios.GetValueOrDefault(sourceName, defaultEffectAudio);
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="cb"></param>
        public void PlayBackgroundAudio(AudioClip clip, UnityAction cb = null)
        {
            PlayAudio(backgroundAudio, clip, cb, true, false);
        }

        /// <summary>
        /// 播放特效音乐
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="sourceName"></param>
        /// <param name="cb"></param>
        /// <param name="overlap"></param>
        public void PlayEffectAudio(AudioClip clip, string sourceName = null, UnityAction cb = null,
            bool overlap = false)
        {
            PlayAudio(GetEffectAudio(sourceName), clip, cb, false, overlap);
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetBackgroundVolume(float volume)
        {
            backgroundAudio.volume = volume;
        }

        /// <summary>
        /// 设置特效音乐音量
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="sourceName"></param>
        public void SetEffectVolume(float volume, string sourceName = null)
        {
            GetEffectAudio(sourceName).volume = volume;
        }

        public void SetEffectMute(bool mute, string sourceName = null)
        {
            GetEffectAudio(sourceName).mute = mute;
        }

        public void SetBackgroundMute(bool mute)
        {
            backgroundAudio.mute = mute;
        }

        public void StopEffectAudio(string sourceName = null)
        {
            StopAllCoroutines();
            StopAudio(GetEffectAudio(sourceName));
        }

        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopBackgroundMusic()
        {
            StopAllCoroutines();
            StopAudio(backgroundAudio);
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="_audio"></param>
        /// <param name="clip"></param>
        /// <param name="callback"></param>
        /// <param name="isLoop"></param>
        /// <param name="overlap"></param>
        private void PlayAudio(AudioSource _audio, AudioClip clip, UnityAction callback = null, bool isLoop = false,
            bool overlap = false)
        {
            _audio.loop = isLoop;
            if (overlap)
            {
                _audio.PlayOneShot(clip);
            }
            else
            {
                _audio.clip = clip;
                _audio.Play();
            }

            StartCoroutine(AudioPlayFinished(clip.length, callback));
        }

        private IEnumerator AudioPlayFinished(float time, UnityAction callback = null)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }

        /// <summary>
        /// 停止播放音乐
        /// </summary>
        /// <param name="_audio"></param>
        private void StopAudio(AudioSource _audio)
        {
            _audio.clip = null;
        }

        #region Recording

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

            if (_audioClip == null)
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
            int currentPosition = Microphone.GetPosition(_deviceName);
            int sampleCount = currentPosition - lastSamplePosition;

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
            byte[] processedAudio = ProcessAudioBuffer(audioBuffer, sampleCount);
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

        #endregion
    }
}