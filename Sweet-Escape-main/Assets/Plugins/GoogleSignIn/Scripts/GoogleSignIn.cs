using System;
using UnityEngine;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace GoogleSignIn
{
	public static class GoogleSignIn
	{
		public static void PromptSignInWithGoogle(string clientId, Action<string> onSuccess, Action<string> onError)
		{
			if (Application.isEditor)
			{
				onError("Won't work in the Editor");
				return;
			}

#if UNITY_ANDROID
			var proxy = new SuccessErrorProxy(onSuccess, onError);

			var bridgeClass = new AndroidJavaClass(Utils.GoogleSignInBridgeClassName);
			bridgeClass.CallStatic("signInWithGoogle", Utils.Activity, clientId, proxy);
#elif UNITY_IOS
			_promptGoogleSignIn(clientId,
				Utils.ActionStringCallback, onSuccess.GetPointer(),
				Utils.ActionStringCallback, onError.GetPointer());
#endif
		}

		public static void SignOut(Action<string> onCompleted)
		{
			if (Application.isEditor)
			{
				onCompleted("Won't work in the Editor");
				return;
			}

#if UNITY_ANDROID
			var bridgeClass = new AndroidJavaClass(Utils.GoogleSignInBridgeClassName);

			var proxy = new SuccessErrorProxy(onCompleted, error => { });
			bridgeClass.CallStatic("logout", Utils.Activity, proxy);
#elif UNITY_IOS
			_logout();
			onCompleted(string.Empty);
#endif
		}

		public static void RequestNotificationPermission(Action<bool> onCompleted)
		{
			if (Application.isEditor)
			{
				onCompleted(true);
				return;
			}

#if UNITY_ANDROID
			var proxy = new PermissionRequestProxy(onCompleted);

			var bridgeClass = new AndroidJavaClass(Utils.GoogleSignInBridgeClassName);
			bridgeClass.CallStatic("requestNotificationPermission", Utils.Activity, proxy);
#elif UNITY_IOS
			_checkNotificationPermission(Utils.ActionBoolCallback, onCompleted.GetPointer());
#else
			onCompleted(true);
#endif
		}

#if UNITY_IOS
		[DllImport("__Internal")]
		static extern void _promptGoogleSignIn(string clientId,
			Utils.ActionStringCallbackDelegate onSuccess, IntPtr onSuccessActionPtr,
			Utils.ActionStringCallbackDelegate onError, IntPtr onErrorActionPtr);

		[DllImport("__Internal")]
		static extern void _logout();

		[DllImport("__Internal")]
		static extern void _checkNotificationPermission(Utils.ActionBoolCallbackDelegate onCompleted, IntPtr actionPtr);
#endif
	}
}