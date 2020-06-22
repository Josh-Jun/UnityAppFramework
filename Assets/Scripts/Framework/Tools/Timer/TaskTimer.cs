using System;
using System.Collections.Generic;
using System.Timers;
/// <summary>功能:计时器</summary>
public class TaskTimer
{
    private Action<Action<int>, int> taskHandle;//任务委托
    private DateTime startDateTime;//开始日期计算机元年
    private Timer sevTimer;//开启时间线程

    private int idIdex;//当前id下标
    private static readonly string lockIndex = "lockIndex";//下标锁
    private List<int> idList;//存储当前已创建id
    private List<int> tmpIdList;//缓存的id

    private double currTime;//当前时间
    private static readonly string lockTime = "lockTime";//时间任务锁
    private List<TimeTask> taskTimeList;//计时任务列表
    private List<TimeTask> tmpTimeList;//缓存的定时任务列表
    private List<int> tmpDeleteTimeList;//缓存的删除定时任务

    private int currFrame;//当前帧
    private static readonly string lockFrame = "lockFrame";//帧任务锁
    private List<FrameTask> taskFrameList;//帧任务列表
    private List<FrameTask> tmpFrameList;//缓存的区帧任务
    private List<int> tmpDeleteFrameList;//缓存的删除帧任务

    public TaskTimer(int interval = 0)
    {
        startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);//1970-1-1 0:0:0  计算机元年
        idList = new List<int>();
        tmpIdList = new List<int>();

        taskTimeList = new List<TimeTask>();
        tmpTimeList = new List<TimeTask>();
        tmpDeleteTimeList = new List<int>();

