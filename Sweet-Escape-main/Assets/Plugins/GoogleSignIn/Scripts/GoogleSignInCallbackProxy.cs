using System;
using JetBrains.Annotations;
using UnityEngine;

namespace GoogleSignIn
{
	public class SuccessErrorProxy : AndroidJavaProxy
	{
		private readonly Action<string> _onSuccess;
		private readonly Action<string> _onError;

		public SuccessErrorProxy(Action<string> onSuccess, Action<string> onError) :
			base("com.ninevastudios.googlesigninlib.SuccessErrorProxy")
		{
			_onSuccess = onSuccess;
			_onError = onError;
		}

		[UsedImplicitly]
		public void OnSuccess(string token)
		{
			AsyncCallbackHelper.Instance.Queue(() => _onSuccess(token));
		}

		[UsedImplicitly]
		public void OnError(string error)
		{
			AsyncCallbackHelper.Instance.Queue(() => _onError(error));
		}
	}
}