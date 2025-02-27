using System;
using System.Collections.Generic;
using System.Linq;
using App.Core.Master;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace App.Core.Tools
{
    public static class Developer
    {
        #region TimeStamp

        /// <summary> DateTime2TimeStamp </summary>
        public static long ToTimeStamp(this DateTime target, bool ms = true)
        {
            var from = new DateTime(1970, 1, 1, 0, 0, 0);
            var ts = new TimeSpan(target.ToUniversalTime().Ticks - from.Ticks);
            var timestamp = ms ? Convert.ToInt64(ts.TotalMilliseconds) : Convert.ToInt64(ts.TotalSeconds);
            return timestamp;
        }

        /// <summary> TimeStamp2DateTime </summary>
        public static DateTime ToDateTime(this long timestamp, bool ms = true)
        {
            var from = new DateTime(1970, 1, 1, 0, 0, 0);
            var target = ms ? from.AddMilliseconds(timestamp) : from.AddSeconds(timestamp);
            return target;
        }

        #endregion
        
        #region AnimationClip
        
        public static void AddAnimationClipEvent<T>(this AnimationClip clip, int frame, T parameter)
        {
            var type = typeof(T);
            var _event  = new AnimationEvent
            {
                time = frame / clip.frameRate,
            };
            if (type == typeof(string))
            {
                _event.functionName = "AnimationEventStringCallback";
                _event.stringParameter = parameter as string;
            }
            else if (type == typeof(int))
            {
                _event.functionName = "AnimationEventIntCallback";
                _event.intParameter = parameter as int? ?? 0;
            }
            else if (type == typeof(float))
            {
                _event.functionName = "AnimationEventFloatCallback";
                _event.floatParameter = parameter as float? ?? 0;
            }
            else if (type == typeof(UnityEngine.Object))
            {
                _event.functionName = "AnimationEventObjectCallback";
                _event.objectReferenceParameter = parameter as UnityEngine.Object;
            }
            else
            {
                Log.W($"AnimationEvent type is not support!!! type:{type}");
            }
            clip.AddEvent(_event);
        }

        #endregion
        
        #region Animator Event

        private static int DEVELOP_ANIMATOR_TIME_ID = -1; //动画时间任务id
        private static int DEVELOP_FRAMES_TIME_ID = -1; //动画时间任务id

        public static void PlayBack(this Animator _animator, string stateName, UnityAction callback = null)
        {
            var AnimationClips = _animator.runtimeAnimatorController.animationClips;
            float _time = 0;
            foreach (var t in AnimationClips)
            {
                if (t.name == stateName)
                {
                    _time = t.length;
                }
            }

            _animator.enabled = true;
            _animator.StartPlayback();
            _animator.speed = -1;
            _animator.Play(stateName, 0, 1);
            DEVELOP_ANIMATOR_TIME_ID = TimeUpdateMaster.Instance.StartTimer((float time) =>
            {
                if (time >= _time)
                {
                    callback?.Invoke();
                    TimeUpdateMaster.Instance.EndTimer(DEVELOP_ANIMATOR_TIME_ID);
                }
            });
        }

        public static void Play(this Animator _animator, string stateName, UnityAction callback = null)
        {
            var AnimationClips = _animator.runtimeAnimatorController.animationClips;
            float _time = 0;
            foreach (var t in AnimationClips)
            {
                if (t.name == stateName)
                {
                    _time = t.length;
                }
            }

            _animator.enabled = true;
            _animator.StartPlayback();
            _animator.speed = 1;
            _animator.Play(stateName, 0, 0);
            DEVELOP_ANIMATOR_TIME_ID = TimeUpdateMaster.Instance.StartTimer((float time) =>
            {
                if (time >= _time)
                {
                    callback?.Invoke();
                    TimeUpdateMaster.Instance.EndTimer(DEVELOP_ANIMATOR_TIME_ID);
                }
            });
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="_animator"></param>
        /// <param name="stateName"></param>
        /// <param name="speed"></param>
        /// <param name="callback"></param>
        public static void Play(this Animator _animator, string stateName, float speed, UnityAction callback = null)
        {
            var AnimationClips = _animator.runtimeAnimatorController.animationClips;
            float _time = 0;
            foreach (var t in AnimationClips)
            {
                if (t.name == stateName)
                {
                    _time = t.length;
                }
            }

            _animator.enabled = true;
            _animator.StartPlayback();
            _animator.speed = speed;
            _animator.Play(stateName, 0, speed < 0 ? 1 : 0);
            DEVELOP_ANIMATOR_TIME_ID = TimeUpdateMaster.Instance.StartTimer((float time) =>
            {
                if (time >= _time)
                {
                    callback?.Invoke();
                    TimeUpdateMaster.Instance.EndTimer(DEVELOP_ANIMATOR_TIME_ID);
                }
            });
        }

        public static void PlayFrames(this Image image, List<Sprite> sequenceFrames, float time = 0.05f, UnityAction callback = null, bool loop = false, bool isNativeSize = false)
        {
            if (image == null)
            {
                Log.E("Image is null!!!");
                return;
            }
            
            var index = 0; //可以用来控制起始播放的动画帧索引
            float recordTime = 0;
            DEVELOP_FRAMES_TIME_ID = TimeUpdateMaster.Instance.StartTimer((float currentTime) =>
            {
                if (currentTime - recordTime >= time)
                {
                    recordTime = currentTime;
                    //当我们需要在整个动画播放完之后  重复播放后面的部分 就可以展现我们纯代码播放的自由性
                    if (index > sequenceFrames.Count - 1)
                    {
                        callback?.Invoke();
                        if (loop)
                        {
                            index = 0;
                        }
                        else
                        {
                            TimeUpdateMaster.Instance.EndTimer(DEVELOP_FRAMES_TIME_ID);
                        }
                    }
                    else
                    {
                        image.sprite = sequenceFrames[index];
                        index++;
                        if (isNativeSize)
                        {
                            image.SetNativeSize();
                        }
                    }
                }
            });
        }

        public static void PlayFrames(this Image image, Sprite[] sequenceFrames, float time = 0.05f, UnityAction callback = null, bool loop = false, bool isNativeSize = false)
        {
            if (image == null)
            {
                Log.E("Image is null!!!");
                return;
            }

            var index = 0; //可以用来控制起始播放的动画帧索引
            float recordTime = 0;
            DEVELOP_FRAMES_TIME_ID = TimeUpdateMaster.Instance.StartTimer((float currentTime) =>
            {
                if (currentTime - recordTime >= time)
                {
                    recordTime = currentTime;
                    //当我们需要在整个动画播放完之后  重复播放后面的部分 就可以展现我们纯代码播放的自由性
                    if (index > sequenceFrames.Length - 1)
                    {
                        callback?.Invoke();
                        if (loop)
                        {
                            index = 0;
                        }
                        else
                        {
                            TimeUpdateMaster.Instance.EndTimer(DEVELOP_FRAMES_TIME_ID);
                        }
                    }
                    else
                    {
                        image.sprite = sequenceFrames[index];
                        index++;
                        if (isNativeSize)
                        {
                            image.SetNativeSize();
                        }
                    }
                }
            });
        }

        #endregion

        #region Enable Objects & Component

        /// <summary>
        /// 设置游戏对象显示隐藏
        /// </summary>
        /// <param name="go"></param>
        /// <param name="enable"></param>
        public static void SetGameObjectActive(this GameObject go, bool enable = true)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            go.SetActive(enable);
        }

        /// <summary>
        /// 设置脚本所在游戏对象的显示隐藏
        /// </summary>
        /// <param name="com"></param>
        /// <param name="enable"></param>
        public static void SetGameObjectActive(this Component com, bool enable = true)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            com.gameObject.SetActive(enable);
        }

        /// <summary>
        /// 设置游戏对象上脚本的开启和关闭
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="enable"></param>
        public static void SetComponentEnable<T>(this GameObject go, bool enable = true) where T : Behaviour
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            T t = go.GetComponent<T>();
            if (t != null)
            {
                t.enabled = enable;
            }
            else
            {
                Log.E($"对象{go.name} --- 没有脚本:{typeof(T).Name}");
            }
        }

        /// <summary>
        /// 设置脚本所在游戏对象上的脚本的开启和关闭
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="com"></param>
        /// <param name="enable"></param>
        public static void SetComponentEnable<T>(this Component com, bool enable = true) where T : Behaviour
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            T t = com.gameObject.GetComponent<T>();
            if (t != null)
            {
                t.enabled = enable;
            }
            else
            {
                Log.E("对象{com.gameObject.name} --- 没有脚本:{typeof(T).Name}");
            }
        }

        #endregion

        #region Get Or Add Component

        /// <summary>
        /// 获取GameObject上的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return null;
            }

            T t = go.GetComponent<T>();
            if (t == null)
            {
                t = go.AddComponent<T>();
            }

            return t;
        }

        /// <summary>
        /// 获取Component对象上的其他组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="com"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this Component com) where T : Component
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return null;
            }

            T t = com.GetComponent<T>();
            if (t == null)
            {
                t = com.gameObject.AddComponent<T>();
            }

            return t;
        }

        #endregion

        #region OnClick params

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        public static void OnClick(this GameObject go, UnityAction cb)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onClick = (obj) => { cb?.Invoke(); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg"></param>
        public static void OnClick<T>(this GameObject go, UnityAction<T> cb, T arg)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onClick = (obj) => { cb?.Invoke(arg); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void OnClick<T0, T1>(this GameObject go, UnityAction<T0, T1> cb, T0 arg0, T1 arg1)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onClick = (obj) => { cb?.Invoke(arg0, arg1); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void OnClick<T0, T1, T2>(this GameObject go, UnityAction<T0, T1, T2> cb, T0 arg0, T1 arg1,
            T2 arg2)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onClick = (obj) => { cb?.Invoke(arg0, arg1, arg2); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void OnClick<T0, T1, T2, T3>(this GameObject go, UnityAction<T0, T1, T2, T3> cb, T0 arg0, T1 arg1,
            T2 arg2, T3 arg3)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onClick = (obj) => { cb?.Invoke(arg0, arg1, arg2, arg3); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        public static void OnClick(this Component com, UnityAction cb)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onClick = (obj) => { cb?.Invoke(); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg"></param>
        public static void OnClick<T>(this Component com, UnityAction<T> cb, T arg)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onClick = (obj) => { cb?.Invoke(arg); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void OnClick<T0, T1>(this Component com, UnityAction<T0, T1> cb, T0 arg0, T1 arg1)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onClick = (obj) => { cb?.Invoke(arg0, arg1); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void OnClick<T0, T1, T2>(this Component com, UnityAction<T0, T1, T2> cb, T0 arg0, T1 arg1,
            T2 arg2)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onClick = (obj) => { cb?.Invoke(arg0, arg1, arg2); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void OnClick<T0, T1, T2, T3>(this Component com, UnityAction<T0, T1, T2, T3> cb, T0 arg0, T1 arg1,
            T2 arg2, T3 arg3)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onClick = (obj) => { cb?.Invoke(arg0, arg1, arg2, arg3); };
        }
        
        #endregion
        
        #region BtnOnClick params
        
        /// <summary>
        /// 多个参数的Button点击事件
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="cb"></param>
        public static void BtnOnClick(this Button btn, UnityAction cb)
        {
            if (btn == null)
            {
                Log.E($"Button({btn}) is null!!!");
                return;
            }

            btn.onClick.AddListener(cb);
        }

        /// <summary>
        /// 多个参数的Button点击事件
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="cb"></param>
        /// <param name="arg"></param>
        public static void BtnOnClick<T>(this Button btn, UnityAction<T> cb, T arg)
        {
            if (btn == null)
            {
                Log.E($"Button({btn}) is null!!!");
                return;
            }

            btn.onClick.AddListener(() => { cb?.Invoke(arg); });
        }

        /// <summary>
        /// 多个参数的Button点击事件
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void BtnOnClick<T0, T1>(this Button btn, UnityAction<T0, T1> cb, T0 arg0, T1 arg1)
        {
            if (btn == null)
            {
                Log.E($"Button({btn}) is null!!!");
                return;
            }

            btn.onClick.AddListener(() => { cb?.Invoke(arg0, arg1); });
        }

        /// <summary>
        /// 多个参数的Button点击事件
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void BtnOnClick<T0, T1, T2>(this Button btn, UnityAction<T0, T1, T2> cb, T0 arg0, T1 arg1,
            T2 arg2)
        {
            if (btn == null)
            {
                Log.E($"Button({btn}) is null!!!");
                return;
            }

            btn.onClick.AddListener(() => { cb?.Invoke(arg0, arg1, arg2); });
        }

        /// <summary>
        /// 多个参数的Button点击事件
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void BtnOnClick<T0, T1, T2, T3>(this Button btn, UnityAction<T0, T1, T2, T3> cb, T0 arg0, T1 arg1,
            T2 arg2, T3 arg3)
        {
            if (btn == null)
            {
                Log.E($"Button({btn}) is null!!!");
                return;
            }

            btn.onClick.AddListener(() => { cb?.Invoke(arg0, arg1, arg2, arg3); });
        }

        #endregion

        #region OnDown params

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        public static void OnDown(this GameObject go, UnityAction cb)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onDown = (obj) => { cb?.Invoke(); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg"></param>
        public static void OnDown<T>(this GameObject go, UnityAction<T> cb, T arg)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onDown = (obj) => { cb?.Invoke(arg); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void OnDown<T0, T1>(this GameObject go, UnityAction<T0, T1> cb, T0 arg0, T1 arg1)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onDown = (obj) => { cb?.Invoke(arg0, arg1); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void OnDown<T0, T1, T2>(this GameObject go, UnityAction<T0, T1, T2> cb, T0 arg0, T1 arg1,
            T2 arg2)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onDown = (obj) => { cb?.Invoke(arg0, arg1, arg2); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void OnDown<T0, T1, T2, T3>(this GameObject go, UnityAction<T0, T1, T2, T3> cb, T0 arg0, T1 arg1,
            T2 arg2, T3 arg3)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onDown = (obj) => { cb?.Invoke(arg0, arg1, arg2, arg3); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        public static void OnDown(this Component com, UnityAction cb)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onDown = (obj) => { cb?.Invoke(); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg"></param>
        public static void OnDown<T>(this Component com, UnityAction<T> cb, T arg)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onDown = (obj) => { cb?.Invoke(arg); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void OnDown<T0, T1>(this Component com, UnityAction<T0, T1> cb, T0 arg0, T1 arg1)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onDown = (obj) => { cb?.Invoke(arg0, arg1); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void OnDown<T0, T1, T2>(this Component com, UnityAction<T0, T1, T2> cb, T0 arg0, T1 arg1,
            T2 arg2)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onDown = (obj) => { cb?.Invoke(arg0, arg1, arg2); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void OnDown<T0, T1, T2, T3>(this Component com, UnityAction<T0, T1, T2, T3> cb, T0 arg0, T1 arg1,
            T2 arg2, T3 arg3)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onDown = (obj) => { cb?.Invoke(arg0, arg1, arg2, arg3); };
        }
        
        #endregion

        #region OnUp params

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        public static void OnUp(this GameObject go, UnityAction cb)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onUp = (obj) => { cb?.Invoke(); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg"></param>
        public static void OnUp<T>(this GameObject go, UnityAction<T> cb, T arg)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onUp = (obj) => { cb?.Invoke(arg); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void OnUp<T0, T1>(this GameObject go, UnityAction<T0, T1> cb, T0 arg0, T1 arg1)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onUp = (obj) => { cb?.Invoke(arg0, arg1); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void OnUp<T0, T1, T2>(this GameObject go, UnityAction<T0, T1, T2> cb, T0 arg0, T1 arg1,
            T2 arg2)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onUp = (obj) => { cb?.Invoke(arg0, arg1, arg2); };
        }

        /// <summary>
        /// 多个参数的GameObject点击事件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void OnUp<T0, T1, T2, T3>(this GameObject go, UnityAction<T0, T1, T2, T3> cb, T0 arg0, T1 arg1,
            T2 arg2, T3 arg3)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return;
            }

            EventListener.Get(go).onUp = (obj) => { cb?.Invoke(arg0, arg1, arg2, arg3); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        public static void OnUp(this Component com, UnityAction cb)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onUp = (obj) => { cb?.Invoke(); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg"></param>
        public static void OnUp<T>(this Component com, UnityAction<T> cb, T arg)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onUp = (obj) => { cb?.Invoke(arg); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void OnUp<T0, T1>(this Component com, UnityAction<T0, T1> cb, T0 arg0, T1 arg1)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onUp = (obj) => { cb?.Invoke(arg0, arg1); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void OnUp<T0, T1, T2>(this Component com, UnityAction<T0, T1, T2> cb, T0 arg0, T1 arg1,
            T2 arg2)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onUp = (obj) => { cb?.Invoke(arg0, arg1, arg2); };
        }

        /// <summary>
        /// 多个参数的组件点击事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="cb"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void OnUp<T0, T1, T2, T3>(this Component com, UnityAction<T0, T1, T2, T3> cb, T0 arg0, T1 arg1,
            T2 arg2, T3 arg3)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return;
            }

            EventListener.Get(com).onUp = (obj) => { cb?.Invoke(arg0, arg1, arg2, arg3); };
        }

        #endregion

        #region EventTrigger

        /// <summary>
        /// 添加EventTrigger事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventType"></param>
        /// <param name="ua"></param>
        public static void AddEventTrigger(this GameObject obj, EventTriggerType eventType, UnityAction<BaseEventData> ua)
        {
            if (obj == null)
            {
                Log.E($"GameObject({obj.name}) is null!!!");
                return;
            }

            var eventTrigger = obj.GetOrAddComponent<EventTrigger>();

            var callback = new UnityAction<BaseEventData>(ua);

            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(callback);
            eventTrigger.triggers.Add(entry);
        }

        /// <summary>
        /// 移除EventTrigger事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="triggerType"></param>
        /// <param name="ua"></param>
        public static void RemoveEventTrigger(this GameObject obj, EventTriggerType triggerType, UnityAction<BaseEventData> ua)
        {
            if (obj == null)
            {
                Log.E($"GameObject({obj.name}) is null!!!");
                return;
            }

            var eventTrigger = obj.GetOrAddComponent<EventTrigger>();
            var entry = eventTrigger.triggers.Find(s => s.eventID == triggerType);

            if (entry != null)
            {
                entry.callback.RemoveListener(ua);
                eventTrigger.triggers.Remove(entry);
            }
        }
        
        /// <summary>
        /// 移除EventTrigger事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventType"></param>
        /// <param name="isClear"></param>
        public static void RemoveEventTrigger(this GameObject obj, EventTriggerType eventType, bool isClear = false)
        {
            if (obj == null)
            {
                Log.E($"GameObject({obj.name}) is null!!!");
                return;
            }

            var eventTrigger = obj.GetOrAddComponent<EventTrigger>();
            var entry = eventTrigger.triggers.Find(s => s.eventID == eventType);

            if (entry != null)
            {
                entry.callback.RemoveAllListeners();
                if (isClear)
                {
                    eventTrigger.triggers.Remove(entry);
                }
            }
        }
        
        /// <summary>
        /// 移除EventTrigger事件(移除所有)
        /// </summary>
        /// <param name="obj"></param>
        public static void RemoveAllEventTrigger(this GameObject obj)
        {
            if (obj == null)
            {
                Log.E($"GameObject({obj.name}) is null!!!");
                return;
            }

            var eventTrigger = obj.GetOrAddComponent<EventTrigger>();
            eventTrigger.triggers.Clear();
            UnityEngine.Object.Destroy(eventTrigger);
        }

        /// <summary>
        /// 添加EventTrigger事件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="eventType"></param>
        /// <param name="ua"></param>
        public static void AddEventTrigger(this Component com, EventTriggerType eventType, UnityAction<BaseEventData> ua)
        {
            if (com == null)
            {
                Log.E($"GameObject({com}) is null!!!");
                return;
            }

            var eventTrigger = com.GetOrAddComponent<EventTrigger>();

            var callback = new UnityAction<BaseEventData>(ua);

            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(callback);
            eventTrigger.triggers.Add(entry);
        }

        /// <summary>
        /// 移除EventTrigger事件(移除一个方法)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventType"></param>
        /// <param name="ua"></param>
        public static void RemoveEventTrigger(this Component obj, EventTriggerType eventType, UnityAction<BaseEventData> ua)
        {
            if (obj == null)
            {
                Log.E($"GameObject({obj.name}) is null!!!");
                return;
            }

            var eventTrigger = obj.GetOrAddComponent<EventTrigger>();
            var entry = eventTrigger.triggers.Find(s => s.eventID == eventType);

            if (entry != null)
            {
                entry.callback.RemoveListener(ua);
                eventTrigger.triggers.Remove(entry);
            }
        }
        
        /// <summary>
        /// 移除EventTrigger事件(移除一个事件类型)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventType"></param>
        /// <param name="isClear"></param>
        public static void RemoveEventTrigger(this Component obj, EventTriggerType eventType, bool isClear = false)
        {
            if (obj == null)
            {
                Log.E($"GameObject({obj.name}) is null!!!");
                return;
            }

            var eventTrigger = obj.GetOrAddComponent<EventTrigger>();
            var entry = eventTrigger.triggers.Find(s => s.eventID == eventType);

            if (entry != null)
            {
                entry.callback.RemoveAllListeners();
                if (isClear)
                {
                    eventTrigger.triggers.Remove(entry);
                }
            }
        }
        
        /// <summary>
        /// 移除EventTrigger事件(移除所有)
        /// </summary>
        /// <param name="com"></param>
        public static void RemoveAllEventTrigger(this Component com)
        {
            if (com == null)
            {
                Log.E($"GameObject({com}) is null!!!");
                return;
            }

            var eventTrigger = com.GetOrAddComponent<EventTrigger>();
            eventTrigger.triggers.Clear();
            UnityEngine.Object.Destroy(eventTrigger);
        }

        #endregion

        #region Find Objects

        /// <summary>
        /// 根据路径查找gameObject
        /// </summary>
        /// <param name="go"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GameObject FindGameObject(this GameObject go, string path)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return null;
            }

            return go.transform.Find(path).gameObject;
        }

        /// <summary>
        /// 根据路径查找gameObject
        /// </summary>
        /// <param name="com"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GameObject FindGameObject(this Component com, string path)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return null;
            }

            return com.transform.Find(path).gameObject;
        }

        /// <summary>
        /// 根据路径查找组件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Component FindComponent(this GameObject go, string path)
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return null;
            }

            return go.transform.Find(path).GetComponent<Component>();
        }

        /// <summary>
        /// 根据路径查找组件
        /// </summary>
        /// <param name="com"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Component FindComponent(this Component com, string path)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return null;
            }

            return com.transform.Find(path).GetComponent<Component>();
        }

        /// <summary>
        /// 根据路径查找组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T FindComponent<T>(this GameObject go, string path) where T : Component
        {
            if (go == null)
            {
                Log.E($"GameObject({go.name}) is null!!!");
                return null;
            }

            return go.transform.Find(path).GetComponent<T>();
        }

        /// <summary>
        /// 根据路径查找组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="com"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T FindComponent<T>(this Component com, string path) where T : Component
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return null;
            }

            return com.transform.Find(path).GetComponent<T>();
        }

        /// <summary>
        /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
        /// </summary>
        /// <param name="gomeObject">要在其中进行查找的父物体</param>
        /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
        /// <returns></returns>
        public static GameObject FindDeepGameObject(this GameObject gomeObject, string childName)
        {
            if (gomeObject == null)
            {
                Log.E($"GameObject({gomeObject.name}) is null!!!");
                return null;
            }

            var transform = gomeObject.transform.Find(childName);
            if (transform == null)
            {
                foreach (Transform trs in gomeObject.transform)
                {
                    transform = trs.gameObject.FindDeepGameObject(childName).transform;
                    if (transform != null)
                        return transform.gameObject;
                }
            }

            return transform.gameObject;
        }

        /// <summary>
        /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
        /// </summary>
        /// <param name="com">要在其中进行查找的父物体</param>
        /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
        /// <returns></returns>
        public static GameObject FindDeepGameObject(this Component com, string childName)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return null;
            }

            var transform = com.transform.Find(childName);
            if (transform == null)
            {
                foreach (Transform trs in com.transform)
                {
                    transform = trs.FindDeepGameObject(childName).transform;
                    if (transform != null)
                        return transform.gameObject;
                }
            }

            return transform.gameObject;
        }

        /// <summary>
        /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
        /// </summary>
        /// <param name="gomeObject">要在其中进行查找的父物体</param>
        /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
        /// <returns></returns>
        public static Component FindDeepComponent(this GameObject gomeObject, string childName)
        {
            if (gomeObject == null)
            {
                Log.E($"GameObject({gomeObject.name}) is null!!!");
                return null;
            }

            var transform = gomeObject.transform.Find(childName);
            if (transform == null)
            {
                foreach (Transform trs in gomeObject.transform)
                {
                    transform = trs.gameObject.FindDeepComponent(childName).transform;
                    if (transform != null)
                        return transform.GetComponent<Component>();
                }
            }

            return transform.GetComponent<Component>();
        }

        /// <summary>
        /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
        /// </summary>
        /// <param name="com">要在其中进行查找的父物体</param>
        /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
        /// <returns></returns>
        public static Component FindDeepComponet(this Component com, string childName)
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return null;
            }

            var transform = com.transform.Find(childName);
            if (transform == null)
            {
                foreach (Transform trs in com.transform)
                {
                    transform = trs.FindDeepComponet(childName).transform;
                    if (transform != null)
                        return transform.GetComponent<Component>();
                }
            }

            return transform.GetComponent<Component>();
        }

        /// <summary>
        /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
        /// </summary>
        /// <param name="gomeObject">要在其中进行查找的父物体</param>
        /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
        /// <returns></returns>
        public static T FindDeepComponent<T>(this GameObject gomeObject, string childName) where T : Component
        {
            if (gomeObject == null)
            {
                Log.E($"GameObject({gomeObject.name}) is null!!!");
                return null;
            }

            var transform = gomeObject.transform.Find(childName);
            if (transform == null)
            {
                foreach (Transform trs in gomeObject.transform)
                {
                    transform = trs.gameObject.FindDeepComponent<T>(childName).transform;
                    if (transform != null)
                        return transform.GetComponent<T>();
                }
            }

            return transform.GetComponent<T>();
        }

        /// <summary>
        /// 查找本游戏物体下的特定名称的子物体系统，并将其返回
        /// </summary>
        /// <param name="com">要在其中进行查找的父物体</param>
        /// <param name="childName">待查找的子物体名称，可以是"/"分割的多级名称</param>
        /// <returns></returns>
        public static T FindDeepComponet<T>(this Component com, string childName) where T : Component
        {
            if (com == null)
            {
                Log.E($"Component({com}) is null!!!");
                return null;
            }

            var transform = com.transform.Find(childName);
            if (transform == null)
            {
                foreach (Transform trs in com.transform)
                {
                    transform = trs.FindDeepComponet<T>(childName).transform;
                    if (transform != null)
                        return transform.GetComponent<T>();
                }
            }

            return transform.GetComponent<T>();
        }

        #endregion

        #region FindLoopSelectable

        /// <summary>
        /// 循环查找Selectable
        /// </summary>
        /// <param name="current"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Selectable FindLoopSelectable(this Selectable current, Vector3 dir)
        {
            var first = current.FindSelectable(dir); //用一个向量Vector3去寻找第一个Selectable
            if (first != null) //如果下一个为null，用递归方法循环继续寻找第一个
            {
                current = first.FindLoopSelectable(dir);
            }

            return current;
        }

        #endregion

        #region String2Vector3

        public static Vector3 ToVector3(this string str, char separator = ',')
        {
            if(string.IsNullOrEmpty(str)) return Vector3.zero;
            var split = str.Split(separator);
            if (split.Length != 3) return Vector3.zero;
            float.TryParse(split[0], out var x);
            float.TryParse(split[1], out var y);
            float.TryParse(split[2], out var z);
            return new Vector3(x, y, z);
        }
        
        public static Vector3[] ToVector3Array(this string str, char separator1 = '|', char separator2 = ',')
        {
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator1);
            var arr = new Vector3[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                var v= Vector3.zero;
                var splits = split[i].Split(separator2);
                if (splits.Length == 3)
                {
                    float.TryParse(splits[0], out var x);
                    float.TryParse(splits[1], out var y);
                    float.TryParse(splits[2], out var z);
                    v = new Vector3(x, y, z);
                }
                arr[i] = v;
            }
            return arr;
        }

        public static List<Vector3> ToVector3List(this string str, char separator1 = '|', char separator2 = ',')
        {
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator1);
            var list = new List<Vector3>();
            foreach (var t in split)
            {
                var v= Vector3.zero;
                var s = t.Split(separator2);
                if (s.Length == 3)
                {
                    float.TryParse(s[0], out var x);
                    float.TryParse(s[1], out var y);
                    float.TryParse(s[2], out var z);
                    v = new Vector3(x, y, z);
                }
                list.Add(v);
            }
            return list;
        }
        
        #endregion

        #region String2Int32
        
        public static int[] ToInt32Array(this string str, char separator = '|')
        {
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator);
            var arr = new int[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                if (int.TryParse(split[i], out var value))
                {
                    arr[i] = value;
                }
            }
            return arr;
        }
        
        public static List<int> ToInt32List(this string str, char separator = '|')
        {
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator);
            var list = split.Select(t => int.TryParse(t, out var value) ? value : 0).ToList();
            return list;
        }

        #endregion
        
        #region String2Float

        public static float[] ToFloatArray(this string str, char separator = '|')
        {
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator);
            var arr = new float[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                if (float.TryParse(split[i], out var value))
                {
                    arr[i] = value;
                }
            }
            return arr;
        }
        
        public static List<float> ToFloatList(this string str, char separator = '|')
        {
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator);
            var list = split.Select(t => float.TryParse(t, out var value) ? value : 0).ToList();
            return list;
        }
        
        #endregion
        
        #region String2Int64

        public static long[] ToInt64Array(this string str, char separator = '|')
        {
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator);
            var arr = new long[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                if (long.TryParse(split[i], out var value))
                {
                    arr[i] = value;
                }
            }
            return arr;
        }
        
        public static List<long> ToInt64List(this string str, char separator = '|')
        {
            
            if(string.IsNullOrEmpty(str)) return null;
            var split = str.Split(separator);
            var list = split.Select(t => long.TryParse(t, out var value) ? value : 0).ToList();
            return list;
        }

        #endregion

        #region Color
        
        public static string SetColor(this string str, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
        }

        public static Color ToColor(this string str)
        {
            if (ColorUtility.TryParseHtmlString(str, out var color))
            {
                return color;
            }
            return Color.black;
        }

        #endregion
    }
}