using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleSignIn
{
    public class AsyncCallbackHelper : MonoBehaviour
    {
        private readonly object _queueLock = new object();
        private readonly List<Action> _queuedActions = new List<Action>();
        private readonly List<Action> _executingActions = new List<Action>();
    
        private static readonly string TAG = nameof(AsyncCallbackHelper);
    
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    
        public static AsyncCallbackHelper Instance { get; private set; }
    
        public void Queue(Action action)
        {
            if (action == null)
            {
                Debug.LogWarning($"{TAG} Trying to queue null action");
                return;
            }
    
            if (Instance == null)
            {
                Debug.LogWarning($"{TAG} Instance is null. Will not queue action.");
                return;
            }
    
            lock (Instance._queueLock)
            {
                Instance._queuedActions.Add(action);
            }
        }
    
        private void Update()
        {
            MoveQueuedActionsToExecuting();
    
            while (_executingActions.Count > 0)
            {
                var action = _executingActions[0];
                _executingActions.RemoveAt(0);
                action();
            }
        }
    
        private void MoveQueuedActionsToExecuting()
        {
            lock (_queueLock)
            {
                while (_queuedActions.Count > 0)
                {
                    var action = _queuedActions[0];
                    _executingActions.Add(action);
                    _queuedActions.RemoveAt(0);
                }
            }
        }
    }
}