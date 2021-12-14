// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace aoc
{
    public static class Helpers
    {
        public static void Deconstruct<T>(this T[] arr, out T a, out T b)
        {
            if (arr.Length != 2)
                throw new ArgumentException($"{nameof(arr)} must be 2 elements in length", nameof(arr));
            a = arr[0];
            b = arr[1];
        }
        
        public static IEnumerable<T> AsEnumerable<T>(this T value)
        {
            return Enumerable.Repeat(value, 1);
        }

        public static void AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            TValue add,
            Func<TValue, TValue> update)
        {
            if (dict.TryGetValue(key, out var existing))
            {
                dict[key] = update(existing);
            }
            else
            {
                dict.Add(key, add);
            }
        }
    }
}