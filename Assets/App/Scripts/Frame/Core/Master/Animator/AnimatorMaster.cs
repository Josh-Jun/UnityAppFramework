/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月23 15:44
 * function    : 
 * ===============================================
 * */

using System;
using UnityEngine;
using App.Core.Tools;

namespace App.Core.Master
{
    public class AnimatorMaster : SingletonMono<AnimatorMaster>
    {
        private const string FUNCTION_STRING_NAME = "AnimationEventStringCallback";
        private const string FUNCTION_INT_NAME = "AnimationEventIntCallback";
        private const string FUNCTION_FLOAT_NAME = "AnimationEventFloatCallback";
        private const string FUNCTION_OBJECT_NAME = "AnimationEventObjectCallback";

        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
        }
        
        public void InitAnimationClipEvent<T>(AnimationClip clip, int frame, T parameter)
        {
            var type = typeof(T);
            var _event  = new AnimationEvent
            {
                time = frame / clip.frameRate,
                functionName = GetAnimationEventFunctionName(type),
            };
            if (type == typeof(string))
            {
                _event.stringParameter = parameter as string;
            }
            else if (type == typeof(int))
            {
                _event.intParameter = parameter as int? ?? 0;
            }
            else if (type == typeof(float))
            {
                _event.floatParameter = parameter as float? ?? 0;
            }
            else if (type == typeof(UnityEngine.Object))
            {
                _event.objectReferenceParameter = parameter as UnityEngine.Object;
            }
            clip.AddEvent(_event);
        }
        
        private string GetAnimationEventFunctionName(Type type)
        {
            if (type == typeof(string))
            {
                return FUNCTION_STRING_NAME;
            }
            else if (type == typeof(int))
            {
                return FUNCTION_INT_NAME;
            }
            else if (type == typeof(float))
            {
                return FUNCTION_FLOAT_NAME;
            }
            else if (type == typeof(UnityEngine.Object))
            {
                return FUNCTION_OBJECT_NAME;
            }
            Log.W($"当前事件类型未处理:{type}");
            return string.Empty;
        }
    }
}
