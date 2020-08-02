using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>功能:定时服务</summary>
public class TimerTaskManager : SingletonMono<TimerTaskManager>
{
    private TimerTask timer;//计时器
    private static readonly string lockTask = "lockTask";//任务锁
    private Queue<TaskPack> timerQueue = new Queue<TaskPack>();//定时任务队列
    public void Awake()
    {
        timer = new TimerTask(50);//1秒=1000毫秒 50毫秒调用1次
        #region 注册Handle后,子线程中执行完后,可在主线程中执行回调
        //1.在子线程处理，子线程中执行逻辑
        //2.在子线程处理，主线程中执行逻辑
        timer.SetHandle((Action<int> cb, int id) =>
        {
            if (cb != null)
            {
                lock (lockTask)
                {
                    timerQueue.Enqueue(new TaskPack
                    {
                        id = id,
                        cb = cb,
                    });
                }
            }
        });
        #endregion
    }
    private void Update()
    {
        if (timerQueue.Count > 0)
        {
            TaskPack taskPack = null;
            lock (lockTask)
            {
                taskPack = timerQueue.Dequeue();
                if (taskPack != null)
                {
                    taskPack.cb(taskPack.id);
                }
            }
        }
    }

    #region 时间任务
    /// <summary>添加时间任务(默认:毫秒)</summary>
    public int AddTimeTask(Action<int> action, float delayTime, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        return timer.AddTimeTask(action, delayTime, timeUnit, count);
    }

    /// <summary>删除时间任务</summary>
    public void DeleteTimeTask(int id)
    {
        timer.DeleteTimeTask(id);
    }

    /// <summary>替换时间任务</summary>
    public bool ReplaceTimeTask(int id, Action<int> action, float delayTime, TimeUnit timeUnit = TimeUnit.Millisecond, int count = 1)
    {
        return timer.ReplaceTimeTask(id, action, delayTime, timeUnit, count);
    }
    #endregion

    #region 帧任务
    /// <summary>添加帧任务</summary>
    public int AddFrameTask(Action<int> action, int delayFrame, int count = 1)
    {
        return timer.AddFrameTask(action, delayFrame, count);
    }

    /// <summary>删除帧任务</summary>
    public void DeleteFrameTask(int id)
    {
        timer.DeleteFrameTask(id);
    }

    /// <summary>替换帧任务</summary>
    public bool ReplaceFrameTask(int id, Action<int> action, int delayFrame, int count = 1)
    {
        return timer.ReplaceFrameTask(id, action, delayFrame, count);
    }
    #endregion

    /// <summary>删除所有任务</summary>
    public void DeleteAllTask()
    {
        timer.DeleteAllTask();
    }

    /// <summary>获取时间戳-秒</summary>
    public double GetUTCSecond()
    {
        return timer.GetUTCSecond();
    }

    /// <summary>获取时间戳-毫秒</summary>
    public double GetUTCMillisecond()
    {
        return timer.GetUTCMillisecond();
    }
}
