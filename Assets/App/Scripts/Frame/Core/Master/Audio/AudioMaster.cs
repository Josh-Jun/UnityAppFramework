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
        private GameObject background;
        private AudioSource backgroundAudio;
        private AudioSource defaultEffectAudio;
        private readonly Dictionary<string, AudioSource> effectAudios = new();



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
            InitializeStreamAudio();
        }

        private void OnDestroy()
        {
            ClearStreamAudio();
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

        public AudioSource GetEffectAudio(string sourceName = null)
        {
            return string.IsNullOrEmpty(sourceName) ? defaultEffectAudio : effectAudios.GetValueOrDefault(sourceName, defaultEffectAudio);
        }

        public void RemoveEffectAudio(string sourceName)
        {
            if (!effectAudios.TryGetValue(sourceName, out var effectAudio)) return;
            Destroy(effectAudio.gameObject);
            effectAudios.Remove(sourceName);
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
    }
}