using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// A thread-safe class which holds a queue with actions to execute on the next Update() method. It can be used to make calls to the main thread for
    /// things such as UI Manipulation in Unity.
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private readonly Queue<Action> _executionQueue = new();

        /// <summary>
        /// Gets the singleton instance of the <see cref="UnityMainThreadDispatcher"/> class.
        /// </summary>
        public static UnityMainThreadDispatcher Instance { get; private set; }

        /// <summary>
        /// Locks the queue and adds the Action to the queue.
        /// </summary>
        /// <param name="action">Action that will be executed from the main thread.</param>
        public void Enqueue(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// Locks the queue and adds the Action to the queue, returning a Task which is completed when the action completes.
        /// </summary>
        /// <param name="action">Action that will be executed from the main thread.</param>
        /// <returns>A Task that can be awaited until the action completes.</returns>
        public Task EnqueueAsync(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var tcs = new TaskCompletionSource<bool>();

            Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            Action[] actions;

            lock (_executionQueue)
            {
                actions = _executionQueue.ToArray();
                _executionQueue.Clear();
            }

            foreach (var action in actions)
            {
                action.Invoke();
            }
        }
    }
}
