// ReSharper disable RedundantUsingDirective

using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace GoogleSignIn
{
	public static class Utils
	{
		internal const string GoogleSignInBridgeClassName = "com.ninevastudios.googlesigninlib.Bridge";

		private static AndroidJavaObject? _activity;

		internal static AndroidJavaObject Activity
		{
			get
			{
				if (_activity == null)
				{
					var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					_activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				}

				return _activity;
			}
		}

		internal static IntPtr GetPointer(this object? obj)
		{
			return obj == null ? IntPtr.Zero : GCHandle.ToIntPtr(GCHandle.Alloc(obj));
		}

		internal static T Cast<T>(this IntPtr instancePtr)
		{
			var instanceHandle = GCHandle.FromIntPtr(instancePtr);
			if (!(instanceHandle.Target is T))
			{
				throw new InvalidCastException("Failed to cast IntPtr");
			}

			var castedTarget = (T)instanceHandle.Target;
			return castedTarget;
		}

		[MonoPInvokeCallback(typeof(ActionStringCallbackDelegate))]
		internal static void ActionStringCallback(IntPtr actionPtr, string data)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("ActionStringCallback");
			}

			if (actionPtr != IntPtr.Zero)
			{
				var action = actionPtr.Cast<Action<string>>();
				action(data);
			}
		}

		internal delegate void ActionStringCallbackDelegate(IntPtr actionPtr, string data);

		[MonoPInvokeCallback(typeof(ActionBoolCallbackDelegate))]
		internal static void ActionBoolCallback(IntPtr actionPtr, bool data)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("ActionBoolCallback");
			}

			if (actionPtr != IntPtr.Zero)
			{
				var action = actionPtr.Cast<Action<bool>>();
				action(data);
			}
		}

		internal delegate void ActionBoolCallbackDelegate(IntPtr actionPtr, bool data);
	}
}