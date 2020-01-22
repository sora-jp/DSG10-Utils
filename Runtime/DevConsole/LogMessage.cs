using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogMessage : MonoBehaviour
{
    public Color selectedColor, warningColor, errorColor;
    public TextMeshProUGUI messageText;
    public Image bg;
    public TextMeshProUGUI stackTrace;

    bool m_expanded;
    RectTransform m_rt, m_messageTextRT, m_stackTraceRT;

    void Awake()
    {
        m_rt = GetComponent<RectTransform>();
        m_messageTextRT = messageText.rectTransform;
        m_stackTraceRT = stackTrace.rectTransform;

        m_expanded = true;
        ActualToggleVisibility();
        m_expanded = true;
        ToggleVisibility();
    }

    public void SetData(LogMessageData d)
    {
        messageText.text = $"[{DateTime.Now:T}]: {d.logString}";

        var threadText = $"Thread #{d.source.ManagedThreadId}";
        if (!string.IsNullOrEmpty(d.source.Name)) threadText += $" ({d.source.Name})";
        if (d.source.ManagedThreadId == 1) threadText = "Main thread";

        stackTrace.text = string.Join("\n", d.trace.Split(new [] {'\n'}, StringSplitOptions.RemoveEmptyEntries).Skip(1).Prepend($"Logged from: {threadText}").Select(PadLeft));
        SetColor(d.logType);
    }

    static string PadLeft(string s) => $"    {s}";

    public void ToggleVisibility()
    {
        StartCoroutine(_ToggleVisibility());
    }

    IEnumerator _ToggleVisibility()
    {
        yield return new WaitForEndOfFrame();

        ActualToggleVisibility();
    }

    public void ActualToggleVisibility()
    {
        m_expanded = !m_expanded;
        bg.color = m_expanded ? selectedColor : Color.clear;
        stackTrace.gameObject.SetActive(m_expanded);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rt);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_rt.parent);

        //var r = m_rt.sizeDelta;
        //r.y = m_messageTextRT.sizeDelta.y + (m_expanded ? m_stackTraceRT.sizeDelta.y : 0);
        //m_rt.sizeDelta = r;
    }

    void SetColor(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                stackTrace.color = messageText.color = errorColor;
                break;
            case LogType.Warning:
                stackTrace.color = messageText.color = warningColor;
                break;
        }
    }
}
