﻿using System;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    /// <summary>功能:计时更新</summary>
    public class TimeUpdateMaster : SingletonMono<TimeUpdateMaster>
    {
        private int timeId = 0; //下标
        private readonly Dictionary<UpdateMold, List<TimerData>> timerPairs = new Dictionary<UpdateMold, List<TimerData>>();

        public float GameTime
        {
            private set { }
            get { return Time.time; }
        }

        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
        }

        private TimerData timeData;

        private void Update()
        {
            if (timerPairs.ContainsKey(UpdateMold.Update))
            {
                if (timerPairs[UpdateMold.Update].Count > 0)
                {
                    for (int i = 0, length = timerPairs[UpdateMold.Update].Count; i < length; i++)
                    {
                        timeData = timerPairs[UpdateMold.Update][i];
                        if (timeData.isTime)
                        {
                            timeData.addTime += Time.deltaTime;
                            timeData.cb(timeData.addTime);
                        }
                    }
                }
            }
        }

        private TimerData fixedTimerData;

        private void FixedUpdate()
        {
            if (timerPairs.ContainsKey(UpdateMold.FixedUpdate))
            {
                if (timerPairs[UpdateMold.FixedUpdate].Count > 0)
                {
                    for (int i = 0, length = timerPairs[UpdateMold.FixedUpdate].Count; i < length; i++)
                    {
                        fixedTimerData = timerPairs[UpdateMold.FixedUpdate][i];
                        if (fixedTimerData.isTime)
                        {
                            fixedTimerData.addTime += Time.fixedDeltaTime;
                            var time = (float)Math.Round(fixedTimerData.addTime, 2, MidpointRounding.AwayFromZero);
                            fixedTimerData.cb(time);
                        }
                    }
                }
            }
        }

        private TimerData lateTimeData;
        private void LateUpdate()
        {
            if (timerPairs.ContainsKey(UpdateMold.LateUpdate))
            {
                if (timerPairs[UpdateMold.LateUpdate].Count > 0)
                {
                    for (int i = 0, length = timerPairs[UpdateMold.LateUpdate].Count; i < length; i++)
                    {
                        lateTimeData = timerPairs[UpdateMold.LateUpdate][i];
                        if (lateTimeData.isTime)
                        {
                            lateTimeData.addTime += Time.deltaTime;
                            lateTimeData.cb(timeData.addTime);
                        }
                    }
                }
            }
        }

        /// <summary>开始计时</summary>
        public int StartTimer(Action<float> cb, UpdateMold updateMold = UpdateMold.Update)
        {
            timeId += 1;
            var timer = new TimerData
            {
                id = timeId,
                isTime = true,
                addTime = 0,
                cb = cb,
            };
            
            if (timerPairs.TryGetValue(updateMold, value: out var pair))
            {
                pair.Add(timer);
            }
            else
            {
                var data = new List<TimerData> { timer };
                timerPairs.Add(updateMold, data);
            }
            return timeId;
        }

        /// <summary>暂停计时</summary>
        public void PauseTimer(int id)
        {
            foreach (var data in timerPairs.Values)
            {
                foreach (var t in data)
                {
                    if (t.id == id)
                    {
                        t.isTime = false;
                        break;
                    }
                }
            }
        }

        /// <summary>继续计时</summary>
        public void ContinueTimer(int id)
        {
            foreach (var data in timerPairs.Values)
            {
                foreach (var t in data)
                {
                    if (t.id == id)
                    {
                        t.isTime = true;
                        break;
                    }
                }
            }
        }

        /// <summary>结束计时</summary>
        public void EndTimer(int id)
        {
            foreach (var data in timerPairs.Values)
            {
                for (var i = 0; i < data.Count; i++)
                {
                    if (data[i].id == id)
                    {
                        data.Remove(data[i]);
                        break;
                    }
                }
            }
        }

        /// <summary>结束所有计时</summary>
        public void EndAllTimer()
        {
            foreach (var data in timerPairs.Values)
            {
                data.Clear();
            }
        }

        /// <summary>获取秒</summary>
        public string GetSecond(float time)
        {
            var s = Mathf.FloorToInt(time);
            return s.ToString();
        }

        /// <summary>获取分钟</summary>
        public string GetMinute(float time)
        {
            var m = Mathf.FloorToInt(time / 60f);
            var s = Mathf.FloorToInt(time - m * 60f);
            return $"{m:00}:{s:00}";
        }

        /// <summary>获取小时</summary>
        public string GetHour(float time)
        {
            var h = Mathf.FloorToInt(time / 3600f);
            var m = Mathf.FloorToInt(time / 60f - h * 60f);
            var s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
            return $"{h:00}:{m:00}:{s:00}";
        }

        public string GetTime(float time)
        {
            var h = Mathf.FloorToInt(time / 3600f);
            var m = Mathf.FloorToInt(time / 60f - h * 60f);
            if (m <= 0)
            {
                m = 1;
            }

            return $"{h}<size=70>小时</size>{m}<size=70>分</size>";
        }

        /// <summary>获取倒计时-秒</summary>
        public string GetCountDownSecond(float sumTime, float time)
        {
            time = sumTime - time;
            if (time < 0)
            {
                time = 0;
            }

            var s = Mathf.FloorToInt(time);
            return s.ToString();
        }

        /// <summary>获取倒计时-分钟</summary>
        public string GetCountDownMinute(float sumTime, float time)
        {
            time = sumTime - time;
            if (time < 0)
            {
                time = 0;
            }

            var m = Mathf.FloorToInt(time / 60f);
            var s = Mathf.FloorToInt(time - m * 60f);
            return $"{m:00}:{s:00}";
        }

        /// <summary>获取倒计时-小时</summary>
        public string GetCountDownHour(float sumTime, float time)
        {
            time = sumTime - time;
            if (time < 0)
            {
                time = 0;
            }

            var h = Mathf.FloorToInt(time / 3600f);
            var m = Mathf.FloorToInt(time / 60f - h * 60f);
            var s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
            return $"{h:00}:{m:00}:{s:00}";
        }
    }
}