using KeyViewer.ModsTagVarArray;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeyViewer.API
{
    public static class InputAPI
    {
        public static bool Lock
        {
            get => m_lock;
            set
            {
                m_lock = value;
                Array.Clear(m_lockkeys, 0, m_lockkeys.Length);
            }
        }

        public static void Update()
        {
            if (!AsyncInputManager.isActive)
                return;
            int count = m_queue.Count;
            while (m_keyQueue.TryDequeue(out var result))
            {
                for (int i = 0; i < count; i++)
                {
                    var (key, action) = m_queue[i];
                    if (result.Item2 == key)
                        action(result.Item1, result.Item3);
                }
            }
        }

        public static void SetKeyState(KeyCode key, bool state, long time = 0)
            => m_keyQueue.Enqueue((time != 0 ? time : TimeGetter.GetTime(), key, state));
        public static void SetLockKeyState(KeyCode key, bool state)
            => m_keyQueue.Enqueue((TimeGetter.GetTime(), key, state));

        public static void RemoveAllListener()
        {
            m_queue.Clear();
        }

        public static void AddKeyListener(KeyCode key, Action<long, bool> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action), "Action Cannot Be Null!");
            m_queue.Add((key, action));
        }

        public static void RemoveKeyListener(KeyCode key, Action<long, bool> action)
        {
            for (int i = 0; i < m_queue.Count; i++)
            {
                var (k, a) = m_queue[i];
                if (k == key && a == action)
                {
                    m_queue.RemoveAt(i);
                    return;
                }
            }
        }

        private static readonly List<(KeyCode, Action<long, bool>)> m_queue = new(4);
        private static readonly SPSCCircularQueue<(long, KeyCode, bool)> m_keyQueue = new(4);
        private static readonly bool[] m_lockkeys = new bool[690];
        private static bool m_lock;
    }
}
