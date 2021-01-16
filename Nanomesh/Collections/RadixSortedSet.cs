﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Nanomesh.Collections
{
    public class RadixSortedSet<T> : IEnumerable<T>
    {
        private Func<T, float> _valueGetter;
        private ConcurrentBag<HashSet<T>> _bag = new ConcurrentBag<HashSet<T>>();

        public RadixSortedSet(Func<T, float> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        private HashSet<T> RentHashSet()
        {
            return _bag.TryTake(out HashSet<T> item) ? item : new HashSet<T>();
        }

        private void ReturnHashSet(HashSet<T> item)
        {
            item.Clear();
            _bag.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe int GetHash(T item)
        {
            float value = _valueGetter(item);

            Debug.Assert(value >= 0f, "Only works with positive numbers !");

            float* fRef = &value;
            return *(int*)fRef;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBitSet(int num, int bit)
        {
            return 1 == ((num >> bit) & 1);
        }

        private BitNode _root = new BitNode();

        public int Count { get; private set; }

        public void Add(T item)
        {
            if (AddInternal(item, 31, _root))
                Count++;
        }

        private bool AddInternal(T item, int bitStart, BitNode current)
        {
            int hash = GetHash(item);

            for (int bit = bitStart; bit >= 0; bit--)
            {
                bool is1 = IsBitSet(hash, bit);

                if (is1)
                {
                    if (current.node1 == null)
                    {
                        current = current.node1 = new BitNode();
                        break;
                    }
                    else
                    {
                        current = current.node1;
                        if (current.values?.Count > 0 && GetHash(current.values.First()) != hash)
                        {
                            foreach (var val in current.values)
                            {
                                AddInternal(val, bit - 1, current);
                            }
                            ReturnHashSet(current.values);
                            current.values = null;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (current.node0 == null)
                    {
                        current = current.node0 = new BitNode();
                        break;
                    }
                    else
                    {
                        current = current.node0;
                        if (current.values?.Count > 0 && GetHash(current.values.First()) != hash)
                        {
                            foreach (var val in current.values)
                            {
                                AddInternal(val, bit - 1, current);
                            }
                            ReturnHashSet(current.values);
                            current.values = null;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (current.values == null)
                current.values = RentHashSet();

            return current.values.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            int currentCount = 0;
            int depth = 0;
            int depthLastSplit = -1;
            var history = new BitNode[33];

            history[0] = _root;

            while (currentCount < Count)
            {
                while (depth < 32)
                {
                    if (history[depth].node0 == null)
                    {
                        history[depth + 1] = history[depth].node1;
                    }
                    else
                    {
                        if (history[depth].node1 == null)
                        {
                            history[depth + 1] = history[depth].node0;
                        }
                        else
                        {
                            // Tree splits here
                            if (history[depth].node0 == history[depth + 1])
                            {
                                // node0 was just browsed, so now we can go to node1
                                history[depth + 1] = history[depth].node1;
                            }
                            else
                            {
                                if (history[depth].node1 == history[depth + 1])
                                {
                                    do
                                    {
                                        history[depth + 1] = null;
                                        depth--;
                                    }
                                    while (depth >=0 && !(history[depth].node0 != null
                                        && history[depth].node1 != null
                                        && history[depth].node0 == history[depth + 1]));

                                    depthLastSplit = depth;

                                    continue;
                                }
                                else
                                {
                                    history[depth + 1] = history[depth].node0;
                                    depthLastSplit = depth;
                                }
                            }
                        }
                    }

                    if (history[depth].values?.Count > 0)
                    {
                        depthLastSplit = depth;
                        break;
                    }

                    depth++;
                }

                foreach (var item in history[depth].values)
                {
                    yield return item;
                    currentCount++;
                }

                depth = depthLastSplit;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            int hash = GetHash(item);
            BitNode current = _root;

            for (int bit = 31; bit >= 0; bit--)
            {
                bool is1 = IsBitSet(hash, bit);

                if (is1)
                {
                    if (current.node1 == null)
                    {
                        break;
                    }
                    else
                    {
                        current = current.node1;
                    }
                }
                else
                {
                    if (current.node0 == null)
                    {
                        break;
                    }
                    else
                    {
                        current = current.node0;
                    }
                }
            }

            if (current.values == null)
                return false;

            return current.values.Contains(item);
        }

        public class BitNode
        {
            public BitNode node0;
            public BitNode node1;
            public HashSet<T> values;
        }
    }
}
