/* ***********************************************
 * author :  Xiaoqifeng
 * function: 
 * history:  created by Xiaoqifeng  2017/6/6 15:43:32
 * ***********************************************/
//#define LOG_SCREEN

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/// <summary>
/// DEBUG_OFF = true 关闭测试版bug调试
/// DEBUG_OFF=true LOG_SCREEN=true log会在屏幕上打出，开启屏幕log输出工具 
/// </summary>
class MyLogUtills
{
    public delegate void LogDelegate(object message);
    public delegate void LogExceptionDelegate(Exception exception);

    public delegate void LogContextDelegate(object message, UnityEngine.Object context);
    public delegate void LogExceptionContextDelegate(Exception exception, UnityEngine.Object context);
#if !DEBUG_OFF
    public static LogDelegate Log = UnityEngine.Debug.Log;
    public static LogDelegate LogError = UnityEngine.Debug.LogError;
    public static LogDelegate LogWarning = UnityEngine.Debug.LogWarning;
    public static LogExceptionDelegate LogException = UnityEngine.Debug.LogException;
#else
    public static LogDelegate Log = LogLevelUtills.Log;
    public static LogDelegate LogError = LogLevelUtills.LogError;
    public static LogDelegate LogWarning = LogLevelUtills.LogWarning;
    public static LogExceptionDelegate LogException = LogLevelUtills.LogException;
#endif
    public static void setStable_vloglevel(MyLogLevel level) {
        LogLevelUtills.MLogDisplayLevel = level;
    }

}

/// <summary>
/// 发布版，日志等级，默认情况下，发布版本只记录并显示error和critical等级
/// </summary>
public enum MyLogLevel
{
    debug,
    info,
    warning,
    error,
    critical,

    max,
}

class LogLevelUtills
{
    private static MyLogLevel m_log_display_level = MyLogLevel.debug;
    
    public static MyLogLevel MLogDisplayLevel
    {
        get { return m_log_display_level; }
        set
        {
            m_log_display_level = value;
            UnityEngine.Debug.Log("[MyLogUtills] 非Dev版，日志等级: " + value.ToString());
        }
    }

    static LogLevelUtills()
    {
#if UNITY_EDITOR
        MLogDisplayLevel = MyLogLevel.debug;
#else
		MLogDisplayLevel = MyLogLevel.error;
#endif
    }
    static void LogLevel(MyLogLevel level,object msg) {
        if (MLogDisplayLevel > level) return;
#if LOG_SCREEN
        ScreenLog.CanLog(true);
#else
        ScreenLog.CanLog(false);
#endif
        switch (level) {
            case MyLogLevel.debug:
                UnityEngine.Debug.Log(msg);
                break;
            case MyLogLevel.warning:
                UnityEngine.Debug.LogWarning(msg);
                break;
            case MyLogLevel.error:
                UnityEngine.Debug.LogError(msg);
                break;
        }
    }

    public static void Log(object msg)
    {
        LogLevel(MyLogLevel.debug,msg);
    }

    public static void LogWarning(object msg)
    {
        LogLevel(MyLogLevel.warning, msg);
    }

    public static void LogError(object msg)
    {
        LogLevel(MyLogLevel.error, msg);
    }

    public static void LogException(Exception ex)
    {
        UnityEngine.Debug.LogException(ex);
    }


}

public class ScreenLog: MonoBehaviour
{
    static List<string> mLines = new List<string>();
    static ScreenLog mInstance = null;
    static int maxline = 100;
    static bool onlyErro = false;
    static bool isHideWarn = true;
    static bool mCanLog = false;
    private bool isShowLog=true;//是否显示日志,
    private Vector2 m_scroll;
    public GUIStyle labelStyle;//日志的OnGUI样式设定
    int stackSize = 4;
    int startStackSize = 0;
    string spilt = " : ";
    internal void OnEnable()
    {

        //Application.logMessageReceived += HandleLog;//注册Unity的日志回调
        Application.logMessageReceivedThreaded += HandleLog;
       
    }

    internal void OnDisable()
    {
        //Application.logMessageReceived -= HandleLog;//去掉Unity的日志回调
        Application.logMessageReceivedThreaded -= HandleLog;
        mLines.Add("OnDisable");
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        
        if (!mCanLog) return;
        if (onlyErro && type>LogType.Error) return;
        if (isHideWarn&& type == LogType.Warning) return;
        StringBuilder sb = new StringBuilder();
        string[] splitStr = stackTrace.Split('\n');
        string strType = "";
        switch (type)
        {//给日志类型加颜色
            case LogType.Error:
                strType = "<color=red>" + type.ToString() + spilt + logString +  "</color>";
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                strType = "<color=yellow>" + type.ToString() + spilt + logString + "</color>";
                break;
            case LogType.Log:
                strType = "<color=white>" + type.ToString() + spilt + logString + "</color>";
                break;
            case LogType.Exception:
                strType = "<color=red>" + type.ToString() + spilt + logString + "</color>";
                break;
            default:
                strType = logString;
                break;
        }
        sb.Append(strType+"\n");
        for (int i = 0, max = splitStr.Length; i < stackSize && i < max; ++i) {
            sb.Append(splitStr[i]);
            sb.Append("\t\n");
        }
        //sb.Append("\t\t<————————————>\n");
        Print(sb.ToString());
    }

    void Print(string text) {
        if (Application.isPlaying)
        {
            if (mInstance == null)
            {
                lock(mInstance){
                    GameObject go = new GameObject("_Screen Debug");
                    mInstance = go.AddComponent<ScreenLog>();
                    DontDestroyOnLoad(go);
                }
            }
            lock (mLines) {
                if (mLines.Count > maxline) mLines.RemoveAt(0);
                mLines.Add(text);
            }
        }
    }

    public static void CanLog(bool canLog) {
        if (mInstance == null)
        {
            GameObject go = new GameObject("_Screen Debug");
            mInstance = go.AddComponent<ScreenLog>();
            DontDestroyOnLoad(go);
        }
        mCanLog = canLog;
    }

    void OnGUI()
    {
        if (!mCanLog) return;
        if (GUI.Button(new Rect(924, 160, 100, 40), "isShowLog"))
        {
            isShowLog = !isShowLog;
        }
        if (!isShowLog) return;
        if (!onlyErro)
        {
            if (GUI.Button(new Rect(924, 0, 100, 40), "showErro"))
            {
                triggleOlyShowErro();
            }
        }
        else
        {
            if (GUI.Button(new Rect(924, 0, 100, 40), "showAll"))
            {
                triggleOlyShowErro();
            }
        }
        if (GUI.Button(new Rect(924, 80, 100, 40), "clear"))
        {
            mLines.Clear();
        }
        m_scroll = GUILayout.BeginScrollView(m_scroll);
        for (int i = 0, imax = mLines.Count; i < imax; ++i)
        {
            GUILayout.Label(mLines[i]);
        }
        GUILayout.EndScrollView();
    }

    public void triggleOlyShowErro() {
        mLines.Clear();
        onlyErro = !onlyErro;
    }
}