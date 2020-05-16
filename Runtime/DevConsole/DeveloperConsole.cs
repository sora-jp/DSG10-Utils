using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public static class DeveloperConsole
{
    static DevConsoleInstance _instance;
    static bool _initialized;
    static ConcurrentQueue<LogMessageData> _queuedMessages = new ConcurrentQueue<LogMessageData>();

    static bool IsConsoleActive => false;//Debug.isDebugBuild || Application.isEditor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void InitializeDebugHook()
    {
        if (!IsConsoleActive) return;
        Application.logMessageReceivedThreaded += HandleLogMessage;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeConsole()
    {
        if (!IsConsoleActive) return;
        var consolePrefab = Resources.Load<GameObject>("DevConsole/Console");
        var console = Object.Instantiate(consolePrefab);
        Object.DontDestroyOnLoad(console);
        _instance = console.GetComponent<DevConsoleInstance>();

        _initialized = true;
        while (_queuedMessages.TryDequeue(out var d)) HandleLogMessage(d.logString, d.trace, d.logType);

        _queuedMessages = null;
    }

    static void HandleLogMessage(string condition, string stackTrace, LogType type)
    {
        var msgData = new LogMessageData {logType = type, logString = condition, trace = stackTrace, source = Thread.CurrentThread};

        if (!_initialized)
            _queuedMessages.Enqueue(msgData);
        else
            _instance.AppendToLogQueue(msgData);
    }
}