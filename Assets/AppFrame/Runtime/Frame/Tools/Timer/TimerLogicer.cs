using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppFrame.Tools
{
    /// <summary>功能:计时器服务</summary>
    public class TimerLogicer : SingletonMono<TimerLogicer>
    {
        private int timeId = 0; //下标
        private List<TimerData> timerLst = new List<TimerData>(); //计时Lst
        private List<TimerData> timerFixedLst = new List<TimerData>(); //计时Lst

        public float GameTime
        {
            private set { }
            get { return Time.time; }
        }

        public override void InitParent(Transform parent)
        {
            base.InitParent(parent);
        }

        private TimerData timeData;

        private void Update()
        {
            if (timerLst.Count > 0)
            {
                for (int i = 0, length = timerLst.Count; i < length; i++)
                {
                    timeData = timerLst[i];
                    if (timeData.isTime)
                    {
                        timeData.addTime = Time.time - timeData.tagTime;
                        timeData.cb(timeData.addTime);
                    }
                }
            }
        }

        private TimerData fixedTimerData;

        private void FixedUpdate()
        {
            if (timerFixedLst.Count > 0)
            {
                for (int i = 0, length = timerFixedLst.Count; i < length; i++)
                {
                    fixedTimerData = timerFixedLst[i];
                    if (fixedTimerData.isTime)
                    {
                        int time = (int)(Time.fixedTime * 100) - (int)(fixedTimerData.tagTime * 100);
                        fixedTimerData.addTime = time / 100f;
                        fixedTimerData.cb(fixedTimerData.addTime);
                    }
                }
            }
        }

        /// <summary>开始计时</summary>
        public int StartTimer(Action<float> cb, bool isFixed = false)
        {
            timeId += 1;
            float tagTime = isFixed ? Time.fixedTime : Time.time;
            TimerData timer = new TimerData
            {
                id = timeId,
                isTime = true,
                addTime = 0,
                tagTime = tagTime,
                cb = cb,
            };
            if (isFixed)
            {
                timerFixedLst.Add(timer);
            }
            else
            {
                timerLst.Add(timer);
            }

            return timeId;
        }

        /// <summary>暂停计时</summary>
        public void PauseTimer(int id)
        {
            for (int i = 0; i < timerFixedLst.Count; i++)
            {
                if (timerFixedLst[i].id == id)
                {
                    timerFixedLst[i].isTime = false;
                    break;
                }
            }

            for (int i = 0; i < timerLst.Count; i++)
            {
                if (timerLst[i].id == id)
                {
                    timerLst[i].isTime = false;
                    break;
                }
            }
        }

        /// <summary>继续计时</summary>
        public void ContinueTimer(int id)
        {
            for (int i = 0; i < timerFixedLst.Count; i++)
            {
                if (timerFixedLst[i].id == id)
                {
                    timerFixedLst[i].isTime = true;
                    break;
                }
            }

            for (int i = 0; i < timerLst.Count; i++)
            {
                if (timerLst[i].id == id)
                {
                    timerLst[i].isTime = true;
                    break;
                }
            }
        }

        /// <summary>结束计时</summary>
        public void EndTimer(int id)
        {
            for (int i = 0; i < timerFixedLst.Count; i++)
            {
                if (timerFixedLst[i].id == id)
                {
                    timerFixedLst.Remove(timerFixedLst[i]);
                    break;
                }
            }

            for (int i = 0; i < timerLst.Count; i++)
            {
                if (timerLst[i].id == id)
                {
                    timerLst.Remove(timerLst[i]);
                    break;
                }
            }
        }

        /// <summary>结束所有计时</summary>
        public void EndAllTimer()
        {
            timerLst.Clear();
            timerFixedLst.Clear();
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