        taskFrameList = new List<FrameTask>();
        tmpFrameList = new List<FrameTask>();
        tmpDeleteFrameList = new List<int>();
        if (interval != 0)
        {
            sevTimer = new Timer(interval)
            {
                AutoReset = true
            };//毫秒
            sevTimer.Elapsed += (object sender, ElapsedEventArgs args) =>
            {
                Update();
            };
            sevTimer.Start();
        }
    }

    public void Update()
    {
        CheckTimeTask();
        DeleteTimeTask();

        CheckFrameTask();
        DeleteFrameTask();

        DeleteId();
    }

    /// <summary> 删除完成任务id </summary>
    private void DeleteId()
    {
        if (tmpIdList.Count > 0)
        {
            lock (lockIndex)
            {
                for (int i = 0; i < tmpIdList.Count; i++)
                {
                    int id = tmpIdList[i];
                    for (int j = 0; j < idList.Count; j++)
                    {
                        if (idList[j] == id)
                        {
                            idList.RemoveAt(j);
                            break;
                        }
                    }
                }
                tmpIdList.Clear();
            }
        }
    }

    #region 时间任务
    /// <summary> 检测计时任务 </summary>
    private void CheckTimeTask()
    {
        if (tmpTimeList.Count > 0)
        {
            lock (lockTime)
            {
                //获取缓存区的定时任务到计时列表
                for (int i = 0; i < tmpTimeList.Count; i++)
                {
                    taskTimeList.Add(tmpTimeList[i]);
                }
                tmpTimeList.Clear();
            }
        }
        currTime = GetUTCMillisecond();//获取当前时间
        //遍历检测任务是否达到条件
        for (int i = 0; i < taskTimeList.Count; i++)
        {
            TimeTask timeTask = taskTimeList[i];
            if (currTime.CompareTo(timeTask.endTime) < 0)
            {
                continue;
            }
            else
            {
                try
                {
                    if (taskHandle != null)
                    {
                        taskHandle(timeTask.action, timeTask.id);
                    }
                    else
                    {
                        timeTask.action?.Invoke(timeTask.id);//Action不为空进行调用
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.ToString());
                }

                if (timeTask.count == 1)
                {
                    taskTimeList.RemoveAt(i); //移除完成的任务
                    i--;//当前索引-1
                    tmpIdList.Add(timeTask.id);
                }
                else
                {
                    if (timeTask.count != 0)
                    {
                        timeTask.count -= 1;
                    }
                    timeTask.endTime += timeTask.delayTime;//设置下一次任务的最终时间
                }
            }
        }
    }

    /// <summary> 删除计时任务 </summary>
    private void DeleteTimeTask()
    {
        if (tmpDeleteTimeList.Count > 0)
        {
            lock (lockTime)
            {
                for (int i = 0; i < tmpDeleteTimeList.Count; i++)
                {
                    bool isDelete = false;
                    int id = tmpDeleteTimeList[i];
                    for (int j = 0; j < taskTimeList.Count; j++)
                    {
                        if (taskTimeList[j].id == id)
                        {
                            taskTimeList.RemoveAt(j);
                            tmpIdList.Add(id);
                            isDelete = true;
                            break;
                        }
                    }
                    if (isDelete)
                    {
                        continue;
                    }
                    for (int j = 0; j < tmpTimeList.Count; j++)
                    {
                        if (tmpTimeList[j].id == id)
                        {
                            tmpTimeList.RemoveAt(j);
                            tmpIdList.Add(id);
                            break;
                        }
                    }

                }
                tmpDeleteTimeList.Clear();
            }
        }
    }

    /// <summary>
    /// 添加时间任务(默认:毫秒)
    /// </summary>
    /// <param name="action">方法</param>
    /// <param name="destTime">时间</param>
    /// <param name="timeUnit">时间类型</param>
    /// <param name="count">执行次数</param>
    public int AddTimeTask(Action<int> action, double delayTime, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        double Millisecond = GetMillisecond(delayTime, timeUnit); //换算同一单位(毫秒)
        int idIndex = GetIdIndex();//获取唯一下标
        lock (lockTime)
        {
            //定时任务加入缓存区
            tmpTimeList.Add(new TimeTask()
            {
                id = idIndex,
                //从游戏启动到延时后的时间(毫秒)
                endTime = GetUTCMillisecond() + Millisecond,
                action = action,
                delayTime = Millisecond,
                count = count,
            });
        }
        return idIndex;
    }

    /// <summary> 删除计时任务 </summary>
    public void DeleteTimeTask(int id)
    {
        lock (lockTime)
        {
            tmpDeleteTimeList.Add(id);
        }
    }

    /// <summary>
    /// 替换计时任务
    /// </summary>
    /// <param name="id">所替换的id</param>
    /// <returns></returns>
    public bool ReplaceTimeTask(int id, Action<int> action, double delayTime, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        double Millisecond = GetMillisecond(delayTime, timeUnit);
        TimeTask timeTask = new TimeTask()
        {
            id = id,
            endTime = GetUTCMillisecond() + Millisecond,
            action = action,
            delayTime = Millisecond,
            count = count,
        };
        bool isSuccess = false;
        for (int i = 0; i < taskTimeList.Count; i++)
        {
            if (taskTimeList[i].id == id)
            {
                //找到任务 进行替换
                taskTimeList[i] = timeTask;
                isSuccess = true;
                break;
            }
        }
        if (!isSuccess)
        {
            for (int i = 0; i < tmpTimeList.Count; i++)
            {
                if (tmpTimeList[i].id == id)
                {
                    tmpTimeList[i] = timeTask;
                    isSuccess = true;
                    break;
                }
            }
        }
        return isSuccess;
    }

    #endregion

    #region 帧任务

    /// <summary> 检测帧任务 </summary>
    private void CheckFrameTask()
    {
        if (tmpFrameList.Count > 0)
        {
            lock (lockFrame)
            {
                //获取缓存区的帧任务到计时列表
                for (int i = 0; i < tmpFrameList.Count; i++)
                {
                    taskFrameList.Add(tmpFrameList[i]);
                }
                tmpFrameList.Clear();
            }
        }
        currFrame += 1;//按每秒60帧(可运行400多天达到int最大值)
        //遍历检测任务是否达到条件
        for (int i = 0; i < taskFrameList.Count; i++)
        {
            FrameTask frameTask = taskFrameList[i];
            if (currFrame < frameTask.endFrame)
            {
                continue;
            }
            else
            {
                try
                {
                    if (taskHandle != null)
                    {
                        taskHandle(frameTask.action, frameTask.id);
                    }
                    else
                    {
                        frameTask.action?.Invoke(frameTask.id);//Action不为空进行调用
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.ToString());
                }

                if (frameTask.count == 1)
                {
                    taskFrameList.RemoveAt(i); //移除完成的任务
                    i--;//当前索引-1
                    tmpIdList.Add(frameTask.id);
                }
                else
                {
                    if (frameTask.count != 0)
                    {
                        frameTask.count -= 1;
                    }
                    frameTask.endFrame += frameTask.delayFrame;//设置下一次任务的结束帧
                }
            }
        }
    }

    /// <summary> 删除帧任务 </summary>
    private void DeleteFrameTask()
    {
        if (tmpDeleteFrameList.Count > 0)
        {
            lock (lockFrame)
            {
                for (int i = 0; i < tmpDeleteFrameList.Count; i++)
                {
                    bool isDelete = false;
                    int id = tmpDeleteFrameList[i];
                    for (int j = 0; j < taskFrameList.Count; j++)
                    {
                        if (taskFrameList[j].id == id)
                        {
                            taskFrameList.RemoveAt(j);
                            tmpIdList.Add(id);
                            isDelete = true;
                            break;
                        }
                    }
                    if (isDelete)
                    {
                        continue;
                    }
                    for (int j = 0; j < tmpFrameList.Count; j++)
                    {
                        if (tmpFrameList[j].id == id)
                        {
                            tmpFrameList.RemoveAt(j);
                            tmpIdList.Add(id);
                            break;
                        }
                    }

                }
                tmpDeleteFrameList.Clear();
            }
        }
    }

    /// <summary>
    /// 添加帧任务
    /// </summary>
    /// <param name="action">方法</param>
    /// <param name="destTime">时间</param>
    /// <param name="count">执行次数</param>
    public int AddFrameTask(Action<int> action, int delayFrame, int count = 1)
    {
        int idIndex = GetIdIndex();//获取唯一下标
        lock (lockFrame)
        {
            //帧任务加入缓存区
            tmpFrameList.Add(new FrameTask()
            {
                id = idIndex,
                endFrame = currFrame + delayFrame,
                action = action,
                delayFrame = delayFrame,
                count = count,
            });
        }
        return idIndex;
    }

    /// <summary>
    /// 删除帧任务
    /// </summary>
    /// <param name="id"></param>
    public void DeleteFrameTask(int id)
    {
        lock (lockFrame)
        {
            tmpDeleteFrameList.Add(id);
        }
        #region 
        //bool isSuccess = false;
        //for (int i = 0; i < taskFrameList.Count; i++) {
        //    FrameTask frameTask = taskFrameList[i];
        //    if (frameTask.id == id) {
        //        taskFrameList.RemoveAt(i);
        //        for (int j = 0; j < idList.Count; j++) {
        //            if (idList[i] == j) {
        //                idList.RemoveAt(j);
        //                break;
        //            }
        //        }
        //        isSuccess = true;
        //        break;
        //    }
        //}
        //if (!isSuccess) {
        //    for (int i = 0; i < tmpFrameList.Count; i++) {
        //        FrameTask frameTask = tmpFrameList[i];
        //        if (frameTask.id == id) {
        //            tmpFrameList.RemoveAt(i);
        //            for (int j = 0; j < idList.Count; j++) {
        //                if (idList[i] == j) {
        //                    idList.RemoveAt(j);
        //                    break;
        //                }
        //            }
        //            isSuccess = true;
        //            break;
        //        }
        //    }
        //}
        //LogInfo("删除帧任务:" + isSuccess);
        //return isSuccess;
        #endregion
    }

    /// <summary>
    /// 替换帧任务
    /// </summary>
    /// <param name="id">所替换的id</param>
    /// <returns></returns>
    public bool ReplaceFrameTask(int id, Action<int> action, int delayFrame, int count = 1)
    {
        FrameTask frameTask = new FrameTask()
        {
            id = id,
            endFrame = currFrame + delayFrame,
            action = action,
            delayFrame = delayFrame,
            count = count,
        };
        bool isSuccess = false;
        for (int i = 0; i < taskFrameList.Count; i++)
        {
            if (taskFrameList[i].id == id)
            {
                //找到任务 进行替换
                taskFrameList[i] = frameTask;
                isSuccess = true;
                break;
            }
        }
        if (!isSuccess)
        {
            for (int i = 0; i < tmpFrameList.Count; i++)
            {
                if (tmpFrameList[i].id == id)
                {
                    tmpFrameList[i] = frameTask;
                    isSuccess = true;
                    break;
                }
            }
        }
        return isSuccess;
    }
    #endregion

    /// <summary> 删除所有任务 </summary>
    public void DeleteAllTask()
    {
        lock (lockIndex)
        {
            idList.Clear();
            tmpIdList.Clear();
        }
        lock (lockTime)
        {
            taskTimeList.Clear();
            tmpTimeList.Clear();
            tmpDeleteTimeList.Clear();
        }

        lock (lockFrame)
        {
            taskFrameList.Clear();
            tmpFrameList.Clear();
            tmpDeleteFrameList.Clear();
        }
    }

    #region Tool
    /// <summary> 获取id下标 </summary>
    private int GetIdIndex()
    {
        lock (lockIndex)
        {
            idIdex += 1;
            while (true)
            {
                if (idIdex == int.MaxValue)
                {
                    idIdex = 0;
                }
                bool isUse = false;
                for (int i = 0; i < idList.Count; i++)
                {
                    if (idIdex == idList[i])
                    {
                        isUse = true;
                        break;
                    }
                }
                if (isUse)
                {
                    idIdex += 1;
                }
                else
                {
                    idList.Add(idIdex);
                    break;
                }
            }
        }
        return idIdex;
    }

    /// <summary> 根据时间、类型转换为毫秒 </summary>
    private double GetMillisecond(double delayTime, TimeUnit timeUnit)
    {
        switch (timeUnit)
        {
            case TimeUnit.Millisecond:
                break;
            case TimeUnit.Second:
                delayTime = delayTime * 1000;
                break;
            case TimeUnit.Minute:
                delayTime = delayTime * 1000 * 60;
                break;
            case TimeUnit.Hour:
                delayTime = delayTime * 1000 * 3600;
                break;
            case TimeUnit.Day:
                delayTime = delayTime * 1000 * 3600 * 24;
                break;
        }
        return delayTime;
    }

    /// <summary> 获取UTC毫秒 </summary>
    public double GetUTCMillisecond()
    {
        //现在-计算机元年的时间间隔
        TimeSpan timeSpan = DateTime.UtcNow - startDateTime;
        return timeSpan.TotalMilliseconds;//返回毫秒
    }

    /// <summary> 获取UTC秒 </summary>
    public double GetUTCSecond()
    {
        //现在-计算机元年的时间间隔
        TimeSpan timeSpan = DateTime.UtcNow - startDateTime;
        return timeSpan.TotalSeconds;//返回秒
    }

    /// <summary> 设置Handle </summary>
    public void SetHandle(Action<Action<int>, int> handle)
    {
        taskHandle = handle;
    }

    /// <summary> 获取当前时间(毫秒) </summary>
    public double GetMillisecondTime()
    {
        return currTime;
    }

    /// <summary> 获取当前时间 </summary>
    [Obsolete]
    public DateTime GetLocalDateTime()
    {
        return TimeZone.CurrentTimeZone.ToLocalTime(startDateTime.AddMilliseconds(currTime));
    }

    /// <summary> 获取年 </summary>
    [Obsolete]
    public int GetYear()
    {
        return GetLocalDateTime().Year;
    }

    /// <summary> 获取月 </summary>
    [Obsolete]
    public int GetMonth()
    {
        return GetLocalDateTime().Month;
    }

    /// <summary> 获取日 </summary>
    [Obsolete]
    public int GetDay()
    {
        return GetLocalDateTime().Day;
    }

    /// <summary> 获取星期 </summary>
    [Obsolete]
    public int GetWeek()
    {
        return (int)GetLocalDateTime().DayOfWeek;
    }

    /// <summary> 获取当前时间字符串 </summary>
    [Obsolete]
    public string GetLocalTime()
    {
        DateTime dateTime = GetLocalDateTime();
        return GetTimeStr(dateTime.Hour) + ":" +
            GetTimeStr(dateTime.Minute) + ":" +
            GetTimeStr(dateTime.Second);

    }

    /// <summary> 获取时间字符串(小于10 如 01，02) </summary>
    private string GetTimeStr(int time)
    {
        if (time < 10)
        {
            return "0" + time;
        }
        else
        {
            return time.ToString();
        }
    }

    /// <summary> 重置所有定时任务 </summary>
    public void Reset()
    {
        idIdex = 0;
        sevTimer.Stop();
        idList.Clear();
        taskTimeList.Clear();
        tmpTimeList.Clear();
        taskFrameList.Clear();
        tmpFrameList.Clear();
    }
    #endregion
}
#region 定时器配置
/// <summary>功能:时间定时数据类 </summary>
public class TimeTask
{
    public int id;
    public double endTime;//结束时间
    public Action<int> action;//调用方法
    public double delayTime;//延迟时间
    public int count;//执行次数(0:一直执行,count对应次数)
}

/// <summary>功能:帧定时数据类 </summary>
public class FrameTask
{
    public int id;
    public int endFrame;//结束帧
    public Action<int> action;//调用方法
    public int delayFrame;//延迟帧
    public int count;//执行次数(0:一直执行,count对应次数)
}

/// <summary>任务包 </summary>
public class TaskPack
{
    public int id;
    public Action<int> cb;
}
/// <summary>事件单位类型 </summary>
public enum TimeUnit
{
    Millisecond,//毫秒
    Second,//秒
    Minute,//分钟
    Hour,//小时
    Day,//天
}
#endregion