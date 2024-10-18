using System;
using JetBrains.Annotations;
using UnityEngine;

namespace GoogleSignIn
{
	public class PermissionRequestProxy : AndroidJavaProxy
	{
		private readonly Action<bool> _callback;

		public PermissionRequestProxy(Action<bool> callback)
			: base("com.ninevastudios.googlesigninlib.PermissionRequestProxy")
		{
			_callback = callback;
		}

		[UsedImplicitly]
		public void OnPermissionGrantResult(bool granted)
		{
			_callback(granted);
		}
	}
}