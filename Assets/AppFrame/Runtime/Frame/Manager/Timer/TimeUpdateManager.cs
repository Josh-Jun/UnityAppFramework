using System;
using System.Collections.Generic;
using AppFrame.Enum;
using UnityEngine;

namespace AppFrame.Tools
{
    /// <summary>功能:计时更新</summary>
    public class TimeUpdateManager : SingletonMono<TimeUpdateManager>
    {
        private int timeId = 0; //下标
        private Dictionary<UpdateMold, List<TimerData>> timerPairs = new Dictionary<UpdateMold, List<TimerData>>();

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
                            timeData.cb((float)timeData.addTime);
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
                            fixedTimerData.cb((float)fixedTimerData.addTime);
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
                            lateTimeData.cb((float)timeData.addTime);
                        }
                    }
                }
            }
        }

        /// <summary>开始计时</summary>
        public int StartTimer(Action<float> cb, UpdateMold updateMold = UpdateMold.Update)
        {
            timeId += 1;
            TimerData timer = new TimerData
            {
                id = timeId,
                isTime = true,
                addTime = 0,
                cb = cb,
            };
            
            if (timerPairs.ContainsKey(updateMold))
            {
                timerPairs[updateMold].Add(timer);
            }
            else
            {
                var data = new List<TimerData>();
                data.Add(timer);
                timerPairs.Add(updateMold, data);
            }
            return timeId;
        }

        /// <summary>暂停计时</summary>
        public void PauseTimer(int id)
        {
            foreach (var data in timerPairs.Values)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].id == id)
                    {
                        data[i].isTime = false;
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
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].id == id)
                    {
                        data[i].isTime = true;
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
                for (int i = 0; i < data.Count; i++)
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
            int s = Mathf.FloorToInt(time);
            return s.ToString();
        }

        /// <summary>获取分钟</summary>
        public string GetMinute(float time)
        {
            int m = Mathf.FloorToInt(time / 60f);
            int s = Mathf.FloorToInt(time - m * 60f);
            return string.Format("{0:00}:{1:00}", m, s);
        }

        /// <summary>获取小时</summary>
        public string GetHour(float time)
        {
            int h = Mathf.FloorToInt(time / 3600f);
            int m = Mathf.FloorToInt(time / 60f - h * 60f);
            int s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
            return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
        }

        public string GetTime(float time)
        {
            int h = Mathf.FloorToInt(time / 3600f);
            int m = Mathf.FloorToInt(time / 60f - h * 60f);
            if (m <= 0)
            {
                m = 1;
            }

            return string.Format("{0}<size=70>小时</size>{1}<size=70>分</size>", h, m);
        }

        /// <summary>获取倒计时-秒</summary>
        public string GetCountDownSecond(float sumTime, float time)
        {
            time = sumTime - time;
            if (time < 0)
            {
                time = 0;
            }

            int s = Mathf.FloorToInt(time);
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

            int m = Mathf.FloorToInt(time / 60f);
            int s = Mathf.FloorToInt(time - m * 60f);
            return string.Format("{0:00}:{1:00}", m, s);
        }

        /// <summary>获取倒计时-小时</summary>
        public string GetCountDownHour(float sumTime, float time)
        {
            time = sumTime - time;
            if (time < 0)
            {
                time = 0;
            }

            int h = Mathf.FloorToInt(time / 3600f);
            int m = Mathf.FloorToInt(time / 60f - h * 60f);
            int s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
            return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
        }
    }
}