using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

/// <summary>
/// Custom log system that allow to choose when to display message
/// Optimized for runtime and builds.
/// </summary>
public static class WPLogger
{
	/// <summary>
	/// String containing all logs
	/// </summary>
	public static StringBuilder LogHistory { get; private set; } = new StringBuilder();
	/// <summary>
	/// List of active tags (tag can be any string)
	/// </summary>
	public static List<string> activeTags { get; private set; } = new List<string>();
	/// <summary>
	/// Write log to Debug unity class
	/// </summary>
	public static bool LogToUnity = true;
	/// <summary>
	/// Write log to local global log
	/// </summary>
	public static bool LogToHistory = true;
	/// <summary>
	/// Diplay tag on front of log
	/// </summary>
	public static bool LogTagHeader = true;
	/// <summary>
	/// Display time on front of log
	/// </summary>
	public static bool LogTime = false;

	public delegate void WPLoggerEvent(string logText);

	/// <summary>
	/// Called when a new log is processed
	/// </summary>
	public static WPLoggerEvent OnLogged;
	/// <summary>
	/// Called when a new error is processed
	/// </summary>
	public static WPLoggerEvent OnErrorLogged;


	[UnityEngine.RuntimeInitializeOnLoadMethod]
	static void Init()
	{
#if UNITY_EDITOR
		// on editor don't apply settings if some are already existing
		if (activeTags != null && activeTags.Count > 0) return;
#endif
		ApplySettings(WPLoggerData.GetCurrentSettings());
	}

	public static void ApplySettings(WPLoggerData.Settings settings)
	{
		LogToUnity = settings.logToUnity;
		LogToHistory = settings.logToHistory;
		LogTagHeader = settings.logTagHeader;
		LogTime = settings.logTime;
		activeTags = new List<string>(settings.defaultActiveTags);

		// Make sure Force tag is present
		if (!activeTags.Contains(WPMainTag.FORCE))
		{
			activeTags.Insert(0, WPMainTag.FORCE);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void Show(params object[] objs)
	{
		if (objs == null) return;

		StringBuilder str = new StringBuilder();
		object o;
		for (int i = 0; i < objs.Length; i++)
		{
			o = objs[i];
			if (o == null) str.Append("[" + i + ": null]");
			else str.Append("[" + i + ": " + o.GetType() + " - " + o.ToString() + "]");
		}

		LogHistory.Append(str);
		UnityEngine.Debug.Log(str);
		OnLogged?.Invoke(str.ToString());
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	/// <summary>
	/// Fast log only wrap default Unity logger and history
	/// </summary>
	public static void LogFast(string message)
	{
		LogHistory.AppendLine(message);
		UnityEngine.Debug.Log(message);
		OnLogged?.Invoke(message);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	/// <summary>
	/// Fast log only wrap default Unity logger
	/// </summary>
	/// <param name="message"></param>
	public static void LogFast(string message, UnityEngine.Object context)
	{
		LogHistory.AppendLine(message);
		UnityEngine.Debug.Log(message, context);
		OnLogged?.Invoke(message);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void Log(string message, params string[] tags)
	{
		if (tags != null && tags.Length > 0)
		{
			if (!HasActiveTag(tags)) return;
			if (LogTagHeader)
				message = "[" + System.String.Join(',', tags) + "] " + message;
		}

		if (LogTime)
		{
			message = "(" + GetTime() + ") " + message;
		}

		if (LogToHistory)
		{
			LogHistory.AppendLine(message);
		}

		if (LogToUnity)
		{
			UnityEngine.Debug.Log(message);
		}

		OnLogged?.Invoke(message);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void Log(object obj, params string[] tags)
	{
		Log(obj.ToString());
	}

	static bool HasActiveTag(params string[] tags)
	{
		foreach (var t in tags)
		{
			if (activeTags.Contains(t))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Logging Error always display them, even when using tags
	/// </summary>
	/// <param name="message"></param>
	/// <param name="tags"></param>
	public static void LogError(string message, params string[] tags)
	{
		if (tags != null && tags.Length > 0)
		{
			if (LogTagHeader)
				message = "[" + System.String.Join(',', tags) + "] " + message;
		}

		if (LogTime)
		{
			message = "(" + GetTime() + ") " + message;
		}

		if (LogToHistory)
		{
			LogHistory.AppendLine(message);
		}

		UnityEngine.Debug.LogError(message);
		OnErrorLogged?.Invoke(message);
	}

	static string GetTime()
	{
		return System.DateTime.Now.ToString("HH:mm:ss");
	}

	public static void Clear()
	{
		LogHistory.Clear();
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void SetTagActive(string tag)
	{
		if (!string.IsNullOrWhiteSpace(tag) && !activeTags.Contains(tag))
		{
			activeTags.Add(tag);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[Conditional("WPLOG")]
	public static void SetTagDisabled(string tag)
	{
		if (!string.IsNullOrWhiteSpace(tag) && tag != "F")  // F tag can't be disabled
		{
			activeTags.Remove(tag);
		}
	}

	public static bool IsTagActive(string tag)
	{
		if (string.IsNullOrWhiteSpace(tag)) return false;
		return activeTags.Contains(tag);
	}

	public static string[] GetTags()
	{
		return activeTags.ToArray();
	}
}

/// <summary>
/// This static class only contain some possible tags.
/// You can use any other string as tag.
/// Tips: Make your own static class that contain const string for your own tags
/// </summary>
public class WPMainTag
{
	// Special tag that always display a log
	public const string FORCE = "F";

	// Examples of tags
	public const string INFO = "INFO";
	public const string WARNING = "WARN";
	public const string IMPORTANT = "IMP";
	public const string ANALYTIC = "ANALYTIC";
	public const string PLAYER = "PLAYER";
	public const string UI = "UI";
}
