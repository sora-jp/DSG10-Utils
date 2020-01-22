using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevConsoleInstance : MonoBehaviour
{
    public Transform logContainer;
    public LogMessage messagePrefab;
    public ScrollRect scroll;
    public TMP_InputField cmdInput;

    bool m_visible;
    Canvas m_canvas;
    ConcurrentQueue<LogMessageData> m_logQueue;

    void Awake()
    {
        m_logQueue = new ConcurrentQueue<LogMessageData>();
        m_canvas = GetComponent<Canvas>();
        m_canvas.enabled = m_visible = false;
        scroll.verticalNormalizedPosition = 0;
        cmdInput.onSubmit.AddListener(_ => SubmitCommand());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            m_visible = !m_visible;
            m_canvas.enabled = m_visible;
        }
        //m_canvas.enabled = m_visible;
        Debug.developerConsoleVisible = false;
    }

    void LateUpdate()
    {
        Debug.developerConsoleVisible = false;
        while (m_logQueue.TryDequeue(out var d))
        {
            LogToScreen(d);
        }
    }

    public void AppendToLogQueue(LogMessageData d)
    {
        m_logQueue.Enqueue(d);
    }

    public void LogToScreen(LogMessageData d)
    {
        var old = scroll.verticalNormalizedPosition;
        var msg = Instantiate(messagePrefab, logContainer);
        msg.SetData(d);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(msg.GetComponent<RectTransform>());
        ScrollToBottomIfNecessary(old);
    }

    void ScrollToBottomIfNecessary(float old)
    {
        if (old > 0) return;
        StartCoroutine(_ScrollToBottom());
    }

    IEnumerator _ScrollToBottom()
    {
        yield return null;
        yield return null;
        yield return null;
        scroll.verticalNormalizedPosition = 0;
    }

    public void SubmitCommand()
    {
        DeveloperCommands.Execute(cmdInput.text);
        cmdInput.text = "";
    }
}

public struct LogMessageData
{
    public LogType logType;
    public string logString, trace;
    public Thread source;
}