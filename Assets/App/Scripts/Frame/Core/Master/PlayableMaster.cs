/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月15 11:58
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace App.Core.Master
{
    public class PlayableData
    {
        public string name;
        public Animator animator;
        public Action onComplete;
        public PlayableGraph graph;
        public AnimationPlayableOutput output;
        public AnimationLayerMixerPlayable LayerMixer;
    }
    
    public class PlayableMaster : SingletonMonoEvent<PlayableMaster>
    {
        private readonly Dictionary<string, PlayableData> _playables = new();
        private int timeTask = 0;
        
        public void Create(string key, Animator animator)
        {
            var data = new PlayableData()
            {
                name = key,
                animator = animator,
                graph = PlayableGraph.Create($"{key}Graph"),
            };
            data.output = AnimationPlayableOutput.Create(data.graph, $"{key}OutPut", animator);
            _playables.Add(key, data);
#if UNITY_EDITOR
            GraphVisualizerClient.Show(data.graph);
#endif
        }

        public void Remove(string  key)
        {
            if (!_playables.TryGetValue(key, out var playable)) return;
            playable.graph.Destroy();
            _playables.Remove(key);
        }
        
        public void Play(string key, int id)
        {
            if (!_playables.TryGetValue(key, out var data)) return;
            var playable = PlayableJsonConfig.Instance.Get(id);

            var clipPairs = new Dictionary<int, List<AnimationClip>>();

            foreach (var clipId in playable.Clips)
            {
                var animationClips = PlayableClipJsonConfig.Instance.Get(clipId);
                var clip = AssetsMaster.Instance.LoadAssetSync<AnimationClip>(animationClips.Path);
                if (!clipPairs.ContainsKey(animationClips.Layer))
                {
                    var clips = new List<AnimationClip> { clip };
                    clipPairs.Add(animationClips.Layer, clips);
                }
                else
                {
                    clipPairs[animationClips.Layer].Add(clip);
                }
            }

            var duration = 0f;
            data.LayerMixer = AnimationLayerMixerPlayable.Create(data.graph, playable.Clips.Length);
            data.output.SetSourcePlayable(data.LayerMixer);
            for (var i = 0; i < clipPairs.Count; i++)
            {
                var layerConfig = PlayableLayerJsonConfig.Instance.Get(clipPairs.ElementAt(i).Key);
                var value = clipPairs.ElementAt(i).Value;
                if (!string.IsNullOrEmpty(layerConfig.LayerAvatarMask))
                {
                    // data.LayerMixer.SetLayerAdditive((uint)layerConfig.Layer, true);
                    var mask = AssetsMaster.Instance.LoadAssetSync<AvatarMask>(layerConfig.LayerAvatarMask);
                    data.LayerMixer.SetLayerMaskFromAvatarMask((uint)layerConfig.Layer,  mask);
                }
                foreach (var clip in value)
                {
                    var clipPlayable = AnimationClipPlayable.Create(data.graph, clip);
                    data.graph.Connect(clipPlayable, 0, data.LayerMixer, layerConfig.Layer);
                    data.LayerMixer.SetInputWeight(layerConfig.Layer, 1f);
                    if (duration < clip.length)
                    {
                        duration = clip.length;
                    }
                }
            }

            if (!playable.IsLoop)
            {
                timeTask = TimeTaskMaster.Instance.AddTimeTask(() => { OnAnimationCompleted(data); }, duration);
            }
            data.graph.Play();
        }
        private void OnAnimationCompleted(PlayableData data)
        {
            TimeTaskMaster.Instance.DeleteTimeTask(timeTask);
            data.onComplete?.Invoke();
        }

        private void OnDestroy()
        {
            foreach (var pair in _playables)
            {
#if UNITY_EDITOR
                GraphVisualizerClient.Hide(pair.Value.graph);
#endif
                pair.Value.graph.Destroy();
            }

            _playables.Clear();
        }
    }
}
