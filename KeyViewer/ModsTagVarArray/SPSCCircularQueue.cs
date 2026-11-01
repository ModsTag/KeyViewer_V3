/*
 * Copyright (c) 2026 ModsTag. All rights reserved.
 *
 * 本文件属于 ModsTagLib 项目。
 * 本项目整体以 GNU General Public License (GPL) 发布，详见项目根目录的 LICENSE 文件。
 * This file is part of the ModsTagLib project.
 * The project as a whole is licensed under the GNU General Public License (GPL).
 * See the LICENSE file in the project root for details.
 *
 * 例外声明：
 * 本类为未开源代码，源自于闭源的 ModsTagLib.dll。
 * 由于其特殊性，本内联实现仅用于当前项目，不受本项目 GPL 协议的覆盖。
 * 你不应在其他项目中直接复制此类的任何代码。
 * EXCEPTION:
 * This class is closed-source code originating from the proprietary ModsTagLib.dll.
 * Due to its special nature, this inline implementation is used solely within this project
 * and is NOT covered by the project's GPL license.
 * You should not copy any code from this class into other projects.
 *
 * 本文件按"现状"提供，不附带任何明示或暗示的担保。
 * Provided "AS IS", without warranty of any kind.
 */

// #define WEAK_MEMORY_ORDER

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KeyViewer.ModsTagVarArray
{
    [StructLayout(LayoutKind.Sequential)]
    internal class SPSCCircularQueue<T>
    {
        private static readonly bool IS_UNMANAGED = false;
        [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 56)] private readonly struct Padding64 { }
        private volatile int m_head;
        private readonly int m_h;
        private readonly Padding64 m_hh;
        private volatile int m_last;
        private readonly int m_l;
        private readonly Padding64 m_ll;
        private volatile T[] m_array;
        private volatile int m_mask;
        private readonly int m_align;
        private readonly object m_wlock;
        private readonly object m_rlock;

        // [ObjectTarget(ObjectTarget.ThreadSafeType.s_MT)]
        public SPSCCircularQueue(int pow2)
        {
            if (pow2 < 1) throw new System.Exception("size so small");
            if (pow2 > 24) throw new System.Exception("size so big");
            int size = 1 << pow2;
            m_array = new T[size];
            m_mask = size - 1;
            m_last = 0;
            m_head = 0;
            m_wlock = new();
            m_rlock = new();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Index(int index) => ((index & m_mask) + m_mask + 1) & m_mask;

        public int Head => m_head;
        public int Last => m_last;
        public int Length => m_mask + 1;
        public int Count => (m_last - m_head + m_mask + 1) & m_mask;
        public int PCount
        {
            get
            {
                lock (m_rlock)
                {
                    return (m_last - m_head + m_mask + 1) & m_mask;
                }
            }
        }
        public int CCount
        {
            get
            {
                lock (m_wlock)
                {
                    return (m_last - m_head + m_mask + 1) & m_mask;
                }
            }
        }
        public T this[int index]
        {
            get => m_array[Index(index + m_last)];
            // [ObjectTarget(ObjectTarget.ThreadSafeType.s_ST)]
            set => m_array[Index(index + m_last)] = value;
        }
        // [ObjectTarget(ObjectTarget.ThreadSafeType.s_ST)]
        public void Clear()
        {
            m_last = 0;
            m_head = 0;
        }
        // [ObjectTarget(ObjectTarget.ThreadSafeType.s_ST)]
        public T[] ToArray()
        {
            throw new NotImplementedException();
        }

        private void ExtendSize()
        {
            lock (m_rlock)
            {
                m_mask = (m_mask << 1) | 1;
                T[] array = new T[m_mask + 1];
                if (m_last > 0)
                    Array.Copy(m_array, 0, array, 0, m_last);
                Array.Copy(m_array, m_head, array, m_array.Length + m_head, m_array.Length - m_head);

                m_head += m_array.Length;
                m_array = array;
            }
        }

        #region Enqueue
        public bool TryEnqueue(T value)
        {
            lock (m_wlock)
            {
            START:
                int last = m_last;
                int next = (last + 1) & m_mask;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (next == m_head)
                {
                    ExtendSize();
                    goto START;
                }
                m_array[last] = value;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                m_last = next;
                return true;
            }
        }
        public void Enqueue(T value)
        {
            lock (m_wlock)
            {
            START:
                int last = m_last;
                int next = (last + 1) & m_mask;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (next == m_head)
                {
                    ExtendSize();
                    goto START;
                }
                m_array[last] = value;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                m_last = next;
            }
        }
        #endregion

        #region Dequeue
        public bool TryDequeue(out T value)
        {
            lock (m_rlock)
            {
                int head = m_head;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (m_last == head) { value = default; return false; }
                value = m_array[head];
                if (!IS_UNMANAGED)
                    m_array[head] = default;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                m_head = (head + 1) & m_mask;
                return true;
            }
        }
        public T Dequeue()
        {
            lock (m_rlock)
            {
                int head = m_head;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (m_last == head) throw new InvalidOperationException("empty array");
                T value = m_array[head];
                if (!IS_UNMANAGED)
                    m_array[head] = default;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                m_head = (head + 1) & m_mask;
                return value;
            }
        }
        #endregion

        #region Peek
        public bool TryPeek(out T value)
        {
            lock (m_rlock)
            {
                int head = m_head;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (m_last == head) { value = default; return false; }
                value = m_array[head];
                return true;
            }
        }
        public T Peek()
        {
            lock (m_rlock)
            {
                int head = m_head;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (m_last == head) throw new InvalidOperationException("empty array");
                return m_array[head];
            }
        }
        #endregion

        #region Remove
        public bool TryRemove()
        {
            lock (m_rlock)
            {
                int head = m_head;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (m_last == head) return false;
                if (!IS_UNMANAGED)
                    m_array[head] = default;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                m_head = (head + 1) & m_mask;
                return true;
            }
        }
        public void Remove()
        {
            lock (m_rlock)
            {
                int head = m_head;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                if (m_last == head) throw new InvalidOperationException("empty array");
                if (!IS_UNMANAGED)
                    m_array[head] = default;
#if WEAK_MEMORY_ORDER
                Thread.MemoryBarrier();
#endif
                m_head = (head + 1) & m_mask;
            }
        }
        #endregion
    }
}
