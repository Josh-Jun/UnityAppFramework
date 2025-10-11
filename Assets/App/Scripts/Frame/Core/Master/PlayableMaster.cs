/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月15 11:58
 * function    : 
 * ===============================================
 * */

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
        public PlayableGraph graph;
        public AnimationPlayableOutput output;
    }
    
    public class PlayableMaster : SingletonMonoEvent<PlayableMaster>
    {
        private readonly Dictionary<string, PlayableData> _playables = new();
        private int timeTask = 0;
        
        public void Create(string key, Animator animator)
        {
            var data = new PlayableData()
            {
                name = name,
                animator = animator,
                graph = PlayableGraph.Create($"{name}Graph"),
            };
            data.output = AnimationPlayableOutput.Create(data.graph, $"{name}OutPut", animator);
            _playables.Add(name, data);
#if UNITY_EDITOR
            GraphVisualizerClient.Show(data.graph);
#endif
        }

        public void Remove(string  key)
        {
            if (!_playables.TryGetValue(name, out var playable)) return;
            playable.graph.Destroy();
            _playables.Remove(name);
        }
        
        public void Play(string key, int id)
        {
            if (!_playables.TryGetValue(name, out var data)) return;
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
            var layer = AnimationLayerMixerPlayable.Create(data.graph, playable.Clips.Length);
            data.output.SetSourcePlayable(layer);
            for (var i = 0; i < clipPairs.Count; i++)
            {
                var layerConfig = PlayableLayerJsonConfig.Instance.Get(clipPairs.ElementAt(i).Key);
                var value = clipPairs.ElementAt(i).Value;
                if (!string.IsNullOrEmpty(layerConfig.LayerAvatarMask))
                {
                    // layer.SetLayerAdditive((uint)layerConfig.Layer, true);
                    var mask = AssetsMaster.Instance.LoadAssetSync<AvatarMask>(layerConfig.LayerAvatarMask);
                    layer.SetLayerMaskFromAvatarMask((uint)layerConfig.Layer,  mask);
                }
                foreach (var clip in value)
                {
                    var clipPlayable = AnimationClipPlayable.Create(data.graph, clip);
                    data.graph.Connect(clipPlayable, 0, layer, layerConfig.Layer);
                    layer.SetInputWeight(layerConfig.Layer, 1f);
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
            if (!data.output.IsOutputValid()) return;

            // 获取输出连接的源 Playable
            var sourcePlayable = data.output.GetSourcePlayable();

            if (!sourcePlayable.IsValid()) return;
            // 递归销毁所有连接的节点
            DestroyConnectedPlayables(sourcePlayable);

            // 断开输出连接
            data.output.SetSourcePlayable(UnityEngine.Playables.Playable.Null);
        }

        private void DestroyConnectedPlayables(UnityEngine.Playables.Playable rootPlayable)
        {
            if (!rootPlayable.IsValid()) return;

            // 遍历所有输入连接
            for (var i = 0; i < rootPlayable.GetInputCount(); i++)
            {
                var inputPlayable = rootPlayable.GetInput(i);
                if (!inputPlayable.IsValid()) continue;
                // 递归销毁子节点
                DestroyConnectedPlayables(inputPlayable);
                // 断开连接
                rootPlayable.DisconnectInput(i);
            }

            // 销毁当前 Playable
            if (rootPlayable.IsValid())
            {
                rootPlayable.Destroy();
            }
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
