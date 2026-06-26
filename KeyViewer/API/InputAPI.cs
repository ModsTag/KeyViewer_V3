using System;
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

        public static void UpdateKeyState(KeyCode key, bool state)
            => m_keys[(int)key] = state;
        public static void UpdateLockKeyState(KeyCode key, bool state)
            => m_lockkeys[(int)key] = state;
        public static bool GetKey(KeyCode key) => (int)key < 679 && (m_lock ? m_lockkeys[(int)key] : m_keys[(int)key]);

        private static readonly bool[] m_keys = new bool[690];
        private static readonly bool[] m_lockkeys = new bool[690];
        private static bool m_lock;
    }
}
