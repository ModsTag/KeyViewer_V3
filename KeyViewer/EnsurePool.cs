using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KeyViewer
{
    public sealed class EnsurePool<T> where T : Component
    {
        [NotNull]
        private readonly List<T> m_pool;
        [NotNull]
        private readonly Func<T> m_ensurer;
        [NotNull]
        private readonly Predicate<T> m_criteria;
        private readonly Action<T> m_onGet;
        [NotNull]
        private readonly Action<T> m_onRemove;
        private T m_last;
        public EnsurePool(Func<T> ensurer, Predicate<T> ensureCriteria, Action<T> onGet = null, Action<T> onRemove = null, int capacity = -1)
        {
            m_pool = new List<T>();
            m_ensurer = ensurer ?? throw new ArgumentNullException(nameof(ensurer), "Ensurer Cannot Be Null!");
            m_criteria = ensureCriteria ?? throw new ArgumentNullException(nameof(ensureCriteria), "Ensure Criteria Cannot Be Null!");
            m_onGet = onGet;
            m_onRemove = onRemove ?? UnityEngine.Object.Destroy;
            if (capacity > 0)
                Fill(capacity);
        }

        public T this[int index] => m_pool[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
        {
            int count = m_pool.Count;
            int i = 0;
            for (; i < count && !m_criteria(m_pool[i]); i++) { }
            if (i != m_pool.Count)
            {
                m_last = m_pool[i];
            }
            else
            {
                m_last = m_ensurer();
                m_pool.Add(m_last);
            }
            m_onGet?.Invoke(m_last);
            return m_last;
        }
        public T GetLast()
        {
            return m_last;
        }
        public void Clear()
        {
            for (int i = 0; i < m_pool.Count; i++)
                m_onRemove?.Invoke(m_pool[i]);
            m_pool.Clear();
        }
        public void Remove(T t)
        {
            m_onRemove?.Invoke(t);
            m_pool.Remove(t);
        }
        public void RemoveAt(int index)
        {
            if (index >= Count) return;
            m_onRemove?.Invoke(m_pool[index]);
            m_pool.RemoveAt(index);
        }
        public void Fill(int count)
        {
            for (int i = 0; i < count; i++)
                m_pool.Add(m_ensurer());
        }
        public int Count => m_pool.Count;
    }
}
