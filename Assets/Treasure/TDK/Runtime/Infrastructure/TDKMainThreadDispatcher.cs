using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Treasure
{
    public class TDKMainThreadDispatcher : MonoBehaviour
    {
        private readonly static Queue<Action> _executionQueue = new Queue<Action>();

        private static TDKMainThreadDispatcher _instance = null;

        public void Update()
        {
            lock(_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        // create a GameObject to grab stuff from the queue
        public static void StartProcessing() {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType(typeof(TDKMainThreadDispatcher)) as TDKMainThreadDispatcher;
                
                if (_instance == null)
                {
                    // create a new instance
                    _instance = new GameObject("TDKMainThreadDispatcher", new Type[] {
                        typeof(TDKMainThreadDispatcher),
                    }).GetComponent<TDKMainThreadDispatcher>();

                    DontDestroyOnLoad(_instance.gameObject);
                }
            }
        }

        /// <summary>
        /// Locks the queue and adds the IEnumerator to the queue
        /// </summary>
        /// <param name="action">IEnumerator function that will be executed from the main thread.</param>
        public static void Enqueue(IEnumerator action)
        {
            // Debug.Log("[TDKMainThreadDispatcher] Enqueuing enumerator");
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() => {
                    _instance.StartCoroutine(action);
                });
            }
        }

        /// <summary>
        /// Locks the queue and adds the Action to the queue
        /// </summary>
        /// <param name="action">function that will be executed from the main thread.</param>
        public static void Enqueue(Action action)
        {
            // Debug.Log("[TDKMainThreadDispatcher] Enqueuing action");
            Enqueue(ActionWrapper(action));
        }

        static IEnumerator ActionWrapper(Action a)
        {
            // Debug.Log("[TDKMainThreadDispatcher] Executing action");
            a();
            yield return null;
        }
    }
}
