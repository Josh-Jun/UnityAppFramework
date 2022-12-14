﻿using System.Collections;
using AppFrame.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace AppFrame.Manager
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        private AudioSource backgroundAudio;
        private AudioSource effectAudio;

        public override void InitParent(Transform parent)
        {
            base.InitParent(parent);
            backgroundAudio = CreateAudioSource();
            effectAudio = CreateAudioSource();
        }

        private AudioSource CreateAudioSource(bool playOnAwake = false)
        {
            AudioSource audio = gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = playOnAwake;
            return audio;
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
        /// <param name="cb"></param>
        public void PlayEffectAudio(AudioClip clip, UnityAction cb = null, bool overlap = false)
        {
            PlayAudio(effectAudio, clip, cb, false, overlap);
        }

        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopBackgroundMusic()
        {
            StopAudio(backgroundAudio);
        }

        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="_audio"></param>
        /// <param name="clip"></param>
        /// <param name="callback"></param>
        /// <param name="isLoop"></param>
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

        private IEnumerator AudioPlayFinished(float time, UnityAction callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }

        /// <summary>
        /// 体质播放音乐
        /// </summary>
        /// <param name="_audio"></param>
        private void StopAudio(AudioSource _audio)
        {
            _audio.clip = null;
        }
    }
}
