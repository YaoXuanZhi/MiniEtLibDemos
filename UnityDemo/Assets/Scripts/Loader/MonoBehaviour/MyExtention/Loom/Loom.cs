using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Linq;

public class Loom : MonoBehaviour
{

    public static int maxThreads = 8;
    static int numThreads;

    private static Loom _current;

    public static Loom Current
    {
        get
        {
            Initialize();
            return _current;
        }
    }

    void Awake()
    {
        _current = this;
        initialized = true;
    }

    static bool initialized;

    /// <summary>
    /// 如果该脚本拖到场景中，运行时会自动加载，如果没有，在其他脚本中调用 RunAsync 或 Loom本身时，会通过它来初始化。
    /// </summary>
    public static void Initialize()
    {
        if (!initialized)
        {

            if (!Application.isPlaying)
                return;
            initialized = true;
            var g = new GameObject("Loom");
            _current = g.AddComponent<Loom>();
        }

    }

    private List<Action> _actions = new List<Action>();
    public struct DelayedQueueItem
    {
        public float time;
        public Action action;
    }


    private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
    List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

    /// <summary>
    /// 其他线程可以通过该方法来让主线程调用某个方法。
    /// </summary>
    /// <param name="action"></param>
    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0f);
    }

    /// <summary>
    /// 其他线程可以通过该方法来让主线程调用某个方法。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="time"></param>
    public static void QueueOnMainThread(Action action, float time)
    {
        if (time != 0)
        {
            lock (Current._delayed)
            {
                Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions.Add(action);
            }
        }
    }

    /// <summary>
    /// 启动一个线程来执行某个方法
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>-
    public static Thread RunAsync(Action a)
    {
        Initialize();
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }
        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }

    private static void RunAction(object action)
    {
        try
        {
            Debug.Log($"RunAction 线程{Thread.CurrentThread.Name}启动");
            
            ((Action)action)();
            Debug.Log($"RunAction 线程{Thread.CurrentThread.Name}执行完成");
        }
        catch
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }

    }


    void OnDisable()
    {
        if (_current == this)
        {
            _current = null;
        }
    }


    List<Action> _currentActions = new List<Action>();

    // Update is called once per frame
    void Update()
    {
        // 设置当前需要执行的操作内容。
        lock (_actions)
        {
            _currentActions.Clear();
            _currentActions.AddRange(_actions);
            _actions.Clear();
        }

        // 执行当前需要的操作
        foreach (var a in _currentActions)
        {
            a();
        }

        lock (_delayed)
        {
            _currentDelayed.Clear();
            _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
            foreach (var item in _currentDelayed)
                _delayed.Remove(item);
        }
        foreach (var delayed in _currentDelayed)
        {
            delayed.action();
        }
    }
}